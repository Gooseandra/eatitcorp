using UnityEngine;
using System.IO;

public static class IconSaver
{
    public static void SaveIcon(Sprite icon, int buildingIndex)
    {
        if (icon == null)
        {
            Debug.LogWarning("Icon is null, skipping save");
            return;
        }

        string folderPath = Path.Combine(Application.persistentDataPath, "Icons");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, $"icon_{buildingIndex}.png");

        Texture2D tex = SpriteToTexture2D(icon);
        if (tex == null)
        {
            Debug.LogWarning($"Failed to convert sprite to texture2D for buildingIndex {buildingIndex}");
            return;
        }

        byte[] pngData = tex.EncodeToPNG();
        if (pngData != null)
        {
            File.WriteAllBytes(filePath, pngData);
            Debug.Log($"Icon saved: {filePath}");
        }
        else
        {
            Debug.LogWarning("Failed to encode texture to PNG");
        }
    }

    // Преобразуем Sprite в Texture2D
    private static Texture2D SpriteToTexture2D(Sprite sprite)
    {
        if (sprite.rect.width != sprite.texture.width)
        {
            // Вырезаем нужную область из текстуры
            Texture2D newTex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
            Color[] pixels = sprite.texture.GetPixels((int)sprite.rect.x, (int)sprite.rect.y, (int)sprite.rect.width, (int)sprite.rect.height);
            newTex.SetPixels(pixels);
            newTex.Apply();
            return newTex;
        }
        else
        {
            return sprite.texture;
        }
    }
}
