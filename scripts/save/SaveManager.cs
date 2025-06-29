using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.IO;
using TMPro;

public class SaveManager : MonoBehaviour
{
    private string saveFilePath;

    void Start()
    {
        saveFilePath = Application.persistentDataPath + "/savegame.json";
    }

    // Метод для сохранения всех данных объектов
    public void SaveGame()
    {
        SaveData saveData = new SaveData();

        // Получаем все объекты на сцене
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        // Перебираем все объекты
        foreach (GameObject obj in allObjects)
        {
            if (obj == null)
            {
                Debug.LogWarning("Объект пустой: " + obj.name);
                continue;
            }

            ObjectSaveData objectSaveData = new ObjectSaveData();
            objectSaveData.objectName = obj.name;

            // Получаем все компоненты на объекте
            Component[] components = obj.GetComponents<Component>();

            // Перебираем все компоненты
            foreach (Component component in components)
            {
                if (component == null)
                {
                    Debug.LogWarning("Компонент пустой на объекте: " + obj.name);
                    continue;
                }

                // Получаем все поля компонента
                FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                // Перебираем все поля
                foreach (FieldInfo field in fields)
                {
                    object fieldValue = field.GetValue(component);
                    string valueAsString = fieldValue != null ? fieldValue.ToString() : "null";  // Проверяем на null

                    // Добавляем данные поля в объект
                    FieldData fieldData = new FieldData
                    {
                        fieldName = field.Name,
                        fieldValue = valueAsString
                    };
                    objectSaveData.fields.Add(fieldData);
                }
            }

            saveData.objectData.Add(objectSaveData);
        }

        // Сохраняем все данные в файл
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Game Saved");
    }

