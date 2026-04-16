using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>*_texture + *_texture_metallicSmoothness + *_texture_normal 세트 감지 시 URP Lit 머테리얼 자동 생성</summary>
public class MaterialAutoCreatorEditor : AssetPostprocessor
{
    private const string SuffixBase     = "_texture";
    private const string SuffixMetallic = "_texture_metallicSmoothness";
    private const string SuffixNormal   = "_texture_normal";

    private static void OnPostprocessAllAssets(
        string[] importedAssets, string[] deletedAssets,
        string[] movedAssets, string[] movedFromAssetPaths)
    {
        var folders = new HashSet<string>();
        foreach (string path in importedAssets)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            if (name.EndsWith(SuffixBase) || name.EndsWith(SuffixMetallic) || name.EndsWith(SuffixNormal))
                folders.Add(Path.GetDirectoryName(path).Replace('\\', '/'));
        }

        if (folders.Count == 0) return;

        foreach (string folder in folders)
            TryCreateMaterials(folder);
    }

    private static void TryCreateMaterials(string folder)
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
        var texPaths = new Dictionary<string, string>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string dir = Path.GetDirectoryName(path).Replace('\\', '/');
            if (dir != folder) continue;
            texPaths[Path.GetFileNameWithoutExtension(path)] = path;
        }

        var processed = new HashSet<string>();
        foreach (var kv in texPaths)
        {
            string baseName = null;
            // _texture_metallicSmoothness / _texture_normal 먼저 체크 (longer suffix first)
            if (kv.Key.EndsWith(SuffixMetallic))
                baseName = kv.Key.Substring(0, kv.Key.Length - SuffixMetallic.Length);
            else if (kv.Key.EndsWith(SuffixNormal))
                baseName = kv.Key.Substring(0, kv.Key.Length - SuffixNormal.Length);
            else if (kv.Key.EndsWith(SuffixBase))
                baseName = kv.Key.Substring(0, kv.Key.Length - SuffixBase.Length);

            if (baseName == null || !processed.Add(baseName)) continue;

            bool hasBase     = texPaths.ContainsKey(baseName + SuffixBase);
            bool hasMetallic = texPaths.ContainsKey(baseName + SuffixMetallic);
            bool hasNormal   = texPaths.ContainsKey(baseName + SuffixNormal);

            if (!hasBase || !hasMetallic || !hasNormal) continue;

            string matName = "Material_" + Path.GetFileName(folder);
            string matPath = $"{folder}/{matName}.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(matPath) != null) continue;

            CreateMaterial(matPath,
                texPaths[baseName + SuffixBase],
                texPaths[baseName + SuffixMetallic],
                texPaths[baseName + SuffixNormal]);
        }
    }

    private static void CreateMaterial(string matPath, string basePath, string metallicPath, string normalPath)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            Debug.LogError("[MaterialAutoCreator] URP Lit 셰이더를 찾을 수 없습니다.");
            return;
        }

        var baseTex     = AssetDatabase.LoadAssetAtPath<Texture2D>(basePath);
        var metallicTex = AssetDatabase.LoadAssetAtPath<Texture2D>(metallicPath);
        var normalTex   = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);

        var mat = new Material(shader);

        // 텍스처 할당
        mat.SetTexture("_BaseMap", baseTex);
        mat.SetTexture("_MainTex", baseTex);
        mat.SetTexture("_MetallicGlossMap", metallicTex);
        mat.SetTexture("_BumpMap", normalTex);

        // 키워드
        mat.EnableKeyword("_METALLICSPECGLOSSMAP");
        mat.EnableKeyword("_NORMALMAP");

        // 머테리얼 프로퍼티 (Material_Destroyable 기준)
        mat.SetFloat("_WorkflowMode", 1f);
        mat.SetFloat("_Smoothness", 1f);
        mat.SetFloat("_GlossMapScale", 0f);
        mat.SetFloat("_Glossiness", 0f);
        mat.SetFloat("_Metallic", 0f);
        mat.SetFloat("_BumpScale", 1f);
        mat.SetFloat("_OcclusionStrength", 1f);
        mat.SetFloat("_Surface", 0f);
        mat.SetFloat("_Blend", 0f);
        mat.SetFloat("_AlphaClip", 0f);
        mat.SetFloat("_Cull", 2f);
        mat.SetFloat("_ZWrite", 1f);
        mat.SetFloat("_SrcBlend", 1f);
        mat.SetFloat("_DstBlend", 0f);
        mat.SetFloat("_SrcBlendAlpha", 1f);
        mat.SetFloat("_DstBlendAlpha", 0f);
        mat.SetFloat("_ReceiveShadows", 1f);
        mat.SetFloat("_SpecularHighlights", 1f);
        mat.SetFloat("_EnvironmentReflections", 1f);
        mat.SetFloat("_SmoothnessTextureChannel", 0f);
        mat.SetFloat("_BlendModePreserveSpecular", 1f);
        mat.SetFloat("_Cutoff", 0.5f);
        mat.SetFloat("_Parallax", 0.005f);
        mat.SetFloat("_QueueOffset", 0f);

        mat.SetColor("_BaseColor", Color.white);
        mat.SetColor("_Color", Color.white);
        mat.SetColor("_EmissionColor", Color.black);
        mat.SetColor("_SpecColor", new Color(0.2f, 0.2f, 0.2f, 1f));

        // 렌더 타입
        mat.renderQueue = -1;
        mat.SetOverrideTag("RenderType", "Opaque");
        mat.SetShaderPassEnabled("MOTIONVECTORS", false);

        AssetDatabase.CreateAsset(mat, matPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"[MaterialAutoCreator] 머테리얼 생성 완료 → {matPath}");
    }
}
