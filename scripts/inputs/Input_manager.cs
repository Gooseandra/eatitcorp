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
    private bool isCameraLocked = false;

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

        // Ѕлокировка камеры при зажатии клавиши R
        if (Input.GetKeyDown(RotationKeyCode))
        {
            LockCamera(true);
        }
        if (Input.GetKeyUp(RotationKeyCode))
        {
            LockCamera(false);
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

    // Ѕлокирует или разблокирует камеру
    public void LockCamera(bool isLocked)
    {
        isCameraLocked = isLocked;
    }

    // ¬озвращает состо€ние блокировки камеры
    public bool IsCameraLocked()
    {
        return isCameraLocked;
    }
}