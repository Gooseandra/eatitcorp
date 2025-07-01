using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    public int slotIndex;
    public TextMeshProUGUI dateText;
    public Image previewImage;

    private void Start()
    {
        LoadPreviewData();
    }

    public void LoadPreviewData()
    {
        string metaPath = Path.Combine(Application.persistentDataPath, $"save_{slotIndex}.meta.json");
        string imagePath = Path.Combine(Application.persistentDataPath, $"save_{slotIndex}.png");

        if (File.Exists(metaPath))
        {
            var metaJson = File.ReadAllText(metaPath);
            var meta = JsonUtility.FromJson<SaveManager.SaveMeta>(metaJson);
            dateText.text = meta.saveDate;
        }
        else
        {
            dateText.text = "-";
            previewImage.gameObject.SetActive(false);
        }

        if (File.Exists(imagePath))
        {
            byte[] imgBytes = File.ReadAllBytes(imagePath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imgBytes);

            previewImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            previewImage.sprite = null;
        }
    }

    public void OnClickLoad()
    {
        if (dateText.text == "-")
        {
            SaveManager.LoadOnNextScene = false;
        }
        else
        {
            SaveManager.LoadOnNextScene = true;
        }
        SaveManager.currentSaveSlot = slotIndex;
        SceneManager.LoadScene(1);
    }
}
