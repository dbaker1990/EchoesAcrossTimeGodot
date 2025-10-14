#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class CharacterBuilderDock : Control
{
    // UI Elements
    private ScrollContainer mainScroll;
    private VBoxContainer mainContainer;
    
    // Basic Info
    private LineEdit characterIdEdit;
    private LineEdit displayNameEdit;
    private TextEdit descriptionEdit;
    private OptionButton characterTypeOption;
    private OptionButton characterClassOption;
    private CheckBox isBossCheck;
    
    // Base Stats
    private SpinBox levelSpin;
    private SpinBox maxHPSpin;
    private SpinBox maxMPSpin;
    private SpinBox attackSpin;
    private SpinBox defenseSpin;
    private SpinBox magicAttackSpin;
    private SpinBox magicDefenseSpin;
    private SpinBox speedSpin;
    private SpinBox luckSpin;
    
    // Growth Rates
    private HSlider hpGrowthSlider;
    private Label hpGrowthLabel;
    private HSlider mpGrowthSlider;
    private Label mpGrowthLabel;
    private HSlider attackGrowthSlider;
    private Label attackGrowthLabel;
    private HSlider defenseGrowthSlider;
    private Label defenseGrowthLabel;
    private HSlider magicAttackGrowthSlider;
    private Label magicAttackGrowthLabel;
    private HSlider magicDefenseGrowthSlider;
    private Label magicDefenseGrowthLabel;
    private HSlider speedGrowthSlider;
    private Label speedGrowthLabel;
    private HSlider luckGrowthSlider;
    private Label luckGrowthLabel;
    
    // Experience Curve
    private OptionButton expCurveOption;
    
    // Stat Curve Graph
    private Control graphContainer;
    private StatCurveGraph statGraph;
    private OptionButton graphStatSelector;
    private CheckBox showAllStatsCheck;
    
    // Element Affinities
    private VBoxContainer affinitiesContainer;
    private Dictionary<string, OptionButton> affinityOptions = new Dictionary<string, OptionButton>();
    
    // Class Templates
    private Button applyTemplateButton;
    
    // Action Buttons
    private Button saveButton;
    private Button loadButton;
    private Button duplicateButton;
    private Button clearButton;
    
    // Current character being edited
    private EchoesAcrossTime.Database.CharacterData currentCharacter;
    private string currentPath = "";

    public override void _Ready()
    {
        Name = "Character Builder";
        CustomMinimumSize = new Vector2(450, 600);
    
        // IMPORTANT: Force layout
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        SizeFlagsVertical = SizeFlags.ExpandFill;
    
        GD.Print("=== CHARACTER BUILDER _Ready() called ===");
    
        BuildUI();
        ConnectSignals();
        CreateNewCharacter();
    
        GD.Print("=== CHARACTER BUILDER UI built successfully ===");
    }

    private void BuildUI()
    {
        // Main scroll container
        mainScroll = new ScrollContainer();
        mainScroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        mainScroll.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        AddChild(mainScroll);
        
        mainContainer = new VBoxContainer();
        mainScroll.AddChild(mainContainer);
        
        // Title
        var title = new Label();
        title.Text = "👤 Character Builder 👤";
        title.AddThemeColorOverride("font_color", new Color(1, 0.8f, 0.2f));
        title.AddThemeFontSizeOverride("font_size", 18);
        mainContainer.AddChild(title);
        
        mainContainer.AddChild(new HSeparator());
        
        // Build sections
        BuildBasicInfoSection();
        BuildClassTemplateSection();
        BuildBaseStatsSection();
        BuildGrowthRatesSection();
        BuildStatCurveGraph();
        BuildExperienceCurveSection();
        BuildElementAffinitiesSection();
        BuildActionButtons();
    }

    private void BuildBasicInfoSection()
    {
        var section = CreateSection("📝 Basic Information");
        
        characterIdEdit = CreateLineEdit("Character ID:", "hero_001");
        section.AddChild(characterIdEdit.GetParent());
        
        displayNameEdit = CreateLineEdit("Display Name:", "Hero");
        section.AddChild(displayNameEdit.GetParent());
        
        var descLabel = new Label();
        descLabel.Text = "Description:";
        section.AddChild(descLabel);
        
        descriptionEdit = new TextEdit();
        descriptionEdit.CustomMinimumSize = new Vector2(0, 60);
        descriptionEdit.PlaceholderText = "A brave hero...";
        section.AddChild(descriptionEdit);
        
        characterTypeOption = CreateOptionButton("Type:", new string[] {
            "Playable Character", "Enemy", "Boss", "NPC"
        });
        section.AddChild(characterTypeOption.GetParent());
        
        characterClassOption = CreateOptionButton("Class:", new string[] {
            "Black Mage", "White Mage", "Knight", "Thief", "Monk", "Dragoon",
            "Summoner", "Red Mage", "Samurai", "Ninja", "Bard", "Dancer"
        });
        section.AddChild(characterClassOption.GetParent());
        
        isBossCheck = CreateCheckBox("Is Boss");
        section.AddChild(isBossCheck);
        
        mainContainer.AddChild(section);
    }

    private void BuildClassTemplateSection()
    {
        var section = CreateSection("🎭 Character Class Templates");
        
        var helpLabel = new Label();
        helpLabel.Text = "Apply balanced stat templates for different character classes:";
        helpLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        section.AddChild(helpLabel);
        
        var buttonContainer = new HBoxContainer();
        
        var warriorBtn = new Button();
        warriorBtn.Text = "⚔️ Warrior";
        warriorBtn.Pressed += () => ApplyTemplate("Warrior");
        buttonContainer.AddChild(warriorBtn);
        
        var mageBtn = new Button();
        mageBtn.Text = "🔮 Mage";
        mageBtn.Pressed += () => ApplyTemplate("Mage");
        buttonContainer.AddChild(mageBtn);
        
        var rogueBtn = new Button();
        rogueBtn.Text = "🗡️ Rogue";
        rogueBtn.Pressed += () => ApplyTemplate("Rogue");
        buttonContainer.AddChild(rogueBtn);
        
        section.AddChild(buttonContainer);
        
        var buttonContainer2 = new HBoxContainer();
        
        var healerBtn = new Button();
        healerBtn.Text = "💚 Healer";
        healerBtn.Pressed += () => ApplyTemplate("Healer");
        buttonContainer2.AddChild(healerBtn);
        
        var tankBtn = new Button();
        tankBtn.Text = "🛡️ Tank";
        tankBtn.Pressed += () => ApplyTemplate("Tank");
        buttonContainer2.AddChild(tankBtn);
        
        var balancedBtn = new Button();
        balancedBtn.Text = "⚖️ Balanced";
        balancedBtn.Pressed += () => ApplyTemplate("Balanced");
        buttonContainer2.AddChild(balancedBtn);
        
        section.AddChild(buttonContainer2);
        
        mainContainer.AddChild(section);
    }

    private void BuildBaseStatsSection()
    {
        var section = CreateSection("📊 Base Stats");
        
        levelSpin = CreateSpinBox("Level:", 1, 100, 1);
        section.AddChild(levelSpin.GetParent());
        
        maxHPSpin = CreateSpinBox("Max HP:", 1, 9999, 100);
        section.AddChild(maxHPSpin.GetParent());
        
        maxMPSpin = CreateSpinBox("Max MP:", 1, 9999, 50);
        section.AddChild(maxMPSpin.GetParent());
        
        attackSpin = CreateSpinBox("Attack:", 1, 999, 10);
        section.AddChild(attackSpin.GetParent());
        
        defenseSpin = CreateSpinBox("Defense:", 1, 999, 10);
        section.AddChild(defenseSpin.GetParent());
        
        magicAttackSpin = CreateSpinBox("Magic Attack:", 1, 999, 10);
        section.AddChild(magicAttackSpin.GetParent());
        
        magicDefenseSpin = CreateSpinBox("Magic Defense:", 1, 999, 10);
        section.AddChild(magicDefenseSpin.GetParent());
        
        speedSpin = CreateSpinBox("Speed:", 1, 999, 10);
        section.AddChild(speedSpin.GetParent());
        
        luckSpin = CreateSpinBox("Luck:", 1, 999, 10);
        section.AddChild(luckSpin.GetParent());
        
        mainContainer.AddChild(section);
    }

    private void BuildGrowthRatesSection()
    {
        var section = CreateSection("📈 Growth Rates (% increase per level)");
        
        var helpLabel = new Label();
        helpLabel.Text = "Adjust how quickly stats grow with each level:";
        helpLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        section.AddChild(helpLabel);
        
        // HP Growth
        var hpRow = CreateGrowthRateRow("HP Growth:", out hpGrowthSlider, out hpGrowthLabel);
        section.AddChild(hpRow);
        
        // MP Growth
        var mpRow = CreateGrowthRateRow("MP Growth:", out mpGrowthSlider, out mpGrowthLabel);
        section.AddChild(mpRow);
        
        // Attack Growth
        var atkRow = CreateGrowthRateRow("Attack Growth:", out attackGrowthSlider, out attackGrowthLabel);
        section.AddChild(atkRow);
        
        // Defense Growth
        var defRow = CreateGrowthRateRow("Defense Growth:", out defenseGrowthSlider, out defenseGrowthLabel);
        section.AddChild(defRow);
        
        // Magic Attack Growth
        var matkRow = CreateGrowthRateRow("Magic Attack Growth:", out magicAttackGrowthSlider, out magicAttackGrowthLabel);
        section.AddChild(matkRow);
        
        // Magic Defense Growth
        var mdefRow = CreateGrowthRateRow("Magic Defense Growth:", out magicDefenseGrowthSlider, out magicDefenseGrowthLabel);
        section.AddChild(mdefRow);
        
        // Speed Growth
        var spdRow = CreateGrowthRateRow("Speed Growth:", out speedGrowthSlider, out speedGrowthLabel);
        section.AddChild(spdRow);
        
        // Luck Growth
        var lckRow = CreateGrowthRateRow("Luck Growth:", out luckGrowthSlider, out luckGrowthLabel);
        section.AddChild(lckRow);
        
        mainContainer.AddChild(section);
    }

    private VBoxContainer CreateGrowthRateRow(string labelText, out HSlider slider, out Label valueLabel)
    {
        var vbox = new VBoxContainer();
        
        var hbox = new HBoxContainer();
        
        var label = new Label();
        label.Text = labelText;
        label.CustomMinimumSize = new Vector2(150, 0);
        hbox.AddChild(label);
        
        valueLabel = new Label();
        valueLabel.Text = "5.0%";
        valueLabel.CustomMinimumSize = new Vector2(60, 0);
        valueLabel.AddThemeColorOverride("font_color", new Color(0.2f, 1, 0.2f));
        hbox.AddChild(valueLabel);
        
        vbox.AddChild(hbox);
        
        slider = new HSlider();
        slider.MinValue = 0;
        slider.MaxValue = 100; // 0% to 100% (stored as 0.00 to 1.00)
        slider.Value = 5;
        slider.Step = 0.1;
        slider.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        vbox.AddChild(slider);
        
        return vbox;
    }

    private void BuildStatCurveGraph()
    {
        var section = CreateSection("📉 Stat Growth Curve Visualization");
        
        var helpLabel = new Label();
        helpLabel.Text = "Visual representation of stat growth from Level 1 to 99:";
        helpLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        section.AddChild(helpLabel);
        
        var controlsHbox = new HBoxContainer();
        
        graphStatSelector = new OptionButton();
        graphStatSelector.AddItem("HP", 0);
        graphStatSelector.AddItem("MP", 1);
        graphStatSelector.AddItem("Attack", 2);
        graphStatSelector.AddItem("Defense", 3);
        graphStatSelector.AddItem("Magic Attack", 4);
        graphStatSelector.AddItem("Magic Defense", 5);
        graphStatSelector.AddItem("Speed", 6);
        graphStatSelector.AddItem("Luck", 7);
        graphStatSelector.ItemSelected += (index) => UpdateGraph();
        controlsHbox.AddChild(graphStatSelector);
        
        showAllStatsCheck = new CheckBox();
        showAllStatsCheck.Text = "Show All Stats";
        showAllStatsCheck.Toggled += (pressed) => UpdateGraph();
        controlsHbox.AddChild(showAllStatsCheck);
        
        section.AddChild(controlsHbox);
        
        // Graph container
        graphContainer = new Panel();
        graphContainer.CustomMinimumSize = new Vector2(400, 250);
        
        statGraph = new StatCurveGraph();
        statGraph.CustomMinimumSize = new Vector2(400, 250);
        graphContainer.AddChild(statGraph);
        
        section.AddChild(graphContainer);
        
        mainContainer.AddChild(section);
    }

    private void BuildExperienceCurveSection()
    {
        var section = CreateSection("⭐ Experience Curve");
        
        expCurveOption = CreateOptionButton("Curve Type:", new string[] {
            "Linear", "Exponential 1", "Exponential 2", "Quadratic", "Cubic", "Custom"
        });
        section.AddChild(expCurveOption.GetParent());
        
        var helpLabel = new Label();
        helpLabel.Text = "• Linear: Constant growth\n• Exponential: Steep late game\n• Quadratic: Balanced (recommended for players)\n• Cubic: Very steep late game";
        helpLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        helpLabel.AddThemeFontSizeOverride("font_size", 11);
        section.AddChild(helpLabel);
        
        mainContainer.AddChild(section);
    }

    private void BuildElementAffinitiesSection()
    {
        var section = CreateSection("🔥 Element Affinities");
        
        var helpLabel = new Label();
        helpLabel.Text = "Set character's elemental strengths and weaknesses:";
        helpLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        section.AddChild(helpLabel);
        
        affinitiesContainer = new VBoxContainer();
        
        string[] elements = new string[] { "Physical", "Fire", "Water", "Thunder", "Ice", "Earth", "Light", "Dark" };
        
        foreach (var element in elements)
        {
            var row = CreateAffinityRow(element);
            affinitiesContainer.AddChild(row);
        }
        
        section.AddChild(affinitiesContainer);
        
        mainContainer.AddChild(section);
    }

    private HBoxContainer CreateAffinityRow(string elementName)
    {
        var hbox = new HBoxContainer();
        
        var label = new Label();
        label.Text = elementName + ":";
        label.CustomMinimumSize = new Vector2(100, 0);
        hbox.AddChild(label);
        
        var optionButton = new OptionButton();
        optionButton.AddItem("Normal", 0);
        optionButton.AddItem("Weak", 1);
        optionButton.AddItem("Resist", 2);
        optionButton.AddItem("Immune", 3);
        optionButton.AddItem("Absorb", 4);
        optionButton.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        hbox.AddChild(optionButton);
        
        affinityOptions[elementName] = optionButton;
        
        return hbox;
    }

    private void BuildActionButtons()
    {
        mainContainer.AddChild(new HSeparator());
        
        var buttonContainer = new HBoxContainer();
        buttonContainer.Alignment = BoxContainer.AlignmentMode.Center;
        
        saveButton = new Button();
        saveButton.Text = "💾 Save Character";
        saveButton.Pressed += OnSaveCharacter;
        buttonContainer.AddChild(saveButton);
        
        loadButton = new Button();
        loadButton.Text = "📂 Load Character";
        loadButton.Pressed += OnLoadCharacter;
        buttonContainer.AddChild(loadButton);
        
        duplicateButton = new Button();
        duplicateButton.Text = "📋 Duplicate";
        duplicateButton.Pressed += OnDuplicateCharacter;
        buttonContainer.AddChild(duplicateButton);
        
        mainContainer.AddChild(buttonContainer);
        
        var buttonContainer2 = new HBoxContainer();
        buttonContainer2.Alignment = BoxContainer.AlignmentMode.Center;
        
        clearButton = new Button();
        clearButton.Text = "🗑️ Clear All";
        clearButton.Pressed += OnClearAll;
        buttonContainer2.AddChild(clearButton);
        
        mainContainer.AddChild(buttonContainer2);
    }

    // Helper methods for creating UI elements
    private VBoxContainer CreateSection(string title)
    {
        var section = new VBoxContainer();
        
        var titleLabel = new Label();
        titleLabel.Text = title;
        titleLabel.AddThemeColorOverride("font_color", new Color(0.3f, 0.8f, 1));
        titleLabel.AddThemeFontSizeOverride("font_size", 14);
        section.AddChild(titleLabel);
        
        var separator = new HSeparator();
        section.AddChild(separator);
        
        return section;
    }

    private LineEdit CreateLineEdit(string labelText, string placeholder = "")
    {
        var hbox = new HBoxContainer();
        
        var label = new Label();
        label.Text = labelText;
        label.CustomMinimumSize = new Vector2(150, 0);
        hbox.AddChild(label);
        
        var lineEdit = new LineEdit();
        lineEdit.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        lineEdit.PlaceholderText = placeholder;
        hbox.AddChild(lineEdit);
        
        return lineEdit;
    }

    private SpinBox CreateSpinBox(string labelText, double min, double max, double value, double step = 1)
    {
        var hbox = new HBoxContainer();
        
        var label = new Label();
        label.Text = labelText;
        label.CustomMinimumSize = new Vector2(150, 0);
        hbox.AddChild(label);
        
        var spinBox = new SpinBox();
        spinBox.MinValue = min;
        spinBox.MaxValue = max;
        spinBox.Value = value;
        spinBox.Step = step;
        spinBox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        hbox.AddChild(spinBox);
        
        return spinBox;
    }

    private OptionButton CreateOptionButton(string labelText, string[] options)
    {
        var hbox = new HBoxContainer();
        
        var label = new Label();
        label.Text = labelText;
        label.CustomMinimumSize = new Vector2(150, 0);
        hbox.AddChild(label);
        
        var optionButton = new OptionButton();
        optionButton.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        
        for (int i = 0; i < options.Length; i++)
        {
            optionButton.AddItem(options[i], i);
        }
        
        hbox.AddChild(optionButton);
        
        return optionButton;
    }

    private CheckBox CreateCheckBox(string text)
    {
        var checkBox = new CheckBox();
        checkBox.Text = text;
        return checkBox;
    }

    private void ConnectSignals()
    {
        // Connect all growth rate sliders to update the graph
        hpGrowthSlider.ValueChanged += (value) => {
            hpGrowthLabel.Text = $"{value:F1}%";
            UpdateGraph();
        };
        
        mpGrowthSlider.ValueChanged += (value) => {
            mpGrowthLabel.Text = $"{value:F1}%";
            UpdateGraph();
        };
        
        attackGrowthSlider.ValueChanged += (value) => {
            attackGrowthLabel.Text = $"{value:F1}%";
            UpdateGraph();
        };
        
        defenseGrowthSlider.ValueChanged += (value) => {
            defenseGrowthLabel.Text = $"{value:F1}%";
            UpdateGraph();
        };
        
        magicAttackGrowthSlider.ValueChanged += (value) => {
            magicAttackGrowthLabel.Text = $"{value:F1}%";
            UpdateGraph();
        };
        
        magicDefenseGrowthSlider.ValueChanged += (value) => {
            magicDefenseGrowthLabel.Text = $"{value:F1}%";
            UpdateGraph();
        };
        
        speedGrowthSlider.ValueChanged += (value) => {
            speedGrowthLabel.Text = $"{value:F1}%";
            UpdateGraph();
        };
        
        luckGrowthSlider.ValueChanged += (value) => {
            luckGrowthLabel.Text = $"{value:F1}%";
            UpdateGraph();
        };
        
        // Connect base stat changes to graph
        maxHPSpin.ValueChanged += (_) => UpdateGraph();
        maxMPSpin.ValueChanged += (_) => UpdateGraph();
        attackSpin.ValueChanged += (_) => UpdateGraph();
        defenseSpin.ValueChanged += (_) => UpdateGraph();
        magicAttackSpin.ValueChanged += (_) => UpdateGraph();
        magicDefenseSpin.ValueChanged += (_) => UpdateGraph();
        speedSpin.ValueChanged += (_) => UpdateGraph();
        luckSpin.ValueChanged += (_) => UpdateGraph();
    }

    private void UpdateGraph()
    {
        if (statGraph == null) return;
        
        var stats = CalculateStatCurves();
        
        if (showAllStatsCheck.ButtonPressed)
        {
            statGraph.SetAllStats(stats);
        }
        else
        {
            int selectedStat = graphStatSelector.Selected;
            statGraph.SetSingleStat(stats[selectedStat], GetStatName(selectedStat), GetStatColor(selectedStat));
        }
        
        statGraph.QueueRedraw();
    }

    private List<List<int>> CalculateStatCurves()
    {
        // Calculate stat progression from level 1 to 99
        var curves = new List<List<int>>();
        
        // Get base stats
        int baseHP = (int)maxHPSpin.Value;
        int baseMP = (int)maxMPSpin.Value;
        int baseAtk = (int)attackSpin.Value;
        int baseDef = (int)defenseSpin.Value;
        int baseMatk = (int)magicAttackSpin.Value;
        int baseMdef = (int)magicDefenseSpin.Value;
        int baseSpd = (int)speedSpin.Value;
        int baseLck = (int)luckSpin.Value;
        
        // Get growth rates (convert from percentage to decimal)
        float hpGrowth = (float)(hpGrowthSlider.Value / 100.0);
        float mpGrowth = (float)(mpGrowthSlider.Value / 100.0);
        float atkGrowth = (float)(attackGrowthSlider.Value / 100.0);
        float defGrowth = (float)(defenseGrowthSlider.Value / 100.0);
        float matkGrowth = (float)(magicAttackGrowthSlider.Value / 100.0);
        float mdefGrowth = (float)(magicDefenseGrowthSlider.Value / 100.0);
        float spdGrowth = (float)(speedGrowthSlider.Value / 100.0);
        float lckGrowth = (float)(luckGrowthSlider.Value / 100.0);
        
        // Initialize curves for each stat
        for (int i = 0; i < 8; i++)
        {
            curves.Add(new List<int>());
        }
        
        // Calculate for each level (1-99)
        for (int level = 1; level < 100; level++)
        {
            int currentHP = baseHP;
            int currentMP = baseMP;
            int currentAtk = baseAtk;
            int currentDef = baseDef;
            int currentMatk = baseMatk;
            int currentMdef = baseMdef;
            int currentSpd = baseSpd;
            int currentLck = baseLck;
            
            // Apply growth for each level after level 1
            for (int lv = 2; lv <= level; lv++)
            {
                currentHP += Mathf.Max(1, Mathf.RoundToInt(currentHP * hpGrowth) + 5);
                currentMP += Mathf.Max(1, Mathf.RoundToInt(currentMP * mpGrowth) + 3);
                currentAtk += Mathf.Max(1, Mathf.RoundToInt(currentAtk * atkGrowth) + 1);
                currentDef += Mathf.Max(1, Mathf.RoundToInt(currentDef * defGrowth) + 1);
                currentMatk += Mathf.Max(1, Mathf.RoundToInt(currentMatk * matkGrowth) + 1);
                currentMdef += Mathf.Max(1, Mathf.RoundToInt(currentMdef * mdefGrowth) + 1);
                currentSpd += Mathf.Max(1, Mathf.RoundToInt(currentSpd * spdGrowth));
                currentLck += Mathf.Max(1, Mathf.RoundToInt(currentLck * lckGrowth));
            }
            
            curves[0].Add(currentHP);
            curves[1].Add(currentMP);
            curves[2].Add(currentAtk);
            curves[3].Add(currentDef);
            curves[4].Add(currentMatk);
            curves[5].Add(currentMdef);
            curves[6].Add(currentSpd);
            curves[7].Add(currentLck);
        }
        
        return curves;
    }

    private string GetStatName(int index)
    {
        return index switch
        {
            0 => "HP",
            1 => "MP",
            2 => "Attack",
            3 => "Defense",
            4 => "Magic Attack",
            5 => "Magic Defense",
            6 => "Speed",
            7 => "Luck",
            _ => "Unknown"
        };
    }

    private Color GetStatColor(int index)
    {
        return index switch
        {
            0 => new Color(1, 0.2f, 0.2f),      // HP - Red
            1 => new Color(0.2f, 0.4f, 1),      // MP - Blue
            2 => new Color(1, 0.5f, 0),         // Attack - Orange
            3 => new Color(0.8f, 0.8f, 0.2f),   // Defense - Yellow
            4 => new Color(0.8f, 0.2f, 1),      // Magic Attack - Purple
            5 => new Color(0.2f, 1, 1),         // Magic Defense - Cyan
            6 => new Color(0.2f, 1, 0.2f),      // Speed - Green
            7 => new Color(1, 0.8f, 0.4f),      // Luck - Gold
            _ => Colors.White
        };
    }

    private void ApplyTemplate(string templateName)
    {
        switch (templateName)
        {
            case "Warrior":
                maxHPSpin.Value = 150;
                maxMPSpin.Value = 30;
                attackSpin.Value = 15;
                defenseSpin.Value = 12;
                magicAttackSpin.Value = 5;
                magicDefenseSpin.Value = 8;
                speedSpin.Value = 10;
                luckSpin.Value = 8;
                
                hpGrowthSlider.Value = 7;
                mpGrowthSlider.Value = 2;
                attackGrowthSlider.Value = 5;
                defenseGrowthSlider.Value = 4;
                magicAttackGrowthSlider.Value = 1;
                magicDefenseGrowthSlider.Value = 3;
                speedGrowthSlider.Value = 2;
                luckGrowthSlider.Value = 2;
                break;
                
            case "Mage":
                maxHPSpin.Value = 80;
                maxMPSpin.Value = 100;
                attackSpin.Value = 6;
                defenseSpin.Value = 7;
                magicAttackSpin.Value = 18;
                magicDefenseSpin.Value = 15;
                speedSpin.Value = 12;
                luckSpin.Value = 10;
                
                hpGrowthSlider.Value = 3;
                mpGrowthSlider.Value = 8;
                attackGrowthSlider.Value = 1;
                defenseGrowthSlider.Value = 2;
                magicAttackGrowthSlider.Value = 6;
                magicDefenseGrowthSlider.Value = 5;
                speedGrowthSlider.Value = 3;
                luckGrowthSlider.Value = 3;
                break;
                
            case "Rogue":
                maxHPSpin.Value = 110;
                maxMPSpin.Value = 50;
                attackSpin.Value = 14;
                defenseSpin.Value = 9;
                magicAttackSpin.Value = 8;
                magicDefenseSpin.Value = 8;
                speedSpin.Value = 18;
                luckSpin.Value = 15;
                
                hpGrowthSlider.Value = 4;
                mpGrowthSlider.Value = 4;
                attackGrowthSlider.Value = 5;
                defenseGrowthSlider.Value = 2;
                magicAttackGrowthSlider.Value = 2;
                magicDefenseGrowthSlider.Value = 2;
                speedGrowthSlider.Value = 6;
                luckGrowthSlider.Value = 5;
                break;
                
            case "Healer":
                maxHPSpin.Value = 100;
                maxMPSpin.Value = 90;
                attackSpin.Value = 7;
                defenseSpin.Value = 9;
                magicAttackSpin.Value = 14;
                magicDefenseSpin.Value = 16;
                speedSpin.Value = 11;
                luckSpin.Value = 12;
                
                hpGrowthSlider.Value = 4;
                mpGrowthSlider.Value = 7;
                attackGrowthSlider.Value = 1;
                defenseGrowthSlider.Value = 3;
                magicAttackGrowthSlider.Value = 4;
                magicDefenseGrowthSlider.Value = 5;
                speedGrowthSlider.Value = 3;
                luckGrowthSlider.Value = 4;
                break;
                
            case "Tank":
                maxHPSpin.Value = 180;
                maxMPSpin.Value = 40;
                attackSpin.Value = 12;
                defenseSpin.Value = 18;
                magicAttackSpin.Value = 5;
                magicDefenseSpin.Value = 14;
                speedSpin.Value = 7;
                luckSpin.Value = 8;
                
                hpGrowthSlider.Value = 8;
                mpGrowthSlider.Value = 2;
                attackGrowthSlider.Value = 3;
                defenseGrowthSlider.Value = 6;
                magicAttackGrowthSlider.Value = 1;
                magicDefenseGrowthSlider.Value = 5;
                speedGrowthSlider.Value = 1;
                luckGrowthSlider.Value = 2;
                break;
                
            case "Balanced":
                maxHPSpin.Value = 120;
                maxMPSpin.Value = 60;
                attackSpin.Value = 12;
                defenseSpin.Value = 12;
                magicAttackSpin.Value = 12;
                magicDefenseSpin.Value = 12;
                speedSpin.Value = 12;
                luckSpin.Value = 10;
                
                hpGrowthSlider.Value = 5;
                mpGrowthSlider.Value = 5;
                attackGrowthSlider.Value = 3;
                defenseGrowthSlider.Value = 3;
                magicAttackGrowthSlider.Value = 3;
                magicDefenseGrowthSlider.Value = 3;
                speedGrowthSlider.Value = 3;
                luckGrowthSlider.Value = 3;
                break;
        }
        
        GD.Print($"✅ Applied {templateName} template!");
        UpdateGraph();
    }

    // Event Handlers
    private void OnSaveCharacter()
    {
        var fileDialog = new FileDialog();
        fileDialog.FileMode = FileDialog.FileModeEnum.SaveFile;
        fileDialog.Access = FileDialog.AccessEnum.Resources;
        fileDialog.AddFilter("*.tres", "Godot Resource");
        fileDialog.CurrentDir = "res://Database/Characters/";
        fileDialog.FileSelected += (path) => SaveCharacterToPath(path);
        AddChild(fileDialog);
        fileDialog.PopupCentered(new Vector2I(800, 600));
    }

    private void SaveCharacterToPath(string path)
    {
        UpdateCharacterFromUI();
        
        var error = ResourceSaver.Save(currentCharacter, path);
        if (error == Error.Ok)
        {
            GD.Print($"✅ Character saved successfully to: {path}");
            currentPath = path;
        }
        else
        {
            GD.PrintErr($"❌ Failed to save character: {error}");
        }
    }

    private void OnLoadCharacter()
    {
        var fileDialog = new FileDialog();
        fileDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
        fileDialog.Access = FileDialog.AccessEnum.Resources;
        fileDialog.AddFilter("*.tres", "Godot Resource");
        fileDialog.CurrentDir = "res://Database/Characters/";
        fileDialog.FileSelected += (path) => LoadCharacterFromPath(path);
        AddChild(fileDialog);
        fileDialog.PopupCentered(new Vector2I(800, 600));
    }

    private void LoadCharacterFromPath(string path)
    {
        var character = GD.Load<EchoesAcrossTime.Database.CharacterData>(path);
        if (character != null)
        {
            currentCharacter = character;
            currentPath = path;
            LoadCharacterToUI();
            GD.Print($"✅ Character loaded: {character.DisplayName}");
        }
        else
        {
            GD.PrintErr($"❌ Failed to load character from: {path}");
        }
    }

    private void OnDuplicateCharacter()
    {
        UpdateCharacterFromUI();
        
        var duplicate = currentCharacter.Duplicate() as EchoesAcrossTime.Database.CharacterData;
        duplicate.CharacterId += "_copy";
        duplicate.DisplayName += " (Copy)";
        
        currentCharacter = duplicate;
        currentPath = "";
        LoadCharacterToUI();
        
        GD.Print("✅ Character duplicated!");
    }

    private void OnClearAll()
    {
        var confirmDialog = new ConfirmationDialog();
        confirmDialog.DialogText = "Clear all fields and create a new character?";
        confirmDialog.Confirmed += () => {
            CreateNewCharacter();
            GD.Print("✅ Cleared all fields");
        };
        AddChild(confirmDialog);
        confirmDialog.PopupCentered();
    }

    private void CreateNewCharacter()
    {
        currentCharacter = new EchoesAcrossTime.Database.CharacterData();
        currentPath = "";
        LoadCharacterToUI();
    }

    private void UpdateCharacterFromUI()
    {
        if (currentCharacter == null) return;
        
        // Basic Info
        currentCharacter.CharacterId = characterIdEdit.Text;
        currentCharacter.DisplayName = displayNameEdit.Text;
        currentCharacter.Description = descriptionEdit.Text;
        currentCharacter.Type = (EchoesAcrossTime.Database.CharacterType)characterTypeOption.Selected;
        currentCharacter.Class = (EchoesAcrossTime.Database.CharacterClass)characterClassOption.Selected;
        currentCharacter.IsBoss = isBossCheck.ButtonPressed;
        
        // Base Stats
        currentCharacter.Level = (int)levelSpin.Value;
        currentCharacter.MaxHP = (int)maxHPSpin.Value;
        currentCharacter.MaxMP = (int)maxMPSpin.Value;
        currentCharacter.Attack = (int)attackSpin.Value;
        currentCharacter.Defense = (int)defenseSpin.Value;
        currentCharacter.MagicAttack = (int)magicAttackSpin.Value;
        currentCharacter.MagicDefense = (int)magicDefenseSpin.Value;
        currentCharacter.Speed = (int)speedSpin.Value;
        currentCharacter.Luck = (int)luckSpin.Value;
        
        // Growth Rates (convert from percentage to decimal)
        currentCharacter.HPGrowthRate = (float)(hpGrowthSlider.Value / 100.0);
        currentCharacter.MPGrowthRate = (float)(mpGrowthSlider.Value / 100.0);
        currentCharacter.AttackGrowthRate = (float)(attackGrowthSlider.Value / 100.0);
        currentCharacter.DefenseGrowthRate = (float)(defenseGrowthSlider.Value / 100.0);
        currentCharacter.MagicAttackGrowthRate = (float)(magicAttackGrowthSlider.Value / 100.0);
        currentCharacter.MagicDefenseGrowthRate = (float)(magicDefenseGrowthSlider.Value / 100.0);
        currentCharacter.SpeedGrowthRate = (float)(speedGrowthSlider.Value / 100.0);
        currentCharacter.LuckGrowthRate = (float)(luckGrowthSlider.Value / 100.0);
        
        // Experience Curve
        if (currentCharacter.ExpCurve == null)
        {
            currentCharacter.ExpCurve = new EchoesAcrossTime.Combat.ExperienceCurve();
        }
        currentCharacter.ExpCurve.CurveType = (EchoesAcrossTime.Combat.ExperienceCurveType)expCurveOption.Selected;
        
        // Element Affinities
        if (currentCharacter.ElementAffinities == null)
        {
            currentCharacter.ElementAffinities = new EchoesAcrossTime.Combat.ElementAffinityData();
        }
        
        var elementMap = new Dictionary<string, EchoesAcrossTime.Combat.ElementType>
        {
            { "Physical", EchoesAcrossTime.Combat.ElementType.Physical },
            { "Fire", EchoesAcrossTime.Combat.ElementType.Fire },
            { "Water", EchoesAcrossTime.Combat.ElementType.Water },
            { "Thunder", EchoesAcrossTime.Combat.ElementType.Thunder },
            { "Ice", EchoesAcrossTime.Combat.ElementType.Ice },
            { "Earth", EchoesAcrossTime.Combat.ElementType.Earth },
            { "Light", EchoesAcrossTime.Combat.ElementType.Light },
            { "Dark", EchoesAcrossTime.Combat.ElementType.Dark }
        };
        
        foreach (var kvp in affinityOptions)
        {
            if (elementMap.TryGetValue(kvp.Key, out var elementType))
            {
                var affinity = (EchoesAcrossTime.Combat.ElementAffinity)kvp.Value.Selected;
                currentCharacter.ElementAffinities.SetAffinity(elementType, affinity);
            }
        }
    }

    private void LoadCharacterToUI()
    {
        if (currentCharacter == null) return;
        
        // Basic Info
        characterIdEdit.Text = currentCharacter.CharacterId ?? "";
        displayNameEdit.Text = currentCharacter.DisplayName ?? "";
        descriptionEdit.Text = currentCharacter.Description ?? "";
        characterTypeOption.Selected = (int)currentCharacter.Type;
        characterClassOption.Selected = (int)currentCharacter.Class;
        isBossCheck.ButtonPressed = currentCharacter.IsBoss;
        
        // Base Stats
        levelSpin.Value = currentCharacter.Level;
        maxHPSpin.Value = currentCharacter.MaxHP;
        maxMPSpin.Value = currentCharacter.MaxMP;
        attackSpin.Value = currentCharacter.Attack;
        defenseSpin.Value = currentCharacter.Defense;
        magicAttackSpin.Value = currentCharacter.MagicAttack;
        magicDefenseSpin.Value = currentCharacter.MagicDefense;
        speedSpin.Value = currentCharacter.Speed;
        luckSpin.Value = currentCharacter.Luck;
        
        // Growth Rates (convert from decimal to percentage)
        hpGrowthSlider.Value = currentCharacter.HPGrowthRate * 100.0;
        mpGrowthSlider.Value = currentCharacter.MPGrowthRate * 100.0;
        attackGrowthSlider.Value = currentCharacter.AttackGrowthRate * 100.0;
        defenseGrowthSlider.Value = currentCharacter.DefenseGrowthRate * 100.0;
        magicAttackGrowthSlider.Value = currentCharacter.MagicAttackGrowthRate * 100.0;
        magicDefenseGrowthSlider.Value = currentCharacter.MagicDefenseGrowthRate * 100.0;
        speedGrowthSlider.Value = currentCharacter.SpeedGrowthRate * 100.0;
        luckGrowthSlider.Value = currentCharacter.LuckGrowthRate * 100.0;
        
        // Experience Curve
        if (currentCharacter.ExpCurve != null)
        {
            expCurveOption.Selected = (int)currentCharacter.ExpCurve.CurveType;
        }
        
        // Element Affinities
        if (currentCharacter.ElementAffinities != null)
        {
            var elementMap = new Dictionary<string, EchoesAcrossTime.Combat.ElementType>
            {
                { "Physical", EchoesAcrossTime.Combat.ElementType.Physical },
                { "Fire", EchoesAcrossTime.Combat.ElementType.Fire },
                { "Water", EchoesAcrossTime.Combat.ElementType.Water },
                { "Thunder", EchoesAcrossTime.Combat.ElementType.Thunder },
                { "Ice", EchoesAcrossTime.Combat.ElementType.Ice },
                { "Earth", EchoesAcrossTime.Combat.ElementType.Earth },
                { "Light", EchoesAcrossTime.Combat.ElementType.Light },
                { "Dark", EchoesAcrossTime.Combat.ElementType.Dark }
            };
            
            foreach (var kvp in affinityOptions)
            {
                if (elementMap.TryGetValue(kvp.Key, out var elementType))
                {
                    var affinity = currentCharacter.ElementAffinities.GetAffinity(elementType);
                    kvp.Value.Selected = (int)affinity;
                }
            }
        }
        
        UpdateGraph();
    }
}

