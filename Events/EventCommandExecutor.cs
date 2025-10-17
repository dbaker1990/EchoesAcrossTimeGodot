// Events/EventCommandExecutor.cs
using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EchoesAcrossTime.UI;
using EchoesAcrossTime.Items;
using EchoesAcrossTime.Combat;
using System.Linq;
using EchoesAcrossTime.Managers;

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
        
        private string preBattleScenePath;
        private Vector2 preBattlePlayerPosition;
        
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
        
        /// <summary>
        /// Change weather with optional transition
        /// Usage: ChangeWeather("Rain", instant: false)
        /// </summary>
        public void ChangeWeather(string weatherType, bool instant = false)
        {
            if (Enum.TryParse<WeatherManager.WeatherType>(weatherType, true, out var weather))
            {
                WeatherManager.Instance?.ChangeWeather(weather, instant);
                GD.Print($"[Event] Weather changed to: {weather}");
            }
            else
            {
                GD.PrintErr($"[Event] Invalid weather type: {weatherType}");
            }
        }
        
        /// <summary>
        /// Set story weather (disables auto-weather changes)
        /// Usage: SetStoryWeather("Storm")
        /// </summary>
        public void SetStoryWeather(string weatherType)
        {
            if (Enum.TryParse<WeatherManager.WeatherType>(weatherType, true, out var weather))
            {
                WeatherManager.Instance?.SetStoryWeather(weather);
                GD.Print($"[Event] Story weather set to: {weather}");
            }
        }
        
        /// <summary>
        /// Resume automatic weather changes
        /// Usage: ResumeAutoWeather()
        /// </summary>
        public void ResumeAutoWeather()
        {
            WeatherManager.Instance?.ResumeAutoWeather();
            GD.Print("[Event] Auto-weather resumed");
        }
        
        /// <summary>
        /// Wait for weather transition to complete
        /// Usage: await WaitForWeatherTransition()
        /// </summary>
        public async Task WaitForWeatherTransition()
        {
            if (WeatherManager.Instance == null) return;
            
            float duration = WeatherManager.Instance.TransitionDuration;
            await ToSignal(GetTree().CreateTimer(duration), "timeout");
        }
        
        /// <summary>
        /// Set the time of day
        /// Usage: SetTime(20, 30) for 8:30 PM
        /// </summary>
        public void SetTime(int hour, int minute)
        {
            TimeManager.Instance?.SetTime(hour, minute);
            GD.Print($"[Event] Time set to {hour:D2}:{minute:D2}");
        }
        
        /// <summary>
        /// Advance time by specified minutes
        /// Usage: AdvanceTime(120) for 2 hours
        /// </summary>
        public void AdvanceTime(int minutes)
        {
            TimeManager.Instance?.AdvanceTime(minutes);
            GD.Print($"[Event] Advanced time by {minutes} minutes");
        }
        
        /// <summary>
        /// Set time to a specific time of day
        /// Usage: SetTimeOfDay("Evening")
        /// </summary>
        public void SetTimeOfDay(string timeOfDay)
        {
            if (TimeManager.Instance == null) return;
            
            int hour = timeOfDay.ToLower() switch
            {
                "dawn" => 6,
                "morning" => 9,
                "afternoon" => 14,
                "evening" => 19,
                "night" => 22,
                _ => 12
            };
            
            TimeManager.Instance.SetTime(hour, 0);
            GD.Print($"[Event] Time set to {timeOfDay}");
        }
        
        /// <summary>
        /// Toggle time progression
        /// Usage: SetTimeEnabled(false) to pause time
        /// </summary>
        public void SetTimeEnabled(bool enabled)
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.TimeEnabled = enabled;
                GD.Print($"[Event] Time progression: {(enabled ? "ENABLED" : "DISABLED")}");
            }
        }
        
        /// <summary>
        /// Fast forward time with fade effect
        /// Usage: await FastForwardTime(240) for 4 hours
        /// </summary>
        public async Task FastForwardTime(int minutes, bool showFade = true)
        {
            if (TimeManager.Instance == null) return;
            
            if (showFade)
            {
                // Fade out
                await FadeScreen(true, 0.5f, Colors.Black);
            }
            
            // Advance time
            TimeManager.Instance.AdvanceTime(minutes);
            
            if (showFade)
            {
                // Small delay
                await ToSignal(GetTree().CreateTimer(0.3f), "timeout");
                
                // Fade in
                await FadeScreen(false, 0.5f, Colors.Black);
            }
            
            GD.Print($"[Event] Fast-forwarded {minutes} minutes");
        }
        
        /// <summary>
        /// Create a dramatic scene with weather and time
        /// Usage: await SetSceneAmbience("Storm", "Evening")
        /// </summary>
        public async Task SetSceneAmbience(string weatherType, string timeOfDay)
        {
            // Set time first
            SetTimeOfDay(timeOfDay);
            
            // Then set weather
            ChangeWeather(weatherType, false);
            
            // Wait for transition
            await WaitForWeatherTransition();
        }
        
        /// <summary>
        /// Wait until a specific time of day
        /// Usage: await WaitUntilTimeOfDay("Night")
        /// </summary>
        public async Task WaitUntilTimeOfDay(string targetTimeOfDay)
        {
            if (TimeManager.Instance == null) return;
            
            if (!Enum.TryParse<TimeManager.TimeOfDay>(targetTimeOfDay, true, out var target))
            {
                GD.PrintErr($"[Event] Invalid time of day: {targetTimeOfDay}");
                return;
            }
            
            // Wait until target time of day is reached
            while (TimeManager.Instance.CurrentTimeOfDay != target)
            {
                await ToSignal(GetTree(), "process_frame");
            }
            
            GD.Print($"[Event] Reached {target}");
        }
        
        /// <summary>
        /// Check if it's currently a specific time of day
        /// Usage: if (IsTimeOfDay("Night")) { ... }
        /// </summary>
        public bool IsTimeOfDay(string timeOfDay)
        {
            if (TimeManager.Instance == null) return false;
            
            if (Enum.TryParse<TimeManager.TimeOfDay>(timeOfDay, true, out var target))
            {
                return TimeManager.Instance.CurrentTimeOfDay == target;
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if weather is a specific type
        /// Usage: if (IsWeather("Rain")) { ... }
        /// </summary>
        public bool IsWeather(string weatherType)
        {
            if (WeatherManager.Instance == null) return false;
            
            if (Enum.TryParse<WeatherManager.WeatherType>(weatherType, true, out var target))
            {
                return WeatherManager.Instance.CurrentWeather == target;
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if it's currently stormy
        /// Usage: if (IsStormy()) { ... }
        /// </summary>
        public bool IsStormy()
        {
            return WeatherManager.Instance?.IsStormy() ?? false;
        }
        
        /// <summary>
        /// Check if it's nighttime
        /// Usage: if (IsNight()) { ... }
        /// </summary>
        public bool IsNight()
        {
            return TimeManager.Instance?.IsNight() ?? false;
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
        
        /// <summary>
        /// Get all flag names that are currently set to true
        /// Useful for systems that need to check multiple flags (like SkitTrigger)
        /// </summary>
        public List<string> GetAllActiveFlags()
        {
            var activeFlags = new List<string>();
    
            foreach (var kvp in variables)
            {
                if (kvp.Value is bool boolValue && boolValue)
                {
                    activeFlags.Add(kvp.Key);
                }
            }
    
            return activeFlags;
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
        
        /// <summary>
        /// Check if a flag is set (convenience method)
        /// </summary>
        public bool HasFlag(string flagName)
        {
            return GetVariable<bool>(flagName, false);
        }
        
        /// <summary>
        /// Set a flag to true (convenience method)
        /// </summary>
        public void SetFlag(string flagName)
        {
            SetVariable(flagName, true);
        }
        
        /// <summary>
        /// Clear a flag (set to false)
        /// </summary>
        public void ClearFlag(string flagName)
        {
            SetVariable(flagName, false);
        }
        
        /// <summary>
        /// Get all variables of a specific type
        /// </summary>
        public Dictionary<string, T> GetVariablesOfType<T>()
        {
            var result = new Dictionary<string, T>();
    
            foreach (var kvp in variables)
            {
                if (kvp.Value is T typedValue)
                {
                    result[kvp.Key] = typedValue;
                }
            }
    
            return result;
        }
        
        /// <summary>
        /// Check if any of the given flags are set
        /// </summary>
        public bool HasAnyFlag(params string[] flagNames)
        {
            foreach (var flag in flagNames)
            {
                if (HasFlag(flag))
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Check if all of the given flags are set
        /// </summary>
        public bool HasAllFlags(params string[] flagNames)
        {
            foreach (var flag in flagNames)
            {
                if (!HasFlag(flag))
                {
                    return false;
                }
            }
            return true;
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
        
        // Add these methods to your Events/EventCommandExecutor.cs file

        #region Battle & Shop Commands

        private AudioStream currentBattleBGM;

        /// <summary>
        /// Change the battle BGM that will be used in future battles
        /// </summary>
        public void ChangeBattleBGM(AudioStream battleBGM)
        {
            currentBattleBGM = battleBGM;
            GD.Print($"[EventCommandExecutor] Battle BGM changed");
        }

        /// <summary>
        /// Initiate a battle encounter
        /// </summary>
        public async Task InitiateBattle(string troopId, bool canEscape, bool canLose, AudioStream battleBGM)
        {
            if (string.IsNullOrEmpty(troopId))
            {
                GD.PrintErr("[EventCommandExecutor] Battle troop ID is empty");
                return;
            }
            
            GD.Print($"[EventCommandExecutor] Initiating battle with troop: {troopId}");
            
            // Use provided BGM or fall back to current battle BGM
            var bgmToUse = battleBGM ?? currentBattleBGM;
            
            // Save current scene and player position for return
            SavePreBattleState();
            
            // Fade out current music
            await StopBGM(0.5f);
            
            // Play battle transition effect
            if (ScreenEffects != null)
            {
                await ScreenEffects.Flash(Colors.White, 0.3f);
            }
            
            // Load the troop data from database
            var troop = LoadTroopData(troopId);
            if (troop == null)
            {
                GD.PrintErr($"[EventCommandExecutor] Troop '{troopId}' not found!");
                return;
            }
            
            // Store battle parameters in GameManager for the battle scene to access
            StoreBattleParameters(troop, canEscape, canLose, bgmToUse);
            
            // Transition to battle scene
            GetTree().ChangeSceneToFile("res://Combat/BattleScene.tscn");
            
            // Battle scene will initialize itself using the stored parameters
            // When battle ends, BattleManager will call SetBattleResult() and return to map
            
            await Task.CompletedTask;
        }

        /// <summary>
        /// Open the shop interface
        /// </summary>
        public async Task InitiateShop(Godot.Collections.Array<string> itemIds, bool canBuy, bool canSell)
        {
            if (itemIds == null || itemIds.Count == 0)
            {
                GD.PrintErr("Shop has no items");
                return;
            }
            
            // TODO: Implement shop system integration
            // When you have a ShopManager, uncomment and modify this code:
            /*
            var tcs = new TaskCompletionSource<bool>();
            var shopManager = GetNode<Node>("/root/ShopManager");
            if (shopManager == null)
            {
                GD.PrintErr("ShopManager not found");
                return;
            }
            
            void OnShopClosed()
            {
                tcs.TrySetResult(true);
            }
            
            shopManager.Connect("ShopClosed", Callable.From(OnShopClosed));
            shopManager.Call("OpenShop", itemIds, canBuy, canSell);
            
            await tcs.Task;
            
            shopManager.Disconnect("ShopClosed", Callable.From(OnShopClosed));
            */
            
            GD.Print($"InitiateShop called: Items={itemIds.Count}, CanBuy={canBuy}, CanSell={canSell}");
            await Task.CompletedTask;
        }

        #endregion

        public async Task ProcessNameInput(string characterId, int maxLength)
        {
            if (NameInputScreenScene == null)
            {
                GD.PrintErr("NameInputScreenScene not assigned in EventCommandExecutor");
                return;
            }

            var nameInputUI = NameInputScreenScene.Instantiate<NameInputUI>();
            AddChild(nameInputUI);
    
            var gameManager = GetNode<GameManager>("/root/GameManager");
            if (gameManager?.Database != null)
            {
                nameInputUI.Database = gameManager.Database;
            }
            else
            {
                GD.PrintErr("GameManager or Database not found");
            }
    
            nameInputUI.Initialize(characterId, maxLength);
    
            var result = await ToSignal(nameInputUI, NameInputUI.SignalName.NameConfirmed);
    
            // ✅ BEST PRACTICE: Use Any() instead of Count() > 0
            if (result != null && result.Any())
            {
                string newName = result[0].AsString();
        
                var character = gameManager?.Database?.GetCharacter(characterId);
                if (character != null)
                {
                    character.DisplayName = newName;
                    GD.Print($"Character '{characterId}' renamed to '{newName}'");
                }
        
                var partyMenuManager = PartyMenuManager.Instance;
                if (partyMenuManager != null)
                {
                    var mainParty = partyMenuManager.GetMainParty();
                    var partyMember = mainParty.FirstOrDefault(m => m.CharacterId == characterId);
                    if (partyMember?.Stats != null)
                    {
                        GD.Print($"Party member '{characterId}' name updated in party");
                    }
                }
            }
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
        
        /// <summary>
        /// Advanced variable setting with math operations and random values
        /// </summary>
        public void SetVariableAdvanced(string variableName, Godot.Variant value, 
            bool useRandom, int randomMin, int randomMax,
            bool useMath, SetVariableCommand.MathOperation operation, Godot.Variant operand)
        {
            object finalValue = value.Obj;
    
            // Handle random value generation
            if (useRandom)
            {
                var rng = new RandomNumberGenerator();
                rng.Randomize();
                finalValue = rng.RandiRange(randomMin, randomMax);
            }
    
            // Handle math operations
            if (useMath && variables.TryGetValue(variableName, out var currentValue))
            {
                finalValue = PerformMathOperation(currentValue, operand.Obj, operation);
            }
    
            SetVariable(variableName, finalValue);
        }
        
        /// <summary>
        /// Perform mathematical operation on values
        /// </summary>
        private object PerformMathOperation(object current, object operand, SetVariableCommand.MathOperation operation)
        {
            // Try to convert to numbers
            if (!TryConvertToNumber(current, out double currentNum) || !TryConvertToNumber(operand, out double operandNum))
            {
                GD.PrintErr($"Cannot perform math operation on non-numeric values");
                return current;
            }
    
            return operation switch
            {
                SetVariableCommand.MathOperation.Set => operandNum,
                SetVariableCommand.MathOperation.Add => currentNum + operandNum,
                SetVariableCommand.MathOperation.Subtract => currentNum - operandNum,
                SetVariableCommand.MathOperation.Multiply => currentNum * operandNum,
                SetVariableCommand.MathOperation.Divide => operandNum != 0 ? currentNum / operandNum : currentNum,
                SetVariableCommand.MathOperation.Modulo => operandNum != 0 ? currentNum % operandNum : currentNum,
                _ => current
            };
        }
        
        /// <summary>
        /// Try to convert value to a number
        /// </summary>
        private bool TryConvertToNumber(object value, out double result)
        {
            result = 0;
    
            if (value is int intVal) { result = intVal; return true; }
            if (value is float floatVal) { result = floatVal; return true; }
            if (value is double doubleVal) { result = doubleVal; return true; }
            if (value is long longVal) { result = longVal; return true; }
    
            return false;
        }
        
        private Dictionary<string, EventPage> commonEvents = new Dictionary<string, EventPage>();

        /// <summary>
        /// Register a common event that can be called from anywhere
        /// </summary>
        public void RegisterCommonEvent(string eventId, EventPage eventPage)
        {
            commonEvents[eventId] = eventPage;
            GD.Print($"Registered common event: {eventId}");
        }

        /// <summary>
        /// Get a common event by ID
        /// </summary>
        public EventPage GetCommonEvent(string eventId)
        {
            if (commonEvents.TryGetValue(eventId, out var commonEvent))
            {
                return commonEvent;
            }
    
            GD.PrintErr($"Common event '{eventId}' not found");
            return null;
        }
        
        public void LoadCommonEventsFromFolder(string folderPath)
        {
            var dir = DirAccess.Open(folderPath);
            if (dir == null)
            {
                GD.PrintErr($"Failed to open common events folder: {folderPath}");
                return;
            }
    
            dir.ListDirBegin();
            string fileName = dir.GetNext();
    
            while (fileName != "")
            {
                if (!dir.CurrentIsDir() && fileName.EndsWith(".tres"))
                {
                    string path = $"{folderPath}/{fileName}";
                    var eventPage = GD.Load<EventPage>(path);
            
                    if (eventPage != null)
                    {
                        RegisterCommonEvent(eventPage.EventId, eventPage);
                    }
                }
                fileName = dir.GetNext();
            }
    
            dir.ListDirEnd();
        }
        
        private List<AudioStreamPlayer> activeSoundEffects = new List<AudioStreamPlayer>();

        /// <summary>
        /// Stop a specific sound effect or all SEs if name is empty
        /// </summary>
        public async Task StopSE(string soundEffectName = "")
        {
            if (string.IsNullOrEmpty(soundEffectName))
            {
                // Stop all active sound effects
                foreach (var player in activeSoundEffects.ToArray())
                {
                    if (IsInstanceValid(player) && player.Playing)
                    {
                        player.Stop();
                        player.QueueFree();
                    }
                }
                activeSoundEffects.Clear();
                GD.Print("All sound effects stopped");
            }
            else
            {
                // Stop specific sound effect by name
                var toRemove = new List<AudioStreamPlayer>();
        
                foreach (var player in activeSoundEffects)
                {
                    if (IsInstanceValid(player) && player.Name == soundEffectName)
                    {
                        player.Stop();
                        player.QueueFree();
                        toRemove.Add(player);
                    }
                }
        
                foreach (var player in toRemove)
                {
                    activeSoundEffects.Remove(player);
                }
        
                GD.Print($"Sound effect '{soundEffectName}' stopped");
            }
    
            await Task.CompletedTask;
        }
        
        /// <summary>
        /// Enhanced PlaySE that tracks sound effects for stopping
        /// Override the existing PlaySE method to add tracking
        /// </summary>
        public void PlaySETracked(AudioStream se, float volume, string seName = "")
        {
            if (se == null) return;
    
            var player = new AudioStreamPlayer();
            player.Stream = se;
            player.VolumeDb = volume;
            player.Bus = "SFX";
    
            if (!string.IsNullOrEmpty(seName))
            {
                player.Name = seName;
            }
    
            AddChild(player);
            player.Play();
    
            activeSoundEffects.Add(player);
    
            // Auto-cleanup when finished
            player.Finished += () =>
            {
                activeSoundEffects.Remove(player);
                player.QueueFree();
            };
        }
        
        /// <summary>
        /// Execute a move route on a character
        /// </summary>
        public async Task ExecuteMoveRoute(NodePath characterPath, 
            Godot.Collections.Array<MoveCommand> route, 
            bool repeat, bool skipIfBlocked)
        {
            var character = GetNodeOrNull<Node2D>(characterPath);
            if (character == null)
            {
                GD.PrintErr($"Character not found at path: {characterPath}");
                return;
            }
    
            // Get or add MoveRouteExecutor component
            var executor = character.GetNodeOrNull<MoveRouteExecutor>("MoveRouteExecutor");
            if (executor == null)
            {
                executor = new MoveRouteExecutor();
                character.AddChild(executor);
            }
    
            await executor.ExecuteRoute(character, route, repeat, skipIfBlocked);
        }
        
        /// <summary>
        /// Battle result for conditional branches
        /// </summary>
        public enum BattleResult
        {
            None,
            Victory,
            Escape,
            Defeat
        }
        
        private BattleResult lastBattleResult = BattleResult.None;

        /// <summary>
        /// Set the last battle result (called after battle ends)
        /// </summary>
        public void SetBattleResult(BattleResult result)
        {
            lastBattleResult = result;
            SetVariable("last_battle_result", (int)result);
            GD.Print($"Battle result set to: {result}");
        }

        /// <summary>
        /// Get the last battle result
        /// </summary>
        public BattleResult GetBattleResult()
        {
            return lastBattleResult;
        }

        /// <summary>
        /// Execute commands based on battle result
        /// </summary>
        public async Task ExecuteBattleResultBranch(
            Godot.Collections.Array<EventCommand> ifWin,
            Godot.Collections.Array<EventCommand> ifEscape,
            Godot.Collections.Array<EventCommand> ifLose)
        {
            switch (lastBattleResult)
            {
                case BattleResult.Victory:
                    if (ifWin != null && ifWin.Count > 0)
                    {
                        GD.Print("Executing victory branch");
                        await ExecuteCommands(ifWin);
                    }
                    break;
            
                case BattleResult.Escape:
                    if (ifEscape != null && ifEscape.Count > 0)
                    {
                        GD.Print("Executing escape branch");
                        await ExecuteCommands(ifEscape);
                    }
                    break;
            
                case BattleResult.Defeat:
                    if (ifLose != null && ifLose.Count > 0)
                    {
                        GD.Print("Executing defeat branch");
                        await ExecuteCommands(ifLose);
                    }
                    break;
            
                default:
                    GD.PrintErr("No battle result available for conditional branch");
                    break;
            }
    
            // Reset battle result after processing
            lastBattleResult = BattleResult.None;
        }
        
        /// <summary>
        /// Enhanced InitiateBattle that tracks results
        /// Replace your existing InitiateBattle method with this
        /// </summary>
        public async Task InitiateBattleWithResult(string troopId, bool canEscape, bool canLose, AudioStream battleBGM)
        {
            if (string.IsNullOrEmpty(troopId))
            {
                GD.PrintErr("Battle troop ID is empty");
                return;
            }
    
            // Use provided BGM or fall back to current battle BGM
            var bgmToUse = battleBGM ?? currentBattleBGM;
    
            // Fade out current music
            await StopBGM(0.5f);
    
            // Play battle transition (flash, sound, etc)
            if (ScreenEffects != null)
            {
                await ScreenEffects.Flash(Colors.White, 0.3f);
            }
    
            // TODO: Load battle scene with troopId
            GD.Print($"Initiating battle: Troop={troopId}, CanEscape={canEscape}, CanLose={canLose}");
    
            // Play battle BGM
            if (bgmToUse != null)
            {
                PlayBGM(bgmToUse, 0f, 1.0f);
            }
    
            // IMPORTANT: After battle ends, you need to call:
            // EventCommandExecutor.Instance.SetBattleResult(BattleResult.Victory/Escape/Defeat);
            // This should be done in your BattleManager when the battle concludes
    
            await Task.CompletedTask;
        }
        
        private long Vector2IToId(Vector2I vec)
        {
            return ((long)vec.X << 32) | (long)(uint)vec.Y;
        }
        
        /// <summary>
        /// Save current game state before battle
        /// </summary>
        private void SavePreBattleState()
        {
            // Store current scene path
            preBattleScenePath = GetTree().CurrentScene.SceneFilePath;
            
            // Store player position if player exists
            var player = GetTree().GetFirstNodeInGroup("player") as Node2D;
            if (player != null)
            {
                preBattlePlayerPosition = player.GlobalPosition;
                
                // Also save to GameManager for battle to access
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.LastMapScene = preBattleScenePath;
                    GameManager.Instance.LastPlayerPosition = preBattlePlayerPosition;
                }
            }
            
            GD.Print($"[EventCommandExecutor] Saved pre-battle state: {preBattleScenePath}");
        }
        
        /// <summary>
        /// Load troop data from database or create dynamically
        /// </summary>
        private TroopData LoadTroopData(string troopId)
        {
            // Try to load from database first
            if (GameManager.Instance?.Database != null)
            {
                var troop = GameManager.Instance.Database.GetTroop(troopId);
                if (troop != null) return troop;
            }
            
            // If not found, try loading as a resource file
            try
            {
                var troop = GD.Load<TroopData>($"res://Data/Troops/{troopId}.tres");
                if (troop != null) return troop;
            }
            catch
            {
                // Ignore load errors
            }
            
            // Create dynamic troop if it's in the format "enemy_id" or "enemy_id_count"
            return CreateDynamicTroop(troopId);
        }
        
        /// <summary>
        /// Store battle parameters for BattleScene to access
        /// </summary>
        private void StoreBattleParameters(TroopData troop, bool canEscape, bool canLose, AudioStream battleBGM)
        {
            // Create a dictionary to pass to the battle scene
            var battleParams = new Godot.Collections.Dictionary
            {
                { "TroopId", troop.TroopId },
                // FIX: Don't convert - troop.EnemyIds is already Godot.Collections.Array<string>
                { "EnemyIds", troop.EnemyIds },  // FIXED - no conversion needed!
                { "IsBossBattle", troop.IsBossBattle },
                { "CanEscape", canEscape },
                { "CanLose", canLose },
                { "BattleBGM", battleBGM?.ResourcePath ?? "" },
                { "BattleBackground", troop.BattleBackground?.ResourcePath ?? "" },
                { "ReturnScene", preBattleScenePath },
                { "PlayerPosition", preBattlePlayerPosition }
            };
    
            // Store in GameManager's metadata
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetMeta("PendingBattleData", battleParams);
            }
    
            // Also store locally
            SetMeta("PendingBattleData", battleParams);
        }
        
        /// <summary>
        /// Convert C# List to Godot Array
        /// </summary>
        private Godot.Collections.Array<string> ConvertToGodotArray(List<string> list)
        {
            var array = new Godot.Collections.Array<string>();
            foreach (var item in list)
            {
                array.Add(item);
            }
            return array;
        }
        
        /// <summary>
        /// Return to overworld after battle
        /// Called by BattleManager when battle ends
        /// </summary>
        public async Task ReturnFromBattle()
        {
            GD.Print("[EventCommandExecutor] Returning from battle");
            
            // Fade to black
            if (ScreenEffects != null)
            {
                await ScreenEffects.FadeToBlack(0.5f);
            }
            
            // Return to previous scene
            if (!string.IsNullOrEmpty(preBattleScenePath))
            {
                GetTree().ChangeSceneToFile(preBattleScenePath);
                
                // Wait for scene to load
                await ToSignal(GetTree(), SceneTree.SignalName.NodeAdded);
                
                // Restore player position
                var player = GetTree().GetFirstNodeInGroup("player") as Node2D;
                if (player != null)
                {
                    player.GlobalPosition = preBattlePlayerPosition;
                }
            }
            
            // Fade back in
            if (ScreenEffects != null)
            {
                await ScreenEffects.FadeFromBlack(0.5f);
            }
            
            // Resume previous BGM
            await ResumeBGM(0.5f);
        }
        
        /// <summary>
        /// Create a dynamic troop from an enemy ID pattern
        /// Patterns: "goblin", "goblin_3", "goblin+slime", etc.
        /// </summary>
        private TroopData CreateDynamicTroop(string troopId)
        {
            var troop = new TroopData();
            troop.TroopId = troopId;
            troop.TroopName = $"Dynamic Troop: {troopId}";

            // Parse patterns like "goblin_3" (3 goblins) or "goblin+slime" (mixed)
            if (troopId.Contains("+"))
            {
                // Multiple enemy types: "goblin+slime+wolf"
                var enemyIds = troopId.Split("+");
                foreach (var enemyId in enemyIds)
                {
                    troop.EnemyIds.Add(enemyId.Trim());
                }
            }
            else if (troopId.Contains("_"))
            {
                // Count pattern: "goblin_3" means 3 goblins
                var parts = troopId.Split("_");
                string enemyId = parts[0];
                int count = 1;
    
                if (parts.Length > 1 && int.TryParse(parts[1], out int parsedCount))
                {
                    count = parsedCount;
                }
    
                for (int i = 0; i < count; i++)
                {
                    troop.EnemyIds.Add(enemyId);
                }
            }
            else
            {
                // Single enemy
                troop.EnemyIds.Add(troopId);
            }

            GD.Print($"[EventCommandExecutor] Created dynamic troop with {troop.EnemyIds.Count} enemies");
            return troop;
        }

        /// <summary>
        /// Resume BGM with fade in
        /// </summary>
        private async Task ResumeBGM(float fadeInDuration)
        {
            // This assumes AudioManager has a method to resume or you stored the previous BGM
            // For now, you can just fade in any playing music
            if (AudioManager.Instance != null)
            {
                // If you have a way to store/restore previous BGM, use it here
                // For now, this is a placeholder
                await Task.CompletedTask;
            }
        }
        
        /// <summary>
        /// Troop data - represents an enemy formation
        /// Create these as .tres resources in res://Data/Troops/
        /// </summary>
        [GlobalClass]
        public partial class TroopData : Resource
        {
            [Export] public string TroopId { get; set; } = "troop_001";
            [Export] public string TroopName { get; set; } = "Enemy Group";
    
            // FIX: Use Godot.Collections.Array instead of List for Export compatibility
            [Export] public Godot.Collections.Array<string> EnemyIds { get; set; }
    
            [Export] public bool IsBossBattle { get; set; } = false;
            [Export] public Texture2D BattleBackground { get; set; }
            [Export] public AudioStream BattleBGM { get; set; }

            public TroopData()
            {
                EnemyIds = new Godot.Collections.Array<string>();
            }
        }

        #endregion
    }
}