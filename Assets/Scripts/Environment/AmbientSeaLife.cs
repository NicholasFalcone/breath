using System.Collections.Generic;
using UnityEngine;

// Barchette in superficie e pesci sott'acqua, costruiti con primitive e materiali generati a runtime.
public sealed class AmbientSeaLife : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private Transform player;
    [SerializeField] private float surfaceY = 0f;

    [Header("Area")]
    [SerializeField, Min(1f)] private float halfWidth = 16f;
    [SerializeField, Min(0f)] private float minDepthFromPlayerPlane = 2f;
    [SerializeField, Min(0f)] private float maxDepthFromPlayerPlane = 14f;

    [Header("Barchette")]
    [SerializeField, Min(0)] private int boatCount = 4;
    [SerializeField, Min(0f)] private float boatSpeed = 0.35f;
    [SerializeField, Min(0f)] private float boatBobHeight = 0.12f;

    [Header("Pesci")]
    [SerializeField, Min(0)] private int fishCount = 18;
    [SerializeField, Min(0f)] private float fishSpeed = 1.4f;
    [SerializeField, Min(0.5f)] private float fishAboveRecycleDistance = 9f;
    [SerializeField, Min(1f)] private float fishSpawnBelowPlayer = 10f;

    private sealed class Boat
    {
        public Transform root;
        public float direction;
        public float speed;
        public float phase;
    }

    private sealed class Fish
    {
        public Transform root;
        public Transform tail;
        public float direction;
        public float speed;
        public float phase;
        public float baseY;
        public float wobble;
    }

    private static readonly Color[] BoatHullColors =
    {
        new Color(0.85f, 0.2f, 0.18f), new Color(0.95f, 0.75f, 0.2f),
        new Color(0.2f, 0.45f, 0.8f), new Color(0.9f, 0.9f, 0.88f)
    };

    private static readonly Color[] FishColors =
    {
        new Color(1f, 0.55f, 0.15f), new Color(1f, 0.82f, 0.2f),
        new Color(0.3f, 0.75f, 0.95f), new Color(0.95f, 0.4f, 0.55f),
        new Color(0.55f, 0.85f, 0.45f)
    };

    private readonly List<Boat> boats = new List<Boat>();
    private readonly List<Fish> fishes = new List<Fish>();
    private readonly Dictionary<Color, Material> materials = new Dictionary<Color, Material>();
    private float centerX;

    private void Awake()
    {
        if (player == null)
        {
            PlayerController playerController = FindFirstObjectByType<PlayerController>();
            if (playerController != null)
            {
                player = playerController.transform;
            }
        }

        if (player == null)
        {
            Debug.LogError($"{nameof(AmbientSeaLife)} requires a player reference.", this);
            enabled = false;
            return;
        }

        centerX = player.position.x;

        for (int i = 0; i < boatCount; i++)
        {
            boats.Add(CreateBoat(i));
        }

        for (int i = 0; i < fishCount; i++)
        {
            fishes.Add(CreateFish(i));
        }
    }

    private void Update()
    {
        UpdateBoats();
        UpdateFish();
    }

    private Boat CreateBoat(int index)
    {
        Color hullColor = BoatHullColors[index % BoatHullColors.Length];
        Transform root = new GameObject($"Boat_{index}").transform;
        root.SetParent(transform, false);

        float scale = Random.Range(0.9f, 1.5f);
        root.localScale = Vector3.one * scale;

        CreatePart(PrimitiveType.Cube, root, "Hull", new Vector3(0f, 0.12f, 0f), new Vector3(2.4f, 0.4f, 0.95f), hullColor);
        CreatePart(PrimitiveType.Cube, root, "Bow", new Vector3(1.3f, 0.2f, 0f), new Vector3(0.5f, 0.3f, 0.6f), hullColor, Quaternion.Euler(0f, 0f, 20f));
        CreatePart(PrimitiveType.Cube, root, "Stripe", new Vector3(0f, 0.3f, 0f), new Vector3(2.45f, 0.08f, 1f), Color.white);
        CreatePart(PrimitiveType.Cube, root, "Cabin", new Vector3(-0.45f, 0.6f, 0f), new Vector3(0.7f, 0.45f, 0.65f), new Color(0.95f, 0.93f, 0.88f));
        CreatePart(PrimitiveType.Cylinder, root, "Mast", new Vector3(0.45f, 1.05f, 0f), new Vector3(0.06f, 0.9f, 0.06f), new Color(0.4f, 0.28f, 0.18f));
        CreatePart(PrimitiveType.Cube, root, "Sail", new Vector3(0.12f, 1.1f, 0f), new Vector3(0.65f, 1f, 0.03f), Color.white, Quaternion.identity);

        float direction = Random.value < 0.5f ? -1f : 1f;
        root.rotation = Quaternion.Euler(0f, direction > 0f ? 0f : 180f, 0f);
        root.position = new Vector3(
            centerX + Random.Range(-halfWidth, halfWidth),
            surfaceY,
            player.position.z + Random.Range(minDepthFromPlayerPlane + 2f, maxDepthFromPlayerPlane + 2f));

        return new Boat
        {
            root = root,
            direction = direction,
            speed = boatSpeed * Random.Range(0.6f, 1.3f),
            phase = Random.Range(0f, Mathf.PI * 2f)
        };
    }

    private Fish CreateFish(int index)
    {
        Color color = FishColors[index % FishColors.Length];
        Transform root = new GameObject($"Fish_{index}").transform;
        root.SetParent(transform, false);
        root.localScale = Vector3.one * Random.Range(0.7f, 1.4f);

        CreatePart(PrimitiveType.Sphere, root, "Body", Vector3.zero, new Vector3(0.9f, 0.45f, 0.22f), color);
        CreatePart(PrimitiveType.Sphere, root, "Eye", new Vector3(0.3f, 0.07f, 0.09f), Vector3.one * 0.08f, Color.black);

        Transform tail = new GameObject("TailPivot").transform;
        tail.SetParent(root, false);
        tail.localPosition = new Vector3(-0.4f, 0f, 0f);
        CreatePart(PrimitiveType.Sphere, tail, "Tail", new Vector3(-0.15f, 0f, 0f), new Vector3(0.35f, 0.4f, 0.05f), color * 0.85f);

        Fish fish = new Fish
        {
            root = root,
            tail = tail,
            direction = Random.value < 0.5f ? -1f : 1f,
            speed = fishSpeed * Random.Range(0.6f, 1.4f),
            phase = Random.Range(0f, Mathf.PI * 2f),
            wobble = Random.Range(0.1f, 0.3f)
        };

        Respawn(fish, true);
        return fish;
    }

    private void Respawn(Fish fish, bool anywhere)
    {
        float maxY = surfaceY - 1.5f;
        float minY = player.position.y - fishSpawnBelowPlayer;
        float y = anywhere
            ? Random.Range(Mathf.Min(minY, maxY), maxY)
            : Mathf.Min(maxY, player.position.y - Random.Range(fishSpawnBelowPlayer * 0.5f, fishSpawnBelowPlayer));

        fish.baseY = y;
        fish.direction = Random.value < 0.5f ? -1f : 1f;
        fish.root.position = new Vector3(
            centerX + Random.Range(-halfWidth, halfWidth),
            y,
            player.position.z + Random.Range(minDepthFromPlayerPlane, maxDepthFromPlayerPlane));
        fish.root.rotation = Quaternion.Euler(0f, fish.direction > 0f ? 0f : 180f, 0f);
    }

    private void UpdateBoats()
    {
        float time = Time.time;
        foreach (Boat boat in boats)
        {
            Vector3 position = boat.root.position;
            position.x += boat.direction * boat.speed * Time.deltaTime;
            position.y = surfaceY + Mathf.Sin(time * 1.3f + boat.phase) * boatBobHeight;

            if (Mathf.Abs(position.x - centerX) > halfWidth)
            {
                boat.direction = -boat.direction;
                position.x = centerX + Mathf.Sign(position.x - centerX) * halfWidth;
            }

            boat.root.position = position;
            float yaw = boat.direction > 0f ? 0f : 180f;
            float roll = Mathf.Sin(time * 1.1f + boat.phase) * 3.5f;
            float pitch = Mathf.Sin(time * 0.9f + boat.phase * 1.7f) * 2f;
            boat.root.rotation = Quaternion.Euler(pitch, yaw, roll);
        }
    }

    private void UpdateFish()
    {
        float time = Time.time;
        float maxY = surfaceY - 1.5f;
        foreach (Fish fish in fishes)
        {
            Vector3 position = fish.root.position;
            position.x += fish.direction * fish.speed * Time.deltaTime;
            position.y = Mathf.Min(maxY, fish.baseY + Mathf.Sin(time * 0.8f + fish.phase) * fish.wobble);
            fish.root.position = position;

            if (Mathf.Abs(position.x - centerX) > halfWidth)
            {
                fish.direction = -fish.direction;
                fish.root.rotation = Quaternion.Euler(0f, fish.direction > 0f ? 0f : 180f, 0f);
            }

            fish.tail.localRotation = Quaternion.Euler(0f, Mathf.Sin(time * 8f * (fish.speed / fishSpeed) + fish.phase) * 25f, 0f);

            // I pesci lasciati sopra dal player vengono riciclati sotto di lui.
            if (fish.root.position.y > player.position.y + fishAboveRecycleDistance)
            {
                Respawn(fish, false);
            }
        }
    }

    private void CreatePart(PrimitiveType type, Transform parent, string partName, Vector3 localPosition, Vector3 localScale, Color color, Quaternion? localRotation = null)
    {
        GameObject part = GameObject.CreatePrimitive(type);
        part.name = partName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = localRotation ?? Quaternion.identity;
        part.transform.localScale = localScale;

        Collider partCollider = part.GetComponent<Collider>();
        if (partCollider != null)
        {
            Destroy(partCollider);
        }

        Renderer partRenderer = part.GetComponent<Renderer>();
        partRenderer.sharedMaterial = GetMaterial(color);
        partRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    private Material GetMaterial(Color color)
    {
        if (materials.TryGetValue(color, out Material material))
        {
            return material;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        material = new Material(shader) { name = "SeaLife_Runtime", color = color };
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.35f);
        }

        materials[color] = material;
        return material;
    }

    private void OnValidate()
    {
        maxDepthFromPlayerPlane = Mathf.Max(minDepthFromPlayerPlane, maxDepthFromPlayerPlane);
    }
}
