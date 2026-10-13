using UnityEngine;
using UnityEngine.UI;
public class RadialSlider : MonoBehaviour
{
    
    private float value;
    [SerializeField]
    private Image fillImage; 

    public float Value
    {
        get => value;
        set { this.value = Mathf.Clamp01(value); UpdateView(); }
    }

    void UpdateView()
    {
        fillImage.fillAmount = value;
    }
}
