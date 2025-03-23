using System.Collections.Generic;
using UnityEngine;

public class Ingredient : MonoBehaviour
{
    [SerializeField] public string Name; // Имя ингредиента
    [SerializeField] private List<string> containedIngredients = new List<string>(); // Список вложенных ингредиентов

    // Свойство для получения имени ингредиента

    // Свойство для получения списка вложенных ингредиентов
    public List<string> ContainedIngredients => containedIngredients;

    // Метод для добавления вложенного ингредиента
    public void AddContainedIngredient(string ingredient)
    {
        if (!containedIngredients.Contains(ingredient))
        {
            containedIngredients.Add(ingredient);
        }
    }

    // Метод для создания заготовки
    public static Ingredient CreatePreparation(List<string> ingredients, string preparationName)
    {
        // Создаем новый объект для заготовки
        GameObject preparationObject = new GameObject(preparationName);
        Ingredient preparation = preparationObject.AddComponent<Ingredient>();

        // Устанавливаем имя заготовки
        preparation.Name = preparationName;

        // Добавляем все ингредиенты в заготовку
        foreach (var ingredient in ingredients)
        {
            preparation.AddContainedIngredient(ingredient);
        }

        return preparation;
    }

    // Метод для объединения двух ингредиентов
    public static Ingredient Combine(Ingredient ingredient1, Ingredient ingredient2, string resultName)
    {
        // Создаем новый объект для результата
        GameObject resultObject = new GameObject(resultName);
        Ingredient resultIngredient = resultObject.AddComponent<Ingredient>();

        // Устанавливаем имя результата
        resultIngredient.Name = resultName;

        // Добавляем вложенные ингредиенты из первого ингредиента
        foreach (var ingredient in ingredient1.ContainedIngredients)
        {
            resultIngredient.AddContainedIngredient(ingredient);
        }

        // Добавляем вложенные ингредиенты из второго ингредиента
        foreach (var ingredient in ingredient2.ContainedIngredients)
        {
            resultIngredient.AddContainedIngredient(ingredient);
        }

        // Добавляем сами ингредиенты как вложенные
        resultIngredient.AddContainedIngredient(ingredient1.Name);
        resultIngredient.AddContainedIngredient(ingredient2.Name);

        return resultIngredient;
    }
}