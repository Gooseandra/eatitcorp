using System.Collections.Generic;
using UnityEngine;

public class JsonManager : MonoBehaviour
{
    public static JsonManager Instance { get; private set; } // ��������

    [SerializeField] private TextAsset recipesJSON; // JSON-���� � ���������
    private List<Recipe> recipes = new List<Recipe>(); // ������ ��������

    private void Awake()
    {
        // ���������� ���������
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �� ���������� ��� �������� ����� �����
            LoadRecipes(); // ��������� �������
        }
        else
        {
            Destroy(gameObject); // ���������� ��������
        }
    }

    // �������� �������� �� JSON
    private void LoadRecipes()
    {
        if (recipesJSON == null)
        {
            Debug.LogError("JSON ���� � ��������� �� ��������.");
            return;
        }

        RecipeList recipeList = JsonUtility.FromJson<RecipeList>(recipesJSON.text);
        recipes = recipeList.recipes;
        Debug.Log("������� ���������.");
    }

    // ��������� ������ ��������
    public List<Recipe> GetRecipes()
    {
        return recipes;
    }

    // ��������, ������������� �� ����������� ������-���� �������
    public string CheckRecipes(List<string> ingredients)
    {
        // ������� ��������� ������ ���������� � ���������
        foreach (var recipe in recipes)
        {
            bool fullMatch = true;

            // ���������, ��� �� ����������� ������� ���� � ������
            foreach (var ingredient in recipe.ingredients)
            {
                if (!ingredients.Contains(ingredient))
                {
                    fullMatch = false;
                    break;
                }
            }

            // ���� ��� ����������� ������� ������������ � ���������� ������������ ���������
            if (fullMatch && recipe.ingredients.Count == ingredients.Count)
            {
                return recipe.name;
            }
        }

        // ��������� ��������� ���������� (������ ���� ��� ����������� ������ � �����-�� ������)
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

        // ���� ��� ����������� �������, �� �� ���������� ������ ������
        if (allIngredientsValid && ingredients.Count > 0)
        {
            return "���������";
        }

        // ���� ���� ���� �� ���� ���������� ����������
        return null;
    }

    public List<string> CheckPartialRecipes(List<string> ingredients)
    {
        List<string> matchedIngredients = new List<string>();

        foreach (var recipe in recipes)
        {
            // ���������, ������� ������������ �� ������� ���� � ������� ������
            int matchCount = 0;
            foreach (var ingredient in recipe.ingredients)
            {
                if (ingredients.Contains(ingredient))
                {
                    matchCount++;
                }
            }

            // ���� ���� ���� �� ���� ����������, ��������� ����������� � ������
            if (matchCount > 0)
            {
                matchedIngredients.AddRange(recipe.ingredients);
            }
        }

        // ���������� ������ ������������, ������� �������� ������� � ���������
        return matchedIngredients;
    }
}

// ������ ��� �������� JSON
[System.Serializable]
public class Recipe
{
    public string name;
    public List<string> ingredients;
    public int cost; // ����� ���� - ��������� �������
}

[System.Serializable]
public class RecipeList
{
    public List<Recipe> recipes;
}