// Custom Control for drawing the stat curve graph
public partial class StatCurveGraph : Control
{
    private List<List<int>> allStatCurves;
    private List<int> singleStatCurve;
    private string statName = "";
    private Color statColor = Colors.White;
    private bool showingAllStats = false;

    private static readonly Color[] StatColors = new Color[]
    {
        new Color(1, 0.2f, 0.2f),      // HP - Red
        new Color(0.2f, 0.4f, 1),      // MP - Blue
        new Color(1, 0.5f, 0),         // Attack - Orange
        new Color(0.8f, 0.8f, 0.2f),   // Defense - Yellow
        new Color(0.8f, 0.2f, 1),      // Magic Attack - Purple
        new Color(0.2f, 1, 1),         // Magic Defense - Cyan
        new Color(0.2f, 1, 0.2f),      // Speed - Green
        new Color(1, 0.8f, 0.4f)       // Luck - Gold
    };

    private static readonly string[] StatNames = new string[]
    {
        "HP", "MP", "ATK", "DEF", "MATK", "MDEF", "SPD", "LCK"
    };

    public void SetAllStats(List<List<int>> curves)
    {
        allStatCurves = curves;
        showingAllStats = true;
    }

    public void SetSingleStat(List<int> curve, string name, Color color)
    {
        singleStatCurve = curve;
        statName = name;
        statColor = color;
        showingAllStats = false;
    }

