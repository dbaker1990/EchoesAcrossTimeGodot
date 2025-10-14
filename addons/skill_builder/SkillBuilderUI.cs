#if TOOLS
using Godot;
using System;

[Tool]
public partial class SkillBuilderUI : Control
{
    // Node references (these will be set automatically from the scene)
    private LineEdit skillIdEdit;
    private LineEdit displayNameEdit;
    private TextEdit descriptionEdit;
    private OptionButton skillTypeOption;
    private OptionButton targetOption;
    private SpinBox hitsSpin;
    private SpinBox mpCostSpin;
    private SpinBox hpCostSpin;
    private SpinBox goldCostSpin;
    
    // Buttons
    private Button saveButton;
    private Button loadButton;
    private Button duplicateButton;
    private Button batchCreateButton;
    private Button clearButton;
    
    // Current skill being edited
    private EchoesAcrossTime.Combat.SkillData currentSkill;

    public override void _Ready()
    {
        GD.Print("=== SkillBuilderUI _Ready() called ===");
        
        // Get references to nodes from the scene
        GetNodeReferences();
        
        // Connect button signals
        ConnectSignals();
        
        // Create a new skill to start
        CreateNewSkill();
        
        GD.Print("=== SkillBuilderUI ready! ===");
    }

    private void GetNodeReferences()
    {
        // Get nodes by their paths in the scene tree
        // Adjust these paths based on your actual scene structure
        
        // Basic Info
        skillIdEdit = GetNode<LineEdit>("MainScroll/MainContainer/BasicInfoSection/SkillIDRow/SkillIDEdit");
        displayNameEdit = GetNode<LineEdit>("MainScroll/MainContainer/BasicInfoSection/DisplayNameRow/DisplayNameEdit");
        descriptionEdit = GetNode<TextEdit>("MainScroll/MainContainer/BasicInfoSection/DescriptionEdit");
        skillTypeOption = GetNode<OptionButton>("MainScroll/MainContainer/BasicInfoSection/SkillTypeRow/SkillTypeOption");
        
        // Targeting
        targetOption = GetNode<OptionButton>("MainScroll/MainContainer/TargetingSection/TargetRow/TargetOption");
        hitsSpin = GetNode<SpinBox>("MainScroll/MainContainer/TargetingSection/HitsRow/HitsSpin");
        
        // Costs
        mpCostSpin = GetNode<SpinBox>("MainScroll/MainContainer/CostsSection/MPCostRow/MPCostSpin");
        hpCostSpin = GetNode<SpinBox>("MainScroll/MainContainer/CostsSection/HPCostRow/HPCostSpin");
        goldCostSpin = GetNode<SpinBox>("MainScroll/MainContainer/CostsSection/GoldCostRow/GoldCostSpin");
        
        // Buttons
        saveButton = GetNode<Button>("MainScroll/MainContainer/ButtonRow1/SaveButton");
        loadButton = GetNode<Button>("MainScroll/MainContainer/ButtonRow1/LoadButton");
        duplicateButton = GetNode<Button>("MainScroll/MainContainer/ButtonRow1/DuplicateButton");
        batchCreateButton = GetNode<Button>("MainScroll/MainContainer/ButtonRow2/BatchCreateButton");
        clearButton = GetNode<Button>("MainScroll/MainContainer/ButtonRow2/ClearButton");
        
        GD.Print("✅ All node references obtained successfully!");
    }

    private void ConnectSignals()
    {
        // Connect button pressed signals
        saveButton.Pressed += OnSaveSkill;
        loadButton.Pressed += OnLoadSkill;
        duplicateButton.Pressed += OnDuplicateSkill;
        batchCreateButton.Pressed += OnBatchCreate;
        clearButton.Pressed += OnClearAll;
        
        GD.Print("✅ All signals connected!");
    }

    private void CreateNewSkill()
    {
        currentSkill = new EchoesAcrossTime.Combat.SkillData();
        LoadSkillToUI();
        GD.Print("✅ New skill created!");
    }

