using UnityEngine;
using UnityEngine.UIElements;

public class BuilderUIController : MonoBehaviour
{
    [Header("UI Document")]
    public UIDocument uiDocument;

    [Header("All Parts")]
    public CarPart[] allParts;

    [Header("Part Icons")]
    public Sprite iconFrontWing;
    public Sprite iconRearWing;
    public Sprite iconEngine;
    public Sprite iconSuspension;
    public Sprite iconTyres;
    public Sprite iconBrakes;
    public Sprite iconSidepods;
    public Sprite iconFloor;

    [Header("Fonts")]
    public Font orbitronFont;
    public Font exo2Font;

    private VisualElement circuitBannerFlag;
    private Label circuitBannerName;

    private VisualElement root;
    private VisualElement partListContainer;
    private Label selectedCategoryLabel;
    private Label categoryCountLabel;
    private Label efficiencyLabel;
    private Label lapTimeLabel;
    private Label speedLabel;
    private Label corneringLabel;
    private Label reliabilityLabel;
    private Label buildStatusLabel;
    private VisualElement introBox;
    private VisualElement progressFill;
    private Label progressCount;
    private Label circuitRatingLabel;
    private VisualElement circuitBannerTags;

    private PartCategory currentCategory = PartCategory.FrontWing;

    // Icons per category
    Sprite GetCategoryIcon(PartCategory cat) => cat switch
    {
        PartCategory.FrontWing => iconFrontWing,
        PartCategory.RearWing => iconRearWing,
        PartCategory.Engine => iconEngine,
        PartCategory.Suspension => iconSuspension,
        PartCategory.Tyres => iconTyres,
        PartCategory.Brakes => iconBrakes,
        PartCategory.Sidepods => iconSidepods,
        PartCategory.Floor => iconFloor,
        _ => null
    };
    void OnEnable()
    {
        root = uiDocument.rootVisualElement;

        if (CircuitSelectionController.ChosenCircuit != null)
        {
            Debug.Log($"Building for: {CircuitSelectionController.ChosenCircuit.circuitName}");
        }

        partListContainer = root.Q<VisualElement>("PartList");
        selectedCategoryLabel = root.Q<Label>("CategoryLabel");
        categoryCountLabel = root.Q<Label>("CategoryCount");
        efficiencyLabel = root.Q<Label>("EfficiencyValue");
        lapTimeLabel = root.Q<Label>("LapTimeValue");
        speedLabel = root.Q<Label>("SpeedValue");
        corneringLabel = root.Q<Label>("CorneringValue");
        reliabilityLabel = root.Q<Label>("ReliabilityValue");
        buildStatusLabel = root.Q<Label>("BuildStatus");
        introBox = root.Q<VisualElement>("IntroBox");
        progressFill = root.Q<VisualElement>("ProgressFill");
        progressCount = root.Q<Label>("ProgressCount");
        circuitBannerFlag = root.Q<VisualElement>("CircuitBannerFlag");
        circuitBannerName = root.Q<Label>("CircuitBannerName");
        circuitRatingLabel = root.Q<Label>("CircuitRating");
        circuitBannerTags = root.Q<VisualElement>("CircuitBannerTags");

        // Apply fonts
        ApplyFontToElements();

        // Category buttons hide intro and show parts
        BindCategoryBtn("BtnFrontWing", PartCategory.FrontWing);
        BindCategoryBtn("BtnRearWing", PartCategory.RearWing);
        BindCategoryBtn("BtnEngine", PartCategory.Engine);
        BindCategoryBtn("BtnSuspension", PartCategory.Suspension);
        BindCategoryBtn("BtnTyres", PartCategory.Tyres);
        BindCategoryBtn("BtnBrakes", PartCategory.Brakes);
        BindCategoryBtn("BtnSidepods", PartCategory.Sidepods);
        BindCategoryBtn("BtnFloor", PartCategory.Floor);

        if (CircuitSelectionController.ChosenCircuit != null)
        {
            var circuit = CircuitSelectionController.ChosenCircuit;
            if (circuitBannerName != null)
                circuitBannerName.text = circuit.circuitName.ToUpper();
            if (circuitBannerFlag != null && circuit.flagSprite != null)
                circuitBannerFlag.style.backgroundImage = new StyleBackground(circuit.flagSprite);

            // Add tags
            if (circuitBannerTags != null)
            {
                circuitBannerTags.Clear();
                foreach (string tag in circuit.tags)
                {
                    var tagEl = new Label(tag);
                    tagEl.AddToClassList("banner-tag");
                    circuitBannerTags.Add(tagEl);
                }
            }
        }

        var evaluateBtn = root.Q<Button>("BtnEvaluate");
        if (evaluateBtn != null)
        {
            evaluateBtn.clicked += OnEvaluate;
        }
        else
            Debug.LogError("BtnEvaluate NOT FOUND");

        var resetBtn = root.Q<Button>("BtnReset");
        if (resetBtn != null)
            resetBtn.clicked += OnReset;
        else
            Debug.LogError("BtnReset NOT FOUND");

        // Show intro on start, hide part list
        if (introBox != null) introBox.style.display = DisplayStyle.Flex;
        if (partListContainer != null) partListContainer.style.display = DisplayStyle.None;
        if (selectedCategoryLabel != null) selectedCategoryLabel.style.display = DisplayStyle.None;
        if (categoryCountLabel != null) categoryCountLabel.style.display = DisplayStyle.None;

        UpdateStats();
    }

