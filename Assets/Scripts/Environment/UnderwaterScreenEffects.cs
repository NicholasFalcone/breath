using UnityEngine;
using UnityEngine.UI;

// Effetti a schermo e particelle ambientali visibili solo sott'acqua. Pilotato da UnderwaterFog.
public sealed class UnderwaterScreenEffects
{
    private const int VignetteSize = 128;

    private readonly GameObject overlayRoot;
    private readonly Image overlayImage;
    private readonly ParticleSystem ambientParticles;
    private readonly float maxEmissionRate;

    public UnderwaterScreenEffects(Camera camera, float ambientEmissionRate)
    {
        maxEmissionRate = ambientEmissionRate;

        overlayRoot = new GameObject("UnderwaterOverlay", typeof(RectTransform), typeof(Canvas));
        Canvas canvas = overlayRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        // Sotto la UI di gioco (sortingOrder 0).
        canvas.sortingOrder = -10;

        GameObject imageObject = new GameObject("Tint", typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(overlayRoot.transform, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        overlayImage = imageObject.GetComponent<Image>();
        overlayImage.sprite = CreateVignetteSprite();
        overlayImage.raycastTarget = false;
        overlayImage.color = Color.clear;
        overlayRoot.SetActive(false);

        ambientParticles = CreateAmbientParticles(camera);
    }

    public void Update(float submersion, float depth01, Color tint, float strength)
    {
        bool visible = submersion > 0.001f;
        if (overlayRoot.activeSelf != visible)
        {
            overlayRoot.SetActive(visible);
        }

        if (visible)
        {
            Color color = tint;
            color.a = submersion * Mathf.Lerp(strength, Mathf.Min(1f, strength + 0.3f), depth01);
            overlayImage.color = color;
        }

        ParticleSystem.EmissionModule emission = ambientParticles.emission;
        emission.rateOverTime = maxEmissionRate * submersion;
    }

    public void Destroy()
    {
        if (overlayRoot != null)
        {
            Object.Destroy(overlayRoot);
        }

        if (ambientParticles != null)
        {
            Object.Destroy(ambientParticles.gameObject);
        }
    }

    private static ParticleSystem CreateAmbientParticles(Camera camera)
    {
        GameObject root = new GameObject("UnderwaterAmbience");
        root.transform.SetParent(camera.transform, false);
        root.transform.localPosition = new Vector3(0f, 0f, 7f);
        root.transform.localRotation = Quaternion.identity;

        ParticleSystem particles = root.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 7f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.14f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.75f, 0.95f, 1f, 0.35f),
            new Color(1f, 1f, 1f, 0.7f));
        main.gravityModifier = -0.06f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 300;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(18f, 12f, 12f);

        ParticleSystem.NoiseModule noise = particles.noise;
        noise.enabled = true;
        noise.strength = 0.25f;
        noise.frequency = 0.5f;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient fade = new Gradient();
        fade.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.2f), new GradientAlphaKey(1f, 0.8f), new GradientAlphaKey(0f, 1f) });
        colorOverLifetime.color = fade;

        ParticleSystemRenderer renderer = root.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sharedMaterial = CoinBubbleEffect.GetBubbleMaterial();

        particles.Play();
        return particles;
    }

    // Centro quasi trasparente, bordi piu scuri: l'effetto "maschera subacquea".
    private static Sprite CreateVignetteSprite()
    {
        Texture2D texture = new Texture2D(VignetteSize, VignetteSize, TextureFormat.RGBA32, false)
        {
            name = "UnderwaterVignette_Runtime",
            wrapMode = TextureWrapMode.Clamp
        };

        float center = (VignetteSize - 1) * 0.5f;
        for (int y = 0; y < VignetteSize; y++)
        {
            for (int x = 0; x < VignetteSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center;
                float alpha = Mathf.Lerp(0.55f, 1f, Mathf.SmoothStep(0f, 1f, (distance - 0.3f) / 0.9f));
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, VignetteSize, VignetteSize), new Vector2(0.5f, 0.5f));
    }
}
