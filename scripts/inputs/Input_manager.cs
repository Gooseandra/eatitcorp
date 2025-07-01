using UnityEngine;

public class Input_manager : MonoBehaviour
{
    [SerializeField] KeyCode MoveForwardKeyCode;
    [SerializeField] KeyCode MoveBackwardKeyCode;
    [SerializeField] KeyCode MoveLeftKeyCode;
    [SerializeField] KeyCode MoveRightKeyCode;
    [SerializeField] KeyCode JumpKeyCode;
    [SerializeField] KeyCode CrouchKeyCode;
    [SerializeField] KeyCode BuildingModeKeyCode;
    [SerializeField] KeyCode RotationKeyCode;
    [SerializeField] KeyCode EscapeKeyCode;
    

    [SerializeField] bool MoveForward;
    [SerializeField] bool MoveBackward;
    [SerializeField] bool MoveLeft;
    [SerializeField] bool MoveRight;
    [SerializeField] bool Jump;
    [SerializeField] bool Crouch;
    [SerializeField] bool BuildingMode;
    [SerializeField] bool Rotation;
    [SerializeField] bool Escape;
    [SerializeField] bool OpenBuildingWindow;

    private bool buildingModeToggle = false;

    [SerializeField] GameObject Menu;
    [SerializeField] GameObject BuildingUI;
    [SerializeField] GameObject TerminalUI;
    [SerializeField] Movement movement;

    private bool lastEscape = false;

    private void Start()
    {
        SetDefaultKeyCodesValues();
    }

    void Update()
    {
        MoveForward = Input.GetKey(MoveForwardKeyCode);
        MoveBackward = Input.GetKey(MoveBackwardKeyCode);
        MoveLeft = Input.GetKey(MoveLeftKeyCode);
        MoveRight = Input.GetKey(MoveRightKeyCode);
        Jump = Input.GetKey(JumpKeyCode);
        Crouch = Input.GetKey(CrouchKeyCode);
        Rotation = Input.GetKey(RotationKeyCode);
        Escape = Input.GetKey(EscapeKeyCode);
        OpenBuildingWindow = Input.GetKey(BuildingModeKeyCode);

        if (Input.GetKeyDown(BuildingModeKeyCode))
        {
            buildingModeToggle = !buildingModeToggle;
        }
        BuildingMode = buildingModeToggle;

        // Блокировка камеры при зажатии клавиши R
        if (Input.GetKeyDown(RotationKeyCode))
        {
            LockCamera(true);
        }
        if (Input.GetKeyUp(RotationKeyCode))
        {
            LockCamera(false);
        }

        if (Input.GetKey(EscapeKeyCode) || Input.GetKey(EscapeKeyCode) != lastEscape) {
            lastEscape = true;
            if ( BuildingUI.activeSelf || TerminalUI.activeSelf)
            {
                BuildingUI.SetActive(false);
                TerminalUI.SetActive(false);
                LockCursor();
            }
            else{

                UnlockCursor();
                Menu.SetActive(true);
                LockCamera(true);
            }
        }
        if (!Input.GetKey(EscapeKeyCode)) {
            lastEscape = false;
        }
    }

    void SetDefaultKeyCodesValues()
    {
        MoveForwardKeyCode = KeyCode.W;
        MoveBackwardKeyCode = KeyCode.S;
        MoveLeftKeyCode = KeyCode.A;
        MoveRightKeyCode = KeyCode.D;
        JumpKeyCode = KeyCode.Space;
        CrouchKeyCode = KeyCode.LeftControl;
        BuildingModeKeyCode = KeyCode.Tab;
        RotationKeyCode = KeyCode.R;
        EscapeKeyCode = KeyCode.Escape;
    }

    public bool GetMoveForward() { return MoveForward; }
    public bool GetMoveBackward() { return MoveBackward; }
    public bool GetMoveLeft() { return MoveLeft; }
    public bool GetMoveRight() { return MoveRight; }
    public bool GetJump() { return Jump; }
    public bool GetCrouch() { return Crouch; }
    public bool GetBuildingMode() { return BuildingMode; }
    public bool GetRotation() { return Rotation; }
    public bool GetEscape() { return Escape; }
    public bool GetOpenBuildingWindow() { return OpenBuildingWindow; }
    public void SetBuildingMode(bool mode) { this.buildingModeToggle = mode; }

    // Блокирует или разблокирует камеру
    public void LockCamera(bool isLocked)
    {
         movement.lockCamera = isLocked;
    }

   public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

}