    void BindCategoryBtn(string btnName, PartCategory category)
    {
        var btn = root.Q<Button>(btnName);
        if (btn == null) return;
        btn.RegisterCallback<ClickEvent>(_ =>
        {
            // Hide intro, show parts
            if (introBox != null) introBox.style.display = DisplayStyle.None;
            if (partListContainer != null) partListContainer.style.display = DisplayStyle.Flex;
            if (selectedCategoryLabel != null) selectedCategoryLabel.style.display = DisplayStyle.Flex;
            if (categoryCountLabel != null) categoryCountLabel.style.display = DisplayStyle.Flex;

            ShowCategory(category);
        });
    }

    void ShowCategory(PartCategory category)
    {
        currentCategory = category;

        if (selectedCategoryLabel != null)
            selectedCategoryLabel.text = category.ToString().ToUpper();

        // Count parts in category
        int count = 0;
        foreach (var p in allParts)
            if (p != null && p.category == category) count++;

        if (categoryCountLabel != null)
            categoryCountLabel.text = $"{count} OPTIONS AVAILABLE";

        // Update active button
        string[] allBtns = {
            "BtnFrontWing","BtnRearWing","BtnEngine","BtnSuspension",
            "BtnTyres","BtnBrakes","BtnSidepods","BtnFloor"
        };
        foreach (var name in allBtns)
            root.Q<Button>(name)?.RemoveFromClassList("cat-btn--active");
        root.Q<Button>("Btn" + category.ToString())?.AddToClassList("cat-btn--active");

        // Build part cards
        partListContainer.Clear();

        foreach (CarPart part in allParts)
        {
            if (part == null || part.category != category) continue;

            bool selected = IsSelected(part);

            var card = new VisualElement();
            card.AddToClassList("part-card");
            if (selected) card.AddToClassList("part-card--selected");

            // Top row: icon + name + selected badge
            var topRow = new VisualElement();
            topRow.AddToClassList("card-top-row");

            var iconBox = new Image();
            iconBox.AddToClassList("part-icon");
            Sprite iconSprite = GetCategoryIcon(category);
            if (iconSprite != null)
                iconBox.sprite = iconSprite;

            var nameCol = new VisualElement();
            nameCol.AddToClassList("name-col");

            var nameLabel = new Label(part.partName);
            nameLabel.AddToClassList("part-name");

            var descLabel = new Label(part.description);
            descLabel.AddToClassList("part-desc");

            nameCol.Add(nameLabel);
            nameCol.Add(descLabel);

            topRow.Add(iconBox);
            topRow.Add(nameCol);

            if (selected)
            {
                var badge = new Label("● EQUIPPED");
                badge.AddToClassList("equipped-badge");
                topRow.Add(badge);
            }

            card.Add(topRow);

            // Divider
            var divider = new VisualElement();
            divider.AddToClassList("card-divider");
            card.Add(divider);

            // Stats row
            var statsRow = new VisualElement();
            statsRow.AddToClassList("stats-row");
            if (part.downforce > 0) statsRow.Add(MakeStat("DOWNFORCE", "↓", part.downforce, false));
            if (part.drag > 0) statsRow.Add(MakeStat("DRAG", "→", part.drag, true));
            if (part.weight > 0) statsRow.Add(MakeStat("WEIGHT", "⊕", part.weight, false));
            if (part.power > 0) statsRow.Add(MakeStat("POWER", "⚡", part.power, false));
            statsRow.Add(MakeStat("RELIABILITY", "◎", part.reliability, false));
            card.Add(statsRow);

            // Select button
            var selectBtn = new Button();
            selectBtn.AddToClassList("select-btn");
            selectBtn.text = selected ? "EQUIPPED" : "SELECT PART →";
            if (selected) selectBtn.AddToClassList("select-btn--selected");

            CarPart captured = part;
            selectBtn.RegisterCallback<ClickEvent>(_ =>
            {
                GameManager.Instance.SelectPart(captured);
                ShowCategory(currentCategory);
                UpdateStats();
                UpdateProgressBar();
            });

            card.Add(selectBtn);
            partListContainer.Add(card);
        }
    }
    VisualElement MakeStat(string label, string icon, float value, bool isNegative)
    {
        var chip = new VisualElement();
        chip.AddToClassList("stat-chip");
        if (isNegative) chip.AddToClassList("stat-chip--negative");

        var row = new VisualElement();
        row.AddToClassList("stat-chip-row");

        var iconLbl = new Label(icon);
        iconLbl.AddToClassList("stat-chip-icon");

        var lbl = new Label($"{label} {value:0}");
        lbl.AddToClassList("stat-chip-label");

        row.Add(iconLbl);
        row.Add(lbl);
        chip.Add(row);
        return chip;
    }
    bool IsSelected(CarPart part)
    {
        if (GameManager.Instance == null) return false;
        var b = GameManager.Instance.currentBuild;
        if (b == null) return false;
        return part == b.frontWing || part == b.rearWing ||
               part == b.engine || part == b.suspension ||
               part == b.tyres || part == b.brakes ||
               part == b.sidepods || part == b.floor;
    }

