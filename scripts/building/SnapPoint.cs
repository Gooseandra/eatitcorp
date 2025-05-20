using UnityEngine;

public class SnapPoint : MonoBehaviour
{
    public GameObject entrancePrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Connnector")
        {
            entrancePrefab.SetActive(true);
        }
    }
}
