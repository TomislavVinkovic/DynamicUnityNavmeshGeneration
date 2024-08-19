using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineCameraController : MonoBehaviour
{
    public float MoveSpeed = 10f;
    public float ZoomSpeed = 2f;
    public float MinZoomDistance = -50f;
    public float MaxZoomDistance = 50f;

    private Cinemachine.CinemachineFreeLook freeLookCamera;

    void Start()
    {
        freeLookCamera = GetComponent<Cinemachine.CinemachineFreeLook>();
    }

    void Update() {
        // WASD Movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = (Vector3.right * horizontal) + (Vector3.forward * vertical);

        movement = movement.normalized * MoveSpeed * Time.deltaTime;

        transform.position += new Vector3(movement.x, 0, movement.z);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        freeLookCamera.m_Lens.FieldOfView -= scroll * ZoomSpeed;
        freeLookCamera.m_Lens.FieldOfView = Mathf.Clamp(freeLookCamera.m_Lens.FieldOfView, MinZoomDistance, MaxZoomDistance);
    }

}
