using UnityEngine;

public class IteractorHighlighter : MonoBehaviour
{
    [SerializeField] Camera playerCamera;
    [SerializeField] float distance = 5;
    [SerializeField] GameObject iteractionMessage;
    void Update()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, distance))
        {
            Storage s = hit.collider.GetComponent<Storage>();
            ItemPickup i = hit.collider.GetComponent<ItemPickup>();
            TerminalInteractor t = hit.collider.GetComponent<TerminalInteractor>();
            GardenBed g = hit.collider.GetComponent<GardenBed>();
            if (s != null || i != null || t != null || g != null)
            {
                iteractionMessage.SetActive(true);
            }
            else
            {
                iteractionMessage.SetActive(false);
            }
        }
        else
        {
            iteractionMessage.SetActive(false);
        }
    }
}
