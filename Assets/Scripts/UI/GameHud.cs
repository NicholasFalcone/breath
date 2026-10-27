using UnityEngine;
using UnityEngine.UI;

// HUD di gioco responsive per portrait: contatori in alto, slider radiale in basso, tutto dentro la safe area.
[RequireComponent(typeof(Canvas), typeof(CanvasScaler))]
public sealed class GameHud : MonoBehaviour
{
    private const int CircleTextureSize = 64;
    private const string MainMenuSceneName = "MainMenu";

    [SerializeField] private RectTransform radialSlider;
    [SerializeField] private Vector2 referenceResolution = new Vector2(1080f, 1920f);
    [SerializeField, Min(0f)] private float margin = 40f;
    [SerializeField, Min(1f)] private float sliderSize = 180f;

    private RectTransform safeAreaRoot;
    private Text coinText;
    private Text repetitionText;
    private Text countdownText;
    private GameObject pausePanel;
    private Rect lastSafeArea;
    private Vector2Int lastScreenSize;
    private GameManager gameManager;

    private void Awake()
    {
        ConfigureScaler();
        BuildLayout();
    }

    private void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            return;
        }

        gameManager.OnCoinCountChanged += SetCoins;
        gameManager.OnRepetitionChanged += SetRepetition;
        SetCoins(gameManager.CoinCount);
        SetRepetition(gameManager.CurrentRepetition, gameManager.MaximumRepetitions);
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        if (gameManager != null)
        {
            gameManager.OnCoinCountChanged -= SetCoins;
            gameManager.OnRepetitionChanged -= SetRepetition;
        }
    }

    private void Update()
    {
        UpdateCountdown();
        if (Screen.safeArea != lastSafeArea || lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
        {
            ApplySafeArea();
        }
    }

    private void ConfigureScaler()
    {
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        // In portrait la larghezza e il lato limitante: il layout resta proporzionato su tutti i rapporti d'aspetto.
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0f;
    }

    private void BuildLayout()
    {
        GameObject rootObject = new GameObject("SafeArea", typeof(RectTransform));
        safeAreaRoot = rootObject.GetComponent<RectTransform>();
        safeAreaRoot.SetParent(transform, false);
        safeAreaRoot.SetAsFirstSibling();
        ApplySafeArea();

        coinText = CreateCounter("CoinCounter", new Vector2(0f, 1f), new Vector2(margin, -margin), true, out Image coinIcon);
        coinIcon.sprite = CreateCircleSprite(new Color(1f, 0.82f, 0.2f), new Color(0.85f, 0.55f, 0.05f));

        repetitionText = CreateCounter("RepetitionCounter", new Vector2(1f, 1f), new Vector2(-margin, -margin), false, out Image repetitionIcon);
        repetitionIcon.gameObject.SetActive(false);
        CreateCountdownLabel();
        BuildPauseUi();

        if (radialSlider != null)
        {
            radialSlider.SetParent(safeAreaRoot, false);
            radialSlider.anchorMin = radialSlider.anchorMax = Vector2.zero;
            radialSlider.pivot = Vector2.zero;
            radialSlider.sizeDelta = new Vector2(sliderSize, sliderSize);
            radialSlider.anchoredPosition = new Vector2(margin, margin);
            radialSlider.localScale = Vector3.one;
        }
    }

    private void ApplySafeArea()
    {
        lastSafeArea = Screen.safeArea;
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);

        if (Screen.width <= 0 || Screen.height <= 0)
        {
            return;
        }

        Vector2 min = lastSafeArea.position;
        Vector2 max = lastSafeArea.position + lastSafeArea.size;
        safeAreaRoot.anchorMin = new Vector2(min.x / Screen.width, min.y / Screen.height);
        safeAreaRoot.anchorMax = new Vector2(max.x / Screen.width, max.y / Screen.height);
        safeAreaRoot.offsetMin = Vector2.zero;
        safeAreaRoot.offsetMax = Vector2.zero;
    }

    private Text CreateCounter(string objectName, Vector2 anchor, Vector2 position, bool iconOnLeft, out Image icon)
    {
        GameObject pill = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        RectTransform pillRect = pill.GetComponent<RectTransform>();
        pillRect.SetParent(safeAreaRoot, false);
        pillRect.anchorMin = pillRect.anchorMax = anchor;
        pillRect.pivot = anchor;
        pillRect.anchoredPosition = position;

        Image background = pill.GetComponent<Image>();
        background.sprite = CreatePillSprite();
        background.type = Image.Type.Sliced;
        background.color = new Color(0.02f, 0.14f, 0.24f, 0.7f);
        background.raycastTarget = false;

        HorizontalLayoutGroup layout = pill.GetComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(28, 36, 14, 14);
        layout.spacing = 18f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = pill.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        icon = CreateIcon(pill.transform);
        Text label = CreateLabel(pill.transform);
        if (!iconOnLeft)
        {
            icon.transform.SetAsLastSibling();
        }

        return label;
    }

    private static Image CreateIcon(Transform parent)
    {
        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        iconObject.transform.SetParent(parent, false);
        LayoutElement element = iconObject.GetComponent<LayoutElement>();
        element.preferredWidth = 56f;
        element.preferredHeight = 56f;
        Image image = iconObject.GetComponent<Image>();
        image.raycastTarget = false;
        return image;
    }

    private static Text CreateLabel(Transform parent)
    {
        GameObject textObject = new GameObject("Value", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 52;
        text.fontStyle = FontStyle.Bold;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private void SetCoins(int count)
    {
        coinText.text = count.ToString();
    }

    private void SetRepetition(int current, int maximum)
    {
        repetitionText.text = $"Ripetuta {current}/{maximum}";
    }

    private void BuildPauseUi()
    {
        Button pauseButton = CreateButton("PauseButton", safeAreaRoot, "II", 56, new Vector2(110f, 110f));
        RectTransform pauseRect = (RectTransform)pauseButton.transform;
        pauseRect.anchorMin = pauseRect.anchorMax = pauseRect.pivot = new Vector2(1f, 1f);
        pauseRect.anchoredPosition = new Vector2(-margin, -margin - 120f);
        pauseButton.onClick.AddListener(Pause);

        pausePanel = new GameObject("PausePanel", typeof(RectTransform), typeof(Image));
        RectTransform panelRect = (RectTransform)pausePanel.transform;
        panelRect.SetParent(transform, false);
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;
        pausePanel.GetComponent<Image>().color = new Color(0f, 0.05f, 0.1f, 0.8f);

        Text title = CreateLabel(panelRect);
        title.text = "PAUSA";
        title.fontSize = 90;
        SetCentered(title.rectTransform, new Vector2(800f, 140f), new Vector2(0f, 260f));

        Button resume = CreateButton("ResumeButton", panelRect, "RIPRENDI", 48, new Vector2(620f, 130f));
        SetCentered((RectTransform)resume.transform, new Vector2(620f, 130f), new Vector2(0f, 40f));
        resume.onClick.AddListener(Resume);

        Button menu = CreateButton("MenuButton", panelRect, "MENU PRINCIPALE", 48, new Vector2(620f, 130f));
        SetCentered((RectTransform)menu.transform, new Vector2(620f, 130f), new Vector2(0f, -130f));
        menu.onClick.AddListener(ReturnToMenu);

        pausePanel.SetActive(false);
    }

    private static void SetCentered(RectTransform rect, Vector2 size, Vector2 position)
    {
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    private static Button CreateButton(string objectName, Transform parent, string label, int fontSize, Vector2 size)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        RectTransform rect = (RectTransform)buttonObject.transform;
        rect.SetParent(parent, false);
        rect.sizeDelta = size;

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = CreatePillSprite();
        image.type = Image.Type.Sliced;
        image.color = new Color(0.1f, 0.4f, 0.6f, 0.9f);

        Text text = CreateLabel(rect);
        text.text = label;
        text.fontSize = fontSize;
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
        return buttonObject.GetComponent<Button>();
    }

    private void Pause()
    {
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
    }

    private void Resume()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }

    private void ReturnToMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(MainMenuSceneName);
    }

    private void CreateCountdownLabel()
    {
        countdownText = CreateLabel(safeAreaRoot);
        countdownText.gameObject.name = "Countdown";
        countdownText.fontSize = 260;
        countdownText.gameObject.AddComponent<Outline>().effectDistance = new Vector2(6f, -6f);
        RectTransform rect = countdownText.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.6f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(600f, 400f);
        rect.anchoredPosition = Vector2.zero;
        countdownText.gameObject.SetActive(false);
    }

    private void UpdateCountdown()
    {
        bool active = gameManager != null && gameManager.CurrentState == GameState.Countdown;
        if (countdownText.gameObject.activeSelf != active)
        {
            countdownText.gameObject.SetActive(active);
        }

        if (active)
        {
            countdownText.text = Mathf.CeilToInt(gameManager.StateRemainingTime).ToString();
        }
    }

    private static Sprite CreateCircleSprite(Color fill, Color rim)
    {
        Texture2D texture = new Texture2D(CircleTextureSize, CircleTextureSize, TextureFormat.RGBA32, false)
        {
            name = "HudCircle_Runtime",
            wrapMode = TextureWrapMode.Clamp
        };

        float center = (CircleTextureSize - 1) * 0.5f;
        for (int y = 0; y < CircleTextureSize; y++)
        {
            for (int x = 0; x < CircleTextureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center;
                float alpha = Mathf.Clamp01((1f - distance) / 0.05f);
                Color color = distance > 0.78f ? rim : fill;
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, CircleTextureSize, CircleTextureSize), new Vector2(0.5f, 0.5f));
    }

    // Capsula con bordi arrotondati, ridimensionabile in 9-slice.
    private static Sprite CreatePillSprite()
    {
        const int size = 64;
        const int radius = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "HudPill_Runtime",
            wrapMode = TextureWrapMode.Clamp
        };

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(0f, Mathf.Abs(x - (size - 1) * 0.5f) - (size * 0.5f - radius));
                float dy = Mathf.Max(0f, Mathf.Abs(y - (size - 1) * 0.5f) - (size * 0.5f - radius));
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(radius - distance)));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }
}
