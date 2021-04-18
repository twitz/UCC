using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]

    [SerializeField]
    private float movementSpeed = 4f;

    [SerializeField]
    private float rotationSpeed = 360f;

    [Space]
    [Header("Camera")]
    
    [SerializeField]
    private GameObject followCamera;
    
    [SerializeField]
    private GameObject aimCamera;

    [Space]
    [Header("Throwing")]

    [SerializeField]
    private Projectile snowballPrefab;

    [SerializeField]
    private GameObject snowballEmitter;
    
    [SerializeField]
    private GameObject crossair;

    [SerializeField]
    private float crossairDelay;
    
    [SerializeField]
    private float throwCooldown = 2;
    
    private Transform _transform;
    private Camera _camera;

    private bool _isAiming;
    private bool _isThrowing;
    private bool _isOnCooldown;
    private Vector2 _movementInput;
    private Vector2 _mousePosition;

#if UNITY_EDITOR
    private Vector3 _lookAtLocation;
#endif

    private void Start()
    {
        _transform = transform;
        _camera = Camera.main;
        aimCamera.SetActive(false);
        followCamera.SetActive(true);
    }

    private void Update()
    {
        HandleCameras();
        ProcessMovement();
        ProcessRotation();
        ProcessThrowing();
    }

    private void HandleCameras()
    {
        if (_isAiming && !aimCamera.activeInHierarchy)
        {
            followCamera.SetActive(false);
            aimCamera.SetActive(true);
            StartCoroutine(ShowCrosshair());
        }
        else if(!_isAiming && !followCamera.activeInHierarchy)
        {
            followCamera.SetActive(true);
            aimCamera.SetActive(false);
            crossair.SetActive(false);
        }
    }

    private IEnumerator ShowCrosshair()
    {
        yield return new WaitForSeconds(crossairDelay);
        crossair.SetActive(enabled);
    }

    private void ProcessMovement()
    {
        if (_movementInput.magnitude <= 0) return;
        var forwardMovement = _transform.forward * _movementInput.y;
        var strafingMovement = _transform.right * _movementInput.x;
        var movementVector = forwardMovement + strafingMovement;
        var distance = (movementSpeed * Time.deltaTime);
        _transform.position += movementVector * distance;
    }

    private void ProcessRotation()
    {
        var targetLocation = GetMouseWorldPosition() - _transform.position;
        var targetRotation = Quaternion.LookRotation(targetLocation.normalized);
        var ratio = Time.deltaTime * (Mathf.Deg2Rad * rotationSpeed);
        _transform.rotation = Quaternion.Slerp(_transform.rotation, targetRotation, ratio);
    }

    private void ProcessThrowing()
    {
        if (!_isAiming || !_isThrowing || _isOnCooldown) return;
        var instance = Instantiate(snowballPrefab, snowballEmitter.transform.position, _transform.rotation);
        instance.Fire();
        StartCoroutine(CooldownCounter());
    }

    // TODO Use mouse position coupled with screen size to determine where on the screen we are aiming - we don't really care about where in the world
    private Vector3 GetMouseWorldPosition()
    {
        var position = _transform.position;
        var planeHitLocation = position + (_transform.forward * 100f);
        var projectionPlane = new Plane(Vector3.up, position);
        var projectionRay = _camera.ScreenPointToRay(_mousePosition);

        if (projectionPlane.Raycast(projectionRay, out var distance))
        {
            planeHitLocation = projectionRay.GetPoint(distance);
        }

#if UNITY_EDITOR
        //Cache hit location as LookAtLocation for debug purposes
        _lookAtLocation = planeHitLocation;
#endif

        return planeHitLocation;
    }

    public void OnMovementInput(InputAction.CallbackContext context)
    {
        _movementInput = context.ReadValue<Vector2>();
    }

    public void OnAimInput(InputAction.CallbackContext context)
    {
        _isAiming = context.ReadValueAsButton();
    }

    public void OnFireInput(InputAction.CallbackContext context)
    {
        _isThrowing = context.ReadValueAsButton();
    }

    public void OnMousePositionChange(InputAction.CallbackContext context)
    {
        _mousePosition = context.ReadValue<Vector2>();
    }

    private IEnumerator CooldownCounter()
    {
        _isOnCooldown = true;
        yield return new WaitForSeconds(throwCooldown);
        _isOnCooldown = false;
    }

    private void OnDrawGizmosSelected()
    {
        //OnDrawGizmos should only ever be called in the editor, but might as well be on the safe side to avoid any exceptions in a built game
#if UNITY_EDITOR
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(_lookAtLocation, .5f);

#endif
    }
}