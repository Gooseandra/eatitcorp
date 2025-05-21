using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientData
{
    public string Name;
    public List<string> ContainedIngredients; // ������ ��������� ������������

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

            // �������� ������, ���� ����������� �����������
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
            // ���� ��������� ������ ������
            if (recipeResult != "���������")
            {
                // ���� ������ � ������ ��� � �������
                resultPrefab = Resources.Load<GameObject>(recipeResult);

                if (resultPrefab == null)
                {
                    Debug.LogWarning($"������ ��� ������� {recipeResult} �� ������ � ����� Resources");
                    resultPrefab = inedibleWaste;
                }
            }
            else // ���� ���������� ���������
            {
                // ���� ������ "���������"
                resultPrefab = Resources.Load<GameObject>("���������");

                if (resultPrefab == null)
                {
                    Debug.LogWarning("������ '���������' �� ������ � ����� Resources");
                    resultPrefab = inedibleWaste;
                }
            }
        }
        else // ���� ������ �������
        {
            resultPrefab = inedibleWaste;
        }

        // ������� ���������
        if (resultPrefab != null)
        {
            GameObject res = Instantiate(resultPrefab, spawnPoint.position, spawnPoint.rotation);
            Ingredient ing = res.GetComponent<Ingredient>();
            ing.SetIngridients(ingredients);
            ing.SetName(recipeResult);
        }
        else
        {
            Debug.LogError("�� ������� ���������� ������ ��� ������");
            Instantiate(inedibleWaste, spawnPoint.position, spawnPoint.rotation);
        }
    }
}