using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]

    [SerializeField]
    private float movementSpeed = 4f;

    [SerializeField]
    private float rotationSpeed = 360f;

    [SerializeField]
    [Range(0.1f, 1f)] [Tooltip("Rotation speed multiplier when aiming")]
    private float aimingRotationMultiplier = 0.4f;

    [Space]
    [Header("Camera")]

    [SerializeField]
    private Camera mainCamera;
    
    [SerializeField]
    private GameObject followCamera;
    
    [SerializeField]
    private GameObject aimCamera;

    [Range(10, 200)]
    [SerializeField]
    private float aimDistance = 100f;
    
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

    private bool _isAiming;
    private bool _isThrowing;
    private bool _isOnCooldown;
    private Vector2 _movementInput;
    private Vector2 _mousePosition;

    private readonly Vector2 _screenMidPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);

    private Vector3 AimPoint =>
        mainCamera.ScreenToWorldPoint(new Vector3(_screenMidPoint.x, _screenMidPoint.y, aimDistance)).normalized;

    private float RotationMultiplier => _isAiming ? aimingRotationMultiplier : 1f;

#if UNITY_EDITOR
    private Vector3 _lookAtLocation;
#endif

    private void Start()
    {
        _transform = transform;
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
        targetLocation.y = 0;
        var targetRotation = Quaternion.LookRotation(targetLocation.normalized);
        var ratio = Time.deltaTime * (Mathf.Deg2Rad * rotationSpeed * RotationMultiplier);
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
        var mouseWorldPosition = mainCamera.ScreenToWorldPoint(new Vector3(_mousePosition.x, _mousePosition.y, aimDistance));
#if UNITY_EDITOR
        //Cache hit location as LookAtLocation for debug purposes
        _lookAtLocation = mouseWorldPosition;
#endif
        return mouseWorldPosition;
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