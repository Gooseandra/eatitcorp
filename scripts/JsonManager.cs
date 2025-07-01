using System.Collections.Generic;
using UnityEngine;

public class JsonManager : MonoBehaviour
{
    public static JsonManager Instance { get; private set; } // Синглтон

    [SerializeField] private TextAsset recipesJSON; // JSON-файл с рецептами
    private List<Recipe> recipes = new List<Recipe>(); // Список рецептов

    private void Awake()
    {
        // Реализация синглтона
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Не уничтожаем при загрузке новой сцены
            LoadRecipes(); // Загружаем рецепты
        }
        else
        {
            Destroy(gameObject); // Уничтожаем дубликат
        }
    }

    // Загрузка рецептов из JSON
    private void LoadRecipes()
    {
        if (recipesJSON == null)
        {
            Debug.LogError("JSON файл с рецептами не назначен.");
            return;
        }

        RecipeList recipeList = JsonUtility.FromJson<RecipeList>(recipesJSON.text);
        recipes = recipeList.recipes;
        Debug.Log("Рецепты загружены.");
    }

    // Получение списка рецептов
    public List<Recipe> GetRecipes()
    {
        return recipes;
    }

    // Проверка, соответствуют ли ингредиенты какому-либо рецепту
    public string CheckRecipes(List<string> ingredients)
    {
        // Сначала проверяем полные совпадения с рецептами
        foreach (var recipe in recipes)
        {
            bool fullMatch = true;

            // Проверяем, все ли ингредиенты рецепта есть в списке
            foreach (var ingredient in recipe.ingredients)
            {
                if (!ingredients.Contains(ingredient))
                {
                    fullMatch = false;
                    break;
                }
            }

            // Если все ингредиенты рецепта присутствуют И количество ингредиентов совпадает
            if (fullMatch && recipe.ingredients.Count == ingredients.Count)
            {
                return recipe.name;
            }
        }

        // Проверяем частичные совпадения (только если ВСЕ ингредиенты входят в какой-то рецепт)
        bool allIngredientsValid = true;
        foreach (var ingredient in ingredients)
        {
            bool foundInAnyRecipe = false;
            foreach (var recipe in recipes)
            {
                if (recipe.ingredients.Contains(ingredient))
                {
                    foundInAnyRecipe = true;
                    break;
                }
            }

            if (!foundInAnyRecipe)
            {
                allIngredientsValid = false;
                break;
            }
        }

        // Если все ингредиенты валидны, но не составляют полный рецепт
        if (allIngredientsValid && ingredients.Count > 0)
        {
            return "Заготовка";
        }

        // Если есть хотя бы один невалидный ингредиент
        return null;
    }

    public List<string> CheckPartialRecipes(List<string> ingredients)
    {
        List<string> matchedIngredients = new List<string>();

        foreach (var recipe in recipes)
        {
            // Проверяем, сколько ингредиентов из рецепта есть в текущем списке
            int matchCount = 0;
            foreach (var ingredient in recipe.ingredients)
            {
                if (ingredients.Contains(ingredient))
                {
                    matchCount++;
                }
            }

            // Если есть хотя бы одно совпадение, добавляем ингредиенты в список
            if (matchCount > 0)
            {
                matchedIngredients.AddRange(recipe.ingredients);
            }
        }

        // Возвращаем список ингредиентов, которые частично совпали с рецептами
        return matchedIngredients;
    }
}

// Классы для парсинга JSON
[System.Serializable]
public class Recipe
{
    public string name;
    public List<string> ingredients;
    public int cost; // Новое поле - стоимость рецепта
}

[System.Serializable]
public class RecipeList
{
    public List<Recipe> recipes;
}