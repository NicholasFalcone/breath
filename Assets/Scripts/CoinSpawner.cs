using UnityEngine;

public sealed class CoinSpawner : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField, Min(0.01f)] private float verticalDistanceBetweenCoins = 2f;

    [SerializeField] private float horizontalDistanceRange = 2f;
    [SerializeField] private float spawnHeightOffset = 4f;

    private Vector3 previousPlayerPosition;
    private float descentDistanceSinceLastCoin;

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }

        if (gameManager == null || player == null || coinPrefab == null)
        {
            Debug.LogError(
                $"{nameof(CoinSpawner)} requires a {nameof(GameManager)}, player transform, and coin prefab.",
                this);
            enabled = false;
            return;
        }

        previousPlayerPosition = player.position;
    }

    private void Update()
    {
        if (gameManager.CurrentState != GameState.Diving)
        {
            ResetSpawnTracking();
            return;
        }

        float descentSpeed = GetDescentSpeed();
        previousPlayerPosition = player.position;

        if (descentSpeed <= 0f)
        {
            return;
        }

        descentDistanceSinceLastCoin += descentSpeed * Time.deltaTime;

        while (descentDistanceSinceLastCoin >= verticalDistanceBetweenCoins)
        {
            descentDistanceSinceLastCoin -= verticalDistanceBetweenCoins;
            SpawnCoin();
        }
    }

    private float GetDescentSpeed()
    {
        if (playerRigidbody != null)
        {
            return Mathf.Max(0f, -playerRigidbody.linearVelocity.y);
        }

        return Mathf.Max(
            0f,
            (previousPlayerPosition.y - player.position.y) / Time.deltaTime);
    }

    private void SpawnCoin()
    {
        Vector3 spawnPosition = player.position;
        spawnPosition.y += descentDistanceSinceLastCoin + spawnHeightOffset;
        spawnPosition.x += Random.Range(-horizontalDistanceRange, horizontalDistanceRange);
        Instantiate(coinPrefab, spawnPosition, Quaternion.identity, transform);
    }

    private void ResetSpawnTracking()
    {
        previousPlayerPosition = player.position;
        descentDistanceSinceLastCoin = 0f;
    }

    private void OnValidate()
    {
        verticalDistanceBetweenCoins = Mathf.Max(0.01f, verticalDistanceBetweenCoins);
    }
}
