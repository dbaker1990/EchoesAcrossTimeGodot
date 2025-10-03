// Events/EventCommandExecutor.cs
using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EchoesAcrossTime.UI;
using EchoesAcrossTime.Items;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime.Events
{
    /// <summary>
    /// Executes event commands
    /// </summary>
    public partial class EventCommandExecutor : Node
    {
        public static EventCommandExecutor Instance { get; private set; }
        
        [Export] public MessageBox MessageBox { get; set; }
        [Export] public ChoiceBox ChoiceBox { get; set; }
        [Export] public ScreenEffects ScreenEffects { get; set; }
        [Export] public PlayerCharacter Player { get; set; }
        [Export] public PartyManager PartyManager { get; set; }
        
        private Dictionary<string, object> variables = new Dictionary<string, object>();
        private Dictionary<string, DialogueTable> dialogueTables = new Dictionary<string, DialogueTable>();
        private bool isWaitingForInput = false;
        private bool inputReceived = false;
        
        public override void _Ready()
        {
            if (Instance != null)
            {
                QueueFree();
                return;
            }
            
            Instance = this;
            
            // Find components if not assigned
            if (MessageBox == null)
                MessageBox = GetNodeOrNull<MessageBox>("%MessageBox");
            
            if (ChoiceBox == null)
                ChoiceBox = GetNodeOrNull<ChoiceBox>("%ChoiceBox");
            
            if (ScreenEffects == null)
                ScreenEffects = GetNodeOrNull<ScreenEffects>("%ScreenEffects");
                
            GD.Print("EventCommandExecutor initialized");
        }
        
        public override void _Input(InputEvent @event)
        {
            if (isWaitingForInput && (@event.IsActionPressed("ui_accept") || @event.IsActionPressed("interact")))
            {
                inputReceived = true;
            }
        }
        
        /// <summary>
        /// Execute a list of commands sequentially
        /// </summary>
        public async Task ExecuteCommands(List<EventCommand> commands)
        {
            foreach (var command in commands)
            {
                await command.Execute(this);
            }
        }
        
        /// <summary>
        /// Execute commands from a resource
        /// </summary>
        public async Task ExecuteEvent(EventPage eventPage)
        {
            if (eventPage == null) return;
            
            var commands = new List<EventCommand>();
            foreach (var cmd in eventPage.Commands)
            {
                if (cmd != null)
                    commands.Add(cmd);
            }
            
            await ExecuteCommands(commands);
        }
        
        // Dialogue Table Management
        public void RegisterDialogueTable(string tableId, DialogueTable table)
        {
            dialogueTables[tableId] = table;
        }
        
        public DialogueTable GetDialogueTable(string tableId)
        {
            return dialogueTables.GetValueOrDefault(tableId);
        }
        
        // Command Implementations
        public async Task ShowMessage(DialogueData dialogue)
        {
            if (MessageBox == null || dialogue == null) return;
    
            var tcs = new TaskCompletionSource<bool>();
    
            void OnMessageAdvanced()
            {
                tcs.TrySetResult(true);
            }
    
            MessageBox.MessageAdvanced += OnMessageAdvanced;
            MessageBox.ShowMessage(dialogue);
    
            await tcs.Task;
    
            MessageBox.MessageAdvanced -= OnMessageAdvanced;
            // MessageBox will fade out automatically when advancing to next message
        }
        
        public async Task<int> ShowChoices(List<string> choices, int defaultChoice = 0)
        {
            if (ChoiceBox == null) return 0;
            
            var tcs = new TaskCompletionSource<int>();
            
            void OnChoiceSelected(int index)
            {
                tcs.TrySetResult(index);
            }
            
            ChoiceBox.ChoiceSelected += OnChoiceSelected;
            ChoiceBox.ShowChoices(choices, defaultChoice);
            
            int result = await tcs.Task;
            
            ChoiceBox.ChoiceSelected -= OnChoiceSelected;
            
            return result;
        }
        
        public async Task Wait(float duration)
        {
            await ToSignal(GetTree().CreateTimer(duration), SceneTreeTimer.SignalName.Timeout);
        }
        
        public async Task WaitForInput()
        {
            isWaitingForInput = true;
            inputReceived = false;
            
            while (!inputReceived)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            }
            
            isWaitingForInput = false;
        }
        
        public void ChangeGold(int amount)
        {
            if (amount > 0)
                InventorySystem.Instance?.AddGold(amount);
            else
                InventorySystem.Instance?.RemoveGold(-amount);
        }
        
        public void ChangeItems(string itemId, int quantity)
        {
            var item = InventorySystem.Instance?.GetItem(itemId);
            
            if (item == null)
            {
                GD.PrintErr($"EventCommandExecutor: Item {itemId} not found");
                return;
            }
            
            if (quantity > 0)
                InventorySystem.Instance?.AddItem(item, quantity);
            else
                InventorySystem.Instance?.RemoveItem(itemId, -quantity);
        }
        
        public void ChangeWeapon(string characterId, string weaponId, bool equip)
        {
            // Implementation depends on your equipment system
            GD.Print($"Change weapon for {characterId}: {weaponId} (equip: {equip})");
        }
        
        public void HealCharacter(string characterId, bool healAll, int hpAmount, int mpAmount, bool fullHeal, bool removeStatus)
        {
            // Get character stats from your party system
            GD.Print($"Heal character: {characterId}");
        }
        
        public void ChangePartyMember(string characterId, bool add, bool initialize)
        {
            // Use PartyManager to add/remove party members
            GD.Print($"Change party member: {characterId} (add: {add})");
        }
        
        public async Task MovePlayer(Vector2 position, OverworldCharacter.Direction? direction, bool useTeleport)
        {
            if (Player == null) return;
            
            if (useTeleport)
            {
                Player.TeleportTo(position, direction);
            }
            else
            {
                // Implement walking movement
                // This would need a pathfinding system
                GD.Print($"Move player to {position}");
            }
            
            await Task.CompletedTask;
        }
        
        // Variable System
        public void SetVariable(string name, object value)
        {
            variables[name] = value;
        }
        
        public object GetVariable(string name)
        {
            return variables.GetValueOrDefault(name);
        }
        
        public T GetVariable<T>(string name, T defaultValue = default)
        {
            if (variables.TryGetValue(name, out var value) && value is T typedValue)
                return typedValue;
            return defaultValue;
        }
        
        // Screen Effect Wrappers
        public async Task FadeScreen(bool fadeOut, float duration, Color color)
        {
            if (ScreenEffects != null)
                await ScreenEffects.Fade(fadeOut, duration, color);
        }
        
        public async Task TintScreen(Color color, float duration)
        {
            if (ScreenEffects != null)
                await ScreenEffects.Tint(color, duration);
        }
        
        public async Task FlashScreen(Color color, float duration)
        {
            if (ScreenEffects != null)
                await ScreenEffects.Flash(color, duration);
        }
        
        public async Task ShakeScreen(float intensity, float duration)
        {
            if (ScreenEffects != null)
                await ScreenEffects.Shake(intensity, duration);
        }
        
        // Audio Wrappers
        public void PlayBGM(AudioStream bgm, float volume, float fadeIn)
        {
            AudioManager.Instance?.PlayBGM(bgm, volume, fadeIn);
        }
        
        public async Task StopBGM(float fadeOut)
        {
            if (AudioManager.Instance != null)
                await AudioManager.Instance.StopBGM(fadeOut);
        }
        
        public void PlaySE(AudioStream se, float volume)
        {
            AudioManager.Instance?.PlaySoundEffect(se, volume);
        }
        
        public async Task TransferMap(string mapPath, Vector2 spawnPosition, OverworldCharacter.Direction? direction, bool fadeOut, bool fadeIn)
        {
            if (fadeOut)
                await FadeScreen(true, 0.5f, Colors.Black);
            
            // Load new scene
            GetTree().ChangeSceneToFile(mapPath);
            
            // Wait a frame for scene to load
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            
            // Move player
            if (Player != null)
                Player.TeleportTo(spawnPosition, direction);
            
            if (fadeIn)
                await FadeScreen(false, 0.5f, Colors.Black);
        }
    }
}