#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[Tool]
public partial class SkillBuilderDock : Control
{
    // UI Elements
    private TabContainer tabContainer;
    private ScrollContainer mainScroll;
    private VBoxContainer mainContainer;
    
    // Basic Info
    private LineEdit skillIdEdit;
    private LineEdit displayNameEdit;
    private TextEdit descriptionEdit;
    private OptionButton skillTypeOption;
    
    // Targeting
    private OptionButton targetOption;
    private SpinBox numberOfHitsSpin;
    
    // Costs
    private SpinBox mpCostSpin;
    private SpinBox hpCostSpin;
    private SpinBox goldCostSpin;
    
    // Damage Section
    private OptionButton damageFormulaOption;
    private OptionButton damageTypeOption;
    private OptionButton elementOption;
    private SpinBox basePowerSpin;
    private SpinBox powerMultiplierSpin;
    private SpinBox accuracySpin;
    private SpinBox critBonusSpin;
    private CheckBox ignoreDefenseCheck;
    
    // Healing Section
    private SpinBox healAmountSpin;
    private SpinBox healPercentSpin;
    private CheckBox healsMpCheck;
    private CheckBox revivesCheck;
    
    // Status Effects Section
    private VBoxContainer statusEffectsContainer;
    private List<StatusEffectRow> statusEffectRows = new List<StatusEffectRow>();
    private Button addStatusButton;
    
    // Buffs Section
    private SpinBox attackBuffSpin;
    private SpinBox defenseBuffSpin;
    private SpinBox magicAttackBuffSpin;
    private SpinBox magicDefenseBuffSpin;
    private SpinBox speedBuffSpin;
    private SpinBox buffDurationSpin;
    private CheckBox isDebuffCheck;
    
    // Requirements
    private SpinBox requiredLevelSpin;
    
    // Damage Calculator
    private VBoxContainer calculatorContainer;
    private Label calculatorResult;
    private SpinBox userLevelSpin;
    private SpinBox userAttackSpin;
    private SpinBox targetDefenseSpin;
    private Button calculateButton;
    
    // Action Buttons
    private Button saveButton;
    private Button loadButton;
    private Button duplicateButton;
    private Button batchCreateButton;
    private Button clearButton;
    
    // Current skill being edited
    private EchoesAcrossTime.Combat.SkillData currentSkill;
    private string currentPath = "";
    
    

    public override void _Ready()
    {
        Name = "Skill Builder";
        CustomMinimumSize = new Vector2(400, 600);
    
        // IMPORTANT: Force layout
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        SizeFlagsVertical = SizeFlags.ExpandFill;
    
        GD.Print("=== SKILL BUILDER _Ready() called ===");
    
        BuildUI();
        ConnectSignals();
        CreateNewSkill();
    
        GD.Print("=== SKILL BUILDER UI built successfully ===");
    }

    private void BuildUI()
    {
        // IMPORTANT: Set size and layout flags BEFORE adding children
        CustomMinimumSize = new Vector2(400, 600);
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        SizeFlagsVertical = SizeFlags.ExpandFill;
    
        // Main scroll container
        mainScroll = new ScrollContainer();
        mainScroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        mainScroll.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        mainScroll.CustomMinimumSize = new Vector2(0, 200); // CRITICAL: Set minimum size!
        AddChild(mainScroll);
    
        mainContainer = new VBoxContainer();
        mainContainer.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        mainScroll.AddChild(mainContainer);
    
        // Title
        var title = new Label();
        title.Text = "⚔️ Visual Skill Builder ⚔️";
        title.AddThemeColorOverride("font_color", new Color(1, 0.8f, 0.2f));
        title.AddThemeFontSizeOverride("font_size", 18);
        mainContainer.AddChild(title);
    
        mainContainer.AddChild(new HSeparator());
    
        // Add a test label to confirm it's working
        var testLabel = new Label();
        testLabel.Text = "✅ Skill Builder UI is working!\n\nFull UI will be added here.";
        testLabel.AddThemeColorOverride("font_color", new Color(0, 1, 0));
        testLabel.AddThemeFontSizeOverride("font_size", 14);
        mainContainer.AddChild(testLabel);
    
        // Build sections
        BuildBasicInfoSection();
        BuildTargetingSection();
        BuildCostsSection();
        BuildDamageSection();
        BuildHealingSection();
        BuildStatusEffectsSection();
        BuildBuffsSection();
        BuildRequirementsSection();
        BuildDamageCalculator();
        BuildActionButtons();
    }

