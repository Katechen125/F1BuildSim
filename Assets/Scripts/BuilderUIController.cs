using UnityEngine;
using UnityEngine.UIElements;

public class BuilderUIController : MonoBehaviour
{
    [Header("UI Document")]
    public UIDocument uiDocument;

    [Header("All Parts")]
    public CarPart[] allParts;

    [Header("Fonts")]
    public Font orbitronFont;
    public Font exo2Font;

    [Header("Scene References")]
    public GameObject circuitSelectorObject;

    [Header("Part Icons")]
    public Sprite iconFrontWing;
    public Sprite iconRearWing;
    public Sprite iconEngine;
    public Sprite iconSuspension;
    public Sprite iconTyres;
    public Sprite iconBrakes;
    public Sprite iconSidepods;
    public Sprite iconFloor;

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
    private Label circuitRatingLabel;
    private VisualElement introBox;
    private VisualElement progressFill;
    private Label progressCount;
    private VisualElement circuitBannerFlag;
    private Label circuitBannerName;
    private VisualElement circuitBannerTags;

    // Modal references
    private VisualElement resultsModal;
    private Label modalScore;
    private Label modalRating;
    private Label modalLapTime;
    private Label modalSpeed;
    private Label modalCornering;
    private Label modalCircuitName;
    private VisualElement modalFlag;
    private VisualElement modalDebriefItems;

    private PartCategory currentCategory = PartCategory.FrontWing;

