using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    private Vector2 _movementInput;

    public Vector2 MovementInput => _movementInput;

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        _movementInput = new Vector2(moveX, moveY);
    }
}
