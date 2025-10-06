// Events/EventCommand.cs
using Godot;
using System;
using System.Threading.Tasks;

namespace EchoesAcrossTime.Events
{
    public enum EventCommandType
    {
        ShowText,
        ShowChoices,
        Wait,
        ChangeGold,
        ChangeItems,
        ChangeWeapons,
        HealCharacter,
        ChangePartyMembers,
        MovePlayer,
        FadeScreen,
        TintScreen,
        FlashScreen,
        ShakeScreen,
        PlayBGM,
        StopBGM,
        PlaySE,
        StopSE,
        TransferMap,
        ConditionalBranch,
        SetVariable,
        CallCommonEvent,
        ControlSelfSwitch,
        NameInput,
        ChangeTransparency,
        ShowBalloonIcon,
        ChangeBattleBGM,
        InitiateBattle,
        InitiateShop,
        MoveCommand,
        ConditionalBattleResultCommand
    }

    /// <summary>
    /// Base class for all event commands
    /// </summary>
    [GlobalClass]
    public partial class EventCommand : Resource
    {
        [Export] public EventCommandType Type { get; set; }
        [Export] public bool WaitForCompletion { get; set; } = true;
        
        public virtual async Task Execute(EventCommandExecutor executor)
        {
            await Task.CompletedTask;
        }
    }

    
    /// <summary>
    /// Show text message
    /// </summary>
    [GlobalClass]
    public partial class ShowTextCommand : EventCommand
    {
        [Export] public DialogueData Dialogue { get; set; }
        [Export] public string DialogueTableId { get; set; } = "";
        [Export] public int DialogueIndex { get; set; } = 0;
        
        public ShowTextCommand()
        {
            Type = EventCommandType.ShowText;
        }
        
        public override async Task Execute(EventCommandExecutor executor)
        {
            DialogueData dialogueToShow = Dialogue;
            
            // Load from table if specified
            if (!string.IsNullOrEmpty(DialogueTableId))
            {
                var table = executor.GetDialogueTable(DialogueTableId);
                dialogueToShow = table?.GetEntry(DialogueIndex);
            }
            
            if (dialogueToShow != null)
            {
                await executor.ShowMessage(dialogueToShow);
            }
        }
    }

    /// <summary>
    /// Show choices to player
    /// </summary>
    [GlobalClass]
    public partial class ShowChoicesCommand : EventCommand
    {
        [Export] public Godot.Collections.Array<string> Choices { get; set; }
        [Export] public int DefaultChoice { get; set; } = 0;
        [Export] public string VariableName { get; set; } = "choice_result";
        
        public ShowChoicesCommand()
        {
            Type = EventCommandType.ShowChoices;
            Choices = new Godot.Collections.Array<string>();
        }
        
        public override async Task Execute(EventCommandExecutor executor)
        {
            var choices = new System.Collections.Generic.List<string>();
            foreach (var choice in Choices)
            {
                choices.Add(choice);
            }
            
            int result = await executor.ShowChoices(choices, DefaultChoice);
            executor.SetVariable(VariableName, result);
        }
    }

    /// <summary>
    /// Wait for specified duration
    /// </summary>
    [GlobalClass]
    public partial class WaitCommand : EventCommand
    {
        [Export] public float Duration { get; set; } = 1.0f;
        [Export] public bool WaitForInput { get; set; } = false;
        
        public WaitCommand()
        {
            Type = EventCommandType.Wait;
        }
        
        public override async Task Execute(EventCommandExecutor executor)
        {
            if (WaitForInput)
            {
                await executor.WaitForInput();
            }
            else
            {
                await executor.Wait(Duration);
            }
        }
    }

    /// <summary>
    /// Change player's gold
    /// </summary>
    [GlobalClass]
    public partial class ChangeGoldCommand : EventCommand
    {
        [Export] public int Amount { get; set; } = 0;
        [Export] public bool IsIncrease { get; set; } = true;
        
        public ChangeGoldCommand()
        {
            Type = EventCommandType.ChangeGold;
            WaitForCompletion = false;
        }
        
