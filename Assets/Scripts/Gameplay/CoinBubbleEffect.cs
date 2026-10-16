using UnityEngine;

public static class CoinBubbleEffect
{
    private const int TextureSize = 64;

    private static Material bubbleMaterial;

    public static void Play(Vector3 position)
    {
        GameObject effectObject = new GameObject("CoinBubbles");
        effectObject.transform.position = position;

        ParticleSystem particles = effectObject.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = particles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.8f, 1.4f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
        main.startColor = new Color(0.8f, 0.95f, 1f, 0.8f);
        main.gravityModifier = -0.15f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 8, 14) });

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        ParticleSystem.NoiseModule noise = particles.noise;
        noise.enabled = true;
        noise.strength = 0.3f;
        noise.frequency = 1.5f;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient fade = new Gradient();
        fade.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
        colorOverLifetime.color = fade;

        ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(
            1f,
            new AnimationCurve(new Keyframe(0f, 0.6f), new Keyframe(0.3f, 1f), new Keyframe(1f, 1.2f)));

        ParticleSystemRenderer renderer = effectObject.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sharedMaterial = GetBubbleMaterial();

        particles.Play();
    }

    public static Material GetBubbleMaterial()
    {
        if (bubbleMaterial != null)
        {
            return bubbleMaterial;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
            ?? Shader.Find("Sprites/Default");
        bubbleMaterial = new Material(shader) { name = "CoinBubble_Runtime" };

        Texture2D texture = CreateBubbleTexture();
        bubbleMaterial.mainTexture = texture;
        if (bubbleMaterial.HasProperty("_BaseMap"))
        {
            bubbleMaterial.SetTexture("_BaseMap", texture);
        }

        if (bubbleMaterial.HasProperty("_Surface"))
        {
            bubbleMaterial.SetFloat("_Surface", 1f);
            bubbleMaterial.SetFloat("_Blend", 0f);
            bubbleMaterial.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            bubbleMaterial.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            bubbleMaterial.SetFloat("_ZWrite", 0f);
            bubbleMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            bubbleMaterial.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        return bubbleMaterial;
    }

    // Anello sottile con riempimento leggero e riflesso in alto a sinistra.
    private static Texture2D CreateBubbleTexture()
    {
        Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
        {
            name = "CoinBubble_Runtime",
            wrapMode = TextureWrapMode.Clamp
        };

        float center = (TextureSize - 1) * 0.5f;
        Vector2 highlight = new Vector2(center * 0.65f, center * 1.35f);
        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center;
                float alpha = 0f;
                if (distance <= 1f)
                {
                    float ring = Mathf.SmoothStep(0f, 1f, (distance - 0.7f) / 0.3f);
                    alpha = Mathf.Max(0.12f, ring * 0.9f);
                    alpha *= Mathf.Clamp01((1f - distance) / 0.06f);
                }

                float shine = Mathf.Clamp01(1f - Vector2.Distance(new Vector2(x, y), highlight) / (center * 0.25f));
                alpha = Mathf.Max(alpha, shine * 0.9f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return texture;
    }
}
