using Godot;
using System;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Database;
using EchoesAcrossTime.Items;
using EchoesAcrossTime.Managers;

namespace EchoesAcrossTime.UI
{
    /// <summary>
    /// Status menu for viewing detailed character information
    /// </summary>
    public partial class StatusMenuUI : Control
    {
        #region Node References
        [Export] private ItemList characterList;
        
        // Character info
        [Export] private Label characterNameLabel;
        [Export] private Label levelLabel;
        [Export] private Label classLabel;
        [Export] private TextureRect characterPortrait;
        
        // Experience
        [Export] private Label currentExpLabel;
        [Export] private Label nextLevelExpLabel;
        [Export] private ProgressBar expProgressBar;
        
        // Stats
        [Export] private Label hpLabel;
        [Export] private Label mpLabel;
        [Export] private Label attackLabel;
        [Export] private Label defenseLabel;
        [Export] private Label magicAttackLabel;
        [Export] private Label magicDefenseLabel;
        [Export] private Label speedLabel;
        [Export] private Label luckLabel;
        [Export] private Label criticalRateLabel;
        [Export] private Label evasionRateLabel;
        
        // Equipment bonuses
        [Export] private Label equipmentBonusesLabel;
        
        // Element affinities
        [Export] private VBoxContainer elementAffinitiesContainer;
        
        // Status effects
        [Export] private Label statusEffectsLabel;
        [Export] public VBoxContainer statusEffectsContainer { get; set; }
        
        [Export] private Button closeButton;
        #endregion
        
        #region State
        private CharacterStats selectedCharacter;
        #endregion
        
        public override void _Ready()
        {
            // Connect signals
            characterList.ItemSelected += OnCharacterSelected;
            closeButton.Pressed += OnClosePressed;
            
            Hide();
        }
        
        public void OpenMenu()
        {
            Show();
            RefreshCharacterList();
            
            // Select first character
            if (characterList.ItemCount > 0)
            {
                characterList.Select(0);
                OnCharacterSelected(0);
            }
        }
        
        private void RefreshCharacterList()
        {
            characterList.Clear();
            
            // FIXED: Use PartyMenuManager instead of Party.PartyManager
            var party = PartyMenuManager.Instance?.GetMainPartyStats();
            if (party == null) return;
            
            foreach (var stats in party)
            {
                characterList.AddItem($"{stats.CharacterName} - Lv {stats.Level}");
            }
        }
        
        private void OnCharacterSelected(long index)
        {
            // FIXED: Use PartyMenuManager and proper comparison
            var party = PartyMenuManager.Instance?.GetMainPartyStats();
            if (party == null || index >= party.Count) return;
            
            selectedCharacter = party[(int)index];
            RefreshCharacterDisplay();
            
            Managers.SystemManager.Instance?.PlayCursorSE();
        }
        
        private void RefreshCharacterDisplay()
        {
            if (selectedCharacter == null) return;
            
            // Get CharacterData from GameManager database
            CharacterData characterData = null;
            if (GameManager.Instance?.Database != null)
            {
                characterData = GameManager.Instance.Database.GetCharacter(selectedCharacter.CharacterId);
            }
            
           // Basic info
            characterNameLabel.Text = selectedCharacter.CharacterName;
            levelLabel.Text = $"Lv. {selectedCharacter.Level}";
            
            // Class - Use CharacterData.Class property
            if (characterData != null)
            {
                classLabel.Text = characterData.Class.ToString();
            }
            else
            {
                classLabel.Text = "Unknown"; // Fallback if no CharacterData found
            }
            
            // Portrait - Use CharacterData.GetPortrait() method
            if (characterData != null && characterPortrait != null)
            {
                var portrait = characterData.GetPortrait(PortraitType.Menu);
                if (portrait != null)
                {
                    characterPortrait.Texture = portrait;
                    characterPortrait.Visible = true;
                }
                else
                {
                    characterPortrait.Visible = false;
                }
            }
            
            // Experience
            currentExpLabel.Text = $"EXP: {selectedCharacter.CurrentExp}";
            nextLevelExpLabel.Text = $"Next: {selectedCharacter.ExpToNextLevel}";
            
            if (selectedCharacter.ExpToNextLevel > 0)
            {
                float expProgress = (float)selectedCharacter.CurrentExp / 
                    (selectedCharacter.CurrentExp + selectedCharacter.ExpToNextLevel);
                expProgressBar.Value = expProgress * 100;
            }
            else
            {
                expProgressBar.Value = 100; // Max level
            }
            
            // Stats
            hpLabel.Text = $"HP: {selectedCharacter.CurrentHP} / {selectedCharacter.MaxHP}";
            mpLabel.Text = $"MP: {selectedCharacter.CurrentMP} / {selectedCharacter.MaxMP}";
            attackLabel.Text = $"Attack: {selectedCharacter.Attack}";
            defenseLabel.Text = $"Defense: {selectedCharacter.Defense}";
            magicAttackLabel.Text = $"Magic Attack: {selectedCharacter.MagicAttack}";
            magicDefenseLabel.Text = $"Magic Defense: {selectedCharacter.MagicDefense}";
            speedLabel.Text = $"Speed: {selectedCharacter.Speed}";
            
            // Battle stats
            luckLabel.Text = $"Luck: {selectedCharacter.Luck}";
            criticalRateLabel.Text = $"Critical Rate: {selectedCharacter.BattleStats?.CriticalRate ?? 0}%";
            evasionRateLabel.Text = $"Evasion Rate: {selectedCharacter.BattleStats?.EvasionRate ?? 0}%";
            
            // Equipment bonuses
            DisplayEquipmentBonuses();
            
            // Element affinities
            DisplayElementAffinities();
            
            // Status effects
            DisplayStatusEffects();
        }
        