    private void BuildBasicInfoSection()
    {
        var section = CreateSection("📝 Basic Information");
        
        skillIdEdit = CreateLineEdit("Skill ID:", "fire_spell");
        section.AddChild(skillIdEdit.GetParent());
        
        displayNameEdit = CreateLineEdit("Display Name:", "Fire");
        section.AddChild(displayNameEdit.GetParent());
        
        var descLabel = new Label();
        descLabel.Text = "Description:";
        section.AddChild(descLabel);
        
        descriptionEdit = new TextEdit();
        descriptionEdit.CustomMinimumSize = new Vector2(0, 60);
        descriptionEdit.PlaceholderText = "Unleashes flames on the enemy...";
        section.AddChild(descriptionEdit);
        
        skillTypeOption = CreateOptionButton("Skill Type:", new string[] {
            "Active Attack", "Active Support", "Passive", "Cosmic Ability", "Political Skill"
        });
        section.AddChild(skillTypeOption.GetParent());
        
        mainContainer.AddChild(section);
    }

    private void BuildTargetingSection()
    {
        var section = CreateSection("🎯 Targeting");
        
        targetOption = CreateOptionButton("Target:", new string[] {
            "Single Enemy", "All Enemies", "Random Enemy", "Single Ally", 
            "All Allies", "Self", "Everyone", "Dead Ally"
        });
        section.AddChild(targetOption.GetParent());
        
        numberOfHitsSpin = CreateSpinBox("Number of Hits:", 1, 16, 1);
        section.AddChild(numberOfHitsSpin.GetParent());
        
        mainContainer.AddChild(section);
    }

    private void BuildCostsSection()
    {
        var section = CreateSection("💰 Resource Costs");
        
        mpCostSpin = CreateSpinBox("MP Cost:", 0, 999, 0);
        section.AddChild(mpCostSpin.GetParent());
        
        hpCostSpin = CreateSpinBox("HP Cost:", 0, 999, 0);
        section.AddChild(hpCostSpin.GetParent());
        
        goldCostSpin = CreateSpinBox("Gold Cost:", 0, 9999, 0);
        section.AddChild(goldCostSpin.GetParent());
        
        mainContainer.AddChild(section);
    }

    private void BuildDamageSection()
    {
        var section = CreateSection("⚔️ Damage Properties");
        
        damageFormulaOption = CreateOptionButton("Damage Formula:", new string[] {
            "Simple", "Persona", "FF7", "FF8", "FF9", "Pokemon", "Custom"
        });
        section.AddChild(damageFormulaOption.GetParent());
        
        damageTypeOption = CreateOptionButton("Damage Type:", new string[] {
            "Physical", "Magical", "Fixed", "Percentage", "Recovery"
        });
        section.AddChild(damageTypeOption.GetParent());
        
        elementOption = CreateOptionButton("Element:", new string[] {
            "Physical", "Fire", "Water", "Thunder", "Ice", "Earth", "Light", "Dark", "None"
        });
        section.AddChild(elementOption.GetParent());
        
        basePowerSpin = CreateSpinBox("Base Power:", 0, 9999, 100);
        section.AddChild(basePowerSpin.GetParent());
        
        powerMultiplierSpin = CreateSpinBox("Power Multiplier:", 0.1, 10, 1, 0.1);
        section.AddChild(powerMultiplierSpin.GetParent());
        
        accuracySpin = CreateSpinBox("Accuracy (%):", 0, 100, 100);
        section.AddChild(accuracySpin.GetParent());
        
        critBonusSpin = CreateSpinBox("Critical Bonus (%):", 0, 100, 0);
        section.AddChild(critBonusSpin.GetParent());
        
        ignoreDefenseCheck = CreateCheckBox("Ignore Defense");
        section.AddChild(ignoreDefenseCheck);
        
        mainContainer.AddChild(section);
    }

