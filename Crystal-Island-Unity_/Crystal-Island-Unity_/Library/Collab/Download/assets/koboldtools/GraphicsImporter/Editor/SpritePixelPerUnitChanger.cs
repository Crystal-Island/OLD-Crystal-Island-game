using UnityEngine;
using UnityEditor;

public class SpritePixelsPerUnitChanger : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (EditorPrefs.HasKey("custom_importer_pixelperunit"))
        {
            int pixelperunit;
            pixelperunit = EditorPrefs.GetInt("custom_importer_pixelperunit", 100);
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            textureImporter.spritePixelsPerUnit = pixelperunit;
        }
    }
}