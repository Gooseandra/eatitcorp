using UnityEngine;

public class SnapPoint : MonoBehaviour
{
    public GameObject entrancePrefab;
    public bool isSnaped = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Connnector")
        {
            isSnaped = true;
            entrancePrefab.SetActive(true);
        }
    }
}
