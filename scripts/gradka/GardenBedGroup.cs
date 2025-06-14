using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class GardenBedGroup : MonoBehaviour
{
    private const float checkRadius = 0.6f;
    private const string groupNamePrefix = "GardenGroup";

    private NavMeshSurface navMeshSurface;

    void Start()
    {
        TryJoinOrCreateOrMergeGroups();
    }

    void TryJoinOrCreateOrMergeGroups()
    {
        int gradkaLayer = LayerMask.NameToLayer("Gradka");
        Transform modelTransform = GetModelWithGradkaLayer(transform);
        if (modelTransform == null) return;

        Collider[] neighbors = Physics.OverlapSphere(modelTransform.position, checkRadius);
        HashSet<Transform> nearbyGroups = new HashSet<Transform>();

        foreach (var neighbor in neighbors)
        {
            if (neighbor.gameObject == modelTransform.gameObject) continue;
            if (neighbor.gameObject.layer != gradkaLayer) continue;

            GardenBedGroup neighborScript = neighbor.GetComponentInParent<GardenBedGroup>();
            if (neighborScript != null && neighborScript.transform.parent != null)
            {
                nearbyGroups.Add(neighborScript.transform.parent);
            }
        }

        if (nearbyGroups.Count == 0)
        {
            CreateNewGroup();
            return;
        }

        MergeGroups(nearbyGroups);
    }

    private void CreateNewGroup()
    {
        GameObject newGroup = new GameObject($"{groupNamePrefix}_{Random.Range(1000, 9999)}");
        transform.SetParent(newGroup.transform);
        AddNavMeshSurface(newGroup);
    }

    private void MergeGroups(HashSet<Transform> groups)
    {
        Transform mainGroup = null;
        foreach (var group in groups)
        {
            if (mainGroup == null)
            {
                mainGroup = group;
                continue;
            }

            MoveChildrenToGroup(group, mainGroup);
            DestroyOldGroup(group);
        }

        transform.SetParent(mainGroup);
        UpdateNavMesh(mainGroup.gameObject);
    }

    private void MoveChildrenToGroup(Transform fromGroup, Transform toGroup)
    {
        while (fromGroup.childCount > 0)
        {
            Transform child = fromGroup.GetChild(0);
            child.SetParent(toGroup);
        }
    }

    private void DestroyOldGroup(Transform group)
    {
        NavMeshSurface oldSurface = group.GetComponent<NavMeshSurface>();
        if (oldSurface != null) Destroy(oldSurface);
        Destroy(group.gameObject);
    }

    private void AddNavMeshSurface(GameObject groupObject)
    {
        navMeshSurface = groupObject.AddComponent<NavMeshSurface>();
        navMeshSurface.collectObjects = CollectObjects.Children;
        navMeshSurface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        navMeshSurface.BuildNavMesh();
    }

    private void UpdateNavMesh(GameObject groupObject)
    {
        NavMeshSurface surface = groupObject.GetComponent<NavMeshSurface>();
        if (surface == null) surface = groupObject.AddComponent<NavMeshSurface>();
        surface.UpdateNavMesh(surface.navMeshData);
    }

    private Transform GetModelWithGradkaLayer(Transform root)
    {
        foreach (Transform child in root)
        {
            if (child.gameObject.layer == LayerMask.NameToLayer("Gradka"))
                return child;
        }
        return null;
    }
}