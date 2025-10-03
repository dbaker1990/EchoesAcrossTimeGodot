// Managers/GameManager.cs
using Godot;
using EchoesAcrossTime.Database;

namespace EchoesAcrossTime
{
    public partial class GameManager : Node
    {
        public static GameManager Instance { get; private set; }
        
        [Export] public GameDatabase Database { get; set; }
        
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
    }
}