    void UpdateStats()
    {
        if (GameManager.Instance == null) return;

        bool complete = GameManager.Instance.currentBuild.IsComplete();

        if (buildStatusLabel != null)
        {
            int selected = CountSelectedParts();
            buildStatusLabel.text = complete
                ? "✓ Build complete — ready to evaluate"
                : $"{selected}/8 parts selected";
        }

        if (!complete)
        {
            SetStatLabels("--", "--", "--", "--", "--");
            return;
        }
    }

    int CountSelectedParts()
    {
        if (GameManager.Instance == null) return 0;
        var b = GameManager.Instance.currentBuild;
        int count = 0;
        if (b.frontWing != null) count++;
        if (b.rearWing != null) count++;
        if (b.engine != null) count++;
        if (b.suspension != null) count++;
        if (b.tyres != null) count++;
        if (b.brakes != null) count++;
        if (b.sidepods != null) count++;
        if (b.floor != null) count++;
        return count;
    }

    void SetStatLabels(string eff, string lap, string spd, string cor, string rel)
    {
        if (efficiencyLabel != null) efficiencyLabel.text = eff;
        if (lapTimeLabel != null) lapTimeLabel.text = lap;
        if (speedLabel != null) speedLabel.text = spd;
        if (corneringLabel != null) corneringLabel.text = cor;
        if (reliabilityLabel != null) reliabilityLabel.text = rel;
    }

    void OnEvaluate()
    {
        Debug.Log("OnEvaluate called!");

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance is null");
            return;
        }

