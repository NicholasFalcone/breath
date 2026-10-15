using System;
using UnityEngine.Serialization;
using UnityEngine;

public enum GameState
{
    Surface,
    Diving
}

public sealed class GameManager : Singleton<GameManager>
{
    private const float MinimumStateDuration = 0.01f;

    [Header("State timing")]
    [FormerlySerializedAs("surfaceRestDuration")] public float surfaceDuration = 10f;
    [FormerlySerializedAs("divingActivityDuration")] public float diveDuration = 20f;
    [SerializeField] private GameState initialState = GameState.Surface;

    private float stateElapsedTime;

    public GameState CurrentState { get; private set; }
    public float StateElapsedTime => stateElapsedTime;
    public float StateRemainingTime => Mathf.Max(0f, GetStateDuration(CurrentState) - stateElapsedTime);

    public event Action<GameState> OnStateChanged;

    public event Action<float> OnStateElapsedTimeChanged;

    protected override void Awake()
    {
        base.Awake();
        SetState(initialState);
    }

    private void Update()
    {
        stateElapsedTime += Time.deltaTime;
        OnStateElapsedTimeChanged?.Invoke(stateElapsedTime);

        if (stateElapsedTime >= GetStateDuration(CurrentState))
        {
            SetState(CurrentState == GameState.Surface
                ? GameState.Diving
                : GameState.Surface);
        }
    }

    public void SetState(GameState state)
    {
        CurrentState = state;
        stateElapsedTime = 0f;
        OnStateChanged?.Invoke(CurrentState);
    }

    public void ToggleState()
    {
        SetState(CurrentState == GameState.Surface
            ? GameState.Diving
            : GameState.Surface);
    }

    public float GetStateDuration(GameState state)
    {
        return state == GameState.Surface
            ? Mathf.Max(MinimumStateDuration, surfaceDuration)
            : Mathf.Max(MinimumStateDuration, diveDuration);
    }

    private void OnValidate()
    {
        surfaceDuration = Mathf.Max(MinimumStateDuration, surfaceDuration);
        diveDuration = Mathf.Max(MinimumStateDuration, diveDuration);
    }
}
