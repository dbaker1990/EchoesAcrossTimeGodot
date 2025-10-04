using Godot;
using System;

namespace EchoesAcrossTime.UI
{
    /// <summary>
    /// Helper tool for quickly building menu scene structures in code
    /// Use this as a reference or run it to auto-generate menu scenes
    /// 
    /// USAGE: Attach to a Node and run the scene, it will print GDScript code
    /// you can copy-paste into your .tscn files
    /// </summary>
    [Tool]
    public partial class MenuSceneBuilder : Node
    {
        [Export] private bool generateMainMenu;
        [Export] private bool generateItemMenu;
        [Export] private bool generateSkillMenu;
        [Export] private bool generateEquipMenu;
        [Export] private bool generateStatusMenu;
        [Export] private bool generateOptionsMenu;
        [Export] private bool generateSaveMenu;
        
        public override void _Ready()
        {
            if (Engine.IsEditorHint())
            {
                GD.Print("=== MENU SCENE BUILDER ===");
                GD.Print("Set the export bools to true and re-run to generate scene structures");
                return;
            }
            
            if (generateMainMenu) GenerateMainMenuStructure();
            if (generateItemMenu) GenerateItemMenuStructure();
            if (generateSkillMenu) GenerateSkillMenuStructure();
            if (generateEquipMenu) GenerateEquipMenuStructure();
            if (generateStatusMenu) GenerateStatusMenuStructure();
            if (generateOptionsMenu) GenerateOptionsMenuStructure();
            if (generateSaveMenu) GenerateSaveMenuStructure();
        }
        
