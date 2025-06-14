using System.Collections.Generic;
using UnityEngine;

public class GardenBedGroup : MonoBehaviour
{
    private const float checkRadius = 0.6f;
    private const string groupNamePrefix = "GardenGroup";

    void Start()
    {
        TryJoinOrCreateOrMergeGroups();
    }

    void TryJoinOrCreateOrMergeGroups()
    {
        int gradkaLayer = LayerMask.NameToLayer("Gradka");
        Transform modelTransform = GetModelWithGradkaLayer(transform);
        if (modelTransform == null)
        {
            Debug.LogWarning("Не найден объект со слоем Gradka внутри грядки.");
            return;
        }

        Collider[] neighbors = Physics.OverlapSphere(modelTransform.position, checkRadius);
        HashSet<Transform> nearbyGroups = new HashSet<Transform>();

        foreach (var neighbor in neighbors)
        {
            if (neighbor.gameObject == modelTransform.gameObject) continue;
            if (neighbor.gameObject.layer != gradkaLayer) continue;

            GardenBedGroup neighborScript = neighbor.GetComponentInParent<GardenBedGroup>();
            if (neighborScript != null)
            {
                Transform parentGroup = neighborScript.transform.parent;
                if (parentGroup != null && parentGroup.name.StartsWith(groupNamePrefix))
                {
                    nearbyGroups.Add(parentGroup);
                }
            }
        }

        if (nearbyGroups.Count == 0)
        {
            // ➕ Нет соседей — создаём новую группу
            GameObject newGroup = new GameObject($"{groupNamePrefix}_{Random.Range(1000, 9999)}");
            transform.SetParent(newGroup.transform);
            return;
        }

        Transform mainGroup = null;

        foreach (var group in nearbyGroups)
        {
            if (mainGroup == null)
            {
                mainGroup = group;
                continue;
            }

            // Переносим детей в mainGroup
            List<Transform> childrenToMove = new List<Transform>();
            foreach (Transform child in group)
            {
                childrenToMove.Add(child);
            }
            foreach (Transform child in childrenToMove)
            {
                child.SetParent(mainGroup);
            }

            GameObject.Destroy(group.gameObject);
        }

        // Назначаем текущую грядку в главную группу
        transform.SetParent(mainGroup);
    }

    private Transform GetModelWithGradkaLayer(Transform root)
    {
        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.gameObject.layer == LayerMask.NameToLayer("Gradka"))
            {
                return child;
            }
        }
        return null;
    }
}
