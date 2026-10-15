using UnityEngine;

[RequireComponent(typeof(Camera))]
public sealed class UnderwaterFog : MonoBehaviour
{
    [Header("Profondita")]
    [SerializeField] private float surfaceY = 0f;
    [SerializeField, Min(0.1f)] private float maxDepth = 60f;

    [Header("Superficie")]
    [SerializeField] private Color surfaceColor = new Color(0.55f, 0.85f, 1f);
    [SerializeField, Min(0f)] private float surfaceFogStart = 10f;
    [SerializeField, Min(0.1f)] private float surfaceFogEnd = 80f;

    [Header("Abisso")]
    [SerializeField] private Color abyssColor = new Color(0.01f, 0.04f, 0.15f);
    [SerializeField, Min(0f)] private float abyssFogStart = 0f;
    [SerializeField, Min(0.1f)] private float abyssFogEnd = 18f;

    [SerializeField] private bool tintCameraBackground = true;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
    }

    private void LateUpdate()
    {
        float depth01 = Mathf.Clamp01((surfaceY - transform.position.y) / maxDepth);

        Color color = Color.Lerp(surfaceColor, abyssColor, depth01);
        RenderSettings.fog = true;
        RenderSettings.fogColor = color;
        RenderSettings.fogStartDistance = Mathf.Lerp(surfaceFogStart, abyssFogStart, depth01);
        RenderSettings.fogEndDistance = Mathf.Lerp(surfaceFogEnd, abyssFogEnd, depth01);

        if (tintCameraBackground)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = color;
        }
    }

    private void OnValidate()
    {
        maxDepth = Mathf.Max(0.1f, maxDepth);
        surfaceFogEnd = Mathf.Max(surfaceFogStart + 0.1f, surfaceFogEnd);
        abyssFogEnd = Mathf.Max(abyssFogStart + 0.1f, abyssFogEnd);
    }
}
