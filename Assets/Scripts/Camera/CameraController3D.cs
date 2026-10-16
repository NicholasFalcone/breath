using UnityEngine;

[RequireComponent(typeof(Camera))]
public sealed class CameraController3D : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Transform target;
    [SerializeField] private Camera controlledCamera;

    [Header("Follow")]
    [SerializeField] private Vector3 surfaceOffset = new Vector3(0f, 1f, -10f);
    [SerializeField] private Vector3 divingOffset = new Vector3(0f, 0.5f, -7f);
    [SerializeField, Min(0.01f)] private float horizontalSmoothTime = 0.25f;
    [SerializeField, Min(0.01f)] private float verticalSmoothTime = 0.12f;
    [SerializeField, Min(0.01f)] private float depthSmoothTime = 0.18f;

    [Header("Zoom e inclinazione")]
    [SerializeField, Range(1f, 179f)] private float surfaceFieldOfView = 65f;
    [SerializeField, Range(1f, 179f)] private float divingFieldOfView = 50f;
    [SerializeField, Min(0.01f)] private float transitionSpeed = 3f;
    [SerializeField] private float divingPitch = 8f;
    [SerializeField] private float surfacePitch = 10f;

    private float xVelocity;
    private float yVelocity;
    private float zVelocity;
    private float divingBlend;
    private Quaternion surfaceRotation;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (target == null)
        {
            PlayerController playerController = FindFirstObjectByType<PlayerController>();
            if (playerController != null)
            {
                target = playerController.transform;
            }
        }

        if (controlledCamera == null)
        {
            controlledCamera = GetComponent<Camera>();
        }

        if (gameManager == null || target == null || controlledCamera == null)
        {
            Debug.LogError(
                $"{nameof(CameraController3D)} richiede {nameof(GameManager)}, player e camera.",
                this);
            enabled = false;
            return;
        }

        surfaceRotation = transform.rotation;
        divingBlend = gameManager.CurrentState == GameState.Diving ? 1f : 0f;
    }

    private void LateUpdate()
    {
        float targetDivingBlend = gameManager.CurrentState == GameState.Diving ? 1f : 0f;
        divingBlend = Mathf.Lerp(divingBlend, targetDivingBlend, transitionSpeed * Time.deltaTime);

        // Lo smorzamento orizzontale maggiore conserva una lieve inerzia acquatica.
        Vector3 desiredPosition = target.position + Vector3.Lerp(surfaceOffset, divingOffset, divingBlend);
        Vector3 currentPosition = transform.position;
        currentPosition.x = Mathf.SmoothDamp(
            currentPosition.x,
            desiredPosition.x,
            ref xVelocity,
            horizontalSmoothTime);
        currentPosition.y = Mathf.SmoothDamp(
            currentPosition.y,
            desiredPosition.y,
            ref yVelocity,
            verticalSmoothTime);
        currentPosition.z = Mathf.SmoothDamp(
            currentPosition.z,
            desiredPosition.z,
            ref zVelocity,
            depthSmoothTime);
        transform.position = currentPosition;

        float targetFieldOfView = Mathf.Lerp(surfaceFieldOfView, divingFieldOfView, divingBlend);
        controlledCamera.fieldOfView = Mathf.Lerp(
            controlledCamera.fieldOfView,
            targetFieldOfView,
            transitionSpeed * Time.deltaTime);

        Quaternion targetRotation = surfaceRotation * Quaternion.Euler(Mathf.Lerp(surfacePitch, divingPitch, divingBlend), 0f, 0f);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            transitionSpeed * Time.deltaTime);
    }

    private void OnValidate()
    {
        horizontalSmoothTime = Mathf.Max(0.01f, horizontalSmoothTime);
        verticalSmoothTime = Mathf.Max(0.01f, verticalSmoothTime);
        depthSmoothTime = Mathf.Max(0.01f, depthSmoothTime);
        transitionSpeed = Mathf.Max(0.01f, transitionSpeed);
    }
}
