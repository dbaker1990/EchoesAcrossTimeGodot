using Godot;
using System;
using System.Collections.Generic;

public partial class FollowerCharacter : OverworldCharacter
{
    [ExportGroup("Follower Settings")]
    [Export] public float FollowDistance { get; set; } = 32f;
    [Export] public float MinFollowDistance { get; set; } = 16f;
    [Export] public float CatchUpDistance { get; set; } = 96f;
    [Export] public int TrailLength { get; set; } = 30;
    [Export] public bool MimicLeaderStates { get; set; } = true;
    [Export] public bool CanPassThroughLeader { get; set; } = true;
    
    private Node2D followTarget;
    private OverworldCharacter leaderCharacter;
    private Queue<FollowerTrailPoint> positionTrail = new Queue<FollowerTrailPoint>();
    private Vector2 targetPosition;
    private float distanceToTarget;
    private CharacterStateType previousLeaderState = CharacterStateType.Idle;
    
    private struct FollowerTrailPoint
    {
        public Vector2 Position;
        public Direction Direction;
        public CharacterStateType State;
        
        public FollowerTrailPoint(Vector2 position, Direction direction, CharacterStateType state)
        {
            Position = position;
            Direction = direction;
            State = state;
        }
    }
    
    public override void _Ready()
    {
        base._Ready();
        IsPlayerControlled = false;
    }
    
    public void SetFollowTarget(Node2D target)
    {
        followTarget = target;
        leaderCharacter = target as OverworldCharacter;
        
        if (followTarget != null)
        {
            targetPosition = followTarget.GlobalPosition;
        }
        
        // Connect to leader's state changes if mimicking
        if (MimicLeaderStates && leaderCharacter != null)
        {
            if (!leaderCharacter.IsConnected(OverworldCharacter.SignalName.StateChanged, new Callable(this, MethodName.OnLeaderStateChanged)))
            {
                leaderCharacter.StateChanged += OnLeaderStateChanged;
            }
        }
    }
    
    public override void _PhysicsProcess(double delta)
    {
        if (followTarget == null)
        {
            base._PhysicsProcess(delta);
            return;
        }
        
        // Don't process movement if in a locked state or special interaction state
        if (ShouldFollowBehavior())
        {
            UpdateTrail();
            HandleFollowerMovement(delta);
        }
        else
        {
            // Let the state machine handle it (like cutscenes, animations, etc.)
            base._PhysicsProcess(delta);
        }
    }
    
    private bool ShouldFollowBehavior()
    {
        // Follow behavior active unless in special states
        return CurrentStateType != CharacterStateType.Locked &&
               CurrentStateType != CharacterStateType.OpeningChest &&
               CurrentStateType != CharacterStateType.Fishing &&
               CurrentStateType != CharacterStateType.Climbing &&
               CurrentStateType != CharacterStateType.Jumping;
    }
    
    private void HandleFollowerMovement(double delta)
    {
        // Get target from trail or direct position
        if (positionTrail.Count >= TrailLength / 2)
        {
            var trailPoint = positionTrail.Peek();
            targetPosition = trailPoint.Position;
            
            // Update direction from trail
            if (CurrentStateType == CharacterStateType.Idle || 
                CurrentStateType == CharacterStateType.Walking || 
                CurrentStateType == CharacterStateType.Running)
            {
                CurrentDirection = trailPoint.Direction;
            }
        }
        else if (followTarget != null)
        {
            targetPosition = followTarget.GlobalPosition;
        }
        
        distanceToTarget = GlobalPosition.DistanceTo(targetPosition);
        
        // Determine movement based on distance
        if (distanceToTarget > FollowDistance)
        {
            // Calculate direction to target
            InputDirection = (targetPosition - GlobalPosition).Normalized();
            
            // Determine speed based on distance (catch up if too far)
            if (distanceToTarget > CatchUpDistance)
            {
                IsRunning = true;
                
                // If extremely far, snap closer
                if (distanceToTarget > CatchUpDistance * 2)
                {
                    GlobalPosition = GlobalPosition.Lerp(targetPosition, 0.5f);
                }
            }
            else if (leaderCharacter != null)
            {
                // Match leader's running state
                IsRunning = leaderCharacter.CurrentStateType == CharacterStateType.Running;
            }
            else
            {
                IsRunning = distanceToTarget > FollowDistance * 2;
            }
            
            UpdateDirectionFromInput();
            
            // Transition to appropriate movement state
            if (CurrentStateType == CharacterStateType.Idle)
            {
                ChangeState(IsRunning ? CharacterStateType.Running : CharacterStateType.Walking);
            }
            else if (CurrentStateType == CharacterStateType.Walking && IsRunning)
            {
                ChangeState(CharacterStateType.Running);
            }
            else if (CurrentStateType == CharacterStateType.Running && !IsRunning)
            {
                ChangeState(CharacterStateType.Walking);
            }
            
            // Apply movement through state system
            float currentSpeed = IsRunning ? RunSpeed : WalkSpeed;
            ApplyMovement(delta, currentSpeed);
        }
        else if (distanceToTarget < MinFollowDistance && leaderCharacter != null)
        {
            // Too close, back up slightly
            InputDirection = (GlobalPosition - targetPosition).Normalized() * 0.5f;
            ApplyMovement(delta, WalkSpeed * 0.5f);
        }
        else
        {
            // In good range, stop moving
            InputDirection = Vector2.Zero;
            
            if (CurrentStateType != CharacterStateType.Idle)
            {
                ChangeState(CharacterStateType.Idle);
            }
            
            ApplyFriction(delta);
        }
    }
    
