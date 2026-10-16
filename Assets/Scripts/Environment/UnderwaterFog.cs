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

    [Header("Sott'acqua")]
    [SerializeField] private Color underwaterColor = new Color(0.06f, 0.42f, 0.66f);
    [SerializeField, Min(0f)] private float underwaterFogStart = 0f;
    [SerializeField, Min(0.1f)] private float underwaterFogEnd = 35f;
    [SerializeField, Min(0.01f)] private float submergeSmoothTime = 0.35f;
    [SerializeField, Range(0f, 1f)] private float overlayStrength = 0.4f;
    [SerializeField, Min(0f)] private float ambientParticlesPerSecond = 30f;

    private Camera cam;
    private CameraClearFlags originalClearFlags;
    private Color originalBackground;
    private UnderwaterScreenEffects screenEffects;
    private float submersion;
    private float submersionVelocity;

    public float Submersion => submersion;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        originalClearFlags = cam.clearFlags;
        originalBackground = cam.backgroundColor;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        screenEffects = new UnderwaterScreenEffects(cam, ambientParticlesPerSecond);
    }

    private void OnDestroy()
    {
        screenEffects?.Destroy();
    }

    private void LateUpdate()
    {
        float depth01 = Mathf.Clamp01((surfaceY - transform.position.y) / maxDepth);
        float targetSubmersion = transform.position.y < surfaceY ? 1f : 0f;
        submersion = Mathf.SmoothDamp(submersion, targetSubmersion, ref submersionVelocity, submergeSmoothTime);

        Color aboveColor = surfaceColor;
        Color belowColor = Color.Lerp(underwaterColor, abyssColor, depth01);
        Color color = Color.Lerp(aboveColor, belowColor, submersion);

        float belowStart = Mathf.Lerp(underwaterFogStart, abyssFogStart, depth01);
        float belowEnd = Mathf.Lerp(underwaterFogEnd, abyssFogEnd, depth01);

        RenderSettings.fog = true;
        RenderSettings.fogColor = color;
        RenderSettings.fogStartDistance = Mathf.Lerp(surfaceFogStart, belowStart, submersion);
        RenderSettings.fogEndDistance = Mathf.Lerp(surfaceFogEnd, belowEnd, submersion);

        // Sott'acqua lo skybox non deve vedersi oltre il piano d'acqua.
        if (tintCameraBackground || submersion > 0.5f)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = color;
        }
        else
        {
            cam.clearFlags = originalClearFlags;
            cam.backgroundColor = originalBackground;
        }

        screenEffects.Update(submersion, depth01, Color.Lerp(underwaterColor, abyssColor, depth01 * 0.7f), overlayStrength);
    }

    private void OnValidate()
    {
        maxDepth = Mathf.Max(0.1f, maxDepth);
        surfaceFogEnd = Mathf.Max(surfaceFogStart + 0.1f, surfaceFogEnd);
        abyssFogEnd = Mathf.Max(abyssFogStart + 0.1f, abyssFogEnd);
        underwaterFogEnd = Mathf.Max(underwaterFogStart + 0.1f, underwaterFogEnd);
    }
}
