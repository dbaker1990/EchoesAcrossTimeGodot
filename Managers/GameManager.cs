// Managers/GameManager.cs
using Godot;
using EchoesAcrossTime.Database;

namespace EchoesAcrossTime
{
    public partial class GameManager : Node
    {
        public static GameManager Instance { get; private set; }
        [Export] public GameDatabase Database { get; set; }
        [Export] public GameOverUI GameOverScreen { get; set; }
        
        // Current game state - Changed to public set for SaveSystem
        public SaveData CurrentSave { get; set; }
        public bool IsGameActive { get; private set; }
        
        public override void _Ready()
        {
            if (Instance != null)
            {
                QueueFree();
                return;
            }
            
            Instance = this;
            
            // Load database if not assigned
            if (Database == null)
            {
                Database = GD.Load<GameDatabase>("res://Database/GameDatabase.tres");
            }
            
            if (Database == null)
            {
                GD.PrintErr("GameManager: No database assigned!");
            }
            else
            {
                GD.Print("GameManager initialized with database");
            }
        }
        
        public void StartNewGame()
        {
            CurrentSave = new SaveData();
            CurrentSave.InitializeNewGame(Database);
            IsGameActive = true;
            
            GD.Print("New game started");
        }
        
        public void LoadGame(SaveData saveData)
        {
            CurrentSave = saveData;
            CurrentSave.ApplyToGame();
            IsGameActive = true;
            
            GD.Print($"Game loaded: {saveData.SaveSlotName}");
        }
        
        public void QuitToTitle()
        {
            IsGameActive = false;
            CurrentSave = null;
            GetTree().ChangeSceneToFile("res://Scenes/TitleScreen.tscn");
        }
        
        public void TriggerGameOver()
        {
            // You might want to pause the game or stop player input here
            GetTree().Paused = true; 
            GameOverScreen?.ShowScreen();
        }

        public void ContinueFromLastSave()
        {
            GetTree().Paused = false;

            // Get the index of the most recent save file.
            int lastSaveSlot = SaveSystem.Instance.GetMostRecentSaveSlot();

            if (lastSaveSlot != -1)
            {
                // Attempt to load the game from that slot.
                bool loadSuccessful = SaveSystem.Instance.LoadGame(lastSaveSlot); 
                if (loadSuccessful)
                {
                    SaveData data = SaveSystem.Instance.CurrentSaveData;
                    LoadGame(data);
                    return;
                }
            }

            // If no save exists or it fails to load, go to the title screen.
            ReturnToTitleScreen();
        }

        public void ReturnToTitleScreen()
        {
            GetTree().Paused = false;
            GetTree().ChangeSceneToFile("res://Scenes/TitleScreen.tscn");
        }
    }
}