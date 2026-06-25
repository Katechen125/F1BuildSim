using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class CircuitSelectionController : MonoBehaviour
{
    [Header("UI Document")]
    public UIDocument uiDocument;

    [Header("All Circuits")]
    public Circuit[] allCircuits;

    [Header("Fonts")]
    public Font orbitronFont;
    public Font exo2Font;

    [Header("Builder Reference")]
    public GameObject builderObject;

    private VisualElement root;
    private VisualElement circuitGrid;
    private VisualElement trackImage;
    private Label circuitNameLabel;
    private Label circuitSubLabel;
    private Label lapLengthLabel;
    private Label raceLapsLabel;
    private Label topSpeedLabel;
    private VisualElement tagsContainer;
    private Button confirmBtn;
    private VisualElement circuitFlagLarge;

    private Circuit selectedCircuit;

    public static Circuit ChosenCircuit;

    void OnEnable()
    {
        root = uiDocument.rootVisualElement;

        circuitGrid = root.Q<VisualElement>("CircuitGrid");
        trackImage = root.Q<VisualElement>("TrackImage");
        circuitNameLabel = root.Q<Label>("CircuitName");
        circuitSubLabel = root.Q<Label>("CircuitSub");
        lapLengthLabel = root.Q<Label>("LapLength");
        raceLapsLabel = root.Q<Label>("RaceLaps");
        topSpeedLabel = root.Q<Label>("TopSpeed");
        tagsContainer = root.Q<VisualElement>("TagsContainer");
        confirmBtn = root.Q<Button>("BtnConfirm");
        circuitFlagLarge = root.Q<VisualElement>("CircuitFlagImg");

        
        ApplyFonts();

        if (confirmBtn != null)
            confirmBtn.clicked += OnConfirm;

        BuildCircuitGrid();

        if (allCircuits.Length > 0)
            SelectCircuit(allCircuits[0]);
    }

    void BuildCircuitGrid()
    {
        if (circuitGrid == null) return;
        circuitGrid.Clear();

        foreach (Circuit circuit in allCircuits)
        {
            var card = new VisualElement();
            card.AddToClassList("circuit-card");

            // Flag as image instead of emoji label
            var flagImg = new Image();
            flagImg.AddToClassList("circuit-flag-img");
            if (circuit.flagSprite != null)
                flagImg.sprite = circuit.flagSprite;

            var nameCol = new VisualElement();
            nameCol.AddToClassList("circuit-name-col");

            var name = new Label(circuit.circuitName);
            name.AddToClassList("circuit-card-name");

            var type = new Label(circuit.circuitType.ToUpper());
            type.AddToClassList("circuit-card-type");

            nameCol.Add(name);
            nameCol.Add(type);

            card.Add(flagImg);
            card.Add(nameCol);

            Circuit captured = circuit;
            card.RegisterCallback<ClickEvent>(_ => SelectCircuit(captured));

            circuitGrid.Add(card);
        }
    }
    void SelectCircuit(Circuit circuit)
    {
        selectedCircuit = circuit;

        // Update card styles
        foreach (var card in circuitGrid.Children())
        {
            card.RemoveFromClassList("circuit-card--selected");
            var nameLbl = card.Q<Label>(className: "circuit-card-name");
            if (nameLbl != null && nameLbl.text == circuit.circuitName)
                card.AddToClassList("circuit-card--selected");
        }

        // Update track image
        if (trackImage != null && circuit.trackLayout != null)
            trackImage.style.backgroundImage = new StyleBackground(circuit.trackLayout);

        // Update info
        // Update info
        if (circuitNameLabel != null)
            circuitNameLabel.text = circuit.circuitName.ToUpper();
        if (circuitSubLabel != null)
            circuitSubLabel.text = $"{circuit.country.ToUpper()} · {circuit.region.ToUpper()}";
        if (lapLengthLabel != null)
            lapLengthLabel.text = $"{circuit.lapLengthKm} KM";
        if (raceLapsLabel != null)
            raceLapsLabel.text = $"{circuit.raceLaps}";
        if (topSpeedLabel != null)
            topSpeedLabel.text = $"{circuit.topSpeedKmh} KM/H";
        if (circuitFlagLarge != null && circuit.flagSprite != null)
            circuitFlagLarge.style.backgroundImage = new StyleBackground(circuit.flagSprite);

        // Update tags
        if (tagsContainer != null)
        {
            tagsContainer.Clear();
            foreach (string tag in circuit.tags)
            {
                var tagEl = new Label(tag);
                tagEl.AddToClassList("circuit-tag");
                tagsContainer.Add(tagEl);
            }
        }
    }

    void OnConfirm()
    {
        if (selectedCircuit == null) return;
        ChosenCircuit = selectedCircuit;
        Debug.Log($"Circuit selected: {selectedCircuit.circuitName}");

        gameObject.SetActive(false);

        if (builderObject != null)
        {
            builderObject.SetActive(true);
            Debug.Log("Builder activated");
        }
        else
            Debug.LogError("Builder reference not set!");
    }
    void ApplyFonts()
    {
        if (orbitronFont != null)
        {
            SetFont(root.Q<Label>("AppTitle"), orbitronFont);
            SetFont(root.Q<Label>("ScreenTitle"), orbitronFont);
            SetFont(root.Q<Label>("CircuitName"), orbitronFont);
            SetFont(root.Q<Label>("GridLabel"), orbitronFont);
            SetFont(root.Q<Label>("HeaderRight"), orbitronFont);
        }

        if (exo2Font != null)
        {
            SetFont(root.Q<Label>("CircuitSub"), exo2Font);
            SetFont(root.Q<Label>("LapLength"), exo2Font);
            SetFont(root.Q<Label>("RaceLaps"), exo2Font);
            SetFont(root.Q<Label>("TopSpeed"), exo2Font);

            foreach (var lbl in root.Query<Label>(className: "circuit-card-name").ToList())
                SetFont(lbl, exo2Font);
            foreach (var lbl in root.Query<Label>(className: "circuit-card-type").ToList())
                SetFont(lbl, exo2Font);
            foreach (var lbl in root.Query<Label>(className: "circuit-tag").ToList())
                SetFont(lbl, exo2Font);
            foreach (var lbl in root.Query<Label>(className: "stat-lbl").ToList())
                SetFont(lbl, exo2Font);
            foreach (var lbl in root.Query<Label>(className: "stat-val").ToList())
                SetFont(lbl, exo2Font);
        }
    }

    void SetFont(Label label, Font font)
    {
        if (label == null || font == null) return;
        label.style.unityFont = new StyleFont(font);
    }
}