    private void BuildHealingSection()
    {
        var section = CreateSection("💚 Healing Properties");
        
        healAmountSpin = CreateSpinBox("Heal Amount:", 0, 9999, 0);
        section.AddChild(healAmountSpin.GetParent());
        
        healPercentSpin = CreateSpinBox("Heal Percent (0-1):", 0, 1, 0, 0.01);
        section.AddChild(healPercentSpin.GetParent());
        
        healsMpCheck = CreateCheckBox("Heals MP (instead of HP)");
        section.AddChild(healsMpCheck);
        
        revivesCheck = CreateCheckBox("Revives Target");
        section.AddChild(revivesCheck);
        
        mainContainer.AddChild(section);
    }

    private void BuildStatusEffectsSection()
    {
        var section = CreateSection("🎯 Status Effects");
        
        var helpLabel = new Label();
        helpLabel.Text = "Add status effects with their application chances:";
        helpLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        section.AddChild(helpLabel);
        
        statusEffectsContainer = new VBoxContainer();
        section.AddChild(statusEffectsContainer);
        
        addStatusButton = new Button();
        addStatusButton.Text = "➕ Add Status Effect";
        addStatusButton.Pressed += OnAddStatusEffect;
        section.AddChild(addStatusButton);
        
        mainContainer.AddChild(section);
    }

    private void BuildBuffsSection()
    {
        var section = CreateSection("📊 Stat Buffs/Debuffs");
        
        attackBuffSpin = CreateSpinBox("Attack Buff:", -100, 100, 0);
        section.AddChild(attackBuffSpin.GetParent());
        
        defenseBuffSpin = CreateSpinBox("Defense Buff:", -100, 100, 0);
        section.AddChild(defenseBuffSpin.GetParent());
        
        magicAttackBuffSpin = CreateSpinBox("Magic Attack Buff:", -100, 100, 0);
        section.AddChild(magicAttackBuffSpin.GetParent());
        
        magicDefenseBuffSpin = CreateSpinBox("Magic Defense Buff:", -100, 100, 0);
        section.AddChild(magicDefenseBuffSpin.GetParent());
        
        speedBuffSpin = CreateSpinBox("Speed Buff:", -100, 100, 0);
        section.AddChild(speedBuffSpin.GetParent());
        
        buffDurationSpin = CreateSpinBox("Buff Duration (turns):", 1, 10, 3);
        section.AddChild(buffDurationSpin.GetParent());
        
        isDebuffCheck = CreateCheckBox("Is Debuff (negative effect)");
        section.AddChild(isDebuffCheck);
        
        mainContainer.AddChild(section);
    }

    private void BuildRequirementsSection()
    {
        var section = CreateSection("🔒 Requirements");
        
        requiredLevelSpin = CreateSpinBox("Required Level:", 1, 99, 1);
        section.AddChild(requiredLevelSpin.GetParent());
        
        mainContainer.AddChild(section);
    }

    private void BuildDamageCalculator()
    {
        var section = CreateSection("🧮 Damage Calculator");
        
        var helpLabel = new Label();
        helpLabel.Text = "Calculate expected damage with test values:";
        helpLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        section.AddChild(helpLabel);
        
        calculatorContainer = new VBoxContainer();
        section.AddChild(calculatorContainer);
        
        userLevelSpin = CreateSpinBox("User Level:", 1, 99, 50);
        calculatorContainer.AddChild(userLevelSpin.GetParent());
        
        userAttackSpin = CreateSpinBox("User Attack:", 1, 999, 100);
        calculatorContainer.AddChild(userAttackSpin.GetParent());
        
        targetDefenseSpin = CreateSpinBox("Target Defense:", 1, 999, 50);
        calculatorContainer.AddChild(targetDefenseSpin.GetParent());
        
        calculateButton = new Button();
        calculateButton.Text = "🔢 Calculate Damage";
        calculateButton.Pressed += OnCalculateDamage;
        calculatorContainer.AddChild(calculateButton);
        
        calculatorResult = new Label();
        calculatorResult.Text = "Expected Damage: ---";
        calculatorResult.AddThemeColorOverride("font_color", new Color(0.2f, 1, 0.2f));
        calculatorResult.AddThemeFontSizeOverride("font_size", 16);
        calculatorContainer.AddChild(calculatorResult);
        
        mainContainer.AddChild(section);
    }

