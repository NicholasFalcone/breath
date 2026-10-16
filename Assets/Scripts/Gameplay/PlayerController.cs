using UnityEngine;

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



    private void Update()
    {
        if (IsActivityAllowed)
        {
            // --- FASE 1: ATTIVITÀ / IMMERSIONE ---
            // Scende verticalmente e si muove lateralmente
            float verticalMovement = -diveSpeed * Time.deltaTime;
            float inputX = Input.GetAxis("Horizontal");

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
