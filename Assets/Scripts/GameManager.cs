using System;
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
    [SerializeField, Min(MinimumStateDuration)] private float surfaceRestDuration = 10f;
    [SerializeField, Min(MinimumStateDuration)] private float divingActivityDuration = 20f;
    [SerializeField] private GameState initialState = GameState.Surface;

    private float stateElapsedTime;

    public GameState CurrentState { get; private set; }
    public float StateElapsedTime => stateElapsedTime;
    public float StateRemainingTime => Mathf.Max(0f, GetStateDuration(CurrentState) - stateElapsedTime);

    public event Action<GameState> StateChanged;

    public event Action<float> StateElapsedTimeChanged;

    protected override void Awake()
    {
        base.Awake();
        SetState(initialState);
    }

    private void Update()
    {
        stateElapsedTime += Time.deltaTime;
        StateElapsedTimeChanged?.Invoke(stateElapsedTime);

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
        StateChanged?.Invoke(CurrentState);
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
            ? Mathf.Max(MinimumStateDuration, surfaceRestDuration)
            : Mathf.Max(MinimumStateDuration, divingActivityDuration);
    }

    private void OnValidate()
    {
        surfaceRestDuration = Mathf.Max(MinimumStateDuration, surfaceRestDuration);
        divingActivityDuration = Mathf.Max(MinimumStateDuration, divingActivityDuration);
    }
}