    private void BuildActionButtons()
    {
        mainContainer.AddChild(new HSeparator());
        
        var buttonContainer = new HBoxContainer();
        buttonContainer.Alignment = BoxContainer.AlignmentMode.Center;
        
        saveButton = new Button();
        saveButton.Text = "💾 Save Skill";
        saveButton.Pressed += OnSaveSkill;
        buttonContainer.AddChild(saveButton);
        
        loadButton = new Button();
        loadButton.Text = "📂 Load Skill";
        loadButton.Pressed += OnLoadSkill;
        buttonContainer.AddChild(loadButton);
        
        duplicateButton = new Button();
        duplicateButton.Text = "📋 Duplicate";
        duplicateButton.Pressed += OnDuplicateSkill;
        buttonContainer.AddChild(duplicateButton);
        
        mainContainer.AddChild(buttonContainer);
        
        var buttonContainer2 = new HBoxContainer();
        buttonContainer2.Alignment = BoxContainer.AlignmentMode.Center;
        
        batchCreateButton = new Button();
        batchCreateButton.Text = "⚡ Batch Create";
        batchCreateButton.Pressed += OnBatchCreate;
        buttonContainer2.AddChild(batchCreateButton);
        
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
        // Connect value changed signals for real-time preview
        basePowerSpin.ValueChanged += (_) => UpdateDamagePreview();
        powerMultiplierSpin.ValueChanged += (_) => UpdateDamagePreview();
        userAttackSpin.ValueChanged += (_) => UpdateDamagePreview();
        targetDefenseSpin.ValueChanged += (_) => UpdateDamagePreview();
    }

    private void UpdateDamagePreview()
    {
        // Auto-update damage calculator when relevant values change
        OnCalculateDamage();
    }

    // Event Handlers
    private void OnAddStatusEffect()
    {
        var row = new StatusEffectRow();
        statusEffectsContainer.AddChild(row);
        statusEffectRows.Add(row);
    }

    private void OnCalculateDamage()
    {
        if (currentSkill == null) return;
        
        // Create mock stats for calculation
        var mockUser = new EchoesAcrossTime.Combat.CharacterStats
        {
            Level = (int)userLevelSpin.Value,
            Attack = (int)userAttackSpin.Value,
            MagicAttack = (int)userAttackSpin.Value,
        };
        
        var mockTarget = new EchoesAcrossTime.Combat.CharacterStats
        {
            Defense = (int)targetDefenseSpin.Value,
            MagicDefense = (int)targetDefenseSpin.Value,
        };
        
        // Update current skill with UI values
        UpdateSkillFromUI();
        
        // Calculate damage
        int damage = currentSkill.CalculateDamage(mockUser, mockTarget, false);
        int critDamage = currentSkill.CalculateDamage(mockUser, mockTarget, true);
        
        calculatorResult.Text = $"Expected Damage: {damage}\nCritical: {critDamage}";
    }

    private void OnSaveSkill()
    {
        var fileDialog = new FileDialog();
        fileDialog.FileMode = FileDialog.FileModeEnum.SaveFile;
        fileDialog.Access = FileDialog.AccessEnum.Resources;
        fileDialog.AddFilter("*.tres", "Godot Resource");
        fileDialog.CurrentDir = "res://Data/Skills/";
        fileDialog.FileSelected += (path) => SaveSkillToPath(path);
        AddChild(fileDialog);
        fileDialog.PopupCentered(new Vector2I(800, 600));
    }