        private void GenerateMainMenuStructure()
        {
            GD.Print("\n=== MAIN MENU STRUCTURE ===");
            GD.Print("Create this hierarchy in Godot editor:");
            GD.Print(@"
MainMenuUI (Control) [unique_name_in_owner = true]
├── CanvasLayer
│   └── ColorRect [color = Color(0,0,0,0.7)]
│       └── CenterContainer
│           └── PanelContainer (menuPanel)
│               └── MarginContainer [margins = 20 all sides]
│                   └── VBoxContainer [separation = 10]
│                       ├── Header (HBoxContainer)
│                       │   ├── LocationLabel (Label) [text = 'Location']
│                       │   ├── Control [size_flags_horizontal = 3] // Spacer
│                       │   ├── PlaytimeLabel (Label) [text = '00:00']
│                       │   └── GoldLabel (Label) [text = '0 G']
│                       ├── HSeparator
│                       └── ButtonContainer (VBoxContainer) [separation = 5]
│                           ├── ItemButton (Button) [text = 'Items', min_size = (200, 40)]
│                           ├── SkillButton (Button) [text = 'Skills', min_size = (200, 40)]
│                           ├── EquipButton (Button) [text = 'Equipment', min_size = (200, 40)]
│                           ├── StatusButton (Button) [text = 'Status', min_size = (200, 40)]
│                           ├── CraftingButton (Button) [text = 'Crafting', min_size = (200, 40)]
│                           ├── PartyButton (Button) [text = 'Party', min_size = (200, 40)]
│                           ├── BestiaryButton (Button) [text = 'Bestiary', min_size = (200, 40)]
│                           ├── QuestButton (Button) [text = 'Quests', min_size = (200, 40)]
│                           ├── OptionsButton (Button) [text = 'Options', min_size = (200, 40)]
│                           ├── SaveButton (Button) [text = 'Save', min_size = (200, 40)]
│                           ├── LoadButton (Button) [text = 'Load', min_size = (200, 40)]
│                           └── EndGameButton (Button) [text = 'End Game', min_size = (200, 40)]

Then connect all Export node paths in the inspector!
            ");
        }
        
        private void GenerateItemMenuStructure()
        {
            GD.Print("\n=== ITEM MENU STRUCTURE ===");
            GD.Print(@"
ItemMenuUI (Control) [unique_name_in_owner = true]
├── Panel [anchors preset = 15 (Full Rect)]
│   └── VBoxContainer [margins = 10, separation = 10]
│       ├── TopBar (HBoxContainer) [separation = 10]
│       │   ├── CategoryFilter (OptionButton) [min_size = (150, 0)]
│       │   ├── GoldLabel (Label) [size_flags_h = 3, text = 'Gold: 0 G']
│       │   └── CloseButton (Button) [text = 'Close']
│       ├── MainContent (HBoxContainer) [size_flags_v = 3, separation = 10]
│       │   ├── ItemList (ItemList) [min_size = (300, 0)]
│       │   └── DetailPanel (Panel) [size_flags_h = 3]
│       │       └── VBoxContainer [margins = 10]
│       │           ├── ItemNameLabel (Label) [text = 'Item Name']
│       │           ├── ItemDescriptionLabel (Label) [text = 'Description', autowrap = true]
│       │           └── ItemQuantityLabel (Label) [text = 'x0']
│       └── ButtonBar (HBoxContainer) [separation = 10]
│           ├── UseButton (Button) [text = 'Use']
│           ├── DiscardButton (Button) [text = 'Discard']
│           └── SortButton (Button) [text = 'Sort']
├── CharacterSelectionPanel (Panel) [visible = false, anchors preset = 8 (Center)]
│   └── VBoxContainer [margins = 20]
│       ├── Label [text = 'Select Character']
│       └── CharacterList (VBoxContainer)
            ");
        }
        
        private void GenerateSkillMenuStructure()
        {
            GD.Print("\n=== SKILL MENU STRUCTURE ===");
            GD.Print(@"
SkillMenuUI (Control) [unique_name_in_owner = true]
├── Panel [anchors preset = 15]
│   └── HBoxContainer [margins = 10, separation = 10]
│       ├── CharacterList (ItemList) [min_size = (200, 0)]
│       └── VBoxContainer [size_flags_h = 3, separation = 10]
│           ├── SkillTabs (TabContainer) [size_flags_v = 3]
│           │   ├── Equipped (VBoxContainer)
│           │   │   └── EquippedSkillList (ItemList)
│           │   └── Available (VBoxContainer)
│           │       └── AvailableSkillList (ItemList)
│           ├── DetailPanel (Panel) [min_size = (0, 150)]
│           │   └── VBoxContainer [margins = 10]
│           │       ├── SkillNameLabel (Label)
│           │       ├── SkillDescriptionLabel (Label) [autowrap = true]
│           │       ├── SkillMPCostLabel (Label)
│           │       ├── SkillPowerLabel (Label)
│           │       ├── SkillElementLabel (Label)
│           │       └── SkillTargetLabel (Label)
│           └── ButtonBar (HBoxContainer)
│               ├── EquipButton (Button) [text = 'Equip']
│               ├── UnequipButton (Button) [text = 'Unequip']
│               └── CloseButton (Button) [text = 'Close']
            ");
        }
        
        private void GenerateEquipMenuStructure()
        {
            GD.Print("\n=== EQUIPMENT MENU STRUCTURE ===");
            GD.Print(@"
EquipMenuUI (Control) [unique_name_in_owner = true]
├── Panel [anchors preset = 15]
│   └── HBoxContainer [margins = 10, separation = 10]
│       ├── CharacterList (ItemList) [min_size = (200, 0)]
│       └── VBoxContainer [size_flags_h = 3, separation = 10]
│           ├── CharacterInfo (HBoxContainer)
│           │   ├── CharacterPortrait (TextureRect) [min_size = (100, 100)]
│           │   └── VBoxContainer
│           │       ├── CharacterNameLabel (Label) [font_size = 24]
│           │       └── LevelLabel (Label)
│           ├── EquipmentSlots (VBoxContainer) [separation = 5]
│           │   ├── WeaponSlot (Button) [text = 'Weapon: (Empty)', min_size = (0, 40)]
│           │   ├── ArmorSlot (Button) [text = 'Armor: (Empty)', min_size = (0, 40)]
│           │   ├── Accessory1Slot (Button) [text = 'Accessory 1: (Empty)', min_size = (0, 40)]
│           │   └── Accessory2Slot (Button) [text = 'Accessory 2: (Empty)', min_size = (0, 40)]
│           ├── StatsPanel (Panel)
│           │   └── GridContainer [columns = 2, margins = 10]
│           │       ├── Label [text = 'HP:'] + HPLabel
│           │       ├── Label [text = 'MP:'] + MPLabel
│           │       ├── Label [text = 'ATK:'] + AttackLabel
│           │       ├── Label [text = 'DEF:'] + DefenseLabel
│           │       ├── Label [text = 'M.ATK:'] + MagicAttackLabel
│           │       ├── Label [text = 'M.DEF:'] + MagicDefenseLabel
│           │       └── Label [text = 'SPD:'] + SpeedLabel
│           └── CloseButton (Button) [text = 'Close']
├── EquipmentSelectionPanel (Panel) [visible = false, anchors preset = 8]
│   └── VBoxContainer [min_size = (400, 500), margins = 20]
│       ├── Label [text = 'Select Equipment', font_size = 20]
│       ├── EquipmentList (ItemList) [size_flags_v = 3]
│       ├── DetailPanel (Panel) [min_size = (0, 120)]
│       │   └── VBoxContainer [margins = 10]
│       │       ├── EquipmentNameLabel (Label)
│       │       ├── EquipmentDescriptionLabel (Label) [autowrap = true]
│       │       └── EquipmentStatsLabel (Label)
│       └── ButtonBar (HBoxContainer)
│           ├── EquipConfirmButton (Button) [text = 'Equip', size_flags_h = 3]
│           └── EquipCancelButton (Button) [text = 'Cancel']
            ");
        }
        
        private void GenerateStatusMenuStructure()
        {
            GD.Print("\n=== STATUS MENU STRUCTURE ===");
            GD.Print(@"
StatusMenuUI (Control) [unique_name_in_owner = true]
├── Panel [anchors preset = 15]
│   └── HBoxContainer [margins = 10, separation = 10]
│       ├── CharacterList (ItemList) [min_size = (200, 0)]
│       └── ScrollContainer [size_flags_h = 3]
│           └── VBoxContainer [separation = 15]
│               ├── CharacterInfo (HBoxContainer)
│               │   ├── CharacterPortrait (TextureRect) [min_size = (150, 150)]
│               │   └── VBoxContainer
│               │       ├── CharacterNameLabel (Label) [font_size = 28]
│               │       ├── LevelLabel (Label) [font_size = 20]
│               │       └── ClassLabel (Label)
│               ├── ExpPanel (Panel)
│               │   └── VBoxContainer [margins = 10]
│               │       ├── CurrentExpLabel (Label)
│               │       ├── NextLevelExpLabel (Label)
│               │       └── ExpProgressBar (ProgressBar)
│               ├── StatsPanel (Panel)
│               │   └── GridContainer [columns = 2, margins = 10, h_separation = 20]
│               │       ├── All stat labels (HP, MP, ATK, DEF, etc.)
│               ├── EquipmentBonusesLabel (Label)
│               ├── Label [text = 'Element Affinities', font_size = 18]
│               ├── ElementAffinitiesContainer (VBoxContainer)
│               ├── StatusEffectsLabel (Label)
│               └── CloseButton (Button) [text = 'Close']
            ");
        }
        
        private void GenerateOptionsMenuStructure()
        {
            GD.Print("\n=== OPTIONS MENU STRUCTURE ===");
            GD.Print(@"
OptionsMenuUI (Control) [unique_name_in_owner = true]
├── Panel [anchors preset = 15]
│   └── VBoxContainer [margins = 20, separation = 10]
│       ├── Label [text = 'Options', font_size = 32]
│       ├── TabContainer [size_flags_v = 3]
│       │   ├── Audio (VBoxContainer) [separation = 15]
│       │   │   ├── Label + BGMVolumeSlider (HSlider) + BGMVolumeLabel
│       │   │   ├── Label + SFXVolumeSlider (HSlider) + SFXVolumeLabel
│       │   │   └── Label + VoiceVolumeSlider (HSlider) + VoiceVolumeLabel
│       │   ├── Display (VBoxContainer) [separation = 15]
│       │   │   ├── FullscreenCheckbox (CheckButton) [text = 'Fullscreen']
│       │   │   ├── Label + WindowSizeDropdown (OptionButton)
│       │   │   └── VSyncCheckbox (CheckButton) [text = 'VSync']
│       │   ├── Gameplay (VBoxContainer) [separation = 15]
│       │   │   ├── Label + TextSpeedSlider (HSlider) + TextSpeedLabel
│       │   │   ├── AutoSaveCheckbox (CheckButton)
│       │   │   ├── ShowTutorialsCheckbox (CheckButton)
│       │   │   └── BattleAnimationsCheckbox (CheckButton)
│       │   └── Controls (VBoxContainer)
│       │       └── ControlsDisplay (RichTextLabel) [bbcode_enabled = true]
│       └── ButtonBar (HBoxContainer) [separation = 10]
│           ├── ApplyButton (Button) [text = 'Apply', size_flags_h = 3]
│           ├── DefaultsButton (Button) [text = 'Defaults']
│           └── CloseButton (Button) [text = 'Close']
            ");
        }
        
        private void GenerateSaveMenuStructure()
        {
            GD.Print("\n=== SAVE/LOAD MENU STRUCTURE ===");
            GD.Print(@"
SaveMenuUI (Control) [unique_name_in_owner = true]
LoadMenuUI (Control) [unique_name_in_owner = true]
├── Panel [anchors preset = 15]
│   └── HBoxContainer [margins = 10, separation = 10]
│       ├── SaveSlotList (ItemList) [min_size = (250, 0)]
│       └── VBoxContainer [size_flags_h = 3, separation = 10]
│           ├── SaveDetailPanel (Panel) [size_flags_v = 3]
│           │   └── VBoxContainer [margins = 15, separation = 10]
│           │       ├── SlotLabel (Label) [font_size = 24]
│           │       ├── PreviewImage (TextureRect) [min_size = (320, 180)]
│           │       ├── LocationLabel (Label)
│           │       ├── PlaytimeLabel (Label)
│           │       ├── SaveDateLabel (Label)
│           │       ├── LevelLabel (Label)
│           │       └── GoldLabel (Label)
│           └── ButtonBar (HBoxContainer)
│               ├── SaveButton/LoadButton (Button) [size_flags_h = 3]
│               ├── DeleteButton (Button)
│               └── CloseButton (Button)
├── ConfirmationPanel (Panel) [visible = false, anchors preset = 8]
│   └── VBoxContainer [min_size = (300, 150), margins = 20]
│       ├── ConfirmationText (Label) [autowrap = true]
│       └── HBoxContainer
│           ├── ConfirmYesButton (Button) [text = 'Yes', size_flags_h = 3]
│           └── ConfirmNoButton (Button) [text = 'No', size_flags_h = 3]
            ");
        }
    }
}