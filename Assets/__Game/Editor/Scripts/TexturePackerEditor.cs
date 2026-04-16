using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>*_metallic + *_roughness → *_metallicSmoothness (R=metallic, A=smoothness) 자동 패킹</summary>
public class TexturePackerEditor : AssetPostprocessor
{
    private const string SuffixMetallic = "_metallic";
    private const string SuffixRoughness = "_roughness";
    private const string OutputSuffix = "_metallicSmoothness";

    private static bool s_Packing;

    private static void OnPostprocessAllAssets(
        string[] importedAssets, string[] deletedAssets,
        string[] movedAssets, string[] movedFromAssetPaths)
    {
        if (s_Packing) return;

        var folders = new HashSet<string>();
        foreach (string path in importedAssets)
        {
            if (path.Contains("/Backup/")) continue;
            string name = Path.GetFileNameWithoutExtension(path);
            if (name.EndsWith(SuffixMetallic) || name.EndsWith(SuffixRoughness))
                folders.Add(Path.GetDirectoryName(path).Replace('\\', '/'));
        }

        if (folders.Count == 0) return;

        foreach (string folder in folders)
            TryPackFolder(folder);
    }

    private static void TryPackFolder(string folder)
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
        var texPaths = new Dictionary<string, string>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("/Backup/")) continue;
            string dir = Path.GetDirectoryName(path).Replace('\\', '/');
            if (dir != folder) continue;
            texPaths[Path.GetFileNameWithoutExtension(path)] = path;
        }

        var processed = new HashSet<string>();
        foreach (var kv in texPaths)
        {
            string baseName = null;
            if (kv.Key.EndsWith(SuffixMetallic))
                baseName = kv.Key.Substring(0, kv.Key.Length - SuffixMetallic.Length);
            else if (kv.Key.EndsWith(SuffixRoughness))
                baseName = kv.Key.Substring(0, kv.Key.Length - SuffixRoughness.Length);

            if (baseName == null || !processed.Add(baseName)) continue;

            if (!texPaths.ContainsKey(baseName + SuffixMetallic) ||
                !texPaths.ContainsKey(baseName + SuffixRoughness))
                continue;

            if (texPaths.ContainsKey(baseName + OutputSuffix)) continue;

            PackTextureSet(folder, baseName,
                texPaths[baseName + SuffixMetallic],
                texPaths[baseName + SuffixRoughness]);
        }
    }

    private static void PackTextureSet(string folder, string baseName,
        string metallicPath, string roughnessPath)
    {
        var metallicState = SetReadable(metallicPath);
        var roughnessState = SetReadable(roughnessPath);

        var metallicTex = AssetDatabase.LoadAssetAtPath<Texture2D>(metallicPath);
        var roughnessTex = AssetDatabase.LoadAssetAtPath<Texture2D>(roughnessPath);

        if (metallicTex == null || roughnessTex == null)
        {
            Debug.LogError($"[TexturePacker] 텍스처 로드 실패: {baseName}");
            return;
        }

        int width = metallicTex.width;
        int height = metallicTex.height;

        if (roughnessTex.width != width || roughnessTex.height != height)
            Debug.LogWarning($"[TexturePacker] {baseName}: 텍스처 크기가 다릅니다. Metallic 기준({width}x{height})으로 처리합니다.");

        Color32[] metallicPixels = metallicTex.GetPixels32();
        Color32[] roughnessPixels = GetPixelsResized(roughnessTex, width, height);

        // R=metallic, A=smoothness(1-roughness)
        var packedPixels = new Color32[width * height];
        for (int i = 0; i < packedPixels.Length; i++)
        {
            byte metallic = metallicPixels[i].r;
            byte smoothness = (byte)(255 - roughnessPixels[i].r);
            packedPixels[i] = new Color32(metallic, 0, 0, smoothness);
        }

        var packedTex = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
        packedTex.SetPixels32(packedPixels);
        packedTex.Apply();

        string outputPath = $"{folder}/{baseName}{OutputSuffix}.png";
        File.WriteAllBytes(Path.GetFullPath(outputPath), packedTex.EncodeToPNG());
        Object.DestroyImmediate(packedTex);

        s_Packing = true;
        try
        {
            AssetDatabase.ImportAsset(outputPath);
            var outputImporter = AssetImporter.GetAtPath(outputPath) as TextureImporter;
            if (outputImporter != null)
            {
                outputImporter.textureType = TextureImporterType.Default;
                outputImporter.sRGBTexture = false;
                outputImporter.filterMode = FilterMode.Bilinear;
                outputImporter.SaveAndReimport();
            }

            RestoreReadable(metallicState);
            RestoreReadable(roughnessState);
        }
        finally
        {
            s_Packing = false;
        }

        Debug.Log($"[TexturePacker] {baseName}: 패킹 완료 → {outputPath}");
    }

    private static Color32[] GetPixelsResized(Texture2D tex, int width, int height)
    {
        if (tex.width == width && tex.height == height)
            return tex.GetPixels32();

        var rt = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        Graphics.Blit(tex, rt);
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        var resized = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
        resized.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        resized.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        Color32[] pixels = resized.GetPixels32();
        Object.DestroyImmediate(resized);
        return pixels;
    }

    private struct ReadableState
    {
        public TextureImporter importer;
        public bool wasReadable;
    }

    private static ReadableState SetReadable(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        var state = new ReadableState { importer = importer, wasReadable = true };
        if (importer != null && !importer.isReadable)
        {
            state.wasReadable = false;
            importer.isReadable = true;
            importer.SaveAndReimport();
        }
        return state;
    }

    private static void RestoreReadable(ReadableState state)
    {
        if (state.importer != null && !state.wasReadable)
        {
            state.importer.isReadable = false;
            state.importer.SaveAndReimport();
        }
    }
}
