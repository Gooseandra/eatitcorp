using System.Collections.Generic;
using UnityEngine;

public class Ingredient : MonoBehaviour
{
    [SerializeField] public string Name; // ��� �����������
    [SerializeField] private List<string> containedIngredients = new List<string>(); // ������ ��������� ������������

    // �������� ��� ��������� ����� �����������

    // �������� ��� ��������� ������ ��������� ������������
    public List<string> ContainedIngredients => containedIngredients;

    // ����� ��� ���������� ���������� �����������
    public void AddContainedIngredient(string ingredient)
    {
        if (!containedIngredients.Contains(ingredient))
        {
            containedIngredients.Add(ingredient);
        }
    }

    // ����� ��� �������� ���������
    public static Ingredient CreatePreparation(List<string> ingredients, string preparationName)
    {
        // ������� ����� ������ ��� ���������
        GameObject preparationObject = new GameObject(preparationName);
        Ingredient preparation = preparationObject.AddComponent<Ingredient>();

        // ������������� ��� ���������
        preparation.Name = preparationName;

        // ��������� ��� ����������� � ���������
        foreach (var ingredient in ingredients)
        {
            preparation.AddContainedIngredient(ingredient);
        }

        return preparation;
    }

    // ����� ��� ����������� ���� ������������
    public static Ingredient Combine(Ingredient ingredient1, Ingredient ingredient2, string resultName)
    {
        // ������� ����� ������ ��� ����������
        GameObject resultObject = new GameObject(resultName);
        Ingredient resultIngredient = resultObject.AddComponent<Ingredient>();

        // ������������� ��� ����������
        resultIngredient.Name = resultName;

        // ��������� ��������� ����������� �� ������� �����������
        foreach (var ingredient in ingredient1.ContainedIngredients)
        {
            resultIngredient.AddContainedIngredient(ingredient);
        }

        // ��������� ��������� ����������� �� ������� �����������
        foreach (var ingredient in ingredient2.ContainedIngredients)
        {
            resultIngredient.AddContainedIngredient(ingredient);
        }

        // ��������� ���� ����������� ��� ���������
        resultIngredient.AddContainedIngredient(ingredient1.Name);
        resultIngredient.AddContainedIngredient(ingredient2.Name);

        return resultIngredient;
    }
}