    public override void _Draw()
    {
        if (showingAllStats && allStatCurves != null)
        {
            DrawAllStats();
        }
        else if (!showingAllStats && singleStatCurve != null)
        {
            DrawSingleStat();
        }
        else
        {
            DrawPlaceholder();
        }
    }

    private void DrawAllStats()
    {
        var size = Size;
        float padding = 40;
        float graphWidth = size.X - padding * 2;
        float graphHeight = size.Y - padding * 2;
        
        // Draw background
        DrawRect(new Rect2(0, 0, size.X, size.Y), new Color(0.1f, 0.1f, 0.15f));
        
        // Find max value across all stats for scaling
        int maxValue = 0;
        foreach (var curve in allStatCurves)
        {
            int max = curve.Max();
            if (max > maxValue) maxValue = max;
        }
        
        if (maxValue == 0) maxValue = 100;
        
        // Draw grid lines
        DrawGrid(padding, graphWidth, graphHeight, maxValue);
        
        // Draw each stat curve
        for (int statIndex = 0; statIndex < allStatCurves.Count; statIndex++)
        {
            DrawCurve(allStatCurves[statIndex], padding, graphWidth, graphHeight, maxValue, StatColors[statIndex], 1.5f);
        }
        
        // Draw legend
        DrawLegend(padding, size.X - padding);
        
        // Draw axes
        DrawAxes(padding, size, graphWidth, graphHeight, maxValue);
    }

