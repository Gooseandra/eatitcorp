using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientData
{
    public string Name;
    public List<string> ContainedIngredients; // Список вложенных ингредиентов

    public IngredientData(string name, List<string> ingredients)
    {
        Name = name;
        ContainedIngredients = new List<string>(ingredients);
    }
}

public class Mixer : MonoBehaviour
{
    [SerializeField] private GameObject inedibleWaste;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int maxIngridientsIn;

    [SerializeField] private IngredientData ingredientAData;
    [SerializeField] private IngredientData ingredientBData;
    [SerializeField] private int ingACount = 0;
    [SerializeField] private int ingBCount = 0;

    public void HandleIngredient(Collider other)
    {
        Ingredient ingredient = other.GetComponent<Ingredient>();
        if (ingredient != null && CanAddIngridient(ingredient))
        {
            AddIngridient(ingredient);
            Destroy(other.gameObject);
        }
    }

    private bool CanAddIngridient(Ingredient ingridient)
    {
        if (ingredientBData == null || ingridient.name == ingredientBData.Name)
            return ingBCount < maxIngridientsIn;
        else if (ingredientAData == null || ingridient.name == ingredientAData.Name)
            return ingACount < maxIngridientsIn;
        return false;
    }

    private void AddIngridient(Ingredient ing)
    {
        var data = new IngredientData(ing.name, ing.GetIngridients());
        if (ingredientAData == null || ing.name == ingredientAData.Name)
        {
            ingredientAData = data;
            ingACount++;
        }
        else if (ingredientBData == null || ing.name == ingredientBData.Name)
        {
            ingredientBData = data;
            ingBCount++;
        }
        Mix();
    }

    private void Mix()
    {
        if (ingACount > 0 && ingBCount > 0)
        {
            List<string> mixedIngredients = new List<string>();
            mixedIngredients.AddRange(ingredientAData.ContainedIngredients);
            mixedIngredients.AddRange(ingredientBData.ContainedIngredients);
            SpawnResult(mixedIngredients);
            ingACount--;
            ingBCount--;

            // Обнуляем данные, если ингредиенты закончились
            if (ingACount == 0)
                ingredientAData = null;
            if (ingBCount == 0)
                ingredientBData = null;
        }
    }


    private void SpawnResult(List<string> ingredients)
    {
        string recipeResult = JsonManager.Instance.CheckRecipes(ingredients);

        GameObject resultPrefab = null;

        Debug.Log(recipeResult);

        if (!string.IsNullOrEmpty(recipeResult))
        {
            // Если получился полный рецепт
            if (recipeResult != "Заготовка")
            {
                // Ищем префаб с именем как у рецепта
                resultPrefab = Resources.Load<GameObject>(recipeResult);

                if (resultPrefab == null)
                {
                    Debug.LogWarning($"Префаб для рецепта {recipeResult} не найден в папке Resources");
                    resultPrefab = inedibleWaste;
                }
            }
            else // Если получилась заготовка
            {
                // Ищем префаб "Заготовка"
                resultPrefab = Resources.Load<GameObject>("Заготовка");

                if (resultPrefab == null)
                {
                    Debug.LogWarning("Префаб 'Заготовка' не найден в папке Resources");
                    resultPrefab = inedibleWaste;
                }
            }
        }
        else // Если рецепт нарушен
        {
            resultPrefab = inedibleWaste;
        }

        // Спавним результат
        if (resultPrefab != null)
        {
            GameObject res = Instantiate(resultPrefab, spawnPoint.position, spawnPoint.rotation);
            Ingredient ing = res.GetComponent<Ingredient>();
            ing.SetIngridients(ingredients);
            ing.SetName(recipeResult);
        }
        else
        {
            Debug.LogError("Не удалось определить префаб для спавна");
            Instantiate(inedibleWaste, spawnPoint.position, spawnPoint.rotation);
        }
    }
}