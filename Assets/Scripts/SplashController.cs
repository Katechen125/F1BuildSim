using UnityEngine;
using UnityEngine.UIElements;

public class SplashController : MonoBehaviour
{
    [Header("UI Document")]
    public UIDocument uiDocument;

    [Header("Scene References")]
    public GameObject circuitSelectorObject;

    [Header("Assets")]
    public Sprite mclarenImage;
    public Font orbitronFont;
    public Font exo2Font;

    private VisualElement root;

    void OnEnable()
    {
        root = uiDocument.rootVisualElement;

        var carBg = root.Q<VisualElement>("CarBg");
        if (carBg != null && mclarenImage != null)
            carBg.style.backgroundImage = new StyleBackground(mclarenImage);

        var startBtn = root.Q<Button>("BtnStart");
        if (startBtn != null)
            startBtn.clicked += OnStart;

        ApplyFonts();
    }

    void OnStart()
    {
        gameObject.SetActive(false);
        if (circuitSelectorObject != null)
            circuitSelectorObject.SetActive(true);
    }

    void ApplyFonts()
    {
        if (orbitronFont != null)
        {
            SetFont(root.Q<Label>("TitleSpeed"), orbitronFont);
            SetFont(root.Q<Label>("TitleKraft"), orbitronFont);
            SetFont(root.Q<Label>("BadgeText"), orbitronFont);
            SetFont(root.Q<Label>("Presents"), orbitronFont);
            SetFont(root.Q<Label>("RuleText"), orbitronFont);

            foreach (var l in root.Query<Label>(className: "stat-num").ToList())
                SetFont(l, orbitronFont);
            foreach (var l in root.Query<Label>(className: "bottom-step").ToList())
                SetFont(l, orbitronFont);
        }

        if (exo2Font != null)
        {
            SetFont(root.Q<Label>("Tagline"), exo2Font);
            SetFont(root.Q<Label>("Tagline2"), exo2Font);
            SetFont(root.Q<Label>("BottomRight"), exo2Font);

            foreach (var l in root.Query<Label>(className: "stat-lbl").ToList())
                SetFont(l, exo2Font);
        }
    }

    void SetFont(Label label, Font font)
    {
        if (label == null || font == null) return;
        label.style.unityFont = new StyleFont(font);
    }
}