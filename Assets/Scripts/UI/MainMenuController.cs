using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class MainMenuController : MonoBehaviour
{
    private const float MinimumDuration = 0.01f;
    private const string GameSceneName = "MainGame";

    private InputField surfaceDurationInput;
    private InputField diveDurationInput;
    private InputField repetitionsInput;
    private Text validationMessage;
    private GameObject mainPanel;
    private GameObject settingsPanel;

    private void Awake()
    {
        CreateInterface();
    }

    private void CreateInterface()
    {
        GameObject canvasObject = new GameObject("Main Menu Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Image background = CreateImage("Background", canvasObject.transform, Vector2.zero, Vector2.zero, new Color(0.035f, 0.12f, 0.2f));
        background.rectTransform.anchorMin = Vector2.zero;
        background.rectTransform.anchorMax = Vector2.one;
        background.raycastTarget = false;

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        mainPanel = CreatePanel("Main Panel", canvasObject.transform, new Vector2(560f, 440f));
        CreateText("Title", mainPanel.transform, "BREATH", 56, new Vector2(0f, 115f), new Vector2(500f, 80f), Color.white, FontStyle.Bold);
        CreateText("Subtitle", mainPanel.transform, "Trova il tuo ritmo", 24, new Vector2(0f, 55f), new Vector2(480f, 45f), new Color(0.72f, 0.85f, 0.91f), FontStyle.Normal);
        CreateButton("Play Button", mainPanel.transform, "INIZIA", new Vector2(0f, -35f), Play);
        CreateButton("Settings Button", mainPanel.transform, "IMPOSTAZIONI", new Vector2(0f, -115f), ShowSettings);

        settingsPanel = CreatePanel("Settings Panel", canvasObject.transform, new Vector2(600f, 620f));
        CreateText("Settings Title", settingsPanel.transform, "IMPOSTAZIONI", 38, new Vector2(0f, 240f), new Vector2(540f, 60f), Color.white, FontStyle.Bold);
        CreateText("Surface Label", settingsPanel.transform, "Riposo (secondi)", 23, new Vector2(-105f, 145f), new Vector2(260f, 44f), Color.white, FontStyle.Normal);
        CreateText("Dive Label", settingsPanel.transform, "Attività (secondi)", 23, new Vector2(-105f, 60f), new Vector2(260f, 44f), Color.white, FontStyle.Normal);
        CreateText("Repetitions Label", settingsPanel.transform, "Numero di ripetute", 23, new Vector2(-105f, -25f), new Vector2(260f, 44f), Color.white, FontStyle.Normal);

        surfaceDurationInput = CreateDurationInput("Riposo", settingsPanel.transform, new Vector2(175f, 145f));
        diveDurationInput = CreateDurationInput("Attivita", settingsPanel.transform, new Vector2(175f, 60f));
        repetitionsInput = CreateDurationInput("Ripetute", settingsPanel.transform, new Vector2(175f, -25f));
        repetitionsInput.contentType = InputField.ContentType.IntegerNumber;
        validationMessage = CreateText("Validation Message", settingsPanel.transform, string.Empty, 18, new Vector2(0f, -80f), new Vector2(520f, 38f), new Color(1f, 0.55f, 0.48f), FontStyle.Normal);

        CreateButton("Save Button", settingsPanel.transform, "SALVA", new Vector2(0f, -145f), SaveSettings);
        CreateButton("Back Button", settingsPanel.transform, "INDIETRO", new Vector2(0f, -225f), ShowMain);

        settingsPanel.SetActive(false);
    }

    private static GameObject CreatePanel(string name, Transform parent, Vector2 size)
    {
        Image panel = CreateImage(name, parent, size, Vector2.zero, new Color(0.06f, 0.2f, 0.29f, 0.96f));
        return panel.gameObject;
    }

    private static Image CreateImage(string name, Transform parent, Vector2 size, Vector2 position, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static Text CreateText(
        string name,
        Transform parent,
        string value,
        int fontSize,
        Vector2 position,
        Vector2 size,
        Color color,
        FontStyle style)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static void CreateButton(string name, Transform parent, string label, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        Image image = CreateImage(name, parent, new Vector2(340f, 58f), position, new Color(0.1f, 0.48f, 0.61f));
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);
        CreateText("Label", image.transform, label, 22, Vector2.zero, new Vector2(320f, 50f), Color.white, FontStyle.Bold);
    }

    private static InputField CreateDurationInput(string name, Transform parent, Vector2 position)
    {
        Image background = CreateImage(name + " Input", parent, new Vector2(165f, 48f), position, Color.white);
        InputField input = background.gameObject.AddComponent<InputField>();
        input.contentType = InputField.ContentType.DecimalNumber;
        input.lineType = InputField.LineType.SingleLine;

        Text valueText = CreateText("Value", background.transform, string.Empty, 23, Vector2.zero, new Vector2(145f, 40f), new Color(0.05f, 0.12f, 0.17f), FontStyle.Normal);
        RectTransform valueRect = valueText.rectTransform;
        valueRect.anchorMin = Vector2.zero;
        valueRect.anchorMax = Vector2.one;
        valueRect.offsetMin = new Vector2(8f, 2f);
        valueRect.offsetMax = new Vector2(-8f, -2f);
        valueText.alignment = TextAnchor.MiddleCenter;

        Text placeholder = CreateText("Placeholder", background.transform, "secondi", 18, Vector2.zero, new Vector2(145f, 40f), new Color(0.45f, 0.5f, 0.53f), FontStyle.Italic);
        RectTransform placeholderRect = placeholder.rectTransform;
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(8f, 2f);
        placeholderRect.offsetMax = new Vector2(-8f, -2f);

        input.textComponent = valueText;
        input.placeholder = placeholder;
        return input;
    }

    private void ShowSettings()
    {
        surfaceDurationInput.text = PlayerPrefs.GetFloat(
            GameManager.SurfaceDurationPreferenceKey, GameManager.DefaultSurfaceDuration).ToString(CultureInfo.InvariantCulture);
        diveDurationInput.text = PlayerPrefs.GetFloat(
            GameManager.DiveDurationPreferenceKey, GameManager.DefaultDiveDuration).ToString(CultureInfo.InvariantCulture);
        repetitionsInput.text = PlayerPrefs.GetInt(
            GameManager.MaximumRepetitionsPreferenceKey, GameManager.DefaultMaximumRepetitions).ToString(CultureInfo.InvariantCulture);
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
        validationMessage.text = string.Empty;
    }

    private void ShowMain()
    {
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    private void SaveSettings()
    {
        if (!TryReadDuration(surfaceDurationInput.text, out float surfaceDuration)
            || !TryReadDuration(diveDurationInput.text, out float diveDuration)
            || !int.TryParse(repetitionsInput.text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int maximumRepetitions)
            || maximumRepetitions < 1)
        {
            validationMessage.text = "Inserisci durate valide e almeno una ripetuta.";
            return;
        }

        PlayerPrefs.SetFloat(GameManager.SurfaceDurationPreferenceKey, surfaceDuration);
        PlayerPrefs.SetFloat(GameManager.DiveDurationPreferenceKey, diveDuration);
        PlayerPrefs.SetInt(GameManager.MaximumRepetitionsPreferenceKey, maximumRepetitions);
        PlayerPrefs.Save();
        ShowMain();
    }

    private static bool TryReadDuration(string text, out float duration)
    {
        return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out duration)
            && !float.IsNaN(duration)
            && !float.IsInfinity(duration)
            && duration >= MinimumDuration;
    }

    private static void Play()
    {
        SceneManager.LoadScene(GameSceneName);
    }
}
