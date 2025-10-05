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
    public partial class EventCommandExecutor : Node
    {
        #region Properties & Fields
        
        public static EventCommandExecutor Instance { get; private set; }
        
        [Export] public MessageBox MessageBox { get; set; }
        [Export] public ChoiceBox ChoiceBox { get; set; }
        [Export] public ScreenEffects ScreenEffects { get; set; }
        [Export] public PlayerCharacter Player { get; set; }
        [Export] public PartyManager PartyManager { get; set; }
        [Export] public TileMapLayer NavigationLayer { get; set; }
        
        // Add these to the Properties & Fields region
        [Export] public PackedScene BalloonIconScene { get; set; }
        [Export] public PackedScene NameInputScreenScene { get; set; } // Assuming NameInputUI is a node in your scene
        
        private AStar2D astar = new AStar2D();
        private Dictionary<string, object> variables = new Dictionary<string, object>();
        private Dictionary<string, DialogueTable> dialogueTables = new Dictionary<string, DialogueTable>();
        private bool isWaitingForInput = false;
        private bool inputReceived = false;
        
        #endregion

        #region Godot Lifecycle Methods

        public override void _Ready()
        {
            if (Instance != null)
            {
                QueueFree();
                return;
            }
            Instance = this;

            if (MessageBox == null) MessageBox = GetNodeOrNull<MessageBox>("%MessageBox");
            if (ChoiceBox == null) ChoiceBox = GetNodeOrNull<ChoiceBox>("%ChoiceBox");
            if (ScreenEffects == null) ScreenEffects = GetNodeOrNull<ScreenEffects>("%ScreenEffects");
            
            SetupPathfinding();
        }

        public override void _Input(InputEvent @event)
        {
            if (isWaitingForInput && (@event.IsActionPressed("ui_accept") || @event.IsActionPressed("interact")))
            {
                inputReceived = true;
            }
        }

        #endregion

        #region Public API Methods

        public async Task ExecuteEvent(EventPage eventPage)
        {
            if (eventPage == null) return;
            await ExecuteCommands(eventPage.Commands);
        }

        public async Task ExecuteCommands(IEnumerable<EventCommand> commands)
        {
            if (commands == null) return;
            foreach (var command in commands)
            {
                if (command != null)
                {
                    await command.Execute(this);
                }
            }
        }

        #endregion

        #region Command Implementations
        
        public void RegisterDialogueTable(string tableId, DialogueTable table) => dialogueTables[tableId] = table;
        public DialogueTable GetDialogueTable(string tableId) => dialogueTables.GetValueOrDefault(tableId);

        public async Task ShowMessage(DialogueData dialogue)
        {
            if (MessageBox == null || dialogue == null) return;
            var tcs = new TaskCompletionSource<bool>();
            void OnMessageAdvanced() { tcs.TrySetResult(true); }
            MessageBox.MessageAdvanced += OnMessageAdvanced;
            MessageBox.ShowMessage(dialogue);
            await tcs.Task;
            MessageBox.MessageAdvanced -= OnMessageAdvanced;
        }

        public async Task<int> ShowChoices(List<string> choices, int defaultChoice = 0)
        {
            if (ChoiceBox == null) return 0;
            var tcs = new TaskCompletionSource<int>();
            void OnChoiceSelected(int index) { tcs.TrySetResult(index); }
            ChoiceBox.ChoiceSelected += OnChoiceSelected;
            ChoiceBox.ShowChoices(choices, defaultChoice);
            int result = await tcs.Task;
            ChoiceBox.ChoiceSelected -= OnChoiceSelected;
            return result;
        }

        public async Task Wait(float duration) => await ToSignal(GetTree().CreateTimer(duration), SceneTreeTimer.SignalName.Timeout);

        public async Task WaitForInput()
        {
            isWaitingForInput = true;
            inputReceived = false;
            while (!inputReceived) { await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); }
            isWaitingForInput = false;
        }

        public void ChangeGold(int amount)
        {
            if (amount > 0) InventorySystem.Instance?.AddGold(amount);
            else InventorySystem.Instance?.RemoveGold(-amount);
        }

        public void ChangeItems(string itemId, int quantity)
        {
            var item = InventorySystem.Instance?.GetItem(itemId);
            if (item == null) { GD.PrintErr($"Item {itemId} not found"); return; }
            if (quantity > 0) InventorySystem.Instance?.AddItem(item, quantity);
            else InventorySystem.Instance?.RemoveItem(itemId, -quantity);
        }

        public void ChangeWeapon(string characterId, string weaponId, bool equip) => GD.Print($"Change weapon for {characterId}: {weaponId} (equip: {equip})");
        public void HealCharacter(string characterId, bool healAll, int hpAmount, int mpAmount, bool fullHeal, bool removeStatus) => GD.Print($"Heal character: {characterId}");
        public void ChangePartyMember(string characterId, bool add, bool initialize) => GD.Print($"Change party member: {characterId} (add: {add})");
        public void SetVariable(string name, object value) => variables[name] = value;
        public object GetVariable(string name) => variables.GetValueOrDefault(name);
        public T GetVariable<T>(string name, T defaultValue = default) => (variables.TryGetValue(name, out var value) && value is T typedValue) ? typedValue : defaultValue;
        public async Task FadeScreen(bool fadeOut, float duration, Color color) { if (ScreenEffects != null) await ScreenEffects.Fade(fadeOut, duration, color); }
        public async Task TintScreen(Color color, float duration) { if (ScreenEffects != null) await ScreenEffects.Tint(color, duration); }
        public async Task FlashScreen(Color color, float duration) { if (ScreenEffects != null) await ScreenEffects.Flash(color, duration); }
        public async Task ShakeScreen(float intensity, float duration) { if (ScreenEffects != null) await ScreenEffects.Shake(intensity, duration); }
        public void PlayBGM(AudioStream bgm, float volume, float fadeIn) => AudioManager.Instance?.PlayBGM(bgm, volume, fadeIn);
        public async Task StopBGM(float fadeOut) { if (AudioManager.Instance != null) await AudioManager.Instance.StopBGM(fadeOut); }
        public void PlaySE(AudioStream se, float volume) => AudioManager.Instance?.PlaySoundEffect(se, volume);

        public async Task TransferMap(string mapPath, Vector2 spawnPosition, OverworldCharacter.Direction? direction, bool fadeOut, bool fadeIn)
        {
            if (fadeOut) await FadeScreen(true, 0.5f, Colors.Black);
            GetTree().ChangeSceneToFile(mapPath);
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            if (Player != null) Player.TeleportTo(spawnPosition, direction);
            if (fadeIn) await FadeScreen(false, 0.5f, Colors.Black);
        }
        
        public async Task MoveCharacterOnPath(NodePath characterPath, Vector2 targetPosition, float speed)
        {
            var character = GetNodeOrNull<CharacterBody2D>(characterPath);
            if (character == null || NavigationLayer == null)
            {
                GD.PrintErr("MoveCharacterOnPath: Character or NavigationLayer not set.");
                return;
            }

            var startPos = character.GlobalPosition;
            var startPoint = NavigationLayer.LocalToMap(startPos);
            var endPoint = NavigationLayer.LocalToMap(targetPosition);

            var pathIds = astar.GetIdPath(Vector2IToId(startPoint), Vector2IToId(endPoint));
            if (pathIds.Length == 0) return;

            // character.PlayWalkAnimation();

            foreach (var pointId in pathIds)
            {
                Vector2 nextPos = astar.GetPointPosition(pointId);
                while (character.GlobalPosition.DistanceTo(nextPos) > 4)
                {
                    character.Velocity = character.GlobalPosition.DirectionTo(nextPos) * speed;
                    character.MoveAndSlide();
                    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                }
            }
            character.Velocity = Vector2.Zero;
            // character.PlayIdleAnimation();
        }
        
        public bool EvaluateCondition(string variableName, ConditionalBranchCommand.ComparisonOperator op, Variant valueToCompare)
        {
            if (!variables.TryGetValue(variableName, out var variableValueObj)) return false;
            var var1 = Variant.From(variableValueObj);
            
            bool isVar1Numeric = var1.VariantType == Variant.Type.Int || var1.VariantType == Variant.Type.Float;
            bool isValueToCompareNumeric = valueToCompare.VariantType == Variant.Type.Int || valueToCompare.VariantType == Variant.Type.Float;

            if (isVar1Numeric && isValueToCompareNumeric)
            {
                double val1 = var1.AsDouble();
                double val2 = valueToCompare.AsDouble();
                
                return op switch
                {
                    ConditionalBranchCommand.ComparisonOperator.EqualTo => val1 == val2,
                    ConditionalBranchCommand.ComparisonOperator.NotEqualTo => val1 != val2,
                    ConditionalBranchCommand.ComparisonOperator.GreaterThan => val1 > val2,
                    ConditionalBranchCommand.ComparisonOperator.LessThan => val1 < val2,
                    ConditionalBranchCommand.ComparisonOperator.GreaterThanOrEqualTo => val1 >= val2,
                    ConditionalBranchCommand.ComparisonOperator.LessThanOrEqualTo => val1 <= val2,
                    _ => false,
                };
            }
            
            if (var1.VariantType == Variant.Type.String && valueToCompare.VariantType == Variant.Type.String)
            {
                string str1 = var1.AsString();
                string str2 = valueToCompare.AsString();
                return op switch {
                    ConditionalBranchCommand.ComparisonOperator.EqualTo => str1 == str2,
                    ConditionalBranchCommand.ComparisonOperator.NotEqualTo => str1 != str2,
                    _ => false,
                };
            }
            
            if (var1.VariantType == Variant.Type.Bool && valueToCompare.VariantType == Variant.Type.Bool)
            {
                bool b1 = var1.AsBool();
                bool b2 = valueToCompare.AsBool();
                return op switch {
                    ConditionalBranchCommand.ComparisonOperator.EqualTo => b1 == b2,
                    ConditionalBranchCommand.ComparisonOperator.NotEqualTo => b1 != b2,
                    _ => false,
                };
            }

            GD.PrintErr($"Cannot compare incompatible types: {var1.VariantType} and {valueToCompare.VariantType}");
            return false;
        }
        
        #endregion
        
        #region Private Helpers

        private void SetupPathfinding()
        {
            if (NavigationLayer == null) return;
        
            astar.Clear();
            var usedCells = NavigationLayer.GetUsedCells();
            foreach (var cell in usedCells)
            {
                var cellData = NavigationLayer.GetCellTileData(cell);
                if (cellData != null && cellData.GetCustomData("walkable").AsBool())
                {
                    astar.AddPoint(Vector2IToId(cell), NavigationLayer.MapToLocal(cell));
                }
            }

            foreach (var cell in usedCells)
            {
                if (!astar.HasPoint(Vector2IToId(cell))) continue;
            
                Vector2I[] directions = { Vector2I.Up, Vector2I.Down, Vector2I.Left, Vector2I.Right };
                foreach (var dir in directions)
                {
                    var neighbor = cell + dir;
                    if (astar.HasPoint(Vector2IToId(neighbor)))
                    {
                        astar.ConnectPoints(Vector2IToId(cell), Vector2IToId(neighbor));
                    }
                }
            }
        }
        
        public void ChangeTransparency(NodePath characterPath, bool isHidden)
        {
            var character = GetNodeOrNull<CanvasItem>(characterPath);
            if (character != null)
            {
                character.Visible = !isHidden;
            }
        }

        public async Task ProcessNameInput(string characterId, int maxLength)
        {
            if (NameInputScreenScene == null) return;

            // Create an instance of the name input screen and add it to the scene
            var screen = NameInputScreenScene.Instantiate<Control>(); // Assuming the root is a Control node
            AddChild(screen);

            // Call a method on the screen to initialize it and wait for it to finish
            // The screen scene must emit a "name_confirmed" signal with the final name
            var result = await ToSignal(screen, "name_confirmed");
            var newName = result[0].AsString(); // Godot signals pass arguments in an array

            // Update the character's name in your database/manager
            // Example: CharacterDatabase.GetCharacter(characterId).Name = newName;
            GD.Print($"Character {characterId} is now named {newName}.");
    
            // The screen should handle queue_free() on its own after confirming.
        }

        public async Task ShowBalloonIcon(NodePath characterPath, TransferMapCommand.ShowBalloonIconCommand.BalloonType icon, bool wait)
        {
            var character = GetNodeOrNull<Node2D>(characterPath);
            if (character == null || BalloonIconScene == null) return;

            var balloon = BalloonIconScene.Instantiate<Node2D>(); // Assume the balloon scene root is Node2D
            character.AddChild(balloon);
    
            // You would have logic inside the balloon script to set the animation/texture
            // based on the icon type, play its animation, and queue_free() itself.
            // e.g., balloon.Play(icon.ToString());

            if (wait)
            {
                // The balloon scene should emit a signal when its animation is finished.
                await ToSignal(balloon, "animation_finished");
            }
        }
        
        private long Vector2IToId(Vector2I vec)
        {
            return ((long)vec.X << 32) | (long)(uint)vec.Y;
        }

        #endregion
    }
}