    private void SaveSkillToPath(string path)
    {
        UpdateSkillFromUI();
        
        var error = ResourceSaver.Save(currentSkill, path);
        if (error == Error.Ok)
        {
            GD.Print($"✅ Skill saved successfully to: {path}");
            currentPath = path;
        }
        else
        {
            GD.PrintErr($"❌ Failed to save skill: {error}");
        }
    }

    private void OnLoadSkill()
    {
        var fileDialog = new FileDialog();
        fileDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
        fileDialog.Access = FileDialog.AccessEnum.Resources;
        fileDialog.AddFilter("*.tres", "Godot Resource");
        fileDialog.CurrentDir = "res://Data/Skills/";
        fileDialog.FileSelected += (path) => LoadSkillFromPath(path);
        AddChild(fileDialog);
        fileDialog.PopupCentered(new Vector2I(800, 600));
    }

    private void LoadSkillFromPath(string path)
    {
        var skill = GD.Load<EchoesAcrossTime.Combat.SkillData>(path);
        if (skill != null)
        {
            currentSkill = skill;
            currentPath = path;
            LoadSkillToUI();
            GD.Print($"✅ Skill loaded: {skill.DisplayName}");
        }
        else
        {
            GD.PrintErr($"❌ Failed to load skill from: {path}");
        }
    }

    private void OnDuplicateSkill()
    {
        UpdateSkillFromUI();
        
        // Create a duplicate
        var duplicate = currentSkill.Duplicate() as EchoesAcrossTime.Combat.SkillData;
        duplicate.SkillId += "_copy";
        duplicate.DisplayName += " (Copy)";
        
        currentSkill = duplicate;
        currentPath = "";
        LoadSkillToUI();
        
        GD.Print("✅ Skill duplicated!");
    }

    private void OnBatchCreate()
    {
        var dialog = new BatchCreateDialog();
        dialog.SkillCreated += (skillData) => {
            currentSkill = skillData;
            LoadSkillToUI();
        };
        AddChild(dialog);
        dialog.PopupCentered(new Vector2I(600, 400));
    }

    private void OnClearAll()
    {
        var confirmDialog = new ConfirmationDialog();
        confirmDialog.DialogText = "Clear all fields and create a new skill?";
        confirmDialog.Confirmed += () => {
            CreateNewSkill();
            GD.Print("✅ Cleared all fields");
        };
        AddChild(confirmDialog);
        confirmDialog.PopupCentered();
    }

    private void CreateNewSkill()
    {
        currentSkill = new EchoesAcrossTime.Combat.SkillData();
        currentPath = "";
        LoadSkillToUI();
    }