    private void DrawSingleStat()
    {
        var size = Size;
        float padding = 40;
        float graphWidth = size.X - padding * 2;
        float graphHeight = size.Y - padding * 2;
        
        // Draw background
        DrawRect(new Rect2(0, 0, size.X, size.Y), new Color(0.1f, 0.1f, 0.15f));
        
        // Find max value for scaling
        int maxValue = singleStatCurve.Max();
        if (maxValue == 0) maxValue = 100;
        
        // Draw grid lines
        DrawGrid(padding, graphWidth, graphHeight, maxValue);
        
        // Draw the curve
        DrawCurve(singleStatCurve, padding, graphWidth, graphHeight, maxValue, statColor, 2.5f);
        
        // Draw title
        DrawString(ThemeDB.FallbackFont, new Vector2(padding, 25), $"{statName} Growth Curve", HorizontalAlignment.Left, -1, 16, statColor);
        
        // Draw value labels
        DrawValueLabels(singleStatCurve, padding, graphWidth, graphHeight, maxValue);
        
        // Draw axes
        DrawAxes(padding, size, graphWidth, graphHeight, maxValue);
    }

    private void DrawGrid(float padding, float graphWidth, float graphHeight, int maxValue)
    {
        var gridColor = new Color(0.3f, 0.3f, 0.35f);
        
        // Horizontal grid lines (5 lines)
        for (int i = 0; i <= 5; i++)
        {
            float y = padding + graphHeight - (graphHeight / 5.0f * i);
            DrawLine(new Vector2(padding, y), new Vector2(padding + graphWidth, y), gridColor);
        }
        
        // Vertical grid lines (10 lines for every 10 levels)
        for (int i = 0; i <= 10; i++)
        {
            float x = padding + (graphWidth / 10.0f * i);
            DrawLine(new Vector2(x, padding), new Vector2(x, padding + graphHeight), gridColor);
        }
    }

