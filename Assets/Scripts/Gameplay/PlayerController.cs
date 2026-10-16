using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerController : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Behaviour[] activityBehaviours;

    [SerializeField]
    private float diveSpeed = 1f;
    [SerializeField]
    private float horizontalSpeed = 3f;
    [SerializeField]
    private float xClamp = 8f;
    [SerializeField] private float returnSpeed = 2f;
    private Vector3 surfaceStartPosition;
    private bool isAtSurface = true;
    public bool IsActivityAllowed { get; private set; }

    private void Awake()
    {
        surfaceStartPosition = transform.position;
        EnsureCoinCollisionSetup();
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (gameManager == null)
        {
            Debug.LogError($"{nameof(PlayerController)} requires a {nameof(GameManager)} reference.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (gameManager == null)
        {
            return;
        }

        gameManager.OnStateChanged += HandleStateChanged;
        ApplyState(gameManager.CurrentState);
    }

    private void OnDisable()
    {
        if (gameManager != null)
        {
            gameManager.OnStateChanged -= HandleStateChanged;
        }
    }

    private void HandleStateChanged(GameState state)
    {
        ApplyState(state);
    }

    private void ApplyState(GameState state)
    {
        IsActivityAllowed = state == GameState.Diving;
        if (IsActivityAllowed)
        {
            isAtSurface = false;
        }

        foreach (Behaviour activityBehaviour in activityBehaviours)
        {
            if (activityBehaviour != null && activityBehaviour != this)
            {
                activityBehaviour.enabled = IsActivityAllowed;
            }
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        Coin coin = other.GetComponentInParent<Coin>();
        if (coin != null)
        {
            coin.Collect();
        }
    }

    // Il player non ha fisica nel prefab: serve un Rigidbody cinematico e un collider per ricevere i trigger delle monete.
    private void EnsureCoinCollisionSetup()
    {
        if (!TryGetComponent(out Rigidbody body))
        {
            body = gameObject.AddComponent<Rigidbody>();
        }

        body.isKinematic = true;
        body.useGravity = false;

        if (GetComponentInChildren<Collider>() != null)
        {
            return;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Bounds bounds = renderers.Length > 0
            ? renderers[0].bounds
            : new Bounds(transform.position, Vector3.one);
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.center = transform.InverseTransformPoint(bounds.center);
        Vector3 lossyScale = transform.lossyScale;
        float maxScale = Mathf.Max(Mathf.Abs(lossyScale.x), Mathf.Abs(lossyScale.y), Mathf.Abs(lossyScale.z));
        collider.radius = Mathf.Max(bounds.extents.x, bounds.extents.y) / Mathf.Max(0.0001f, maxScale);
    }

    // Tastiera/gamepad per l'editor, touch/mouse: tenendo premuto si va verso il lato toccato.
    private static float ReadHorizontalInput()
    {
        float value = 0f;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) value -= 1f;
            if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) value += 1f;
        }

        Gamepad gamepad = Gamepad.current;
        if (gamepad != null)
        {
            value += gamepad.leftStick.x.ReadValue();
        }

        Pointer pointer = Pointer.current;
        if (pointer != null && pointer.press.isPressed && Screen.width > 0)
        {
            float normalized = (pointer.position.ReadValue().x - Screen.width * 0.5f) / (Screen.width * 0.25f);
            value += Mathf.Clamp(normalized, -1f, 1f);
        }

        return Mathf.Clamp(value, -1f, 1f);
    }

    private void Update()
    {
        if (IsActivityAllowed)
        {
            // --- FASE 1: ATTIVITÀ / IMMERSIONE ---
            // Scende verticalmente e si muove lateralmente
            float verticalMovement = -diveSpeed * Time.deltaTime;
            float inputX = ReadHorizontalInput();

            // Esempio movimento orizzontale
            float moveX = inputX * horizontalSpeed * Time.deltaTime;

            // Applica il movimento (blocchiamo X nei limiti dello schermo)
            float newX = Mathf.Clamp(transform.position.x + moveX, -xClamp, xClamp);
            float newY = transform.position.y + verticalMovement;

            transform.position = new Vector3(newX, newY, transform.position.z);
        }
        else
        {
            // --- FASE 2: RIPOSO / SUPERFICIE (o Risalita) ---
            if (!isAtSurface)
            {
                // Muove fluidamente il sub verso l'alto fino alla quota di superficie
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    new Vector3(surfaceStartPosition.x, surfaceStartPosition.y, transform.position.z),
                    returnSpeed * Time.deltaTime
                );

                if (transform.position.y >= surfaceStartPosition.y)
                {
                    isAtSurface = true;
                }
            }
            else
            {
                // Una volta a galla, facciamo un leggero effetto di galleggiamento (floating)
                float floatOffset = Mathf.Sin(Time.time * 2f) * 0.15f;
                transform.position = new Vector3(transform.position.x, surfaceStartPosition.y + floatOffset, transform.position.z);
            }
        }
    }

}
