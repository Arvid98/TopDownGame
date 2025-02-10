using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;
    private Vector2 _movement;
    private Rigidbody2D _rb;
    private Animator _animator;
    private PlayerInputHandler _inputHandler;

    private string _horizontal = "Horizontal";
    private string _vertical = "Vertical";
    private string _lastHorizontal = "LastHorizontal";
    private string _lastVertical = "LastVertical";

    private NetworkVariable<Vector2> networkMovement = new NetworkVariable<Vector2>(Vector2.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _inputHandler = GetComponent<PlayerInputHandler>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            networkMovement.OnValueChanged += OnMovementChanged;
        }
    }

    private void OnDestroy()
    {
        if (IsClient)
        {
            networkMovement.OnValueChanged -= OnMovementChanged;
        }
    }

    void Update()
    {
        if (IsOwner)
        {
            _movement.Set(InputManager.Movement.x, InputManager.Movement.y);
            //_movement = _inputHandler.MovementInput;
            Vector3 movement = new Vector3(_movement.x, _movement.y, 0) * _moveSpeed * Time.deltaTime;
            transform.position += movement;

            UpdatePositionServerRpc(transform.position);
            UpdateAnimationServerRpc(_movement);
        }

        _animator.SetFloat(_horizontal, networkMovement.Value.x);
        _animator.SetFloat(_vertical, networkMovement.Value.y);

        if (networkMovement.Value != Vector2.zero)
        {
            _animator.SetFloat(_lastHorizontal, networkMovement.Value.x);
            _animator.SetFloat(_lastVertical, networkMovement.Value.y);
        }
    }

    private void OnMovementChanged(Vector2 previousValue, Vector2 newValue)
    {
        _animator.SetFloat(_horizontal, newValue.x);
        _animator.SetFloat(_vertical, newValue.y);

        if (newValue != Vector2.zero)
        {
            _animator.SetFloat(_lastHorizontal, newValue.x);
            _animator.SetFloat(_lastVertical, newValue.y);
        }
    }

    [ServerRpc]
    private void UpdatePositionServerRpc(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    [ServerRpc]
    private void UpdateAnimationServerRpc(Vector2 movement)
    {
        networkMovement.Value = movement;
    }
}
