using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[Serializable]
public class SavedPlayer
{
    public Vector3 position;
    public Quaternion rotation;
    public int selectedSlot;
    public List<Item> hotbar;
}


public class PlayerSaveSystem : MonoBehaviour
{
    public GameObject player;
    private string savePath => Path.Combine(Application.persistentDataPath, "playerSave.json");

    public void SavePlayer()
    {
        var inventory = player.GetComponent<Inventory>();

        SavedPlayer data = new SavedPlayer
        {
            position = player.transform.position,
            rotation = player.transform.rotation,
            selectedSlot = inventory.selectedSlot,
            hotbar = new List<Item>(inventory.hotbar)
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Игрок сохранён: " + savePath);
    }

    public void LoadPlayer()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("Сохранение не найдено");
            return;
        }

        string json = File.ReadAllText(savePath);
        SavedPlayer data = JsonUtility.FromJson<SavedPlayer>(json);

        player.transform.position = data.position;
        player.transform.rotation = data.rotation;

        var inventory = player.GetComponent<Inventory>();
        inventory.selectedSlot = data.selectedSlot;
        inventory.hotbar = new List<Item>(data.hotbar);

        // Восстанавливаем связи вручную
        inventory.Player = player;
        inventory.hotbarUI = GameObject.Find("HotbarUI").GetComponent<HotbarUI>();

        Debug.Log("Игрок загружен");
    }
}
