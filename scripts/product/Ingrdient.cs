using System.Collections.Generic;
using UnityEngine;

public class Ingredient : MonoBehaviour
{
    [SerializeField] public string Name; // Имя ингредиента
    [SerializeField] private List<string> containedIngredients = new List<string>(); // Список вложенных ингредиентов


    public void SetIngridients(List<string> ingridients)
    {
        containedIngredients = ingridients;
    }

    public List<string> GetIngridients()
    {
        return containedIngredients;
    }

    public void SetName(string name)
    {
        Name = name;
    }
}