    private void DrawCurve(List<int> curve, float padding, float graphWidth, float graphHeight, int maxValue, Color color, float thickness)
    {
        if (curve.Count < 2) return;
        
        for (int i = 0; i < curve.Count - 1; i++)
        {
            float x1 = padding + (graphWidth / 99.0f) * i;
            float y1 = padding + graphHeight - (graphHeight * curve[i] / (float)maxValue);
            
            float x2 = padding + (graphWidth / 99.0f) * (i + 1);
            float y2 = padding + graphHeight - (graphHeight * curve[i + 1] / (float)maxValue);
            
            DrawLine(new Vector2(x1, y1), new Vector2(x2, y2), color, thickness);
        }
    }

    private void DrawLegend(float startX, float endX)
    {
        float legendY = 10;
        float legendItemWidth = (endX - startX) / 8.0f;
        
        for (int i = 0; i < 8; i++)
        {
            float x = startX + legendItemWidth * i;
            DrawRect(new Rect2(x, legendY, 15, 10), StatColors[i]);
            DrawString(ThemeDB.FallbackFont, new Vector2(x + 18, legendY + 8), StatNames[i], HorizontalAlignment.Left, -1, 10);
        }
    }

    private void DrawValueLabels(List<int> curve, float padding, float graphWidth, float graphHeight, int maxValue)
    {
        // Draw value at level 1, 25, 50, 75, 99
        int[] levels = new int[] { 0, 24, 49, 74, 98 };
        
        foreach (int level in levels)
        {
            if (level >= curve.Count) continue;
            
            float x = padding + (graphWidth / 99.0f) * level;
            float y = padding + graphHeight - (graphHeight * curve[level] / (float)maxValue);
            
            DrawCircle(new Vector2(x, y), 3, statColor);
            DrawString(ThemeDB.FallbackFont, new Vector2(x - 10, y - 10), curve[level].ToString(), HorizontalAlignment.Left, -1, 11, Colors.White);
        }
    }

