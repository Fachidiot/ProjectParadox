using UnityEditor;

/// <summary>*_normal 텍스처 임포트 시 Texture Type을 Normal Map으로 자동 설정</summary>
public class NormalMapPostprocessor : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (!assetPath.Contains("_normal.")) return;

        var importer = assetImporter as TextureImporter;
        if (importer == null) return;

        if (importer.textureType == TextureImporterType.NormalMap) return;

        importer.textureType = TextureImporterType.NormalMap;
    }
}