    private void UpdateSkillFromUI()
    {
        if (currentSkill == null) return;
        
        // Basic Info
        currentSkill.SkillId = skillIdEdit.Text;
        currentSkill.DisplayName = displayNameEdit.Text;
        currentSkill.Description = descriptionEdit.Text;
        currentSkill.Type = (EchoesAcrossTime.Combat.SkillType)skillTypeOption.Selected;
        
        // Targeting
        currentSkill.Target = (EchoesAcrossTime.Combat.SkillTarget)targetOption.Selected;
        currentSkill.NumberOfHits = (int)numberOfHitsSpin.Value;
        
        // Costs
        currentSkill.MPCost = (int)mpCostSpin.Value;
        currentSkill.HPCost = (int)hpCostSpin.Value;
        currentSkill.GoldCost = (int)goldCostSpin.Value;
        
        // Damage
        currentSkill.DamageFormula = (EchoesAcrossTime.Combat.DamageFormulaType)damageFormulaOption.Selected;
        currentSkill.DamageType = (EchoesAcrossTime.Combat.DamageType)damageTypeOption.Selected;
        currentSkill.Element = (EchoesAcrossTime.Combat.ElementType)elementOption.Selected;
        currentSkill.BasePower = (int)basePowerSpin.Value;
        currentSkill.PowerMultiplier = (float)powerMultiplierSpin.Value;
        currentSkill.Accuracy = (int)accuracySpin.Value;
        currentSkill.CriticalBonus = (int)critBonusSpin.Value;
        currentSkill.IgnoreDefense = ignoreDefenseCheck.ButtonPressed;
        
        // Healing
        currentSkill.HealAmount = (int)healAmountSpin.Value;
        currentSkill.HealPercent = (float)healPercentSpin.Value;
        currentSkill.HealsMP = healsMpCheck.ButtonPressed;
        currentSkill.RevivesTarget = revivesCheck.ButtonPressed;
        
        // Status Effects
        currentSkill.InflictsStatuses = new Godot.Collections.Array<EchoesAcrossTime.Combat.StatusEffect>();
        currentSkill.StatusChances = new Godot.Collections.Array<int>();
        foreach (var row in statusEffectRows)
        {
            currentSkill.InflictsStatuses.Add((EchoesAcrossTime.Combat.StatusEffect)row.GetStatusEffect());
            currentSkill.StatusChances.Add(row.GetChance());
        }
        
        // Buffs
        currentSkill.AttackBuff = (int)attackBuffSpin.Value;
        currentSkill.DefenseBuff = (int)defenseBuffSpin.Value;
        currentSkill.MagicAttackBuff = (int)magicAttackBuffSpin.Value;
        currentSkill.MagicDefenseBuff = (int)magicDefenseBuffSpin.Value;
        currentSkill.SpeedBuff = (int)speedBuffSpin.Value;
        currentSkill.BuffDuration = (int)buffDurationSpin.Value;
        currentSkill.IsDebuff = isDebuffCheck.ButtonPressed;
        
        // Requirements
        currentSkill.RequiredLevel = (int)requiredLevelSpin.Value;
    }

    private void LoadSkillToUI()
    {
        if (currentSkill == null) return;
        
        // Basic Info
        skillIdEdit.Text = currentSkill.SkillId ?? "";
        displayNameEdit.Text = currentSkill.DisplayName ?? "";
        descriptionEdit.Text = currentSkill.Description ?? "";
        skillTypeOption.Selected = (int)currentSkill.Type;
        
        // Targeting
        targetOption.Selected = (int)currentSkill.Target;
        numberOfHitsSpin.Value = currentSkill.NumberOfHits;
        
        // Costs
        mpCostSpin.Value = currentSkill.MPCost;
        hpCostSpin.Value = currentSkill.HPCost;
        goldCostSpin.Value = currentSkill.GoldCost;
        
        // Damage
        damageFormulaOption.Selected = (int)currentSkill.DamageFormula;
        damageTypeOption.Selected = (int)currentSkill.DamageType;
        elementOption.Selected = (int)currentSkill.Element;
        basePowerSpin.Value = currentSkill.BasePower;
        powerMultiplierSpin.Value = currentSkill.PowerMultiplier;
        accuracySpin.Value = currentSkill.Accuracy;
        critBonusSpin.Value = currentSkill.CriticalBonus;
        ignoreDefenseCheck.ButtonPressed = currentSkill.IgnoreDefense;
        
        // Healing
        healAmountSpin.Value = currentSkill.HealAmount;
        healPercentSpin.Value = currentSkill.HealPercent;
        healsMpCheck.ButtonPressed = currentSkill.HealsMP;
        revivesCheck.ButtonPressed = currentSkill.RevivesTarget;
        
        // Clear and reload status effects
        foreach (var row in statusEffectRows)
        {
            row.QueueFree();
        }
        statusEffectRows.Clear();
        
        if (currentSkill.InflictsStatuses != null && currentSkill.StatusChances != null)
        {
            for (int i = 0; i < currentSkill.InflictsStatuses.Count; i++)
            {
                var row = new StatusEffectRow();
                row.SetStatusEffect((int)currentSkill.InflictsStatuses[i]);
                if (i < currentSkill.StatusChances.Count)
                {
                    row.SetChance(currentSkill.StatusChances[i]);
                }
                statusEffectsContainer.AddChild(row);
                statusEffectRows.Add(row);
            }
        }
        
        // Buffs
        attackBuffSpin.Value = currentSkill.AttackBuff;
        defenseBuffSpin.Value = currentSkill.DefenseBuff;
        magicAttackBuffSpin.Value = currentSkill.MagicAttackBuff;
        magicDefenseBuffSpin.Value = currentSkill.MagicDefenseBuff;
        speedBuffSpin.Value = currentSkill.SpeedBuff;
        buffDurationSpin.Value = currentSkill.BuffDuration;
        isDebuffCheck.ButtonPressed = currentSkill.IsDebuff;
        
        // Requirements
        requiredLevelSpin.Value = currentSkill.RequiredLevel;
        
        OnCalculateDamage();
    }
}

