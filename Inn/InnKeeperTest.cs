// Inn/InnKeeperTest.cs
using Godot;
using System;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Managers;

namespace EchoesAcrossTime.Inn
{
    /// <summary>
    /// Test scene for inn keeper system
    /// Add this to a test scene to quickly verify the inn system works
    /// </summary>
    public partial class InnKeeperTest : Node2D
    {
        [Export] public int StartingGold { get; set; } = 100;
        [Export] public bool AddTestCharacters { get; set; } = true;
        
        private Label debugLabel;
        
        public override void _Ready()
        {
            GD.Print("=== Inn Keeper Test Scene ===");
            
            // Verify required systems exist
            VerifyRequiredSystems();
            
            // Setup test environment
            SetupTestParty();
            SetupTestGold();
            CreateDebugUI();
            
            GD.Print("Test scene ready! Walk up to inn keeper and press E.");
            GD.Print($"Starting gold: {StartingGold}");
        }
        
        private void VerifyRequiredSystems()
        {
            bool allGood = true;
            
            // Check MessageBox
            var messageBox = GetTree().Root.GetNodeOrNull<Node>("%MessageBox");
            if (messageBox == null)
            {
                GD.PrintErr("[Test] ERROR: MessageBox with unique name %MessageBox not found!");
                allGood = false;
            }
            else
            {
                GD.Print("[Test] ✓ MessageBox found");
            }
            
            // Check ChoiceBox
            var choiceBox = GetTree().Root.GetNodeOrNull<Node>("%ChoiceBox");
            if (choiceBox == null)
            {
                GD.PrintErr("[Test] ERROR: ChoiceBox with unique name %ChoiceBox not found!");
                allGood = false;
            }
            else
            {
                GD.Print("[Test] ✓ ChoiceBox found");
            }
            
            // Check ScreenEffects
            var screenEffects = GetTree().Root.GetNodeOrNull<Node>("%ScreenEffects");
            if (screenEffects == null)
            {
                GD.PrintErr("[Test] WARNING: ScreenEffects with unique name %ScreenEffects not found!");
                GD.Print("[Test] Screen fading will not work!");
            }
            else
            {
                GD.Print("[Test] ✓ ScreenEffects found");
            }
            
            // Check PartyMenuManager
            var partyManager = PartyMenuManager.Instance;
            if (partyManager == null)
            {
                GD.PrintErr("[Test] ERROR: PartyMenuManager not in autoload!");
                allGood = false;
            }
            else
            {
                GD.Print("[Test] ✓ PartyMenuManager found");
            }
            
            if (!allGood)
            {
                GD.PrintErr("[Test] Inn keeper will not work properly! Fix errors above.");
            }
        }
        
        private void SetupTestParty()
        {
            if (!AddTestCharacters)
            {
                GD.Print("[Test] Skipping test character setup");
                return;
            }
            
            var partyManager = PartyMenuManager.Instance;
            if (partyManager == null)
            {
                GD.PrintErr("[Test] PartyMenuManager not found! Add it to autoload.");
                return;
            }
            
            GD.Print("[Test] Adding test characters to party...");
            
            // Create test characters with low HP/MP to see restoration
            var testCharacter1 = CreateTestCharacter("TestHero", "Test Hero", 1);
            testCharacter1.CurrentHP = 50;
            testCharacter1.CurrentMP = 20;
            partyManager.AddCharacterToParty("test_hero", testCharacter1);
            
            var testCharacter2 = CreateTestCharacter("TestMage", "Test Mage", 1);
            testCharacter2.CurrentHP = 30;
            testCharacter2.CurrentMP = 10;
            partyManager.AddCharacterToParty("test_mage", testCharacter2);
            
            GD.Print($"[Test] Added 2 characters with damaged HP/MP");
            PrintPartyStatus();
        }
        
        private CharacterStats CreateTestCharacter(string id, string name, int level)
        {
            var stats = new CharacterStats
            {
                CharacterId = id,
                CharacterName = name,
                Level = level,
                MaxHP = 100,
                CurrentHP = 100,
                MaxMP = 50,
                CurrentMP = 50,
                Attack = 10,
                MagicAttack = 10,
                Defense = 10,
                MagicDefense = 10,
                Speed = 10,
                Luck = 10
            };
            
            return stats;
        }
        
