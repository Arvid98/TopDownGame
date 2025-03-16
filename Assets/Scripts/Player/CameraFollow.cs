using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class CameraFollow : NetworkBehaviour
{
    public Vector3 offset = new Vector3(0, 2, -10);
    public float smoothSpeed = 5f;

    private Camera mainCamera;

    void Start()
    {
        if (!IsOwner) return;

        mainCamera = Camera.main;

        if (mainCamera != null)
        {
            mainCamera.transform.position = transform.position + offset;
        }
    }

    void LateUpdate()
    {
        if (!IsOwner || mainCamera == null) return;

        Vector3 desiredPosition = new Vector3(transform.position.x + offset.x, transform.position.y + offset.y, mainCamera.transform.position.z);
        mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
