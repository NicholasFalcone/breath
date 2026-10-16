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
    public const float DefaultSurfaceDuration = 10f;
    public const float DefaultDiveDuration = 20f;
    public const int DefaultMaximumRepetitions = 3;
    public const string SurfaceDurationPreferenceKey = "GameManager.SurfaceDuration";
    public const string DiveDurationPreferenceKey = "GameManager.DiveDuration";
    public const string MaximumRepetitionsPreferenceKey = "GameManager.MaximumRepetitions";
    private const string MainMenuSceneName = "MainMenu";

    [Header("State timing")]
    [FormerlySerializedAs("surfaceRestDuration")] public float surfaceDuration = DefaultSurfaceDuration;
    [FormerlySerializedAs("divingActivityDuration")] public float diveDuration = DefaultDiveDuration;
    [Min(1)] public int maximumRepetitions = DefaultMaximumRepetitions;
    [SerializeField] private GameState initialState = GameState.Surface;

    private float stateElapsedTime;
    private int completedRepetitions;
    private bool isReturningToMenu;

    public GameState CurrentState { get; private set; }
    public int CoinCount { get; private set; }
    public int CurrentRepetition => Mathf.Min(completedRepetitions + 1, maximumRepetitions);
    public int MaximumRepetitions => maximumRepetitions;
    public float StateElapsedTime => stateElapsedTime;
    public float StateRemainingTime => Mathf.Max(0f, GetStateDuration(CurrentState) - stateElapsedTime);

    public event Action<GameState> OnStateChanged;

    public event Action<float> OnStateElapsedTimeChanged;

    public event Action<int> OnCoinCountChanged;

    public event Action<int, int> OnRepetitionChanged;

    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        CoinCount += amount;
        OnCoinCountChanged?.Invoke(CoinCount);
    }

    protected override void Awake()
    {
        base.Awake();
        surfaceDuration = Mathf.Max(
            MinimumStateDuration,
            PlayerPrefs.GetFloat(SurfaceDurationPreferenceKey, surfaceDuration));
        diveDuration = Mathf.Max(
            MinimumStateDuration,
            PlayerPrefs.GetFloat(DiveDurationPreferenceKey, diveDuration));
        maximumRepetitions = Mathf.Max(
            1,
            PlayerPrefs.GetInt(MaximumRepetitionsPreferenceKey, maximumRepetitions));
        SetState(initialState);
    }

    private void Update()
    {
        if (isReturningToMenu)
        {
            return;
        }

        stateElapsedTime += Time.deltaTime;
        OnStateElapsedTimeChanged?.Invoke(stateElapsedTime);

        if (stateElapsedTime >= GetStateDuration(CurrentState))
        {
            if (CurrentState == GameState.Surface)
            {
                SetState(GameState.Diving);
                return;
            }

            completedRepetitions++;
            if (completedRepetitions >= maximumRepetitions)
            {
                isReturningToMenu = true;
                UnityEngine.SceneManagement.SceneManager.LoadScene(MainMenuSceneName);
                return;
            }

            SetState(GameState.Surface);
            OnRepetitionChanged?.Invoke(CurrentRepetition, maximumRepetitions);
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
        maximumRepetitions = Mathf.Max(1, maximumRepetitions);
    }
}