// Helper class for status effect rows
public partial class StatusEffectRow : HBoxContainer
{
    private OptionButton statusOption;
    private HSlider chanceSlider;
    private Label chanceLabel;
    private Button removeButton;

    public StatusEffectRow()
    {
        statusOption = new OptionButton();
        statusOption.CustomMinimumSize = new Vector2(150, 0);
        
        string[] statuses = new string[] {
            "None", "Poison", "Sleep", "Paralysis", "Confusion", "Silence",
            "Blind", "Berserk", "Charm", "Curse", "Burn", "Freeze", "Shock"
        };
        
        for (int i = 0; i < statuses.Length; i++)
        {
            statusOption.AddItem(statuses[i], i);
        }
        AddChild(statusOption);
        
        chanceSlider = new HSlider();
        chanceSlider.MinValue = 0;
        chanceSlider.MaxValue = 100;
        chanceSlider.Value = 50;
        chanceSlider.Step = 5;
        chanceSlider.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        chanceSlider.ValueChanged += (value) => {
            chanceLabel.Text = $"{value}%";
        };
        AddChild(chanceSlider);
        
        chanceLabel = new Label();
        chanceLabel.Text = "50%";
        chanceLabel.CustomMinimumSize = new Vector2(50, 0);
        AddChild(chanceLabel);
        
        removeButton = new Button();
        removeButton.Text = "❌";
        removeButton.Pressed += () => QueueFree();
        AddChild(removeButton);
    }

    public int GetStatusEffect() => statusOption.Selected;
    public int GetChance() => (int)chanceSlider.Value;
    public void SetStatusEffect(int status) => statusOption.Selected = status;
    public void SetChance(int chance)
    {
        chanceSlider.Value = chance;
        chanceLabel.Text = $"{chance}%";
    }
}

// Batch Create Dialog
public partial class BatchCreateDialog : Window
{
    [Signal]
    public delegate void SkillCreatedEventHandler(EchoesAcrossTime.Combat.SkillData skillData);
    
    private LineEdit baseNameEdit;
    private OptionButton elementOption;
    private SpinBox tierCountSpin;
    private SpinBox basePowerSpin;
    private SpinBox powerIncreaseSpin;
    private SpinBox mpCostBaseSpin;
    private SpinBox mpCostIncreaseSpin;
    private Button createButton;

    public BatchCreateDialog()
    {
        Title = "Batch Create Skill Family";
        Size = new Vector2I(500, 400);
        
        var vbox = new VBoxContainer();
        AddChild(vbox);
        
        var label = new Label();
        label.Text = "Create a family of skills (e.g., Fire I, Fire II, Fire III)";
        vbox.AddChild(label);
        
        vbox.AddChild(new HSeparator());
        
        baseNameEdit = CreateLineEdit("Base Name:", "Fire");
        vbox.AddChild(baseNameEdit.GetParent());
        
        elementOption = CreateOptionButton("Element:", new string[] {
            "Fire", "Water", "Thunder", "Ice", "Earth", "Light", "Dark"
        });
        vbox.AddChild(elementOption.GetParent());
        
        tierCountSpin = CreateSpinBox("Number of Tiers:", 1, 5, 3);
        vbox.AddChild(tierCountSpin.GetParent());
        
        basePowerSpin = CreateSpinBox("Base Power (Tier 1):", 50, 500, 80);
        vbox.AddChild(basePowerSpin.GetParent());
        
        powerIncreaseSpin = CreateSpinBox("Power Increase per Tier:", 10, 200, 40);
        vbox.AddChild(powerIncreaseSpin.GetParent());
        
        mpCostBaseSpin = CreateSpinBox("MP Cost (Tier 1):", 5, 50, 8);
        vbox.AddChild(mpCostBaseSpin.GetParent());
        
        mpCostIncreaseSpin = CreateSpinBox("MP Cost Increase per Tier:", 1, 20, 4);
        vbox.AddChild(mpCostIncreaseSpin.GetParent());
        
        createButton = new Button();
        createButton.Text = "⚡ Create Skill Family";
        createButton.Pressed += OnCreateFamily;
        vbox.AddChild(createButton);
    }

