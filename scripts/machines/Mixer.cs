using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mixer : MonoBehaviour
{
    [SerializeField] private GameObject inedibleWaste; // Объект "несъедобные помои"
    [SerializeField] private Transform spawnPoint; // Точка появления результата
    [SerializeField] private LayerMask ingredientLayer; // Слой для ингредиентов

    private List<Ingredient> currentIngredients = new List<Ingredient>(); // Текущие ингредиенты в миксере

    // Метод для обработки ингредиента, вызванный из дочернего коннектора
    public void HandleIngredient(Collider other)
    {
        Debug.Log("ингридиент приехал");
        Ingredient ingredient = other.GetComponent<Ingredient>();
        if (ingredient != null)
        {
            // Добавляем ингредиент в миксер
            AddIngredient(ingredient);

            // Уничтожаем объект ингредиента, так как он "поглощается" миксером
            Destroy(other.gameObject);
        }
    }

    // Добавление ингредиента в миксер
    private void AddIngredient(Ingredient ingredient)
    {
        if (currentIngredients.Count < 2) // Миксер может принимать только два ингредиента
        {
            currentIngredients.Add(ingredient);
            Debug.Log($"Добавлен ингредиент: {ingredient.Name}");

            if (currentIngredients.Count == 2) // Если два ингредиента, начинаем смешивание
            {
                MixIngredients();
            }
        }
        else
        {
            Debug.Log("Миксер переполнен. Сначала смешайте текущие ингредиенты.");
        }
    }

    // Смешивание ингредиентов
    private void MixIngredients()
    {
        // Получаем список всех вложенных ингредиентов
        List<string> allIngredients = new List<string>();
        foreach (var ingredient in currentIngredients)
        {
            allIngredients.Add(ingredient.Name);
            allIngredients.AddRange(ingredient.ContainedIngredients);
        }

        // Используем RecipeManager для проверки рецептов
        string result = JsonManager.Instance.CheckRecipes(allIngredients);

        if (result != null)
        {
            if (result == "заготовка")
            {
                Debug.Log("Создана заготовка с ингредиентами: " + string.Join(", ", allIngredients));
                SpawnPreparation(allIngredients);
            }
            else
            {
                Debug.Log($"Результат смешивания: {result}");
                SpawnResult(result, allIngredients);
            }
        }
        else
        {
            Debug.Log("Несъедобные помои!");
            SpawnResult("несъедобные помои", allIngredients);
        }

        currentIngredients.Clear(); // Очищаем миксер после смешивания
    }

    // Создание результата с объединённым списком ингредиентов
    private void SpawnResult(string resultName, List<string> ingredients)
    {
        // Создаем новый объект результата
        GameObject resultObject = new GameObject(resultName);
        resultObject.transform.position = spawnPoint.position;
        resultObject.transform.rotation = spawnPoint.rotation;

        // Добавляем компонент Ingredient к объекту результата
        Ingredient resultIngredient = resultObject.AddComponent<Ingredient>();
        resultIngredient.Name = resultName;

        // Добавляем все ингредиенты в результат
        foreach (var ingredient in ingredients)
        {
            resultIngredient.AddContainedIngredient(ingredient);
        }

        // Можно добавить визуальное представление результата (например, префаб)
        GameObject resultPrefab = Resources.Load<GameObject>(resultName);
        if (resultPrefab != null)
        {
            Instantiate(resultPrefab, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            Debug.LogError($"Префаб для {resultName} не найден в папке Resources.");
            Instantiate(inedibleWaste, spawnPoint.position, spawnPoint.rotation); // Создаем "несъедобные помои"
        }
    }

    // Создание заготовки
    private void SpawnPreparation(List<string> ingredients)
    {
        // Создаем заготовку с именем "Заготовка"
        Ingredient preparation = Ingredient.CreatePreparation(ingredients, "Заготовка");

        // Создаем объект заготовки в мире
        GameObject preparationObject = new GameObject(preparation.Name);
        preparationObject.transform.position = spawnPoint.position;
        preparationObject.transform.rotation = spawnPoint.rotation;

        // Добавляем компонент Ingredient к объекту заготовки
        Ingredient preparationIngredient = preparationObject.AddComponent<Ingredient>();
        preparationIngredient.Name = preparation.Name;
        foreach (var ingredient in preparation.ContainedIngredients)
        {
            preparationIngredient.AddContainedIngredient(ingredient);
        }

        // Можно добавить визуальное представление заготовки (например, префаб)
        GameObject preparationPrefab = Resources.Load<GameObject>("Preparation");
        if (preparationPrefab != null)
        {
            Instantiate(preparationPrefab, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            Debug.LogError("Префаб для заготовки не найден в папке Resources.");
        }
    }
}