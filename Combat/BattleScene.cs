using Godot;
using System;
using System.Linq;
using EchoesAcrossTime.Managers;

namespace EchoesAcrossTime.Combat
{
    public partial class BattleScene : Node
    {
        #region Exports
        [Export] private PackedScene gameOverScene; // Drag GameOverUI.tscn here in inspector
        [Export] private BattleManager battleManager;
        #endregion
        
        private Control gameOverUI;
        
        public override void _Ready()
        {
            // Connect to battle manager signals
            if (battleManager != null)
            {
                battleManager.BattleEnded += OnBattleEnded;
            }
            else
            {
                GD.PrintErr("BattleScene: BattleManager not found!");
            }
        }
        
        /// <summary>
        /// Called when battle ends (victory or defeat)
        /// </summary>
        private void OnBattleEnded(bool victory)
        {
            if (victory)
            {
                ShowVictoryScreen();
            }
            else
            {
                // Check if it was defeat or escape
                if (battleManager.CurrentPhase == BattlePhase.Defeat)
                {
                    ShowDefeatScreen();
                }
                else if (battleManager.CurrentPhase == BattlePhase.Escaped)
                {
                    ReturnToOverworld();
                }
            }
        }
        
        /// <summary>
        /// Show game over screen
        /// </summary>
        private void ShowDefeatScreen()
        {
            GD.Print("[BattleScene] Showing defeat screen...");
            
            // Play game over music
            if (SystemManager.Instance != null)
            {
                SystemManager.Instance.PlayGameOverMusic();
            }
            
            // Instance the game over UI
            if (gameOverScene != null)
            {
                gameOverUI = gameOverScene.Instantiate<Control>();
                AddChild(gameOverUI);
                
                // Connect buttons if they exist
                var continueBtn = gameOverUI.GetNode<Button>("VBoxContainer/ContinueButton");
                var titleBtn = gameOverUI.GetNode<Button>("VBoxContainer/TitleButton");
                
                if (continueBtn != null)
                {
                    continueBtn.Pressed += OnContinuePressed;
                }
                
                if (titleBtn != null)
                {
                    titleBtn.Pressed += OnTitleScreenPressed;
                }
                
                GD.Print("[BattleScene] Game Over UI displayed");
            }
            else
            {
                GD.PrintErr("[BattleScene] GameOverScene not assigned! Falling back to title screen.");
                ReturnToTitleScreen();
            }
        }
        
        /// <summary>
        /// Continue button - reload the battle or load last save
        /// </summary>
        private void OnContinuePressed()
        {
            GD.Print("[BattleScene] Continue pressed - loading last save");
            
            if (SystemManager.Instance != null)
            {
                SystemManager.Instance.PlayOkSE();
            }
            
            // Try to load the most recent save
            var saveSystem = EchoesAcrossTime.SaveSystem.Instance;
            if (saveSystem != null)
            {
                // Load autosave (slot 0) or last quick save
                if (saveSystem.SaveExists(0))
                {
                    saveSystem.LoadGame(0);
                }
                else
                {
                    GD.PrintErr("[BattleScene] No save file found! Returning to title screen.");
                    ReturnToTitleScreen();
                }
            }
            else
            {
                GD.PrintErr("[BattleScene] SaveSystem not available! Returning to title screen.");
                ReturnToTitleScreen();
            }
        }
        
        /// <summary>
        /// Return to title screen button
        /// </summary>
        private void OnTitleScreenPressed()
        {
            GD.Print("[BattleScene] Returning to title screen");
            
            if (SystemManager.Instance != null)
            {
                SystemManager.Instance.PlayCancelSE();
            }
            
            ReturnToTitleScreen();
        }
        
        /// <summary>
        /// Return to title screen
        /// </summary>
        private void ReturnToTitleScreen()
        {
            // Unpause the game
            GetTree().Paused = false;
            
            // Change to title screen
            GetTree().ChangeSceneToFile("res://Scenes/TitleScreen.tscn");
        }
        
        /// <summary>
        /// Show victory screen
        /// </summary>
        private void ShowVictoryScreen()
        {
            GD.Print("[BattleScene] Victory!");
            GD.Print("═══════════════════════════════════════");
            
            // Get rewards from BattleManager
            // Display victory UI, rewards, etc.
            // Then return to overworld
            
            // For now, just return to overworld
            CallDeferred(nameof(ReturnToOverworld));
        }
        
        /// <summary>
        /// Return to overworld after battle
        /// </summary>
        private void ReturnToOverworld()
        {
            GD.Print("[BattleScene] Returning to overworld");
            
            // Get the overworld scene path from GameManager or use default
            string overworldScene = "res://Maps/Veridia/VeridiaCapital.tscn"; // Adjust to your map
            
            if (GameManager.Instance != null)
            {
                // If you store the last map in GameManager
                overworldScene = GameManager.Instance.LastMapScene ?? overworldScene;
            }
            
            GetTree().ChangeSceneToFile(overworldScene);
        }
        
        public override void _ExitTree()
        {
            // Clean up signal connections
            if (battleManager != null)
            {
                battleManager.BattleEnded -= OnBattleEnded;
            }
        }
    }
}