    // Метод для загрузки данных
    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            // Загружаем данные для каждого объекта
            foreach (ObjectSaveData objectSaveData in saveData.objectData)
            {
                // Находим объект по имени
                GameObject obj = GameObject.Find(objectSaveData.objectName);
                if (obj == null) continue;

                // Перебираем все компоненты объекта
                Component[] components = obj.GetComponents<Component>();

                foreach (Component component in components)
                {
                    // Перебираем все сохранённые поля
                    foreach (FieldData fieldData in objectSaveData.fields)
                    {
                        FieldInfo field = component.GetType().GetField(fieldData.fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (field != null)
                        {
                            // Преобразуем строковое значение обратно в соответствующий тип
                            object fieldValue = ConvertStringToFieldType(fieldData.fieldValue, field.FieldType);
                            field.SetValue(component, fieldValue);
                        }
                    }
                }
            }

            Debug.Log("Game Loaded");
        }
        else
        {
            Debug.LogWarning("Save file not found!");
        }
    }

    // Метод для преобразования строки в нужный тип
    public static object ConvertStringToFieldType(string value, System.Type fieldType)
    {
        // Преобразование для примитивных типов
        if (fieldType == typeof(int))
        {
            if (int.TryParse(value, out int intValue))
            {
                return intValue;
            }
            else
            {
                Debug.LogError("Invalid integer format: " + value);
                return 0; // Значение по умолчанию
            }
        }
        else if (fieldType == typeof(float))
        {
            if (float.TryParse(value, out float floatValue))
            {
                return floatValue;
            }
            else
            {
                Debug.LogError("Invalid float format: " + value);
                return 0f; // Значение по умолчанию
            }
        }
        else if (fieldType == typeof(bool))
        {
            if (bool.TryParse(value, out bool boolValue))
            {
                return boolValue;
            }
            else
            {
                Debug.LogError("Invalid boolean format: " + value);
                return false; // Значение по умолчанию
            }
        }
        else if (fieldType == typeof(string))
        {
            return value; // Строка остается как есть
        }
        else if (fieldType == typeof(Vector3))
        {
            string[] values = value.Split(',');
            if (values.Length == 3)
            {
                float x = float.Parse(values[0]);
                float y = float.Parse(values[1]);
                float z = float.Parse(values[2]);
                return new Vector3(x, y, z);
            }
            else
            {
                Debug.LogError("Invalid Vector3 format: " + value);
                return Vector3.zero; // Значение по умолчанию
            }
        }
        else if (fieldType == typeof(Quaternion))
        {
            string[] values = value.Split(',');
            if (values.Length == 4)
            {
                float x = float.Parse(values[0]);
                float y = float.Parse(values[1]);
                float z = float.Parse(values[2]);
                float w = float.Parse(values[3]);
                return new Quaternion(x, y, z, w);
            }
            else
            {
                Debug.LogError("Invalid Quaternion format: " + value);
                return Quaternion.identity; // Значение по умолчанию
            }
        }
        else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
        {
            var listType = fieldType.GetGenericArguments()[0]; // Тип элементов в списке
            var list = (System.Collections.IList)System.Activator.CreateInstance(fieldType);

            // Если строка представляет список, разделенный запятыми
            foreach (string item in value.Split(','))
            {
                list.Add(ConvertStringToFieldType(item.Trim(), listType));
            }

            return list;
        }
        else if (fieldType.IsArray && fieldType.GetElementType() == typeof(Vector2))
        {
            // Обработка массивов Vector2
            string[] values = value.Split(';');
            Vector2[] array = new Vector2[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                string[] parts = values[i].Split(',');
                array[i] = new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
            }
            return array;
        }

        // Добавляем исключение для сложных Unity типов
        if (typeof(Component).IsAssignableFrom(fieldType) || typeof(UnityEngine.Object).IsAssignableFrom(fieldType) ||
            fieldType == typeof(GameObject) ||
            fieldType == typeof(Sprite) ||
            fieldType == typeof(Material) ||
            fieldType == typeof(CanvasRenderer) ||
            fieldType == typeof(Canvas) ||
            fieldType == typeof(Image) ||
            fieldType == typeof(Button.ButtonClickedEvent) ||
            fieldType == typeof(Coroutine) ||
            fieldType == typeof(Mesh) ||
            fieldType == typeof(UnityEngine.Events.UnityAction) || // Пропускаем UnityAction
            fieldType == typeof(UnityEngine.UI.Image.FillMethod) || // Пропускаем Image.FillMethod
            fieldType == typeof(ScreenOrientation) ||
            fieldType == typeof(System.Action) ||
            fieldType == typeof(Dictionary<,>) || // Для Dictionary
            fieldType == typeof(TMP_TextInfo) ||
            fieldType == typeof(TMP_FontAsset) ||
            fieldType == typeof(TMP_SpriteAsset) ||
            fieldType == typeof(TMP_StyleSheet) ||
            fieldType == typeof(TMP_Style) ||
            fieldType == typeof(Color32) ||
            fieldType == typeof(Color) ||
            fieldType == typeof(Rect) ||
            fieldType == typeof(Vector4) ||
            fieldType == typeof(Vector3[]) ||
            fieldType == typeof(Material[]) ||
            fieldType == typeof(TMPro.HighlightState) ||
            fieldType == typeof(TMPro.ColorMode) ||
            fieldType == typeof(TMPro.VertexGradient) ||
            fieldType == typeof(TMPro.TMP_ColorGradient) ||
            fieldType == typeof(TMPro.TMP_Style) ||
            fieldType == typeof(TMPro.TMP_SpriteAsset))
        {
            // Пропускаем компоненты Unity и другие сложные типы
            Debug.LogWarning($"Skipping unsupported field type: {fieldType} (Unity component or unsupported type)");
            return null; // Для Unity компонентов возвращаем null
        }

        // Если тип не поддерживается, логируем ошибку
        Debug.LogError("Unsupported field type: " + fieldType);
        return null; // Если тип не поддерживается
    }




    [System.Serializable]
    public class SaveData
    {
        public List<ObjectSaveData> objectData = new List<ObjectSaveData>();
    }

    [System.Serializable]
    public class ObjectSaveData
    {
        public string objectName;
        public List<FieldData> fields = new List<FieldData>();
    }

    [System.Serializable]
    public class FieldData
    {
        public string fieldName;
        public string fieldValue;
    }
}