        private void SetupTestGold()
        {
            // Try to set gold in available systems
            bool goldSet = false;
            
            // Try ShopManager
            var shopManager = GetNodeOrNull("/root/ShopManager");
            if (shopManager != null && shopManager.HasMethod("AddGold"))
            {
                shopManager.Call("AddGold", StartingGold);
                goldSet = true;
                GD.Print($"[Test] Set {StartingGold} gold via ShopManager");
            }
            
            // Try GameManager
            if (!goldSet)
            {
                var gameManager = GetNodeOrNull("/root/GameManager");
                if (gameManager != null && gameManager.HasMethod("AddGold"))
                {
                    gameManager.Call("AddGold", StartingGold);
                    goldSet = true;
                    GD.Print($"[Test] Set {StartingGold} gold via GameManager");
                }
            }
            
            // Try InventorySystem
            if (!goldSet)
            {
                var inventorySystem = GetNodeOrNull("/root/InventorySystem");
                if (inventorySystem != null && inventorySystem.HasMethod("AddGold"))
                {
                    inventorySystem.Call("AddGold", StartingGold);
                    goldSet = true;
                    GD.Print($"[Test] Set {StartingGold} gold via InventorySystem");
                }
            }
            
            if (!goldSet)
            {
                GD.PrintErr("[Test] Could not set gold! No compatible system found.");
                GD.PrintErr("[Test] Make sure ShopManager, GameManager, or InventorySystem has AddGold() method.");
            }
        }
        
        private void CreateDebugUI()
        {
            // Create debug label to show party status
            debugLabel = new Label();
            debugLabel.Position = new Vector2(10, 10);
            debugLabel.AddThemeColorOverride("font_color", Colors.White);
            debugLabel.AddThemeColorOverride("font_outline_color", Colors.Black);
            debugLabel.AddThemeConstantOverride("outline_size", 2);
            AddChild(debugLabel);
            
            UpdateDebugLabel();
        }
        
        public override void _Process(double delta)
        {
            UpdateDebugLabel();
            
            // Press R to reset party HP/MP
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                ResetPartyDamage();
            }
        }
        
        private void UpdateDebugLabel()
        {
            if (debugLabel == null) return;
            
            var partyManager = PartyMenuManager.Instance;
            if (partyManager == null)
            {
                debugLabel.Text = "PartyMenuManager not found!";
                return;
            }
            
            var mainParty = partyManager.GetMainParty();
            
            string text = "=== INN KEEPER TEST ===\n";
            text += $"Gold: {GetCurrentGold()}\n";
            text += "\nParty Status:\n";
            
            foreach (var member in mainParty)
            {
                text += $"{member.CharacterId}: HP {member.Stats.CurrentHP}/{member.Stats.MaxHP} | MP {member.Stats.CurrentMP}/{member.Stats.MaxMP}\n";
            }
            
            text += "\nPress ESC to damage party";
            
            debugLabel.Text = text;
        }
        
        private void ResetPartyDamage()
        {
            var partyManager = PartyMenuManager.Instance;
            if (partyManager == null) return;
            
            var mainParty = partyManager.GetMainParty();
            foreach (var member in mainParty)
            {
                member.Stats.CurrentHP = member.Stats.MaxHP / 2;
                member.Stats.CurrentMP = member.Stats.MaxMP / 2;
            }
            
            GD.Print("[Test] Party damaged to 50% HP/MP");
        }
        
        private void PrintPartyStatus()
        {
            var partyManager = PartyMenuManager.Instance;
            if (partyManager == null) return;
            
            var mainParty = partyManager.GetMainParty();
            GD.Print("=== Party Status ===");
            foreach (var member in mainParty)
            {
                GD.Print($"{member.CharacterId}: HP {member.Stats.CurrentHP}/{member.Stats.MaxHP}, MP {member.Stats.CurrentMP}/{member.Stats.MaxMP}");
            }
        }
        
        private int GetCurrentGold()
        {
            // Try ShopManager
            var shopManager = GetNodeOrNull("/root/ShopManager");
            if (shopManager != null && shopManager.HasMethod("GetGold"))
            {
                return (int)shopManager.Call("GetGold");
            }
            
            // Try GameManager
            var gameManager = GetNodeOrNull("/root/GameManager");
            if (gameManager != null && gameManager.HasMethod("GetGold"))
            {
                return (int)gameManager.Call("GetGold");
            }
            
            // Try InventorySystem
            var inventorySystem = GetNodeOrNull("/root/InventorySystem");
            if (inventorySystem != null && inventorySystem.HasMethod("GetGold"))
            {
                return (int)inventorySystem.Call("GetGold");
            }
            
            return 0;
        }
    }
}