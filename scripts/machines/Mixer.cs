using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mixer : MonoBehaviour
{
    [SerializeField] private GameObject inedibleWaste; // ������ "����������� �����"
    [SerializeField] private Transform spawnPoint; // ����� ��������� ����������
    [SerializeField] private LayerMask ingredientLayer; // ���� ��� ������������

    private List<Ingredient> currentIngredients = new List<Ingredient>(); // ������� ����������� � �������

    // ����� ��� ��������� �����������, ��������� �� ��������� ����������
    public void HandleIngredient(Collider other)
    {
        Debug.Log("���������� �������");
        Ingredient ingredient = other.GetComponent<Ingredient>();
        if (ingredient != null)
        {
            // ��������� ���������� � ������
            AddIngredient(ingredient);

            // ���������� ������ �����������, ��� ��� �� "�����������" ��������
            Destroy(other.gameObject);
        }
    }

    // ���������� ����������� � ������
    private void AddIngredient(Ingredient ingredient)
    {
        if (currentIngredients.Count < 2) // ������ ����� ��������� ������ ��� �����������
        {
            currentIngredients.Add(ingredient);
            Debug.Log($"�������� ����������: {ingredient.Name}");

            if (currentIngredients.Count == 2) // ���� ��� �����������, �������� ����������
            {
                MixIngredients();
            }
        }
        else
        {
            Debug.Log("������ ����������. ������� �������� ������� �����������.");
        }
    }

    // ���������� ������������
    private void MixIngredients()
    {
        // �������� ������ ���� ��������� ������������
        List<string> allIngredients = new List<string>();
        foreach (var ingredient in currentIngredients)
        {
            allIngredients.Add(ingredient.Name);
            allIngredients.AddRange(ingredient.ContainedIngredients);
        }

        // ���������� RecipeManager ��� �������� ��������
        string result = JsonManager.Instance.CheckRecipes(allIngredients);

        if (result != null)
        {
            if (result == "���������")
            {
                Debug.Log("������� ��������� � �������������: " + string.Join(", ", allIngredients));
                SpawnPreparation(allIngredients);
            }
            else
            {
                Debug.Log($"��������� ����������: {result}");
                SpawnResult(result, allIngredients);
            }
        }
        else
        {
            Debug.Log("����������� �����!");
            SpawnResult("����������� �����", allIngredients);
        }

        currentIngredients.Clear(); // ������� ������ ����� ����������
    }

    // �������� ���������� � ����������� ������� ������������
    private void SpawnResult(string resultName, List<string> ingredients)
    {
        // ������� ����� ������ ����������
        GameObject resultObject = new GameObject(resultName);
        resultObject.transform.position = spawnPoint.position;
        resultObject.transform.rotation = spawnPoint.rotation;

        // ��������� ��������� Ingredient � ������� ����������
        Ingredient resultIngredient = resultObject.AddComponent<Ingredient>();
        resultIngredient.Name = resultName;

        // ��������� ��� ����������� � ���������
        foreach (var ingredient in ingredients)
        {
            resultIngredient.AddContainedIngredient(ingredient);
        }

        // ����� �������� ���������� ������������� ���������� (��������, ������)
        GameObject resultPrefab = Resources.Load<GameObject>(resultName);
        if (resultPrefab != null)
        {
            Instantiate(resultPrefab, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            Debug.LogError($"������ ��� {resultName} �� ������ � ����� Resources.");
            Instantiate(inedibleWaste, spawnPoint.position, spawnPoint.rotation); // ������� "����������� �����"
        }
    }

    // �������� ���������
    private void SpawnPreparation(List<string> ingredients)
    {
        // ������� ��������� � ������ "���������"
        Ingredient preparation = Ingredient.CreatePreparation(ingredients, "���������");

        // ������� ������ ��������� � ����
        GameObject preparationObject = new GameObject(preparation.Name);
        preparationObject.transform.position = spawnPoint.position;
        preparationObject.transform.rotation = spawnPoint.rotation;

        // ��������� ��������� Ingredient � ������� ���������
        Ingredient preparationIngredient = preparationObject.AddComponent<Ingredient>();
        preparationIngredient.Name = preparation.Name;
        foreach (var ingredient in preparation.ContainedIngredients)
        {
            preparationIngredient.AddContainedIngredient(ingredient);
        }

        // ����� �������� ���������� ������������� ��������� (��������, ������)
        GameObject preparationPrefab = Resources.Load<GameObject>("Preparation");
        if (preparationPrefab != null)
        {
            Instantiate(preparationPrefab, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            Debug.LogError("������ ��� ��������� �� ������ � ����� Resources.");
        }
    }
}