        var result = GameManager.Instance.EvaluateBuild();

        if (!result.isValid)
        {
            if (buildStatusLabel != null)
                buildStatusLabel.text = $"⚠ Incomplete — {CountSelectedParts()}/8 parts selected";
            return;
        }

        Debug.Log($"Setting efficiency to {result.efficiencyScore}");

        if (efficiencyLabel != null) efficiencyLabel.text = $"{result.efficiencyScore}/100";
        if (lapTimeLabel != null) lapTimeLabel.text = $"{result.lapTimeSeconds}s";
        if (speedLabel != null) speedLabel.text = $"{result.speedScore}";
        if (corneringLabel != null) corneringLabel.text = $"{result.corneringScore}";
        if (reliabilityLabel != null) reliabilityLabel.text = $"{result.reliability}";
        if (circuitRatingLabel != null)
            circuitRatingLabel.text = result.circuitRating;

        if (buildStatusLabel != null)
            buildStatusLabel.text = "✓ Build evaluated!";

        UpdateProgressBar();
    }

    void OnReset()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.ResetBuild();
        UpdateStats();
        UpdateProgressBar();
        if (currentCategory != PartCategory.FrontWing)
            ShowCategory(currentCategory);

        if (buildStatusLabel != null)
            buildStatusLabel.text = "Build reset — start fresh";
    }

    void UpdateProgressBar()
    {
        int count = CountSelectedParts();

        if (progressCount != null)
            progressCount.text = $"{count}/8";

        if (progressFill != null)
            progressFill.style.width = new StyleLength(new Length((count / 8f) * 100f, LengthUnit.Percent));

        // Update sidebar button done states
        UpdateSidebarStatus();
    }

    void UpdateSidebarStatus()
    {
        if (GameManager.Instance == null) return;
        var b = GameManager.Instance.currentBuild;

        SetBtnStatus("BtnFrontWing", "StatusFrontWing", b.frontWing);
        SetBtnStatus("BtnRearWing", "StatusRearWing", b.rearWing);
        SetBtnStatus("BtnEngine", "StatusEngine", b.engine);
        SetBtnStatus("BtnSuspension", "StatusSuspension", b.suspension);
        SetBtnStatus("BtnTyres", "StatusTyres", b.tyres);
        SetBtnStatus("BtnBrakes", "StatusBrakes", b.brakes);
        SetBtnStatus("BtnSidepods", "StatusSidepods", b.sidepods);
        SetBtnStatus("BtnFloor", "StatusFloor", b.floor);
    }

    void SetBtnStatus(string btnName, string statusName, CarPart part)
    {
        var btn = root.Q<Button>(btnName);
        var statusLabel = root.Q<Label>(statusName);

        if (btn == null) return;

        if (part != null)
        {
            btn.AddToClassList("cat-btn--done");
            if (statusLabel != null) statusLabel.text = "DONE";
        }
        else
        {
            btn.RemoveFromClassList("cat-btn--done");
            if (statusLabel != null) statusLabel.text = "——";
        }
    }

    void ApplyFontToElements()
    {
        if (orbitronFont != null)
        {
            SetFont(root.Q<Label>("AppTitle"), orbitronFont);
            SetFont(root.Q<Label>("CategoryLabel"), orbitronFont);
            SetFont(root.Q<Label>("BuildTitle"), orbitronFont);
            SetFont(root.Q<Label>("IntroTitle"), orbitronFont);
            SetFont(root.Q<Label>("ProgressCount"), orbitronFont);
        }

        if (exo2Font != null)
        {
            foreach (var el in root.Query<Label>(className: "part-name").ToList())
                SetFont(el, exo2Font);
            foreach (var el in root.Query<Label>(className: "part-desc").ToList())
                SetFont(el, exo2Font);
            foreach (var el in root.Query<Label>(className: "btn-lbl").ToList())
                SetFont(el, exo2Font);
        }
    }

    void SetFont(Label label, Font font)
    {
        if (label == null || font == null) return;
        label.style.unityFont = new StyleFont(font);
    }
}