    private void DrawAxes(float padding, Vector2 size, float graphWidth, float graphHeight, int maxValue)
    {
        var axisColor = new Color(0.8f, 0.8f, 0.8f);
        
        // X axis
        DrawLine(new Vector2(padding, padding + graphHeight), new Vector2(padding + graphWidth, padding + graphHeight), axisColor, 2);
        
        // Y axis
        DrawLine(new Vector2(padding, padding), new Vector2(padding, padding + graphHeight), axisColor, 2);
        
        // X axis labels (Level)
        for (int i = 0; i <= 10; i++)
        {
            int level = i * 10;
            if (level == 0) level = 1;
            float x = padding + (graphWidth / 10.0f * i);
            DrawString(ThemeDB.FallbackFont, new Vector2(x - 10, size.Y - 5), level.ToString(), HorizontalAlignment.Left, -1, 10);
        }
        
        // Y axis labels (Value)
        for (int i = 0; i <= 5; i++)
        {
            int value = (maxValue / 5) * i;
            float y = padding + graphHeight - (graphHeight / 5.0f * i);
            DrawString(ThemeDB.FallbackFont, new Vector2(5, y + 4), value.ToString(), HorizontalAlignment.Left, -1, 10);
        }
        
        // Axis titles
        DrawString(ThemeDB.FallbackFont, new Vector2(size.X / 2 - 15, size.Y - 5), "Level", HorizontalAlignment.Left, -1, 12, axisColor);
        DrawString(ThemeDB.FallbackFont, new Vector2(5, padding - 10), "Value", HorizontalAlignment.Left, -1, 12, axisColor);
    }

    private void DrawPlaceholder()
    {
        var size = Size;
        DrawRect(new Rect2(0, 0, size.X, size.Y), new Color(0.1f, 0.1f, 0.15f));
        DrawString(ThemeDB.FallbackFont, new Vector2(size.X / 2 - 100, size.Y / 2), "Adjust stats to see growth curves", HorizontalAlignment.Left, -1, 14, Colors.Gray);
    }
}
#endif