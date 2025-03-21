using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Movement : MonoBehaviour
{
    [SerializeField] Input_manager _inputs;
    [SerializeField] GameObject _moveableObj;
    [SerializeField] Transform cameraTransform;

    [SerializeField] float _speed;
    [SerializeField] float _airSpeed = 5f;
    [SerializeField] float _normalAirSpeed = 5f;
    [SerializeField] float _normalSpeed = 10f;
    [SerializeField] float _maxSpeed = 25f;
    [SerializeField] float _crouchSpeedDebuff = 0.00005f;
    [SerializeField] float _bunnyHopAdditionalSpeed = 2f;
    [SerializeField] float _groundedBunnyHopTime;
    [SerializeField] float _normalCameraHeight = 0.9f;
    [SerializeField] float _crouchCameraHeight = 0f;

    [SerializeField] float _jumpForce = 100f;
    [SerializeField] float mouseSensitivity = 1000f;
    [SerializeField] LayerMask groundMask;

    Rigidbody rb;
    Transform playerTransform;
    CapsuleCollider playerCollider;

    bool isGrounded;
    bool isCroaching;
    float xRotation = 0f;

    float onGroundTime = 0f;

    // ƒобавл€ем переменную дл€ блокировки камеры
    public bool lockCamera = false;

    private void Start()
    {
        rb = _moveableObj.GetComponent<Rigidbody>();
        playerTransform = _moveableObj.GetComponent<Transform>();
        playerCollider = _moveableObj.GetComponent<CapsuleCollider>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleMouseLook();
        HandleJump();
        HandleCrouch();
    }

    void HandleMovement()
    {
        Vector3 moveDirection = Vector3.zero;

        Vector3 forward = new Vector3(cameraTransform.forward.x, 0, cameraTransform.forward.z).normalized;
        Vector3 right = new Vector3(cameraTransform.right.x, 0, cameraTransform.right.z).normalized;

        if (_inputs.GetMoveForward())
        {
            moveDirection += forward;
        }
        if (_inputs.GetMoveBackward())
        {
            moveDirection -= forward;
        }
        if (_inputs.GetMoveRight())
        {
            moveDirection += right;
        }
        if (_inputs.GetMoveLeft())
        {
            moveDirection -= right;
        }

        if (moveDirection.magnitude > 0.1f)
        {
            moveDirection.Normalize();
        }

        if (!CheckGrounded())
        {
            if (onGroundTime <= _groundedBunnyHopTime && onGroundTime != 0 && _airSpeed < _maxSpeed)
            {
                _airSpeed += _bunnyHopAdditionalSpeed;
            }
            if (onGroundTime > _groundedBunnyHopTime)
            {
                _airSpeed = _normalAirSpeed;
            }
            onGroundTime = 0f;
            _speed = _airSpeed;
        }
        else
        {
            onGroundTime += Time.deltaTime;
        }

        if (isCroaching && CheckGrounded())
        {
            if (_speed > 0)
            {
                _speed -= _crouchSpeedDebuff * Time.deltaTime;
            }
        }
        else if (!isCroaching && CheckGrounded())
        {
            _speed = _normalSpeed;
        }

        rb.linearVelocity = moveDirection * _speed + new Vector3(0, rb.linearVelocity.y, 0);
    }

    void HandleJump()
    {
        if (_inputs.GetJump())
        {
            if (CheckGrounded())
            {
                rb.AddForce(Vector3.up * _jumpForce);
            }
            else
            {
                _airSpeed = _normalAirSpeed;
            }
        }
    }

    void HandleMouseLook()
    {
        // ≈сли камера заблокирована, не обрабатываем вращение
        if (lockCamera)
        {
            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        playerTransform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void HandleCrouch()
    {
        if (_inputs.GetCrouch())
        {
            playerCollider.height = playerCollider.height / 1.5f;
            playerCollider.center = new Vector3(0, 0.3f, 0);
            isCroaching = true;
        }
        else
        {
            playerCollider.height = 1.8f;
            playerCollider.center = new Vector3(0, 0, 0);
            isCroaching = false;
        }
    }

    bool CheckGrounded()
    {
        float rayLength = 1.4f;

        Debug.DrawLine(_moveableObj.transform.position, _moveableObj.transform.position + Vector3.down * rayLength, isGrounded ? Color.green : Color.red);

        return Physics.Raycast(_moveableObj.transform.position, Vector3.down, rayLength, groundMask);
    }
}