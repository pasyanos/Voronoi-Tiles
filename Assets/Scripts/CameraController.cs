using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _lookAtTarget;
    [SerializeField] private float _cameraRotationRadius = 15f;
    [SerializeField] private float _rotationSpeed = 1f;

    // in radians!!!
    private float _rotation = 0f;
    private float yVal;

    private float pi2;

    private void Start()
    {
        pi2 = 2f*Mathf.PI;
        
        yVal = transform.position.y;
        UpdateCamera();
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            // Debug.LogError("Left");
            UpdateRotation(_rotationSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            // Debug.LogError("right");
            UpdateRotation(-_rotationSpeed * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        UpdateCamera();
    }

    private void UpdateCamera()
    {
        float xPosn = _cameraRotationRadius * Mathf.Cos(_rotation);
        float zPosn = _cameraRotationRadius * Mathf.Sin(_rotation);

        transform.position = new Vector3(xPosn, yVal, zPosn);
        
        transform.LookAt(_lookAtTarget);
    }

    private void UpdateRotation(float delta)
    {
        _rotation = (pi2 + _rotation + delta) % pi2;
    }
}
