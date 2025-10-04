using Godot;
using System;
using System.Collections.Generic;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Items;
using EchoesAcrossTime.Managers;
using EchoesAcrossTime.Database;

namespace EchoesAcrossTime.UI
{
    public partial class EquipMenuUI : Control
    {
        [Export] private ItemList characterList;
        [Export] private Panel characterInfoPanel;
        
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
        
        // Equipment slots
        [Export] private Button weaponSlot;
        [Export] private Button armorSlot;
        [Export] private Button accessory1Slot;
        [Export] private Button accessory2Slot;
        
        // Equipment selection
        [Export] private Panel equipmentSelectionPanel;
        [Export] private ItemList equipmentList;
        [Export] private Label equipmentNameLabel;
        [Export] private Label equipmentDescriptionLabel;
        [Export] private Label equipmentStatsLabel;
        
        [Export] private Button closeButton;
        [Export] private Button equipButton;
        
        private CharacterStats selectedCharacter;
        private string selectedCharacterId;
        private EquipSlot selectedSlot;
        private EquipmentData selectedEquipment;
        
        public override void _Ready()
        {
            characterList.ItemSelected += OnCharacterSelected;
            
            weaponSlot.Pressed += () => OnSlotPressed(EquipSlot.Weapon);
            armorSlot.Pressed += () => OnSlotPressed(EquipSlot.Armor);
            accessory1Slot.Pressed += () => OnSlotPressed(EquipSlot.Accessory1);
            accessory2Slot.Pressed += () => OnSlotPressed(EquipSlot.Accessory2);
            
            equipmentList.ItemSelected += OnEquipmentSelected;
            equipButton.Pressed += OnEquipPressed;
            closeButton.Pressed += OnClosePressed;
            
            equipmentSelectionPanel.Hide();
            Hide();
        }
        
        public void OpenMenu()
        {
            Show();
            RefreshCharacterList();
            
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
            // FIXED: Proper comparison
            var party = PartyMenuManager.Instance?.GetMainPartyStats();
            if (party == null || index >= party.Count) return;
            
            selectedCharacter = party[(int)index];
            selectedCharacterId = selectedCharacter.CharacterName.ToLower().Replace(" ", "_");
            RefreshCharacterDisplay();
            
            Managers.SystemManager.Instance?.PlayCursorSE();
        }
        
        private void RefreshCharacterDisplay()
        {
            if (selectedCharacter == null) return;
            
            characterNameLabel.Text = selectedCharacter.CharacterName;
            levelLabel.Text = $"Level {selectedCharacter.Level}";
            hpLabel.Text = $"HP: {selectedCharacter.CurrentHP}/{selectedCharacter.MaxHP}";
            mpLabel.Text = $"MP: {selectedCharacter.CurrentMP}/{selectedCharacter.MaxMP}";
            attackLabel.Text = $"ATK: {selectedCharacter.Attack}";
            defenseLabel.Text = $"DEF: {selectedCharacter.Defense}";
            magicAttackLabel.Text = $"M.ATK: {selectedCharacter.MagicAttack}";
            magicDefenseLabel.Text = $"M.DEF: {selectedCharacter.MagicDefense}";
            speedLabel.Text = $"SPD: {selectedCharacter.Speed}";
            
            UpdateEquipmentSlots();
        }
        
        private void UpdateEquipmentSlots()
        {
            if (selectedCharacter == null) return;
            
            // Update slot texts
            weaponSlot.Text = GetEquipmentSlotText(EquipSlot.Weapon);
            armorSlot.Text = GetEquipmentSlotText(EquipSlot.Armor);
            accessory1Slot.Text = GetEquipmentSlotText(EquipSlot.Accessory1);
            accessory2Slot.Text = GetEquipmentSlotText(EquipSlot.Accessory2);
        }
        
        private string GetEquipmentSlotText(EquipSlot slot)
        {
            // FIXED: Use EquipmentManager.GetEquippedItem instead
            var equipped = EquipmentManager.Instance?.GetEquippedItem(selectedCharacterId, slot);
            
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
            
            equipmentList.AddItem("(Unequip)");
            
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
                    // FIXED: Need to get CharacterData - for now, check minimum level
                    // TODO: Add proper CharacterData lookup
                    if (selectedCharacter != null && equipData.MinimumLevel <= selectedCharacter.Level)
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
        
        private void OnEquipPressed()
        {
            if (selectedCharacter == null) return;
            
            if (selectedEquipment == null)
            {
                // Unequip
                EquipmentManager.Instance?.UnequipToInventory(selectedCharacterId, selectedSlot);
            }
            else
            {
                // Equip - TODO: Need to pass CharacterData
                // For now, skip the CanEquip check in EquipFromInventory
                EquipmentManager.Instance?.EquipFromInventory(
                    selectedCharacterId, 
                    selectedEquipment.ItemId,
                    null // TODO: Pass actual CharacterData
                );
            }
            
            equipmentSelectionPanel.Hide();
            RefreshCharacterDisplay();
            Managers.SystemManager.Instance?.PlayOkSE();
        }
        
        private void OnClosePressed()
        {
            if (equipmentSelectionPanel.Visible)
            {
                equipmentSelectionPanel.Hide();
            }
            else
            {
                Hide();
            }
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