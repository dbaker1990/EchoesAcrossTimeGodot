using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Items;

namespace EchoesAcrossTime.UI
{
    /// <summary>
    /// Equipment menu for managing character equipment
    /// </summary>
    public partial class EquipMenuUI : Control
    {
        #region Node References
        [Export] private ItemList characterList;
        
        // Equipment slots
        [Export] private Button weaponSlot;
        [Export] private Button armorSlot;
        [Export] private Button accessory1Slot;
        [Export] private Button accessory2Slot;
        
        // Character stats display
        [Export] private Label characterNameLabel;
        [Export] private Label levelLabel;
        [Export] private Label hpLabel;
        [Export] private Label mpLabel;
        [Export] private Label attackLabel;
        [Export] private Label defenseLabel;
        [Export] private Label magicAttackLabel;
        [Export] private Label magicDefenseLabel;
        [Export] private Label speedLabel;
        
        // Equipment selection panel
        [Export] private Control equipmentSelectionPanel;
        [Export] private ItemList equipmentList;
        [Export] private Label equipmentNameLabel;
        [Export] private Label equipmentDescriptionLabel;
        [Export] private Label equipmentStatsLabel;
        [Export] private Button equipConfirmButton;
        [Export] private Button equipCancelButton;
        
        [Export] private Button closeButton;
        #endregion
        
        #region State
        private CharacterStats selectedCharacter;
        private EquipSlot selectedSlot;
        private EquipmentData selectedEquipment;
        #endregion
        
        public override void _Ready()
        {
            // Connect signals
            characterList.ItemSelected += OnCharacterSelected;
            weaponSlot.Pressed += () => OnSlotPressed(EquipSlot.Weapon);
            armorSlot.Pressed += () => OnSlotPressed(EquipSlot.Armor);
            accessory1Slot.Pressed += () => OnSlotPressed(EquipSlot.Accessory1);
            accessory2Slot.Pressed += () => OnSlotPressed(EquipSlot.Accessory2);
            equipmentList.ItemSelected += OnEquipmentSelected;
            equipConfirmButton.Pressed += OnEquipConfirmed;
            equipCancelButton.Pressed += OnEquipCancelled;
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
            
            // Update character info
            characterNameLabel.Text = selectedCharacter.CharacterName;
            levelLabel.Text = $"Lv {selectedCharacter.Level}";
            
            // Update stats
            hpLabel.Text = $"HP: {selectedCharacter.CurrentHP}/{selectedCharacter.MaxHP}";
            mpLabel.Text = $"MP: {selectedCharacter.CurrentMP}/{selectedCharacter.MaxMP}";
            attackLabel.Text = $"ATK: {selectedCharacter.Attack}";
            defenseLabel.Text = $"DEF: {selectedCharacter.Defense}";
            magicAttackLabel.Text = $"M.ATK: {selectedCharacter.MagicAttack}";
            magicDefenseLabel.Text = $"M.DEF: {selectedCharacter.MagicDefense}";
            speedLabel.Text = $"SPD: {selectedCharacter.Speed}";
            
            // Update equipment slots
            UpdateEquipmentSlots();
        }
        
        private void UpdateEquipmentSlots()
        {
            if (selectedCharacter == null) return;
            
            var equipment = selectedCharacter.Equipment;
            
            // Update slot texts
            weaponSlot.Text = GetEquipmentSlotText(equipment, EquipSlot.Weapon);
            armorSlot.Text = GetEquipmentSlotText(equipment, EquipSlot.Armor);
            accessory1Slot.Text = GetEquipmentSlotText(equipment, EquipSlot.Accessory1);
            accessory2Slot.Text = GetEquipmentSlotText(equipment, EquipSlot.Accessory2);
        }
        
        private string GetEquipmentSlotText(EquipmentManager equipment, EquipSlot slot)
        {
            var equipped = equipment.GetEquipped(slot);
            
            if (equipped == null)
            {
                return $"{slot}: (Empty)";
            }
            
            return $"{slot}: {equipped.DisplayName}";
        }
        
        private void OnSlotPressed(EquipSlot slot)
        {
            selectedSlot = slot;
            ShowEquipmentSelection();
            Managers.SystemManager.Instance?.PlayOkSE();
        }
        
