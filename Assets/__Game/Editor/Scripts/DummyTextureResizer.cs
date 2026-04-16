using UnityEditor;
using UnityEngine;

/// <summary>Dummy 폴더 텍스처 maxTextureSize 일괄 축소 (2048→512, 1024→256)</summary>
public static class DummyTextureResizer
{
    private const string TargetFolder = "Assets/__Game/_Core/Dummy";

    [MenuItem("Tools/Dummy 텍스처 사이즈 축소")]
    private static void ResizeAll()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { TargetFolder });
        int changed = 0;

        try
        {
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                EditorUtility.DisplayProgressBar("텍스처 리사이즈", path, (float)i / guids.Length);

                int cur = importer.maxTextureSize;
                int next = GetReducedSize(cur);
                if (next == cur) continue;

                importer.maxTextureSize = next;
                importer.SaveAndReimport();
                changed++;
                Debug.Log($"[DummyTextureResizer] {path}: {cur} → {next}");
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        Debug.Log($"[DummyTextureResizer] 완료: {changed}/{guids.Length} 텍스처 변경됨");
    }

    private static int GetReducedSize(int _size)
    {
        return _size switch
        {
            2048 => 512,
            1024 => 256,
            512 => 128,
            _ => _size
        };
    }
}