    private void LoadSkillToUI()
    {
        if (currentSkill == null) return;
        
        // Load skill data into UI fields
        skillIdEdit.Text = currentSkill.SkillId ?? "";
        displayNameEdit.Text = currentSkill.DisplayName ?? "";
        descriptionEdit.Text = currentSkill.Description ?? "";
        skillTypeOption.Selected = (int)currentSkill.Type;
        targetOption.Selected = (int)currentSkill.Target;
        hitsSpin.Value = currentSkill.NumberOfHits;
        mpCostSpin.Value = currentSkill.MPCost;
        hpCostSpin.Value = currentSkill.HPCost;
        goldCostSpin.Value = currentSkill.GoldCost;
    }

    private void UpdateSkillFromUI()
    {
        if (currentSkill == null) return;
        
        // Update skill data from UI fields
        currentSkill.SkillId = skillIdEdit.Text;
        currentSkill.DisplayName = displayNameEdit.Text;
        currentSkill.Description = descriptionEdit.Text;
        currentSkill.Type = (EchoesAcrossTime.Combat.SkillType)skillTypeOption.Selected;
        currentSkill.Target = (EchoesAcrossTime.Combat.SkillTarget)targetOption.Selected;
        currentSkill.NumberOfHits = (int)hitsSpin.Value;
        currentSkill.MPCost = (int)mpCostSpin.Value;
        currentSkill.HPCost = (int)hpCostSpin.Value;
        currentSkill.GoldCost = (int)goldCostSpin.Value;
    }

    // Button handlers
    private void OnSaveSkill()
    {
        GD.Print("💾 Save button clicked!");
        UpdateSkillFromUI();
        
        var fileDialog = new FileDialog();
        fileDialog.FileMode = FileDialog.FileModeEnum.SaveFile;
        fileDialog.Access = FileDialog.AccessEnum.Resources;
        fileDialog.AddFilter("*.tres", "Godot Resource");
        fileDialog.CurrentDir = "res://Data/Skills/";
        fileDialog.FileSelected += (path) => {
            var error = ResourceSaver.Save(currentSkill, path);
            if (error == Error.Ok)
            {
                GD.Print($"✅ Skill saved to: {path}");
            }
            else
            {
                GD.PrintErr($"❌ Failed to save: {error}");
            }
        };
        AddChild(fileDialog);
        fileDialog.PopupCentered(new Vector2I(800, 600));
    }

    private void OnLoadSkill()
    {
        GD.Print("📂 Load button clicked!");
        
        var fileDialog = new FileDialog();
        fileDialog.FileMode = FileDialog.FileModeEnum.OpenFile;
        fileDialog.Access = FileDialog.AccessEnum.Resources;
        fileDialog.AddFilter("*.tres", "Godot Resource");
        fileDialog.CurrentDir = "res://Data/Skills/";
        fileDialog.FileSelected += (path) => {
            var skill = GD.Load<EchoesAcrossTime.Combat.SkillData>(path);
            if (skill != null)
            {
                currentSkill = skill;
                LoadSkillToUI();
                GD.Print($"✅ Skill loaded: {skill.DisplayName}");
            }
        };
        AddChild(fileDialog);
        fileDialog.PopupCentered(new Vector2I(800, 600));
    }

    private void OnDuplicateSkill()
    {
        GD.Print("📋 Duplicate button clicked!");
        UpdateSkillFromUI();
        
        var duplicate = currentSkill.Duplicate() as EchoesAcrossTime.Combat.SkillData;
        duplicate.SkillId += "_copy";
        duplicate.DisplayName += " (Copy)";
        
        currentSkill = duplicate;
        LoadSkillToUI();
        
        GD.Print("✅ Skill duplicated!");
    }

    private void OnBatchCreate()
    {
        GD.Print("⚡ Batch create button clicked!");
        GD.Print("Batch create dialog will be implemented next!");
    }

    private void OnClearAll()
    {
        GD.Print("🗑️ Clear button clicked!");
        CreateNewSkill();
    }
}
#endif