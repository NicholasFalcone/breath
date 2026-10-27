using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private RadialSlider radialSlider;

    private void Start()
    {
        GameManager.Instance.OnStateChanged += OnStateChanged;
        GameManager.Instance.OnStateElapsedTimeChanged += OnStateElapsedTimeChanged;
    }
    
    private void OnDestroy()
    {
        GameManager.Instance.OnStateChanged -= OnStateChanged;
        GameManager.Instance.OnStateElapsedTimeChanged -= OnStateElapsedTimeChanged;
    }

    private void OnStateChanged(GameState state)
    {
        radialSlider.Value = state == GameState.Diving ? 1f : 0f;
    }

    private void OnStateElapsedTimeChanged(float elapsedTime)
    {
        if(radialSlider == null) return;
        
        radialSlider.Value = Mathf.Clamp01(elapsedTime / GameManager.Instance.GetStateDuration(GameManager.Instance.CurrentState));
    }
}
