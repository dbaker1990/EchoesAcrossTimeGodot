using Godot;
using System;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Database;
using System.Collections.Generic;
using EchoesAcrossTime.Characters.States;

public partial class OverworldCharacter : CharacterBody2D
{
    [ExportGroup("Character Properties")]
    [Export] public string CharacterName { get; set; } = "Character";
    [Export] public bool IsPlayerControlled { get; set; } = false;
    
    [ExportGroup("Movement")]
    [Export] public float WalkSpeed { get; set; } = 100f;
    [Export] public float RunSpeed { get; set; } = 200f;
    [Export] public float ClimbSpeed { get; set; } = 80f;
    [Export] public float PushPullSpeed { get; set; } = 60f;
    [Export] public float Acceleration { get; set; } = 800f;
    [Export] public float Friction { get; set; } = 600f;
    
    [ExportGroup("Jumping")]
    [Export] public float JumpDistance { get; set; } = 64f;
    [Export] public float JumpDuration { get; set; } = 0.4f;
    [Export] public float JumpHeight { get; set; } = 32f;
    
    [ExportGroup("Animation")]
    [Export] public AnimatedSprite2D AnimatedSprite { get; set; }
    [Export] public Sprite2D ShadowSprite { get; set; }
    
    [ExportGroup("Interaction")]
    [Export] public Area2D InteractionArea { get; set; }
    [Export] public float InteractionRange { get; set; } = 32f;
    
    [ExportGroup("Character Data")]
    public CharacterData CharacterData { get; set; }
    public CharacterStats Stats { get; set; }
    
    // Direction enum
    public enum Direction
    {
        Down,
        Up,
        Left,
        Right
    }
    
    // State management
    public CharacterStateType CurrentStateType { get; private set; } = CharacterStateType.Idle;
    public Direction CurrentDirection { get; set; } = Direction.Down;
    
    private Dictionary<CharacterStateType, CharacterState> states = new Dictionary<CharacterStateType, CharacterState>();
    private CharacterState currentState;
    
    // Movement data
    public Vector2 InputDirection { get; set; } = Vector2.Zero;
    public bool IsRunning { get; set; } = false;
    
    // Interaction data
    public Node2D CurrentInteractable { get; private set; }
    
    [Signal]
    public delegate void StateChangedEventHandler(CharacterStateType newState);
    
    [Signal]
    public delegate void DirectionChangedEventHandler(Direction newDirection);
    
    public override void _Ready()
    {
        SetupComponents();
        InitializeStates();
        if (CharacterData != null && Stats == null)
        {
            Stats = CharacterData.CreateStatsInstance();
        }
        
        ChangeState(CharacterStateType.Idle);
    }
    
    private void SetupComponents()
    {
        if (AnimatedSprite == null)
            AnimatedSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        
        if (ShadowSprite == null)
            ShadowSprite = GetNodeOrNull<Sprite2D>("Shadow");
        
        if (InteractionArea == null)
            InteractionArea = GetNodeOrNull<Area2D>("InteractionArea");
        
        if (InteractionArea != null)
        {
            InteractionArea.AreaEntered += OnInteractionAreaEntered;
            InteractionArea.AreaExited += OnInteractionAreaExited;
        }
    }
    
    private void InitializeStates()
    {
        // Add state nodes
        var stateContainer = new Node { Name = "States" };
        AddChild(stateContainer);
        
        AddState(new IdleState(), stateContainer);
        AddState(new WalkingState(), stateContainer);
        AddState(new RunningState(), stateContainer);
        AddState(new JumpingState(), stateContainer);
        AddState(new FishingState(), stateContainer);
        AddState(new PushingState(), stateContainer);
        AddState(new PullingState(), stateContainer);
        AddState(new ClimbingState(), stateContainer);
        AddState(new OpeningChestState(), stateContainer);
        AddState(new LockedState(), stateContainer);
    }
    
    private void AddState(CharacterState state, Node parent)
    {
        parent.AddChild(state);
        states[state.StateType] = state;
    }
    
    public override void _Process(double delta)
    {
        currentState?.Update(delta);
    }
    
    public override void _PhysicsProcess(double delta)
    {
        currentState?.PhysicsUpdate(delta);
    }
    
    public override void _Input(InputEvent @event)
    {
        currentState?.HandleInput(@event);
    }
    
