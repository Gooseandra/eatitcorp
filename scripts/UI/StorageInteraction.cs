using UnityEngine;

public class StorageInteraction : MonoBehaviour
{
    public KeyCode interactionKey = KeyCode.E;
    private Storage currentStorage;

    private void Update()
    {
        if (Input.GetKeyDown(interactionKey))
        {
            CheckForStorage();
        }
    }

    private void CheckForStorage()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 3f)) // 3 метра - дистанция взаимодействия
        {
            Storage storage = hit.collider.GetComponent<Storage>();
            if (storage != null)
            {
                currentStorage = storage;
                currentStorage.TryOpenStorage();
            }
        }
    }
}