    void OnEnable()
    {
        root = uiDocument.rootVisualElement;

        partListContainer = root.Q<VisualElement>("PartList");
        selectedCategoryLabel = root.Q<Label>("CategoryLabel");
        categoryCountLabel = root.Q<Label>("CategoryCount");
        efficiencyLabel = root.Q<Label>("EfficiencyValue");
        lapTimeLabel = root.Q<Label>("LapTimeValue");
        speedLabel = root.Q<Label>("SpeedValue");
        corneringLabel = root.Q<Label>("CorneringValue");
        reliabilityLabel = root.Q<Label>("ReliabilityValue");
        buildStatusLabel = root.Q<Label>("BuildStatus");
        circuitRatingLabel = root.Q<Label>("CircuitRating");
        introBox = root.Q<VisualElement>("IntroBox");
        progressFill = root.Q<VisualElement>("ProgressFill");
        progressCount = root.Q<Label>("ProgressCount");
        circuitBannerFlag = root.Q<VisualElement>("CircuitBannerFlag");
        circuitBannerName = root.Q<Label>("CircuitBannerName");
        circuitBannerTags = root.Q<VisualElement>("CircuitBannerTags");

        // Modal references
        resultsModal = root.Q<VisualElement>("ResultsModal");
        modalScore = root.Q<Label>("ModalScore");
        modalRating = root.Q<Label>("ModalRating");
        modalLapTime = root.Q<Label>("ModalLapTime");
        modalSpeed = root.Q<Label>("ModalSpeed");
        modalCornering = root.Q<Label>("ModalCornering");
        modalCircuitName = root.Q<Label>("ModalCircuitName");
        modalFlag = root.Q<VisualElement>("ModalFlag");
        modalDebriefItems = root.Q<VisualElement>("ModalDebriefItems");

        // Category buttons
        BindCategoryBtn("BtnFrontWing", PartCategory.FrontWing);
        BindCategoryBtn("BtnRearWing", PartCategory.RearWing);
        BindCategoryBtn("BtnEngine", PartCategory.Engine);
        BindCategoryBtn("BtnSuspension", PartCategory.Suspension);
        BindCategoryBtn("BtnTyres", PartCategory.Tyres);
        BindCategoryBtn("BtnBrakes", PartCategory.Brakes);
        BindCategoryBtn("BtnSidepods", PartCategory.Sidepods);
        BindCategoryBtn("BtnFloor", PartCategory.Floor);

        // Evaluate and reset
        var evaluateBtn = root.Q<Button>("BtnEvaluate");
        if (evaluateBtn != null)
            evaluateBtn.clicked += OnEvaluate;

        // Modal buttons
        var closeBtn = root.Q<Button>("BtnModalClose");
        if (closeBtn != null)
            closeBtn.clicked += () =>
            {
                if (resultsModal != null)
                    resultsModal.style.display = DisplayStyle.None;
            };

        var newCircuitBtn = root.Q<Button>("BtnModalNewCircuit");
        if (newCircuitBtn != null)
            newCircuitBtn.clicked += () =>
            {
                GameManager.Instance?.ResetBuild();
                if (resultsModal != null)
                    resultsModal.style.display = DisplayStyle.None;
                gameObject.SetActive(false);
                if (circuitSelectorObject != null)
                    circuitSelectorObject.SetActive(true);
            };

        // Show intro on start
        if (introBox != null) introBox.style.display = DisplayStyle.Flex;
        if (partListContainer != null) partListContainer.style.display = DisplayStyle.None;
        if (selectedCategoryLabel != null) selectedCategoryLabel.style.display = DisplayStyle.None;
        if (categoryCountLabel != null) categoryCountLabel.style.display = DisplayStyle.None;

        // Show circuit banner
        if (CircuitSelectionController.ChosenCircuit != null)
        {
            var circuit = CircuitSelectionController.ChosenCircuit;
            if (circuitBannerName != null)
                circuitBannerName.text = circuit.circuitName.ToUpper();
            if (circuitBannerFlag != null && circuit.flagSprite != null)
                circuitBannerFlag.style.backgroundImage = new StyleBackground(circuit.flagSprite);
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

        UpdateStats();
        UpdateProgressBar();
        ApplyFonts();
    }

    void BindCategoryBtn(string btnName, PartCategory category)
    {
        var btn = root.Q<Button>(btnName);
        if (btn == null) return;
        btn.RegisterCallback<ClickEvent>(_ =>
        {
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

        int count = 0;
        foreach (var p in allParts)
            if (p != null && p.category == category) count++;

        if (categoryCountLabel != null)
            categoryCountLabel.text = $"{count} OPTIONS AVAILABLE";

        string[] allBtns = {
            "BtnFrontWing","BtnRearWing","BtnEngine","BtnSuspension",
            "BtnTyres","BtnBrakes","BtnSidepods","BtnFloor"
        };
        foreach (var name in allBtns)
            root.Q<Button>(name)?.RemoveFromClassList("cat-btn--active");
        root.Q<Button>("Btn" + category.ToString())?.AddToClassList("cat-btn--active");

        partListContainer.Clear();

        foreach (CarPart part in allParts)
        {
            if (part == null || part.category != category) continue;

            bool selected = IsSelected(part);

            var card = new VisualElement();
            card.AddToClassList("part-card");
            if (selected) card.AddToClassList("part-card--selected");

            var topRow = new VisualElement();
            topRow.AddToClassList("card-top-row");

            var iconBox = new Image();
            iconBox.AddToClassList("part-icon");
            Sprite iconSprite = GetCategoryIcon(category);
            if (iconSprite != null) iconBox.sprite = iconSprite;

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

            var divider = new VisualElement();
            divider.AddToClassList("card-divider");
            card.Add(divider);

            var statsRow = new VisualElement();
            statsRow.AddToClassList("stats-row");
            if (part.downforce > 0) statsRow.Add(MakeStat("DOWNFORCE", "↓", part.downforce, false));
            if (part.drag > 0) statsRow.Add(MakeStat("DRAG", "→", part.drag, true));
            if (part.weight > 0) statsRow.Add(MakeStat("WEIGHT", "⊕", part.weight, false));
            if (part.power > 0) statsRow.Add(MakeStat("POWER", "⚡", part.power, false));
            statsRow.Add(MakeStat("RELIABILITY", "◎", part.reliability, false));
            card.Add(statsRow);

            var selectBtn = new Button();
            selectBtn.AddToClassList("select-btn");
            selectBtn.text = selected ? "EQUIPPED" : "SELECT PART →";
            if (selected) selectBtn.AddToClassList("select-btn--selected");

            CarPart captured = part;
            selectBtn.clicked += () =>
            {
                GameManager.Instance.SelectPart(captured);
                ShowCategory(currentCategory);
                UpdateStats();
                UpdateProgressBar();
            };

            card.Add(selectBtn);
            partListContainer.Add(card);
        }
    }

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
            if (circuitRatingLabel != null) circuitRatingLabel.text = "--";
        }
    }

    void UpdateProgressBar()
    {
        int count = CountSelectedParts();

        if (progressCount != null)
            progressCount.text = $"{count}/8";

        if (progressFill != null)
            progressFill.style.width = new StyleLength(
                new Length((count / 8f) * 100f, LengthUnit.Percent));

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
        if (GameManager.Instance == null) return;

        var result = GameManager.Instance.EvaluateBuild();

        if (!result.isValid)
        {
            if (buildStatusLabel != null)
                buildStatusLabel.text = $"⚠ Incomplete — {CountSelectedParts()}/8 parts selected";
            return;
        }

        SetStatLabels(
            $"{result.efficiencyScore}/100",
            $"{result.lapTimeSeconds}s",
            $"{result.speedScore}",
            $"{result.corneringScore}",
            $"{result.reliability}"
        );

        if (circuitRatingLabel != null)
            circuitRatingLabel.text = result.circuitRating;

        if (buildStatusLabel != null)
            buildStatusLabel.text = "✓ Build evaluated!";

        UpdateProgressBar();
        ShowResultsModal(result);
    }

    void ShowResultsModal(PerformanceResult result)
    {
        if (resultsModal == null) return;
        resultsModal.style.display = DisplayStyle.Flex;

        var circuit = CircuitSelectionController.ChosenCircuit;

        if (modalCircuitName != null && circuit != null)
            modalCircuitName.text = circuit.circuitName.ToUpper();
        if (modalFlag != null && circuit?.flagSprite != null)
            modalFlag.style.backgroundImage = new StyleBackground(circuit.flagSprite);

        if (modalScore != null) modalScore.text = $"{result.efficiencyScore}";
        if (modalRating != null) modalRating.text = result.circuitRating;
        if (modalLapTime != null) modalLapTime.text = $"{result.lapTimeSeconds}s";
        if (modalSpeed != null) modalSpeed.text = $"{result.speedScore}";
        if (modalCornering != null) modalCornering.text = $"{result.corneringScore}";

        SetModalBar("ModalBarDownforce", "ModalBarDownforceVal", result.aeroEfficiency);
        SetModalBar("ModalBarSpeed", "ModalBarSpeedVal", result.speedScore);
        SetModalBar("ModalBarCornering", "ModalBarCorneringVal", result.corneringScore);
        SetModalBar("ModalBarReliability", "ModalBarReliabilityVal", result.reliability);

        PopulateModalDebrief(result, circuit);
    }

    void SetModalBar(string barName, string valName, float value)
    {
        var bar = root.Q<VisualElement>(barName);
        var val = root.Q<Label>(valName);

        if (bar != null)
        {
            bar.style.width = new StyleLength(
                new Length(Mathf.Clamp(value, 0f, 100f), LengthUnit.Percent));
            bar.RemoveFromClassList("modal-bar-fill--good");
            bar.RemoveFromClassList("modal-bar-fill--warn");
            if (value >= 85f) bar.AddToClassList("modal-bar-fill--good");
            else if (value < 60f) bar.AddToClassList("modal-bar-fill--warn");
        }
        if (val != null) val.text = $"{value:0}";
    }

    void PopulateModalDebrief(PerformanceResult result, Circuit circuit)
    {
        if (modalDebriefItems == null) return;
        modalDebriefItems.Clear();

        var tips = new System.Collections.Generic.List<string>();

        if (result.efficiencyScore >= 85f)
            tips.Add("Outstanding build. Your setup matches this circuit perfectly.");
        else if (result.efficiencyScore >= 70f)
            tips.Add("Solid build with room to improve. Focus on your weakest stat.");
        else
            tips.Add("This setup is struggling here. Try a different aero configuration.");

        if (result.aeroEfficiency < 60f)
            tips.Add("Too much drag for the downforce generated. Consider a lower drag package.");
        else if (result.aeroEfficiency > 85f)
            tips.Add("Excellent aero efficiency. Your lift-to-drag ratio is well optimised.");

        if (result.reliability < 70f)
            tips.Add("Reliability is a concern. High stress components risk failure over race distance.");

        if (result.speedScore < 60f)
            tips.Add("Top speed is limited. A higher performance power unit would help.");

        if (circuit != null && circuit.tyreStress > 80f && result.reliability < 75f)
            tips.Add("This circuit is hard on tyres. A more durable compound is recommended.");

        foreach (string tip in tips)
        {
            var item = new VisualElement();
            item.AddToClassList("modal-debrief-item");

            var dot = new VisualElement();
            dot.AddToClassList("modal-debrief-dot");

            var text = new Label(tip);
            text.AddToClassList("modal-debrief-text");

            item.Add(dot);
            item.Add(text);
            modalDebriefItems.Add(item);
        }
    }

    void OnReset()
    {
        GameManager.Instance?.ResetBuild();
        UpdateStats();
        UpdateProgressBar();
        if (currentCategory != PartCategory.FrontWing)
            ShowCategory(currentCategory);
        if (buildStatusLabel != null)
            buildStatusLabel.text = "Build reset — start fresh";
    }

    void ApplyFonts()
    {
        if (orbitronFont != null)
        {
            SetFont(root.Q<Label>("AppTitle"), orbitronFont);
            SetFont(root.Q<Label>("CategoryLabel"), orbitronFont);
            SetFont(root.Q<Label>("BuildTitle"), orbitronFont);
            SetFont(root.Q<Label>("IntroTitle"), orbitronFont);
            SetFont(root.Q<Label>("ProgressCount"), orbitronFont);
            SetFont(root.Q<Label>("ModalTitle"), orbitronFont);
            SetFont(root.Q<Label>("ModalScore"), orbitronFont);
        }

        if (exo2Font != null)
        {
            foreach (var el in root.Query<Label>(className: "part-name").ToList())
                SetFont(el, exo2Font);
            foreach (var el in root.Query<Label>(className: "part-desc").ToList())
                SetFont(el, exo2Font);
            foreach (var el in root.Query<Label>(className: "btn-lbl").ToList())
                SetFont(el, exo2Font);
            foreach (var el in root.Query<Label>(className: "modal-debrief-text").ToList())
                SetFont(el, exo2Font);
        }
    }

    void SetFont(Label label, Font font)
    {
        if (label == null || font == null) return;
        label.style.unityFont = new StyleFont(font);
    }
}