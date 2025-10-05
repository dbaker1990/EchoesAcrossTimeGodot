// SaveSystem/SaveSystem.cs
using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EchoesAcrossTime
{
    /// <summary>
    /// Manages saving and loading game data
    /// </summary>
    public partial class SaveSystem : Node
    {
        public static SaveSystem Instance { get; private set; }
        
        private const string SAVE_DIRECTORY = "user://saves/";
        private const string SAVE_FILE_PREFIX = "save_";
        private const string SAVE_FILE_EXTENSION = ".json";
        private const int MAX_SAVE_SLOTS = 10;
        
        [Export] public bool EnableAutoSave { get; set; } = true;
        [Export] public float AutoSaveInterval { get; set; } = 300f; // 5 minutes
        public SaveData CurrentSaveData { get; private set; }
        
        private float autoSaveTimer = 0f;
        
        [Signal]
        public delegate void GameSavedEventHandler(int slotIndex);
        
        [Signal]
        public delegate void GameLoadedEventHandler(int slotIndex);
        
        [Signal]
        public delegate void SaveErrorEventHandler(string error);
        
        public override void _Ready()
        {
            if (Instance != null)
            {
                QueueFree();
                return;
            }
            
            Instance = this;
            
            // Ensure save directory exists
            DirAccess.MakeDirRecursiveAbsolute(SAVE_DIRECTORY);
            
            GD.Print("SaveSystem initialized");
        }
        
        public override void _Process(double delta)
        {
            if (EnableAutoSave && GameManager.Instance?.IsGameActive == true)
            {
                autoSaveTimer += (float)delta;
                
                if (autoSaveTimer >= AutoSaveInterval)
                {
                    AutoSave();
                    autoSaveTimer = 0f;
                }
            }
        }
        
        /// <summary>
        /// Save game to a specific slot
        /// </summary>
        public bool SaveGame(int slotIndex, string slotName = null)
        {
            if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
            {
                GD.PrintErr($"Invalid save slot: {slotIndex}");
                EmitSignal(SignalName.SaveError, "Invalid save slot");
                return false;
            }
            
            try
            {
                // Create save data
                var saveData = new SaveData
                {
                    SaveSlotIndex = slotIndex,
                    SaveSlotName = slotName ?? $"Save {slotIndex + 1}",
                    PlayTimeSeconds = GameManager.Instance?.CurrentSave?.PlayTimeSeconds ?? 0f
                };
                
                // Capture current game state
                saveData.CaptureCurrentState();
                
                // Capture screenshot
                saveData.ScreenshotData = CaptureScreenshot();
                
                // Serialize to JSON
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                
                string json = JsonSerializer.Serialize(saveData, options);
                
                // Write to file
                string filePath = GetSaveFilePath(slotIndex);
                var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Write);
                
                if (file == null)
                {
                    GD.PrintErr($"Failed to open save file: {FileAccess.GetOpenError()}");
                    EmitSignal(SignalName.SaveError, "Failed to create save file");
                    return false;
                }
                
                file.StoreString(json);
                file.Close();
                
                // Update current save reference
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.CurrentSave = saveData;
                }
                
                EmitSignal(SignalName.GameSaved, slotIndex);
                GD.Print($"Game saved to slot {slotIndex}: {filePath}");
                
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error saving game: {e.Message}");
                EmitSignal(SignalName.SaveError, e.Message);
                return false;
            }
        }
        
        /// <summary>
        /// Load game from a specific slot
        /// </summary>
        public bool LoadGame(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
            {
                GD.PrintErr($"Invalid save slot: {slotIndex}");
                EmitSignal(SignalName.SaveError, "Invalid save slot");
                return false;
            }
            
            if (!SaveExists(slotIndex))
            {
                GD.PrintErr($"Save slot {slotIndex} does not exist");
                EmitSignal(SignalName.SaveError, "Save file not found");
                return false;
            }
            
            try
            {
                string filePath = GetSaveFilePath(slotIndex);
                var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                
                if (file == null)
                {
                    GD.PrintErr($"Failed to open save file: {FileAccess.GetOpenError()}");
                    EmitSignal(SignalName.SaveError, "Failed to open save file");
                    return false;
                }
                
                string json = file.GetAsText();
                file.Close();
                
                // Deserialize
                var saveData = JsonSerializer.Deserialize<SaveData>(json);
                
                if (saveData == null)
                {
                    GD.PrintErr("Failed to deserialize save data");
                    EmitSignal(SignalName.SaveError, "Corrupted save file");
                    return false;
                }
                
                // Apply save data to game
                GameManager.Instance?.LoadGame(saveData);
                
                // Load the saved map
                GetTree().ChangeSceneToFile(saveData.CurrentMapPath);
                
                // Position will be applied after scene loads
                
                EmitSignal(SignalName.GameLoaded, slotIndex);
                GD.Print($"Game loaded from slot {slotIndex}");
                
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error loading game: {e.Message}");
                EmitSignal(SignalName.SaveError, e.Message);
                return false;
            }
        }
        
        /// <summary>
        /// Finds the index of the most recently saved game file.
        /// </summary>
        /// <returns>The slot index of the most recent save, or -1 if no saves are found.</returns>
        public int GetMostRecentSaveSlot()
        {
            var saveFiles = GetSaveFileInfoList();
    
            if (saveFiles.Count == 0)
            {
                return -1; // No save files found
            }

            // Find the save file with the latest DateTime
            SaveFileInfo mostRecentSave = null;
            foreach (var saveInfo in saveFiles)
            {
                if (mostRecentSave == null || saveInfo.SaveDateTime > mostRecentSave.SaveDateTime)
                {
                    mostRecentSave = saveInfo;
                }
            }

            return mostRecentSave.SlotIndex;
        }
        
        /// <summary>
        /// Auto-save to slot 0
        /// </summary>
        public void AutoSave()
        {
            GD.Print("Auto-saving...");
            SaveGame(0, "Auto Save");
        }
        
        /// <summary>
        /// Quick save to last used slot
        /// </summary>
        public void QuickSave()
        {
            int lastSlot = GameManager.Instance?.CurrentSave?.SaveSlotIndex ?? 0;
            SaveGame(lastSlot, "Quick Save");
        }
        
        /// <summary>
        /// Check if a save exists in slot
        /// </summary>
        public bool SaveExists(int slotIndex)
        {
            string filePath = GetSaveFilePath(slotIndex);
            return FileAccess.FileExists(filePath);
        }
        
        /// <summary>
        /// Get save file info without fully loading
        /// </summary>
        public SaveFileInfo GetSaveInfo(int slotIndex)
        {
            if (!SaveExists(slotIndex))
                return null;
            
            try
            {
                string filePath = GetSaveFilePath(slotIndex);
                var file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
                
                if (file == null) return null;
                
                string json = file.GetAsText();
                file.Close();
                
                var saveData = JsonSerializer.Deserialize<SaveData>(json);
                
                if (saveData == null) return null;
                
                return new SaveFileInfo
                {
                    SlotIndex = slotIndex,
                    SlotName = saveData.SaveSlotName,
                    SaveDateTime = saveData.SaveDateTime,
                    PlayTimeSeconds = saveData.PlayTimeSeconds,
                    MapName = System.IO.Path.GetFileNameWithoutExtension(saveData.CurrentMapPath),
                    PartyLevel = saveData.Party.Count > 0 ? saveData.Party[0].Level : 1,
                    Screenshot = LoadScreenshotTexture(saveData.ScreenshotData)
                };
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error reading save info: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets a list of info for all existing save files.
        /// </summary>
        public List<SaveFileInfo> GetSaveFileInfoList()
        {
            var list = new List<SaveFileInfo>();
    
            if (!DirAccess.DirExistsAbsolute(SAVE_DIRECTORY))
            {
                return list;
            }

            using var dir = DirAccess.Open(SAVE_DIRECTORY);
            if (dir != null)
            {
                dir.ListDirBegin();
                string fileName = dir.GetNext();
                while (fileName != "")
                {
                    if (!dir.CurrentIsDir() && fileName.EndsWith(SAVE_FILE_EXTENSION))
                    {
                        // Extract slot index from file name
                        string slotStr = fileName.Replace(SAVE_FILE_PREFIX, "").Replace(SAVE_FILE_EXTENSION, "");
                        if (int.TryParse(slotStr, out int slotIndex))
                        {
                            var saveInfo = GetSaveFileInfo(slotIndex);
                            if (saveInfo != null)
                            {
                                list.Add(saveInfo);
                            }
                        }
                    }
                    fileName = dir.GetNext();
                }
            }

            return list;
        }
        
        
        /// <summary>
        /// Reads the metadata from a single save file without loading the full game state.
        /// </summary>
        public SaveFileInfo GetSaveFileInfo(int slotIndex)
        {
            string path = SAVE_DIRECTORY + SAVE_FILE_PREFIX + slotIndex + SAVE_FILE_EXTENSION;
    
            if (!FileAccess.FileExists(path))
            {
                return null;
            }

            try
            {
                using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
                string jsonString = file.GetAsText();
        
                // Deserialize the full data to easily access header info
                SaveData data = JsonSerializer.Deserialize<SaveData>(jsonString);

                if (data == null) return null;

                // Create a lightweight info object for the UI
                var info = new SaveFileInfo
                {
                    SlotIndex = slotIndex,
                    SlotName = data.SaveSlotName,
                    SaveDateTime = data.SaveDateTime,
                    PlayTimeSeconds = data.PlayTimeSeconds,
                    MapName = data.CurrentMapPath, // You might want to format this later
                    PartyLevel = data.Party.Count > 0 ? data.Party[0].Level : 1, // Example: gets level of first party member
                    // Screenshot loading can be added here if you save it in SaveData
                };
                return info;
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error reading save file info for slot {slotIndex}: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get all save file infos
        /// </summary>
        public List<SaveFileInfo> GetAllSaveInfos()
        {
            var infos = new List<SaveFileInfo>();
            
            for (int i = 0; i < MAX_SAVE_SLOTS; i++)
            {
                var info = GetSaveInfo(i);
                if (info != null)
                {
                    infos.Add(info);
                }
            }
            
            return infos;
        }
        
        /// <summary>
        /// Delete a save file
        /// </summary>
        public bool DeleteSave(int slotIndex)
        {
            if (!SaveExists(slotIndex))
                return false;
            
            try
            {
                string filePath = GetSaveFilePath(slotIndex);
                DirAccess.RemoveAbsolute(filePath);
                GD.Print($"Deleted save slot {slotIndex}");
                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error deleting save: {e.Message}");
                return false;
            }
        }
        
        private string GetSaveFilePath(int slotIndex)
        {
            return $"{SAVE_DIRECTORY}{SAVE_FILE_PREFIX}{slotIndex}{SAVE_FILE_EXTENSION}";
        }
        
        private byte[] CaptureScreenshot()
        {
            try
            {
                var viewport = GetViewport();
                var image = viewport.GetTexture().GetImage();
                
                // Resize for thumbnail
                image.Resize(320, 180, Image.Interpolation.Bilinear);
                
                return image.SavePngToBuffer();
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error capturing screenshot: {e.Message}");
                return null;
            }
        }
        
        private ImageTexture LoadScreenshotTexture(byte[] data)
        {
            if (data == null || data.Length == 0)
                return null;
            
            try
            {
                var image = new Image();
                var error = image.LoadPngFromBuffer(data);
                
                if (error != Error.Ok)
                    return null;
                
                return ImageTexture.CreateFromImage(image);
            }
            catch (Exception e)
            {
                GD.PrintErr($"Error loading screenshot: {e.Message}");
                return null;
            }
        }
    }
    
    /// <summary>
    /// Lightweight save file information for UI display
    /// </summary>
    public class SaveFileInfo
    {
        public int SlotIndex { get; set; }
        public string SlotName { get; set; }
        public DateTime SaveDateTime { get; set; }
        public float PlayTimeSeconds { get; set; }
        public string MapName { get; set; }
        public int PartyLevel { get; set; }
        public ImageTexture Screenshot { get; set; }
        
        public string GetPlayTimeFormatted()
        {
            var timeSpan = TimeSpan.FromSeconds(PlayTimeSeconds);
            return $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
        
        public string GetSaveDateFormatted()
        {
            return SaveDateTime.ToString("MM/dd/yyyy HH:mm");
        }
    }
}