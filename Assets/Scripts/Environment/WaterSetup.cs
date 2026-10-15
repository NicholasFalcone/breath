using UnityEngine;

public sealed class WaterSetup : MonoBehaviour
{
    [SerializeField] private float surfaceY = 0f;
    [SerializeField] private Vector2 size = new Vector2(200f, 200f);
    [SerializeField] private Color waterColor = new Color(0.2f, 0.6f, 0.9f, 0.35f);
    [SerializeField] private bool createOnAwake = true;

    public GameObject WaterPlane { get; private set; }

    private void Awake()
    {
        if (createOnAwake)
        {
            CreateWater();
        }
    }

    public void CreateWater()
    {
        if (WaterPlane != null)
        {
            return;
        }

        WaterPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        WaterPlane.name = "WaterSurface";
        WaterPlane.transform.SetParent(transform, false);
        WaterPlane.transform.position = new Vector3(transform.position.x, surfaceY, transform.position.z);
        // Il Plane di Unity misura 10x10 unita a scala 1.
        WaterPlane.transform.localScale = new Vector3(size.x / 10f, 1f, size.y / 10f);

        Collider planeCollider = WaterPlane.GetComponent<Collider>();
        if (planeCollider != null)
        {
            Destroy(planeCollider);
        }

        MeshRenderer meshRenderer = WaterPlane.GetComponent<MeshRenderer>();
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        meshRenderer.sharedMaterial = CreateTransparentMaterial();
    }

    private Material CreateTransparentMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        Material material = new Material(shader) { name = "WaterSurface_Runtime", color = waterColor };

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", waterColor);
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

        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return material;
    }

    private void OnValidate()
    {
        size.x = Mathf.Max(1f, size.x);
        size.y = Mathf.Max(1f, size.y);
    }
}
