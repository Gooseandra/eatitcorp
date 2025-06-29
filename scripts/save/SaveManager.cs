using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SaveManager : MonoBehaviour
{
    public static bool LoadOnNextScene = false;

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

#if UNITY_EDITOR
        string[] guids = AssetDatabase.FindAssets("t:prefab", new[] { "Assets/SavePrefabs" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                var indexComp = prefab.GetComponent<PrefabIndex>();
                if (indexComp != null)
                {
                    int index = indexComp.index;
                    if (!prefabDict.ContainsKey(index))
                        prefabDict.Add(index, prefab);
                    else
                        Debug.LogWarning($"Duplicate prefab index {index} at {path}");
                }
            }
        }
#else
        Debug.LogError("Prefab loading only works in the editor.");
#endif
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

            savedObjects.Add(data);
        }

        // Save the Player
        if (player != null)
        {
            var data = new SavedObjectData
            {
                id = nextId++,
                prefabIndex = -1,
                isSceneObject = true,
                sceneObjectName = "Player",
                position = player.transform.position,
                rotation = player.transform.rotation,
                scale = player.transform.localScale,
                parentId = null
            };

            foreach (var script in player.GetComponents<MonoBehaviour>())
            {
                var type = script.GetType();
                if (type == typeof(SaveManager)) continue;

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

            savedObjects.Add(data);
        }

        string jsonPath = Application.persistentDataPath + "/save.json";
        File.WriteAllText(jsonPath, JsonUtility.ToJson(new SaveWrapper { objects = savedObjects }));
        Debug.Log("Scene saved: " + jsonPath);
    }

    public void LoadScene()
    {
        string jsonPath = Application.persistentDataPath + "/save.json";
        if (!File.Exists(jsonPath))
        {
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

        // Instantiate normal objects
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

            foreach (var script in saved.scripts)
            {
                var type = Type.GetType(script.scriptName);
                if (type == null) continue;

                var comp = go.GetComponent(type) ?? go.AddComponent(type);
                JsonUtility.FromJsonOverwrite(script.jsonData, comp);
            }
        }

        // Apply parent relationships
        foreach (var saved in wrapper.objects)
        {
            if (saved.parentId.HasValue && idToObject.ContainsKey(saved.parentId.Value) && idToObject.ContainsKey(saved.id))
            {
                idToObject[saved.id].transform.SetParent(idToObject[saved.parentId.Value].transform, false);
            }
        }

        // Apply transforms
        foreach (var saved in wrapper.objects)
        {
            GameObject go = null;

            if (saved.isSceneObject && saved.sceneObjectName == "Player")
            {
                go = GameObject.FindGameObjectWithTag("Player");
            }
            else if (idToObject.TryGetValue(saved.id, out var obj))
            {
                go = obj;
            }

            if (go == null) continue;

            go.transform.position = saved.position;
            go.transform.rotation = saved.rotation;
            go.transform.localScale = saved.scale;

            foreach (var script in saved.scripts)
            {
                var type = Type.GetType(script.scriptName);
                if (type == null) continue;

                var comp = go.GetComponent(type) ?? go.AddComponent(type);
                JsonUtility.FromJsonOverwrite(script.jsonData, comp);
            }
        }

        Debug.Log("Scene loaded.");
    }

    public void LoadSceneByIndex(int sceneIndex)
    {
        LoadOnNextScene = true;
        SceneManager.LoadScene(sceneIndex);
    }
}