        public override async Task Execute(EventCommandExecutor executor)
        {
            executor.ChangeGold(IsIncrease ? Amount : -Amount);
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Change items in inventory
    /// </summary>
    [GlobalClass]
    public partial class ChangeItemsCommand : EventCommand
    {
        [Export] public string ItemId { get; set; } = "";
        [Export] public int Quantity { get; set; } = 1;
        [Export] public bool IsAdd { get; set; } = true;
        
        public ChangeItemsCommand()
        {
            Type = EventCommandType.ChangeItems;
            WaitForCompletion = false;
        }
        
        public override async Task Execute(EventCommandExecutor executor)
        {
            executor.ChangeItems(ItemId, IsAdd ? Quantity : -Quantity);
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Change weapons/equipment
    /// </summary>
    [GlobalClass]
    public partial class ChangeWeaponsCommand : EventCommand
    {
        [Export] public string CharacterId { get; set; } = "";
        [Export] public string WeaponId { get; set; } = "";
        [Export] public bool Equip { get; set; } = true;
        
        public ChangeWeaponsCommand()
        {
            Type = EventCommandType.ChangeWeapons;
            WaitForCompletion = false;
        }
        
        public override async Task Execute(EventCommandExecutor executor)
        {
            executor.ChangeWeapon(CharacterId, WeaponId, Equip);
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Heal character in party
    /// </summary>
    [GlobalClass]
    public partial class HealCharacterCommand : EventCommand
    {
        [Export] public string CharacterId { get; set; } = "";
        [Export] public bool HealAll { get; set; } = false;
        [Export] public int HPAmount { get; set; } = 0;
        [Export] public int MPAmount { get; set; } = 0;
        [Export] public bool FullHeal { get; set; } = false;
        [Export] public bool RemoveStatusEffects { get; set; } = false;
        
        public HealCharacterCommand()
        {
            Type = EventCommandType.HealCharacter;
            WaitForCompletion = false;
        }
        
        public override async Task Execute(EventCommandExecutor executor)
        {
            executor.HealCharacter(CharacterId, HealAll, HPAmount, MPAmount, FullHeal, RemoveStatusEffects);
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Change party members
    /// </summary>
    [GlobalClass]
    public partial class ChangePartyMembersCommand : EventCommand
    {
        [Export] public string CharacterId { get; set; } = "";
        [Export] public bool AddToParty { get; set; } = true;
        [Export] public bool Initialize { get; set; } = false;
        
        public ChangePartyMembersCommand()
        {
            Type = EventCommandType.ChangePartyMembers;
            WaitForCompletion = false;
        }
        
        public override async Task Execute(EventCommandExecutor executor)
        {
            executor.ChangePartyMember(CharacterId, AddToParty, Initialize);
            await Task.CompletedTask;
        }
    }

    // Inside your EventCommand.cs file

    /// <summary>
    /// Move a character to a location using pathfinding.
    /// </summary>
    [GlobalClass]
    public partial class MovePlayerCommand : EventCommand
    {
        [Export] public NodePath TargetCharacterPath { get; set; }
        [Export] public Vector2 TargetPosition { get; set; }
        [Export] public float MoveSpeed { get; set; } = 200.0f;

        public MovePlayerCommand()
        {
            Type = EventCommandType.MovePlayer;
        }

        public override async Task Execute(EventCommandExecutor executor)
        {
            // CORRECTED: The method name now matches the one in the executor.
            await executor.MoveCharacterOnPath(TargetCharacterPath, TargetPosition, MoveSpeed);
        }
    }
    
    /// <summary>
    /// Executes a different set of commands based on a variable's value.
    /// </summary>
    [GlobalClass]
    public partial class ConditionalBranchCommand : EventCommand
    {
        [ExportGroup("Condition")]
        [Export] public string VariableName { get; set; } = "";
        [Export] public Godot.Variant ValueToCompare { get; set; }
        [Export] public ComparisonOperator Operator { get; set; } = ComparisonOperator.EqualTo;

        [ExportGroup("Branches")]
        [Export] public Godot.Collections.Array<EventCommand> ThenCommands { get; set; }
        [Export] public Godot.Collections.Array<EventCommand> ElseCommands { get; set; }


        public ConditionalBranchCommand()
        {
            Type = EventCommandType.ConditionalBranch;
            ThenCommands = new Godot.Collections.Array<EventCommand>();
            ElseCommands = new Godot.Collections.Array<EventCommand>();
        }

        public override async Task Execute(EventCommandExecutor executor)
        {
            bool conditionMet = executor.EvaluateCondition(VariableName, Operator, ValueToCompare);

            if (conditionMet)
            {
                await executor.ExecuteCommands(ThenCommands);
            }
            else
            {
                await executor.ExecuteCommands(ElseCommands);
            }
        }

        public enum ComparisonOperator
        {
            EqualTo,
            NotEqualTo,
            GreaterThan,
            LessThan,
            GreaterThanOrEqualTo,
            LessThanOrEqualTo
        }
    }

    /// <summary>
    /// Fade screen in/out
    /// </summary>
    [GlobalClass]
    public partial class FadeScreenCommand : EventCommand
    {
        [Export] public bool FadeOut { get; set; } = true;
        [Export] public float Duration { get; set; } = 1.0f;
        [Export] public Color FadeColor { get; set; } = Colors.Black;
        
        public FadeScreenCommand()
        {
            Type = EventCommandType.FadeScreen;
        }
        
        public override async Task Execute(EventCommandExecutor executor)
        {
            await executor.FadeScreen(FadeOut, Duration, FadeColor);
        }
    }

    /// <summary>
    /// Tint screen
    /// </summary>
    [GlobalClass]
    public partial class TintScreenCommand : EventCommand
    {
        [Export] public Color TintColor { get; set; } = Colors.White;
        [Export] public float Duration { get; set; } = 1.0f;
        
        public TintScreenCommand()
        {
            Type = EventCommandType.TintScreen;
        }
        
        public override async Task Execute(EventCommandExecutor executor)
        {
            await executor.TintScreen(TintColor, Duration);
        }
    }
    
    [GlobalClass]
    public partial class SetVariableCommand : EventCommand
    {
        [Export] public string VariableName { get; set; } = "";
        [Export] public Godot.Variant Value { get; set; }

        // Advanced options
        [Export] public bool UseRandomValue { get; set; } = false;
        [Export] public int RandomMin { get; set; } = 0;
        [Export] public int RandomMax { get; set; } = 100;

        // Math operations
        [Export] public bool UseMathOperation { get; set; } = false;
        [Export] public MathOperation Operation { get; set; } = MathOperation.Set;
        [Export] public Godot.Variant Operand { get; set; }

        public enum MathOperation { Set, Add, Subtract, Multiply, Divide, Modulo }
    
        public SetVariableCommand()
        {
            Type = EventCommandType.SetVariable;
            WaitForCompletion = false;
        }
    
        public override async Task Execute(EventCommandExecutor executor)
        {
            executor.SetVariableAdvanced(
                VariableName, 
                Value, 
                UseRandomValue, 
                RandomMin, 
                RandomMax,
                UseMathOperation, 
                Operation, 
                Operand
            );
        
            await Task.CompletedTask;
        }
    }
    
    // Enum exists but no implementation!
    [GlobalClass]
    public partial class CallCommonEventCommand : EventCommand
    {
        [Export] public EventPage CommonEvent { get; set; }
        [Export] public string CommonEventId { get; set; } = "";
    
        public override async Task Execute(EventCommandExecutor executor)
        {
            // Load from database if ID provided
            if (!string.IsNullOrEmpty(CommonEventId))
            {
                CommonEvent = executor.GetCommonEvent(CommonEventId);
            }
        
            if (CommonEvent != null)
            {
                await executor.ExecuteEvent(CommonEvent);
            }
        }
    }
    
    [GlobalClass]
    public partial class StopSECommand : EventCommand
    {
        [Export] public string SoundEffectName { get; set; } = ""; // Optional: stop specific SE
    
        public StopSECommand()
        {
            Type = EventCommandType.StopSE;
        }
    
        public override async Task Execute(EventCommandExecutor executor)
        {
            await executor.StopSE(SoundEffectName);
        }
    }
    
    [GlobalClass]
    public partial class SetMoveRouteCommand : EventCommand
    {
        [Export] public NodePath TargetCharacterPath { get; set; }
        [Export] public Godot.Collections.Array<MoveCommand> Route { get; set; }
        [Export] public bool Repeat { get; set; } = false;
        [Export] public bool SkipIfBlocked { get; set; } = false;
    
        public SetMoveRouteCommand()
        {
            Type = EventCommandType.MovePlayer; // Or add new type: SetMoveRoute
            Route = new Godot.Collections.Array<MoveCommand>();
        }
    
        public override async Task Execute(EventCommandExecutor executor)
        {
            await executor.ExecuteMoveRoute(TargetCharacterPath, Route, Repeat, SkipIfBlocked);
        }
    }

    public enum MoveCommand
    {
        MoveUp, MoveDown, MoveLeft, MoveRight,
        TurnUp, TurnDown, TurnLeft, TurnRight,
        Turn90Left, Turn90Right, Turn180, TurnRandom,
        Jump, Wait, StepForward, StepBackward,
        ChangeSpeed, ChangeFrequency,
        SwitchOn, SwitchOff,
        ChangeGraphic, ChangeOpacity,
        PlayAnimation
    }
    
    [GlobalClass]
    public partial class ConditionalBattleResultCommand : EventCommand
    {
        [Export] public Godot.Collections.Array<EventCommand> IfWin { get; set; }
        [Export] public Godot.Collections.Array<EventCommand> IfEscape { get; set; }
        [Export] public Godot.Collections.Array<EventCommand> IfLose { get; set; }
    
        public ConditionalBattleResultCommand()
        {
            Type = EventCommandType.ConditionalBranch; // Or add new type: BattleResultBranch
            IfWin = new Godot.Collections.Array<EventCommand>();
            IfEscape = new Godot.Collections.Array<EventCommand>();
            IfLose = new Godot.Collections.Array<EventCommand>();
        }
    
        public override async Task Execute(EventCommandExecutor executor)
        {
            await executor.ExecuteBattleResultBranch(IfWin, IfEscape, IfLose);
        }
    }
    
    /// <summary>
    /// Change the battle BGM
    /// </summary>
    [GlobalClass]
    public partial class ChangeBattleBGMCommand : EventCommand
    {
        [Export] public AudioStream BattleBGM { get; set; }
    
        public ChangeBattleBGMCommand()
        {
            Type = EventCommandType.ChangeBattleBGM;
            WaitForCompletion = false;
        }
    
        public override async Task Execute(EventCommandExecutor executor)
        {
            executor.ChangeBattleBGM(BattleBGM);
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Initiate a battle encounter
    /// </summary>
    [GlobalClass]
    public partial class InitiateBattleCommand : EventCommand
    {
        [Export] public string TroopId { get; set; } = "";
        [Export] public bool CanEscape { get; set; } = true;
        [Export] public bool CanLose { get; set; } = false;
        [Export] public AudioStream BattleBGM { get; set; }
    
        public InitiateBattleCommand()
        {
            Type = EventCommandType.InitiateBattle;
        }
    
        public override async Task Execute(EventCommandExecutor executor)
        {
            await executor.InitiateBattle(TroopId, CanEscape, CanLose, BattleBGM);
        }
    }

    /// <summary>
    /// Open the shop interface
    /// </summary>
    [GlobalClass]
    public partial class InitiateShopCommand : EventCommand
    {
        [Export] public Godot.Collections.Array<string> ItemIds { get; set; }
        [Export] public bool CanSell { get; set; } = true;
        [Export] public bool CanBuy { get; set; } = true;
    
        public InitiateShopCommand()
        {
            Type = EventCommandType.InitiateShop;
            ItemIds = new Godot.Collections.Array<string>();
        }
    
        public override async Task Execute(EventCommandExecutor executor)
        {
            await executor.InitiateShop(ItemIds, CanBuy, CanSell);
        }
    }

    /// <summary>
    /// Flash screen
    /// </summary>
    [GlobalClass]
    public partial class FlashScreenCommand : EventCommand
    {
        [Export] public Color FlashColor { get; set; } = Colors.White;
        [Export] public float Duration { get; set; } = 0.5f;
        
        public FlashScreenCommand()
        {
            Type = EventCommandType.FlashScreen;
        }
        
        public override async Task Execute(EventCommandExecutor executor)
        {
            await executor.FlashScreen(FlashColor, Duration);
        }
    }

    /// <summary>
    /// Shake screen
    /// </summary>
    [GlobalClass]
    public partial class ShakeScreenCommand : EventCommand
    {
        [Export] public float Intensity { get; set; } = 10f;
        [Export] public float Duration { get; set; } = 0.5f;
        
        public ShakeScreenCommand()
        {
            Type = EventCommandType.ShakeScreen;
        }
        
        public override async Task Execute(EventCommandExecutor executor)
        {
            await executor.ShakeScreen(Intensity, Duration);
        }
    }

    /// <summary>
    /// Play BGM
    /// </summary>
    [GlobalClass]
    public partial class PlayBGMCommand : EventCommand
    {
        [Export] public AudioStream BGM { get; set; }
        [Export] public float Volume { get; set; } = 0f; // dB
        [Export] public float FadeInDuration { get; set; } = 0f;
        
        public PlayBGMCommand()
        {
            Type = EventCommandType.PlayBGM;
            WaitForCompletion = false;
        }
        
        public override async Task Execute(EventCommandExecutor executor)
        {
            executor.PlayBGM(BGM, Volume, FadeInDuration);
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Stop BGM
    /// </summary>
    [GlobalClass]
    public partial class StopBGMCommand : EventCommand
    {
        [Export] public float FadeOutDuration { get; set; } = 1.0f;
        
        public StopBGMCommand()
        {
            Type = EventCommandType.StopBGM;
        }
        
        public override async Task Execute(EventCommandExecutor executor)
        {
            await executor.StopBGM(FadeOutDuration);
        }
    }

    /// <summary>
    /// Play sound effect
    /// </summary>
    [GlobalClass]
    public partial class PlaySECommand : EventCommand
    {
        [Export] public AudioStream SoundEffect { get; set; }
        [Export] public float Volume { get; set; } = 0f; // dB
        
        public PlaySECommand()
        {
            Type = EventCommandType.PlaySE;
            WaitForCompletion = false;
        }
        
        public override async Task Execute(EventCommandExecutor executor)
        {
            executor.PlaySE(SoundEffect, Volume);
            await Task.CompletedTask;
        }
    }

    /// <summary>
    /// Transfer to another map
    /// </summary>
    [GlobalClass]
    public partial class TransferMapCommand : EventCommand
    {
        [Export] public string MapPath { get; set; } = "";
        [Export] public Vector2 SpawnPosition { get; set; }
        [Export] public int FacingDirection { get; set; } = -1; // -1 = no change, 0-3 = directions
        [Export] public bool FadeOut { get; set; } = true;
        [Export] public bool FadeIn { get; set; } = true;
        
        public TransferMapCommand()
        {
            Type = EventCommandType.TransferMap;
        }
        
        public override async Task Execute(EventCommandExecutor executor)
        {
            OverworldCharacter.Direction? direction = FacingDirection >= 0 
                ? (OverworldCharacter.Direction)FacingDirection 
                : null;
            
            await executor.TransferMap(MapPath, SpawnPosition, direction, FadeOut, FadeIn);
        }
        
        /// <summary>
        /// Controls a switch (A, B, C, D) local to a specific event object.
        /// </summary>
        [GlobalClass]
        public partial class ControlSelfSwitchCommand : EventCommand
        {
            [Export] public NodePath TargetEventPath { get; set; }
            [Export(PropertyHint.Enum, "A,B,C,D")] public string Switch { get; set; } = "A";
            [Export] public bool Value { get; set; } = true;

            public ControlSelfSwitchCommand()
            {
                Type = EventCommandType.ControlSelfSwitch;
                WaitForCompletion = false;
            }

            public override async Task Execute(EventCommandExecutor executor)
            {
                var targetEvent = executor.GetNodeOrNull<EventObject>(TargetEventPath);
                targetEvent?.SetSelfSwitch(Switch, Value);
                await Task.CompletedTask;
            }
        }
        
        /// <summary>
        /// Shows the name input screen for a character.
        /// </summary>
        [GlobalClass]
        public partial class NameInputCommand : EventCommand
        {
            [Export] public string CharacterId { get; set; } // The ID of the character in your database
            [Export] public int MaxLength { get; set; } = 10;

            public NameInputCommand()
            {
                Type = EventCommandType.NameInput;
            }

            public override async Task Execute(EventCommandExecutor executor)
            {
                await executor.ProcessNameInput(CharacterId, MaxLength);
            }
        }

        /// <summary>
        /// Changes a character's visibility on the map.
        /// </summary>
        [GlobalClass]
        public partial class ChangeTransparencyCommand : EventCommand
        {
            [Export] public NodePath TargetCharacterPath { get; set; }
            [Export] public bool IsHidden { get; set; } = false;

            public ChangeTransparencyCommand()
            {
                Type = EventCommandType.ChangeTransparency;
                WaitForCompletion = false;
            }

            public override async Task Execute(EventCommandExecutor executor)
            {
                executor.ChangeTransparency(TargetCharacterPath, IsHidden);
                await Task.CompletedTask;
            }
        }

        /// <summary>
        /// Shows a balloon icon (e.g., "!") over a character's head.
        /// </summary>
        [GlobalClass]
        public partial class ShowBalloonIconCommand : EventCommand
        {
            public enum BalloonType { Exclamation, Question, MusicNote, Heart, Anger, Sweat, Silence, Lightbulb }
            [Export] public NodePath TargetCharacterPath { get; set; }
            [Export] public BalloonType Icon { get; set; } = BalloonType.Exclamation;

            public ShowBalloonIconCommand()
            {
                Type = EventCommandType.ShowBalloonIcon;
            }

            public override async Task Execute(EventCommandExecutor executor)
            {
                await executor.ShowBalloonIcon(TargetCharacterPath, Icon, WaitForCompletion);
            }
        }
        
    }
}