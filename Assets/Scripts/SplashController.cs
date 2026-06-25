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

        // Assign car image
        var carEl = root.Q<VisualElement>("CarBg");
        if (carEl != null && mclarenImage != null)
            carEl.style.backgroundImage = new StyleBackground(mclarenImage);

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
            SetFont(root.Q<Label>("LogoTitle"), orbitronFont);
            SetFont(root.Q<Label>("LogoTitleAccent"), orbitronFont);
            SetFont(root.Q<Label>("LogoEyebrow"), orbitronFont);
            SetFont(root.Q<Label>("LogoSubtitle"), orbitronFont);

            foreach (var lbl in root.Query<Label>(className: "stat-num").ToList())
                SetFont(lbl, orbitronFont);
        }

        if (exo2Font != null)
        {
            SetFont(root.Q<Label>("LogoDesc"), exo2Font);
            SetFont(root.Q<Label>("BottomText"), exo2Font);

            foreach (var lbl in root.Query<Label>(className: "stat-lbl").ToList())
                SetFont(lbl, exo2Font);
            foreach (var lbl in root.Query<Label>(className: "step-lbl").ToList())
                SetFont(lbl, exo2Font);
        }
    }

    void SetFont(Label label, Font font)
    {
        if (label == null || font == null) return;
        label.style.unityFont = new StyleFont(font);
    }
}