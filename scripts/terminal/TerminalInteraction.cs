using UnityEngine;

public class TerminalInteractor : MonoBehaviour
{
    public float interactionDistance = 3f;
    public Camera playerCamera;
    public GameObject terminalUI;
    public Movement playerMovement;

    private bool isTerminalOpen = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteractWithTerminal();
        }

        if (isTerminalOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseTerminalUI();
        }
    }

    private void TryInteractWithTerminal()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            if (hit.collider.GetComponent<TerminalOrderSystem>() != null)
            {
                OpenTerminalUI();
            }
        }
    }

    private void OpenTerminalUI()
    {
        terminalUI.SetActive(true);
        isTerminalOpen = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerMovement != null)
        {
            playerMovement.SetPlayerControlLock(true);
        }
    }

    private void CloseTerminalUI()
    {
        terminalUI.SetActive(false);
        isTerminalOpen = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerMovement != null)
        {
            playerMovement.SetPlayerControlLock(false);
        }
    }
}
