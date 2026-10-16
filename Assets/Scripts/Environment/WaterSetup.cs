using UnityEngine;

public sealed class WaterSetup : MonoBehaviour
{
    private const int TextureSize = 128;

    [SerializeField] private float surfaceY = 0f;
    [SerializeField] private Vector2 size = new Vector2(200f, 200f);
    [SerializeField] private Color waterColor = new Color(0.12f, 0.5f, 0.85f, 0.7f);
    [SerializeField] private bool createOnAwake = true;

    [Header("Movimento superficie")]
    [SerializeField, Min(0.5f)] private float rippleTileSize = 8f;
    [SerializeField, Min(0.5f)] private float highlightTileSize = 4.5f;
    [SerializeField] private Vector2 rippleScrollSpeed = new Vector2(0.02f, 0.012f);
    [SerializeField] private Vector2 highlightScrollSpeed = new Vector2(-0.035f, 0.025f);
    [SerializeField, Range(0f, 1f)] private float highlightStrength = 0.55f;

    [Header("Vista da sotto")]
    [SerializeField] private Color undersideColor = new Color(0.45f, 0.82f, 1f, 0.9f);

    private Material rippleMaterial;
    private Material highlightMaterial;
    private Material undersideMaterial;
    private Material undersideHighlightMaterial;
    private Vector2 rippleOffset;
    private Vector2 highlightOffset;

    public GameObject WaterPlane { get; private set; }

    private void Awake()
    {
        if (createOnAwake)
        {
            CreateWater();
        }
    }

    private void Update()
    {
        if (WaterPlane == null)
        {
            return;
        }

        rippleOffset += rippleScrollSpeed * Time.deltaTime;
        highlightOffset += highlightScrollSpeed * Time.deltaTime;
        SetOffset(rippleMaterial, rippleOffset);
        SetOffset(undersideMaterial, rippleOffset);
        SetOffset(highlightMaterial, highlightOffset);
        SetOffset(undersideHighlightMaterial, highlightOffset);
    }

    public void CreateWater()
    {
        if (WaterPlane != null)
        {
            return;
        }

        Texture2D rippleTexture = CreatePatternTexture(false);
        Texture2D highlightTexture = CreatePatternTexture(true);

        Color highlightColor = new Color(1f, 1f, 1f, highlightStrength);
        rippleMaterial = CreateTransparentMaterial("WaterSurface_Runtime", waterColor, rippleTexture, rippleTileSize, 3000);
        highlightMaterial = CreateTransparentMaterial("WaterHighlights_Runtime", highlightColor, highlightTexture, highlightTileSize, 3001);
        undersideMaterial = CreateTransparentMaterial("WaterUnderside_Runtime", undersideColor, rippleTexture, rippleTileSize, 3000);
        undersideHighlightMaterial = CreateTransparentMaterial("WaterUndersideHighlights_Runtime", highlightColor, highlightTexture, highlightTileSize, 3001);

        WaterPlane = CreatePlane("WaterSurface", transform, 0f, false, rippleMaterial);
        CreatePlane("WaterHighlights", WaterPlane.transform, 0.02f, false, highlightMaterial);
        CreatePlane("WaterUnderside", WaterPlane.transform, -0.02f, true, undersideMaterial);
        CreatePlane("WaterUndersideHighlights", WaterPlane.transform, -0.04f, true, undersideHighlightMaterial);
    }

    private GameObject CreatePlane(string objectName, Transform parent, float yOffset, bool flipped, Material material)
    {
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = objectName;
        plane.transform.SetParent(parent, false);
        plane.transform.position = new Vector3(transform.position.x, surfaceY + yOffset, transform.position.z);
        plane.transform.rotation = flipped ? Quaternion.Euler(180f, 0f, 0f) : Quaternion.identity;
        // Il Plane di Unity misura 10x10 unita a scala 1.
        plane.transform.localScale = new Vector3(size.x / 10f, 1f, size.y / 10f);

        Collider planeCollider = plane.GetComponent<Collider>();
        if (planeCollider != null)
        {
            Destroy(planeCollider);
        }

        MeshRenderer meshRenderer = plane.GetComponent<MeshRenderer>();
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        meshRenderer.sharedMaterial = material;
        return plane;
    }

    private Material CreateTransparentMaterial(string materialName, Color color, Texture2D texture, float tileSize, int renderQueue)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        Material material = new Material(shader) { name = materialName, color = color };

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        }

        string textureProperty = GetTextureProperty(material);
        material.SetTexture(textureProperty, texture);
        material.SetTextureScale(textureProperty, new Vector2(size.x / tileSize, size.y / tileSize));
        material.renderQueue = renderQueue;
        return material;
    }

    private static string GetTextureProperty(Material material)
    {
        return material.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex";
    }

    private static void SetOffset(Material material, Vector2 offset)
    {
        if (material != null)
        {
            material.SetTextureOffset(GetTextureProperty(material), offset);
        }
    }

    // Pattern di caustiche senza giunture: con frequenze intere |sin| si ripete esattamente ogni tile.
    private static Texture2D CreatePatternTexture(bool highlightOnly)
    {
        const int terms = 4;
        int[] freqU = { 2, 1, 3, -2 };
        int[] freqV = { 1, 3, -2, 4 };
        float[] phase = { 0f, 1.3f, 2.1f, 0.7f };

        float[] values = new float[TextureSize * TextureSize];
        float min = float.MaxValue;
        float max = float.MinValue;
        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                float u = x / (float)TextureSize;
                float v = y / (float)TextureSize;
                float sum = 0f;
                for (int i = 0; i < terms; i++)
                {
                    float wave = Mathf.Sin(Mathf.PI * (freqU[i] * u + freqV[i] * v) + phase[i]);
                    sum += 1f - Mathf.Abs(wave);
                }

                float value = Mathf.Pow(sum / terms, 3f);
                values[y * TextureSize + x] = value;
                min = Mathf.Min(min, value);
                max = Mathf.Max(max, value);
            }
        }

        Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, true)
        {
            name = highlightOnly ? "WaterHighlights_Runtime" : "WaterRipples_Runtime",
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Trilinear,
            anisoLevel = 8
        };

        Color32[] pixels = new Color32[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            float t = Mathf.InverseLerp(min, max, values[i]);
            pixels[i] = highlightOnly
                ? new Color(1f, 1f, 1f, Mathf.SmoothStep(0f, 1f, t))
                : new Color(Mathf.Lerp(0.7f, 1f, t), Mathf.Lerp(0.7f, 1f, t), Mathf.Lerp(0.7f, 1f, t), 1f);
        }

        texture.SetPixels32(pixels);
        texture.Apply(true);
        return texture;
    }

    private void OnValidate()
    {
        size.x = Mathf.Max(1f, size.x);
        size.y = Mathf.Max(1f, size.y);
    }
}