        private void DisplayEquipmentBonuses()
        {
            if (selectedCharacter == null) return;
            
            // Use CharacterId from CharacterStats (already exists)
            var bonuses = EquipmentManager.Instance?.GetCharacterBonuses(selectedCharacter.CharacterId) 
                          ?? new EquipmentBonuses();
            
            // Display equipment bonuses
            if (equipmentBonusesLabel != null)
            {
                var bonusText = "";
                
                if (bonuses.AttackBonus > 0) bonusText += $"+{bonuses.AttackBonus} ATK ";
                if (bonuses.DefenseBonus > 0) bonusText += $"+{bonuses.DefenseBonus} DEF ";
                if (bonuses.MagicAttackBonus > 0) bonusText += $"+{bonuses.MagicAttackBonus} MAG ";
                if (bonuses.MagicDefenseBonus > 0) bonusText += $"+{bonuses.MagicDefenseBonus} MDF ";
                if (bonuses.SpeedBonus > 0) bonusText += $"+{bonuses.SpeedBonus} SPD ";
                if (bonuses.LuckBonus > 0) bonusText += $"+{bonuses.LuckBonus} LCK ";
                if (bonuses.MaxHPBonus > 0) bonusText += $"+{bonuses.MaxHPBonus} HP ";
                if (bonuses.MaxMPBonus > 0) bonusText += $"+{bonuses.MaxMPBonus} MP ";
                
                equipmentBonusesLabel.Text = string.IsNullOrEmpty(bonusText) 
                    ? "No equipment bonuses" 
                    : $"Equipment: {bonusText.Trim()}";
            }
        }
        
        private void DisplayElementAffinities()
        {
            if (selectedCharacter == null || selectedCharacter.ElementAffinities == null) return;
            
            if (elementAffinitiesContainer != null)
            {
                // Clear existing children
                foreach (Node child in elementAffinitiesContainer.GetChildren())
                {
                    child.QueueFree();
                }
                
                // Display affinities
                var affinities = selectedCharacter.ElementAffinities;
                
                AddAffinityLabel("Physical", affinities.PhysicalAffinity);
                AddAffinityLabel("Fire", affinities.FireAffinity);
                AddAffinityLabel("Ice", affinities.IceAffinity);
                AddAffinityLabel("Electric", affinities.ThunderAffinity);
                AddAffinityLabel("Wind", affinities.WindAffinity);
                AddAffinityLabel("Light", affinities.LightAffinity);
                AddAffinityLabel("Dark", affinities.DarkAffinity);
                AddAffinityLabel("Almighty", affinities.AlmightyAffinity);
            }
        }
        
        private void AddAffinityLabel(string element, ElementAffinity affinity)
        {
            if (affinity == ElementAffinity.Normal) return; // Don't show neutral affinities
            
            var label = new Label();
            label.Text = $"{element}: {GetAffinityText(affinity)}";
            label.AddThemeColorOverride("font_color", GetAffinityColor(affinity));
            elementAffinitiesContainer.AddChild(label);
        }
        
        private string GetAffinityText(ElementAffinity affinity)
        {
            return affinity switch
            {
                ElementAffinity.Weak => "Weak",
                ElementAffinity.Resist => "Resist",
                ElementAffinity.Immune => "Immune",
                ElementAffinity.Absorb => "Absorb",
                ElementAffinity.Reflect => "Reflect",
                _ => "Normal"
            };
        }
        
        private Color GetAffinityColor(ElementAffinity affinity)
        {
            return affinity switch
            {
                ElementAffinity.Weak => new Color(1.0f, 0.3f, 0.3f), // Red
                ElementAffinity.Resist => new Color(0.5f, 0.5f, 1.0f), // Blue
                ElementAffinity.Immune => new Color(0.7f, 0.7f, 0.7f), // Gray
                ElementAffinity.Absorb => new Color(0.3f, 1.0f, 0.3f), // Green
                ElementAffinity.Reflect => new Color(1.0f, 1.0f, 0.3f), // Yellow
                _ => Colors.White
            };
        }

        private void DisplayStatusEffects()
        {
            if (selectedCharacter == null) return;

            if (statusEffectsContainer != null)
            {
                // Clear existing children
                foreach (Node child in statusEffectsContainer.GetChildren())
                {
                    child.QueueFree();
                }

                // Display active status effects
                if (selectedCharacter.ActiveStatuses != null && selectedCharacter.ActiveStatuses.Count > 0)
                {
                    foreach (var status in selectedCharacter.ActiveStatuses)
                    {
                        var label = new Label();
                        // Fixed: StatusEffect is an enum, use it directly. Duration is the correct property.
                        label.Text = $"{status.Effect} ({status.Duration} turns)";
                        statusEffectsContainer.AddChild(label);
                    }
                }
                else
                {
                    var label = new Label();
                    label.Text = "No status effects";
                    label.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
                    statusEffectsContainer.AddChild(label);
                }
            }
        }

        private void OnClosePressed()
        {
            Hide();
            Managers.SystemManager.Instance?.PlayCancelSE();
        }
        
        public override void _Input(InputEvent @event)
        {
            if (!Visible) return;
            
            if (@event.IsActionPressed("ui_cancel"))
            {
                OnClosePressed();
                GetViewport().SetInputAsHandled();
            }
        }
    }
}