using Godot;
using System;
using EchoesAcrossTime.Combat;

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
            
            var party = Party.PartyManager.Instance?.GetMainParty();
            if (party == null) return;
            
            foreach (var stats in party)
            {
                characterList.AddItem($"{stats.CharacterName} - Lv {stats.Level}");
            }
        }
        
        private void OnCharacterSelected(long index)
        {
            var party = Party.PartyManager.Instance?.GetMainParty();
            if (party == null || index >= party.Count) return;
            
            selectedCharacter = party[(int)index];
            RefreshCharacterDisplay();
            
            Managers.SystemManager.Instance?.PlayCursorSE();
        }
        
        private void RefreshCharacterDisplay()
        {
            if (selectedCharacter == null) return;
            
            // Basic info
            characterNameLabel.Text = selectedCharacter.CharacterName;
            levelLabel.Text = $"Level {selectedCharacter.Level}";
            classLabel.Text = selectedCharacter.CharacterData?.CharacterClass ?? "---";
            
            // Portrait
            if (selectedCharacter.CharacterData?.Portrait != null)
            {
                characterPortrait.Texture = selectedCharacter.CharacterData.Portrait;
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
            luckLabel.Text = $"Luck: {selectedCharacter.Luck}";
            criticalRateLabel.Text = $"Critical Rate: {selectedCharacter.CriticalRate}%";
            evasionRateLabel.Text = $"Evasion Rate: {selectedCharacter.EvasionRate}%";
            
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
            
            var bonuses = selectedCharacter.Equipment.GetTotalBonuses();
            var bonusList = new System.Collections.Generic.List<string>();
            
            if (bonuses.MaxHPBonus > 0) bonusList.Add($"HP+{bonuses.MaxHPBonus}");
            if (bonuses.MaxMPBonus > 0) bonusList.Add($"MP+{bonuses.MaxMPBonus}");
            if (bonuses.AttackBonus > 0) bonusList.Add($"ATK+{bonuses.AttackBonus}");
            if (bonuses.DefenseBonus > 0) bonusList.Add($"DEF+{bonuses.DefenseBonus}");
            if (bonuses.MagicAttackBonus > 0) bonusList.Add($"M.ATK+{bonuses.MagicAttackBonus}");
            if (bonuses.MagicDefenseBonus > 0) bonusList.Add($"M.DEF+{bonuses.MagicDefenseBonus}");
            if (bonuses.SpeedBonus > 0) bonusList.Add($"SPD+{bonuses.SpeedBonus}");
            
            equipmentBonusesLabel.Text = bonusList.Count > 0 
                ? $"Equipment Bonuses: {string.Join(", ", bonusList)}"
                : "Equipment Bonuses: None";
        }
        
        private void DisplayElementAffinities()
        {
            if (selectedCharacter == null) return;
            
            // Clear existing affinity labels
            foreach (var child in elementAffinitiesContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            // Create labels for each element
            var elements = new[]
            {
                Combat.ElementType.Physical,
                Combat.ElementType.Fire,
                Combat.ElementType.Ice,
                Combat.ElementType.Thunder,
                Combat.ElementType.Wind,
                Combat.ElementType.Light,
                Combat.ElementType.Dark,
                Combat.ElementType.Almighty
            };
            
            foreach (var element in elements)
            {
                var affinity = selectedCharacter.ElementAffinities.GetAffinity(element);
                string affinityText = affinity switch
                {
                    Combat.ElementAffinity.Weak => "Weak",
                    Combat.ElementAffinity.Resist => "Resist",
                    Combat.ElementAffinity.Null => "Null",
                    Combat.ElementAffinity.Absorb => "Absorb",
                    _ => "Normal"
                };
                
                var label = new Label();
                label.Text = $"{element}: {affinityText}";
                
                // Color code for readability
                label.AddThemeColorOverride("font_color", GetAffinityColor(affinity));
                
                elementAffinitiesContainer.AddChild(label);
            }
        }
        
        private Color GetAffinityColor(Combat.ElementAffinity affinity)
        {
            return affinity switch
            {
                Combat.ElementAffinity.Weak => new Color(1, 0.3f, 0.3f),      // Red
                Combat.ElementAffinity.Resist => new Color(0.5f, 0.5f, 1),    // Blue
                Combat.ElementAffinity.Null => new Color(0.7f, 0.7f, 0.7f),   // Gray
                Combat.ElementAffinity.Absorb => new Color(0.3f, 1, 0.3f),    // Green
                _ => new Color(1, 1, 1)                                        // White
            };
        }
        
        private void DisplayStatusEffects()
        {
            if (selectedCharacter == null) return;
            
            // Get active status effects
            var activeStatuses = selectedCharacter.StatusEffects?.GetActiveStatuses();
            
            if (activeStatuses == null || activeStatuses.Count == 0)
            {
                statusEffectsLabel.Text = "Status Effects: None";
            }
            else
            {
                var statusNames = new System.Collections.Generic.List<string>();
                foreach (var status in activeStatuses)
                {
                    statusNames.Add(status.ToString());
                }
                statusEffectsLabel.Text = $"Status Effects: {string.Join(", ", statusNames)}";
            }
        }
        
        private void OnClosePressed()
        {
            Hide();
            
            // Return to main menu
            var mainMenu = GetParent().GetNodeOrNull<MainMenuUI>("%MainMenuUI");
            mainMenu?.ReturnToMainMenu();
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