        private void ShowEquipmentSelection()
        {
            equipmentSelectionPanel.Show();
            equipmentList.Clear();
            
            // Add "Unequip" option
            equipmentList.AddItem("(Unequip)");
            
            // Get available equipment from inventory
            var availableEquipment = GetAvailableEquipment(selectedSlot);
            
            foreach (var equipment in availableEquipment)
            {
                equipmentList.AddItem(equipment.DisplayName);
            }
            
            if (equipmentList.ItemCount > 0)
            {
                equipmentList.Select(0);
                OnEquipmentSelected(0);
            }
        }
        
        private List<EquipmentData> GetAvailableEquipment(EquipSlot slot)
        {
            if (InventorySystem.Instance == null)
                return new List<EquipmentData>();
            
            var allItems = InventorySystem.Instance.GetAllItems();
            var equipment = new List<EquipmentData>();
            
            foreach (var inventorySlot in allItems)
            {
                if (inventorySlot.Item is EquipmentData equipData && equipData.Slot == slot)
                {
                    // Check if character can equip
                    if (selectedCharacter != null && equipData.CanEquip(selectedCharacter.CharacterData))
                    {
                        equipment.Add(equipData);
                    }
                }
            }
            
            return equipment;
        }
        
        private void OnEquipmentSelected(long index)
        {
            if (index == 0)
            {
                // Unequip selected
                selectedEquipment = null;
                equipmentNameLabel.Text = "(Unequip)";
                equipmentDescriptionLabel.Text = "Remove currently equipped item";
                equipmentStatsLabel.Text = "";
            }
            else
            {
                var available = GetAvailableEquipment(selectedSlot);
                if (index - 1 < available.Count)
                {
                    selectedEquipment = available[(int)(index - 1)];
                    UpdateEquipmentInfo(selectedEquipment);
                }
            }
            
            Managers.SystemManager.Instance?.PlayCursorSE();
        }
        
        private void UpdateEquipmentInfo(EquipmentData equipment)
        {
            if (equipment == null) return;
            
            equipmentNameLabel.Text = equipment.DisplayName;
            equipmentDescriptionLabel.Text = equipment.Description;
            
            // Show stat bonuses
            var bonuses = new List<string>();
            if (equipment.MaxHPBonus > 0) bonuses.Add($"HP+{equipment.MaxHPBonus}");
            if (equipment.MaxMPBonus > 0) bonuses.Add($"MP+{equipment.MaxMPBonus}");
            if (equipment.AttackBonus > 0) bonuses.Add($"ATK+{equipment.AttackBonus}");
            if (equipment.DefenseBonus > 0) bonuses.Add($"DEF+{equipment.DefenseBonus}");
            if (equipment.MagicAttackBonus > 0) bonuses.Add($"M.ATK+{equipment.MagicAttackBonus}");
            if (equipment.MagicDefenseBonus > 0) bonuses.Add($"M.DEF+{equipment.MagicDefenseBonus}");
            if (equipment.SpeedBonus > 0) bonuses.Add($"SPD+{equipment.SpeedBonus}");
            
            equipmentStatsLabel.Text = string.Join(", ", bonuses);
        }
        
        private void OnEquipConfirmed()
        {
            if (selectedCharacter == null) return;
            
            Managers.SystemManager.Instance?.PlayOkSE();
            
            if (selectedEquipment == null)
            {
                // Unequip
                EquipmentManager.Instance?.UnequipToInventory(
                    selectedCharacter.CharacterId,
                    selectedSlot
                );
            }
            else
            {
                // Equip new item
                EquipmentManager.Instance?.EquipFromInventory(
                    selectedCharacter.CharacterId,
                    selectedEquipment.ItemId,
                    selectedCharacter.CharacterData
                );
            }
            
            // Refresh display
            RefreshCharacterDisplay();
            equipmentSelectionPanel.Hide();
        }
        
        private void OnEquipCancelled()
        {
            equipmentSelectionPanel.Hide();
            Managers.SystemManager.Instance?.PlayCancelSE();
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
                if (equipmentSelectionPanel.Visible)
                {
                    OnEquipCancelled();
                }
                else
                {
                    OnClosePressed();
                }
                GetViewport().SetInputAsHandled();
            }
        }
    }
}