    public void ChangeState(CharacterStateType newStateType)
    {
        if (CurrentStateType == newStateType) return;
        
        if (!states.ContainsKey(newStateType))
        {
            GD.PrintErr($"State {newStateType} not found!");
            return;
        }
        
        currentState?.Exit();
        
        CurrentStateType = newStateType;
        currentState = states[newStateType];
        currentState.Enter(this);
        
        UpdateAnimation();
        EmitSignal(SignalName.StateChanged, (int)newStateType);
    }
    
    public void UpdateAnimation()
    {
        if (AnimatedSprite == null) return;
        
        string animationName = currentState?.GetAnimationName() ?? GetDefaultAnimationName();
        
        if (AnimatedSprite.SpriteFrames.HasAnimation(animationName))
        {
            if (AnimatedSprite.Animation != animationName)
            {
                AnimatedSprite.Play(animationName);
            }
        }
        else
        {
            GD.PrintErr($"Animation '{animationName}' not found!");
        }
    }
    
    private string GetDefaultAnimationName()
    {
        string state = CurrentStateType switch
        {
            CharacterStateType.Idle => "idle",
            CharacterStateType.Walking => "walk",
            CharacterStateType.Running => "run",
            CharacterStateType.Jumping => "jump",
            CharacterStateType.Fishing => "fishing",
            CharacterStateType.Pushing => "push",
            CharacterStateType.Pulling => "pull",
            CharacterStateType.Climbing => "climb",
            CharacterStateType.OpeningChest => "open_chest",
            _ => "idle"
        };
        
        string direction = CurrentDirection switch
        {
            Direction.Down => "down",
            Direction.Up => "up",
            Direction.Left => "left",
            Direction.Right => "right",
            _ => "down"
        };
        
        return $"{state}_{direction}";
    }
    
    public void SetDirection(Direction direction)
    {
        if (CurrentDirection != direction)
        {
            CurrentDirection = direction;
            EmitSignal(SignalName.DirectionChanged, (int)direction);
            UpdateAnimation();
        }
    }
    
    public void UpdateDirectionFromInput()
    {
        if (InputDirection == Vector2.Zero) return;
        
        if (Mathf.Abs(InputDirection.X) > Mathf.Abs(InputDirection.Y))
        {
            SetDirection(InputDirection.X > 0 ? Direction.Right : Direction.Left);
        }
        else
        {
            SetDirection(InputDirection.Y > 0 ? Direction.Down : Direction.Up);
        }
    }
    
    public Vector2 GetDirectionVector()
    {
        return CurrentDirection switch
        {
            Direction.Down => Vector2.Down,
            Direction.Up => Vector2.Up,
            Direction.Left => Vector2.Left,
            Direction.Right => Vector2.Right,
            _ => Vector2.Down
        };
    }
    
    public void LockCharacter()
    {
        ChangeState(CharacterStateType.Locked);
    }
    
    public void UnlockCharacter()
    {
        ChangeState(CharacterStateType.Idle);
    }
    
    public void ApplyMovement(double delta, float speed)
    {
        if (InputDirection != Vector2.Zero)
        {
            Velocity = Velocity.MoveToward(InputDirection * speed, Acceleration * (float)delta);
        }
        else
        {
            Velocity = Velocity.MoveToward(Vector2.Zero, Friction * (float)delta);
        }
        
        MoveAndSlide();
    }
    
    public void ApplyFriction(double delta)
    {
        Velocity = Velocity.MoveToward(Vector2.Zero, Friction * (float)delta);
        MoveAndSlide();
    }
    
    private void OnInteractionAreaEntered(Area2D area)
    {
        if (area.GetParent() is IInteractable interactable)
        {
            CurrentInteractable = area.GetParent<Node2D>();
        }
    }
    
    private void OnInteractionAreaExited(Area2D area)
    {
        if (area.GetParent() == CurrentInteractable)
        {
            CurrentInteractable = null;
        }
    }
    
    public IInteractable GetCurrentInteractable()
    {
        return CurrentInteractable as IInteractable;
    }
    
    public void TeleportTo(Vector2 position, Direction? facingDirection = null)
    {
        GlobalPosition = position;
        Velocity = Vector2.Zero;
        InputDirection = Vector2.Zero;
    
        if (facingDirection.HasValue)
        {
            SetDirection(facingDirection.Value);
        }
    
        // Ensure we're in idle state after teleport
        if (CurrentStateType != CharacterStateType.Locked)
        {
            ChangeState(CharacterStateType.Idle);
        }
    
        UpdateAnimation();
    }
}