    private LineEdit CreateLineEdit(string labelText, string placeholder)
    {
        var hbox = new HBoxContainer();
        var label = new Label();
        label.Text = labelText;
        label.CustomMinimumSize = new Vector2(200, 0);
        hbox.AddChild(label);
        
        var lineEdit = new LineEdit();
        lineEdit.PlaceholderText = placeholder;
        lineEdit.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        hbox.AddChild(lineEdit);
        
        return lineEdit;
    }

    private SpinBox CreateSpinBox(string labelText, double min, double max, double value)
    {
        var hbox = new HBoxContainer();
        var label = new Label();
        label.Text = labelText;
        label.CustomMinimumSize = new Vector2(200, 0);
        hbox.AddChild(label);
        
        var spinBox = new SpinBox();
        spinBox.MinValue = min;
        spinBox.MaxValue = max;
        spinBox.Value = value;
        spinBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        hbox.AddChild(spinBox);
        
        return spinBox;
    }

    private OptionButton CreateOptionButton(string labelText, string[] options)
    {
        var hbox = new HBoxContainer();
        var label = new Label();
        label.Text = labelText;
        label.CustomMinimumSize = new Vector2(200, 0);
        hbox.AddChild(label);
        
        var optionButton = new OptionButton();
        for (int i = 0; i < options.Length; i++)
        {
            optionButton.AddItem(options[i], i);
        }
        optionButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        hbox.AddChild(optionButton);
        
        return optionButton;
    }

    private void OnCreateFamily()
    {
        string baseName = baseNameEdit.Text;
        int tiers = (int)tierCountSpin.Value;
        int basePower = (int)basePowerSpin.Value;
        int powerIncrease = (int)powerIncreaseSpin.Value;
        int mpCostBase = (int)mpCostBaseSpin.Value;
        int mpIncrease = (int)mpCostIncreaseSpin.Value;
        
        string[] tierNames = new string[] { "I", "II", "III", "IV", "V" };
        
        for (int i = 0; i < tiers; i++)
        {
            var skill = new EchoesAcrossTime.Combat.SkillData();
            skill.SkillId = $"{baseName.ToLower()}_{i + 1}";
            skill.DisplayName = $"{baseName} {tierNames[i]}";
            skill.Description = $"Tier {i + 1} {baseName} spell.";
            skill.Element = (EchoesAcrossTime.Combat.ElementType)(elementOption.Selected + 1);
            skill.BasePower = basePower + (powerIncrease * i);
            skill.MPCost = mpCostBase + (mpIncrease * i);
            skill.DamageType = EchoesAcrossTime.Combat.DamageType.Magical;
            skill.DamageFormula = EchoesAcrossTime.Combat.DamageFormulaType.Persona;
            skill.Accuracy = 95;
            skill.RequiredLevel = 1 + (i * 5);
            skill.Target = EchoesAcrossTime.Combat.SkillTarget.SingleEnemy;
            skill.Type = EchoesAcrossTime.Combat.SkillType.ActiveAttack;
            
            // Save each skill
            string path = $"res://Data/Skills/{skill.SkillId}.tres";
            var error = ResourceSaver.Save(skill, path);
            if (error == Error.Ok)
            {
                GD.Print($"✅ Created: {skill.DisplayName} at {path}");
            }
        }
        
        GD.Print($"✅ Created {tiers} skills in the {baseName} family!");
        Hide();
    }
}
#endif