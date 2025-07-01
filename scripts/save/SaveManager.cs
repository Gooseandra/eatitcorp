using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SaveManager : MonoBehaviour
{
    public static bool LoadOnNextScene = false;
    public static int currentSaveSlot = 0; // по умолчанию слот 0


    [Serializable]
    public class ScriptData
    {
        public string scriptName;
        public string jsonData;
    }

    [Serializable]
    public class SavedObjectData
    {
        public int id;
        public int prefabIndex;
        public bool isSceneObject;
        public string sceneObjectName;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public int? parentId;
        public List<ScriptData> scripts = new List<ScriptData>();
    }

    [Serializable]
    public class SaveWrapper
    {
        public List<SavedObjectData> objects;
    }

    private Dictionary<int, GameObject> prefabDict;

    private void Awake()
    {
        prefabDict = new Dictionary<int, GameObject>();

        GameObject[] prefabs = Resources.LoadAll<GameObject>("SavePrefabs");
        foreach (var prefab in prefabs)
        {
            var indexComp = prefab.GetComponent<PrefabIndex>();
            if (indexComp != null)
            {
                int index = indexComp.index;
                if (!prefabDict.ContainsKey(index))
                    prefabDict.Add(index, prefab);
                else
                    Debug.LogWarning($"Duplicate prefab index {index} in {prefab.name}");
            }
            else
            {
                Debug.LogWarning($"Prefab {prefab.name} missing PrefabIndex component");
            }
        }
    }


    private void Start()
    {
        if (LoadOnNextScene)
        {
            LoadOnNextScene = false;
            LoadScene();
        }
    }

    public void SaveScene()
    {
        var allObjects = FindObjectsOfType<PrefabIndex>();
        List<PrefabIndex> rootObjects = new List<PrefabIndex>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        foreach (var obj in allObjects)
        {
            if (obj.gameObject == player) continue;

            if (obj.transform.parent == null || obj.transform.parent.GetComponent<PrefabIndex>() == null)
            {
                rootObjects.Add(obj);
            }
        }

        List<SavedObjectData> savedObjects = new List<SavedObjectData>();
        Dictionary<GameObject, int> objectToId = new Dictionary<GameObject, int>();
        int nextId = 0;

        foreach (var obj in rootObjects)
        {
            objectToId[obj.gameObject] = nextId++;
        }

        foreach (var obj in rootObjects)
        {
            GameObject go = obj.gameObject;
            var data = new SavedObjectData
            {
                id = objectToId[go],
                prefabIndex = obj.index,
                isSceneObject = false,
                sceneObjectName = "",
                position = go.transform.localPosition,
                rotation = go.transform.localRotation,
                scale = go.transform.localScale,
                parentId = go.transform.parent != null && objectToId.ContainsKey(go.transform.parent.gameObject)
                            ? objectToId[go.transform.parent.gameObject]
                            : (int?)null
            };

            // Сохраняем MonoBehaviour скрипты (включая Storage)
            foreach (var script in go.GetComponents<MonoBehaviour>())
            {
                var type = script.GetType();
                if (type == typeof(PrefabIndex) || type == typeof(SaveManager)) continue;

                try
                {
                    string json = JsonUtility.ToJson(script);
                    data.scripts.Add(new ScriptData
                    {
                        scriptName = type.AssemblyQualifiedName,
                        jsonData = json
                    });
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Serialization error for {type}: {e.Message}");
                }
            }

            // Хранилище — сохраняем его вручную
            if (go.TryGetComponent(out Storage storage))
            {
                var storedItems = storage.GetSavedItems(); // твой метод
                string json = JsonUtility.ToJson(new ItemListWrapper { items = storedItems });
                data.scripts.Add(new ScriptData
                {
                    scriptName = typeof(Storage).AssemblyQualifiedName + "_StorageItems",
                    jsonData = json
                });
            }

            savedObjects.Add(data);
        }

        // Сохраняем инвентарь игрока
        if (player.TryGetComponent(out Inventory inv))
        {
            inv.SaveInventory(this);
        }

        string jsonPath = GetSaveFilePath();

        File.WriteAllText(jsonPath, JsonUtility.ToJson(new SaveWrapper { objects = savedObjects }));

        var meta = new SaveMeta { saveDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm") };
        string metaPath = GetMetaFilePath();
        File.WriteAllText(metaPath, JsonUtility.ToJson(meta));

        // Сохраняем скриншот
        StartCoroutine(SaveScreenshot(currentSaveSlot));

        Debug.Log("Scene saved: " + jsonPath);
    }

    private string GetMetaFilePath()
    {
        return Path.Combine(Application.persistentDataPath, $"save_{currentSaveSlot}.meta.json");
    }

    private string GetScreenshotPath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"save_{slot}.png");
    }

    private IEnumerator SaveScreenshot(int slot)
    {
        yield return new WaitForEndOfFrame();

        Camera cam = Camera.main;
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        cam.targetTexture = rt;

        RenderTexture oldRT = RenderTexture.active;
        RenderTexture.active = rt;

        cam.Render();

        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply();

        cam.targetTexture = null;
        RenderTexture.active = oldRT;
        Destroy(rt);

        byte[] bytes = screenshot.EncodeToPNG();
        File.WriteAllBytes(GetScreenshotPath(slot), bytes);
        Destroy(screenshot);
    }


    private string GetSaveFilePath()
    {
        return Path.Combine(Application.persistentDataPath, $"save_{currentSaveSlot}.json");
    }


    public void LoadScene()
    {
        string jsonPath = GetSaveFilePath();
        if (!File.Exists(jsonPath))
        {
            SceneManager.LoadScene(2);
            Debug.LogError("Save file not found.");
            return;
        }

        string json = File.ReadAllText(jsonPath);
        SaveWrapper wrapper = JsonUtility.FromJson<SaveWrapper>(json);

        foreach (var obj in FindObjectsOfType<PrefabIndex>())
        {
            Destroy(obj.gameObject);
        }

        Dictionary<int, GameObject> idToObject = new Dictionary<int, GameObject>();

        foreach (var saved in wrapper.objects)
        {
            if (saved.isSceneObject) continue;

            if (!prefabDict.TryGetValue(saved.prefabIndex, out GameObject prefab))
            {
                Debug.LogWarning($"Prefab with index {saved.prefabIndex} not found.");
                continue;
            }

            GameObject go = Instantiate(prefab);
            idToObject[saved.id] = go;

            // Восстанавливаем компоненты
            foreach (var script in saved.scripts)
            {
                if (script.scriptName.EndsWith("_StorageItems")) continue;

                var type = Type.GetType(script.scriptName);
                if (type == null) continue;

                var comp = go.GetComponent(type) ?? go.AddComponent(type);
                JsonUtility.FromJsonOverwrite(script.jsonData, comp);
            }

            // Восстанавливаем предметы хранилища
            var storageScript = go.GetComponent<Storage>();
            if (storageScript != null)
            {
                foreach (var script in saved.scripts)
                {
                    if (script.scriptName == typeof(Storage).AssemblyQualifiedName + "_StorageItems")
                    {
                        var wrapperObj = JsonUtility.FromJson<ItemListWrapper>(script.jsonData);
                        storageScript.LoadFromSavedItems(wrapperObj.items, this);
                    }
                }
            }
        }

        // Устанавливаем родительские связи
        foreach (var saved in wrapper.objects)
        {
            if (saved.parentId.HasValue && idToObject.ContainsKey(saved.parentId.Value) && idToObject.ContainsKey(saved.id))
            {
                idToObject[saved.id].transform.SetParent(idToObject[saved.parentId.Value].transform, false);
            }
        }

        // Устанавливаем трансформации
        foreach (var saved in wrapper.objects)
        {
            if (!idToObject.TryGetValue(saved.id, out GameObject go)) continue;
            go.transform.localPosition = saved.position;
            go.transform.localRotation = saved.rotation;
            go.transform.localScale = saved.scale;
        }

        // Загружаем инвентарь игрока
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.TryGetComponent(out Inventory inv))
        {
            inv.LoadInventory(this);
        }

        // Восстанавливаем thisPrefab у всех ItemPickup
        foreach (var itemPickup in FindObjectsOfType<ItemPickup>())
        {
            if (itemPickup.item == null) continue;

            GameObject prefab = GetPrefabByIndex(itemPickup.item.prefabIndex);
            if (prefab != null)
            {
                itemPickup.thisPrefab = prefab;
                itemPickup.item.thisPrefab = prefab;
            }
            else
            {
                Debug.LogWarning($"[SaveManager] Не найден префаб для prefabIndex = {itemPickup.item.prefabIndex}");
            }
        }


        Debug.Log("Scene loaded.");
    }

    public void LoadSceneByIndex(int sceneIndex)
    {
        LoadOnNextScene = true;
        SceneManager.LoadScene(sceneIndex);
    }

    public GameObject GetPrefabByIndex(int index)
    {
        prefabDict.TryGetValue(index, out var prefab);
        return prefab;
    }

    [Serializable]
    public class ItemListWrapper
    {
        public List<Item.SavedInventoryItem> items;
    }

    [Serializable]
    public class SaveMeta
    {
        public string saveDate;
    }

}
