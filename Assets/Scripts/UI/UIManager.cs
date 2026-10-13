using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private RadialSlider radialSlider;

    private void Start()
    {
        GameManager.Instance.StateChanged += OnStateChanged;
        GameManager.Instance.StateElapsedTimeChanged += OnStateElapsedTimeChanged;
    }
    
    private void OnDestroy()
    {
        GameManager.Instance.StateChanged -= OnStateChanged;
        GameManager.Instance.StateElapsedTimeChanged -= OnStateElapsedTimeChanged;
    }

    private void OnStateChanged(GameState state)
    {
        radialSlider.Value = state == GameState.Surface ? 0f : 1f;
    }

    private void OnStateElapsedTimeChanged(float elapsedTime)
    {
        if(radialSlider == null) return;
        
        radialSlider.Value = Mathf.Clamp01(elapsedTime / GameManager.Instance.GetStateDuration(GameManager.Instance.CurrentState));
    }
}
