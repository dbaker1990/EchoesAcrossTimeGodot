using Godot;
using System;
using EchoesAcrossTime.Combat;
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
            
            // Basic info
            characterNameLabel.Text = selectedCharacter.CharacterName;
            levelLabel.Text = $"Level {selectedCharacter.Level}";
            // FIXED: Class info needs to be stored/retrieved differently
            classLabel.Text = "Fighter"; // TODO: Add class property to CharacterStats
            
            // Portrait - TODO: Add portrait reference to CharacterStats
            // if (selectedCharacter.Portrait != null)
            // {
            //     characterPortrait.Texture = selectedCharacter.Portrait;
            // }
            
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
            
            // FIXED: Access through BattleStats
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
            
            // FIXED: Use EquipmentManager to get bonuses
            // Need character ID - TODO: Add CharacterId property to CharacterStats
            var characterId = selectedCharacter.CharacterName.ToLower().Replace(" ", "_");
            var bonuses = EquipmentManager.Instance?.GetCharacterBonuses(characterId) ?? new EquipmentBonuses();
            
            var bonusList = new System.Collections.Generic.List<string>();
            
            if (bonuses.MaxHPBonus > 0) bonusList.Add($"HP+{bonuses.MaxHPBonus}");
            if (bonuses.MaxMPBonus > 0) bonusList.Add($"MP+{bonuses.MaxMPBonus}");
            if (bonuses.AttackBonus > 0) bonusList.Add($"ATK+{bonuses.AttackBonus}");
            if (bonuses.DefenseBonus > 0) bonusList.Add($"DEF+{bonuses.DefenseBonus}");
            if (bonuses.MagicAttackBonus > 0) bonusList.Add($"M.ATK+{bonuses.MagicAttackBonus}");
            if (bonuses.MagicDefenseBonus > 0) bonusList.Add($"M.DEF+{bonuses.MagicDefenseBonus}");
            if (bonuses.SpeedBonus > 0) bonusList.Add($"SPD+{bonuses.SpeedBonus}");
            
            equipmentBonusesLabel.Text = bonusList.Count > 0 
                ? string.Join(", ", bonusList) 
                : "No equipment bonuses";
        }
        
        private void DisplayElementAffinities()
        {
            if (selectedCharacter == null || elementAffinitiesContainer == null) return;
            
            // Clear existing
            foreach (var child in elementAffinitiesContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            // Display affinities
            if (selectedCharacter.ElementAffinities != null)
            {
                foreach (ElementType element in System.Enum.GetValues(typeof(ElementType)))
                {
                    if (element == ElementType.None) continue;
                    
                    var affinity = selectedCharacter.ElementAffinities.GetAffinity(element);
                    
                    if (affinity != ElementAffinity.Normal)
                    {
                        var label = new Label();
                        label.Text = $"{element}: {affinity}";
                        elementAffinitiesContainer.AddChild(label);
                    }
                }
            }
        }
        
        private void DisplayStatusEffects()
        {
            if (selectedCharacter == null) return;
            
            // FIXED: Use ActiveStatuses instead of StatusEffects
            if (selectedCharacter.ActiveStatuses == null || selectedCharacter.ActiveStatuses.Count == 0)
            {
                statusEffectsLabel.Text = "No status effects";
            }
            else
            {
                var effects = new System.Collections.Generic.List<string>();
                foreach (var status in selectedCharacter.ActiveStatuses)
                {
                    effects.Add($"{status.Effect} ({status.Duration} turns)");
                }
                statusEffectsLabel.Text = string.Join(", ", effects);
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