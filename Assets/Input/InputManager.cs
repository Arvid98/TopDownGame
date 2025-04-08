using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class InputManager : MonoBehaviour
{
    public static Vector2 Movement;

    private PlayerInput _playerInput;
    private InputAction _moveAction;

    // Attack input state
    public static bool Attack { get; private set; }
    public static Vector2 AimDirection { get; private set; }

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();

        _moveAction = _playerInput.actions["Movement"];
    }

    private void Update()
    {
        Movement = _moveAction.ReadValue<Vector2>();

        // Attack input
        Attack = Input.GetMouseButtonDown(0);

        // Get aim direction for ranged attacks (from mouse position)
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector3 playerPos = Camera.main.transform.position - new Vector3(0, 0, Camera.main.transform.position.z);
        AimDirection = (mousePos - playerPos).normalized;
    }
}