    private void UpdateTrail()
    {
        if (followTarget == null) return;
        
        // Record leader's position and state
        Direction leaderDirection = leaderCharacter?.CurrentDirection ?? Direction.Down;
        CharacterStateType leaderState = leaderCharacter?.CurrentStateType ?? CharacterStateType.Idle;
        
        positionTrail.Enqueue(new FollowerTrailPoint(
            followTarget.GlobalPosition,
            leaderDirection,
            leaderState
        ));
        
        // Remove old positions
        while (positionTrail.Count > TrailLength)
        {
            positionTrail.Dequeue();
        }
    }
    
    private void OnLeaderStateChanged(CharacterStateType newState)
    {
        if (!MimicLeaderStates) return;
        
        // Mimic certain leader states with delay
        switch (newState)
        {
            case CharacterStateType.Jumping:
                // Don't auto-jump, but could add logic here
                break;
                
            case CharacterStateType.Fishing:
            case CharacterStateType.OpeningChest:
            case CharacterStateType.Climbing:
                // Don't mimic these states automatically
                break;
                
            case CharacterStateType.Locked:
                // Lock follower too
                LockCharacter();
                break;
        }
        
        previousLeaderState = newState;
    }
    
    public void SnapToTarget()
    {
        if (followTarget != null)
        {
            GlobalPosition = followTarget.GlobalPosition;
            Velocity = Vector2.Zero;
            positionTrail.Clear();
            
            if (leaderCharacter != null)
            {
                CurrentDirection = leaderCharacter.CurrentDirection;
            }
            
            ChangeState(CharacterStateType.Idle);
        }
    }
    
    public void SnapBehindTarget(float distance = 32f)
    {
        if (followTarget == null) return;
        
        Vector2 offset = Vector2.Zero;
        
        if (leaderCharacter != null)
        {
            // Position based on leader's direction
            offset = leaderCharacter.GetDirectionVector() * -distance;
        }
        
        GlobalPosition = followTarget.GlobalPosition + offset;
        Velocity = Vector2.Zero;
        positionTrail.Clear();
        
        if (leaderCharacter != null)
        {
            CurrentDirection = leaderCharacter.CurrentDirection;
        }
        
        ChangeState(CharacterStateType.Idle);
    }
    
    public void TeleportToPosition(Vector2 position, Direction? facingDirection = null)
    {
        GlobalPosition = position;
        Velocity = Vector2.Zero;
        positionTrail.Clear();
        
        if (facingDirection.HasValue)
        {
            CurrentDirection = facingDirection.Value;
        }
        
        ChangeState(CharacterStateType.Idle);
    }
    
    public override void _ExitTree()
    {
        // Disconnect signals
        if (leaderCharacter != null && leaderCharacter.IsConnected(OverworldCharacter.SignalName.StateChanged, new Callable(this, MethodName.OnLeaderStateChanged)))
        {
            leaderCharacter.StateChanged -= OnLeaderStateChanged;
        }
        
        base._ExitTree();
    }
}