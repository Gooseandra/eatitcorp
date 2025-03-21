using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;

public class Product_creds : MonoBehaviour
{
    private string name;
    private int id;
    private List<int> structure;

    public string GetName()
    {
        return name;
    }

    public int GetId()
    {
        return id;
    }

    public List<int> GetStruture()
    {
        return structure;
    }

    public void SetName(string name)
    {
        this.name = name;
    }

    public void SetId(int id)
    {
        this.id = id;
    }

    public void AddStructureElem(int id)
    {
        structure.Add(id);
    }
}
