using Godot;
using System;
using EchoesAcrossTime.Managers;

public partial class FishingState : CharacterState
{
    #region State Machine
    private enum FishingPhase
    {
        Casting,
        Waiting,
        Hooked,
        Reeling,
        Success,
        Failed
    }
    
    private FishingPhase currentPhase = FishingPhase.Casting;
    #endregion
    
    #region Timing & Difficulty
    private float waitTimer = 0f;
    private float biteWaitTime = 3f;
    private float hookWindow = 1.5f; // Time window to press button when fish bites
    private float hookTimer = 0f;
    
    private float tension = 0f; // 0-100, line breaks at 100
    private float tensionSpeed = 15f;
    private int successCount = 0;
    private int requiredSuccesses = 3;
    #endregion
    
    #region Current Fish
    private FishData currentFish;
    private float escapeTimer = 0f;
    #endregion
    
    #region Signals
    [Signal] public delegate void FishingStartedEventHandler();
    [Signal] public delegate void FishBiteEventHandler();
    [Signal] public delegate void FishHookedEventHandler(FishData fish);
    [Signal] public delegate void TensionChangedEventHandler(float tension);
    [Signal] public delegate void SuccessCountChangedEventHandler(int count, int required);
    [Signal] public delegate void FishCaughtEventHandler(FishData fish);
    [Signal] public delegate void FishEscapedEventHandler();
    [Signal] public delegate void LineBrokeEventHandler();
    #endregion
    
    public FishingState()
    {
        StateType = CharacterStateType.Fishing;
    }
    
    public override void Enter(OverworldCharacter character)
    {
        base.Enter(character);
        ResetFishing();
        currentPhase = FishingPhase.Casting;
        EmitSignal(SignalName.FishingStarted);
        
        // Play cast sound
        SystemManager.Instance?.PlayOkSE();
    }
    
    private void ResetFishing()
    {
        waitTimer = 0f;
        biteWaitTime = GD.Randf() * 4f + 2f; // 2-6 seconds
        hookTimer = 0f;
        tension = 0f;
        successCount = 0;
        currentFish = null;
        escapeTimer = 0f;
    }
    
    public override void Update(double delta)
    {
        float dt = (float)delta;
        
        switch (currentPhase)
        {
            case FishingPhase.Casting:
                UpdateCasting();
                break;
                
            case FishingPhase.Waiting:
                UpdateWaiting(dt);
                break;
                
            case FishingPhase.Hooked:
                UpdateHooked(dt);
                break;
                
            case FishingPhase.Reeling:
                UpdateReeling(dt);
                break;
                
            case FishingPhase.Success:
                UpdateSuccess();
                break;
                
            case FishingPhase.Failed:
                UpdateFailed();
                break;
        }
    }
    
    #region Phase Updates
    private void UpdateCasting()
    {
        // Wait for cast animation to finish
        if (character.AnimatedSprite != null && !character.AnimatedSprite.IsPlaying())
        {
            currentPhase = FishingPhase.Waiting;
        }
    }
    
    private void UpdateWaiting(float delta)
    {
        waitTimer += delta;
        
        if (waitTimer >= biteWaitTime)
        {
            // Fish bites!
            OnFishBite();
        }
    }
    
    private void OnFishBite()
    {
        currentPhase = FishingPhase.Hooked;
        hookTimer = 0f;
        hookWindow = GD.Randf() * 0.5f + 1.0f; // 1.0-1.5 second window
        
        EmitSignal(SignalName.FishBite);
        SystemManager.Instance?.PlayBuzzerSE(); // Alert sound
    }
    
    private void UpdateHooked(float delta)
    {
        hookTimer += delta;
        
        if (hookTimer >= hookWindow)
        {
            // Player missed the hook window
            OnFishEscaped();
        }
    }
    
    private void UpdateReeling(float delta)
    {
        // Increase tension over time
        tension += tensionSpeed * delta;
        tension = Mathf.Clamp(tension, 0f, 100f);
        EmitSignal(SignalName.TensionChanged, tension);
        
        // Check for line break
        if (tension >= 100f)
        {
            OnLineBreak();
            return;
        }
        
        // Random escape chance
        if (currentFish != null)
        {
            escapeTimer += delta;
            if (escapeTimer >= 1f)
            {
                escapeTimer = 0f;
                if (GD.Randf() < currentFish.EscapeChance)
                {
                    OnFishEscaped();
                }
            }
        }
    }
    
    private void UpdateSuccess()
    {
        // Wait for success animation
        if (character.AnimatedSprite != null && !character.AnimatedSprite.IsPlaying())
        {
            CompleteFishing();
        }
    }
    
    private void UpdateFailed()
    {
        // Wait for failed animation
        if (character.AnimatedSprite != null && !character.AnimatedSprite.IsPlaying())
        {
            character.ChangeState(CharacterStateType.Idle);
        }
    }
    #endregion
    
    #region Input Handling
    public override void HandleInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            // Cancel fishing
            character.ChangeState(CharacterStateType.Idle);
            return;
        }
        
        // Hook the fish when it bites
        if (currentPhase == FishingPhase.Hooked && @event.IsActionPressed("ui_accept"))
        {
            OnHookSuccess();
        }
        
        // Reel in during the minigame
        if (currentPhase == FishingPhase.Reeling && @event.IsActionPressed("ui_accept"))
        {
            OnReelSuccess();
        }
    }
    
    private void OnHookSuccess()
    {
        // Successfully hooked the fish!
        currentPhase = FishingPhase.Reeling;
        currentFish = SelectRandomFish();
        
        if (currentFish != null)
        {
            tensionSpeed = currentFish.TensionSpeed;
            requiredSuccesses = currentFish.RequiredSuccesses;
        }
        else
        {
            // Default fish
            tensionSpeed = 15f;
            requiredSuccesses = 3;
        }
        
        successCount = 0;
        tension = 0f;
        
        EmitSignal(SignalName.FishHooked, currentFish);
        EmitSignal(SignalName.SuccessCountChanged, successCount, requiredSuccesses);
        SystemManager.Instance?.PlayOkSE();
    }
    
    private void OnReelSuccess()
    {
        // Successful button press
        successCount++;
        tension = Mathf.Max(0f, tension - 25f); // Reduce tension
        
        EmitSignal(SignalName.SuccessCountChanged, successCount, requiredSuccesses);
        EmitSignal(SignalName.TensionChanged, tension);
        SystemManager.Instance?.PlayOkSE();
        
        if (successCount >= requiredSuccesses)
        {
            // Fish caught!
            currentPhase = FishingPhase.Success;
            SystemManager.Instance?.PlayEquipSE(); // Victory sound
        }
    }
    #endregion
    
    #region Fish Selection
    private FishData SelectRandomFish()
    {
        // Get fish from FishingManager
        return FishingManager.Instance?.GetRandomFish();
    }
    #endregion
    
    #region End States
    private void OnFishEscaped()
    {
        currentPhase = FishingPhase.Failed;
        EmitSignal(SignalName.FishEscaped);
        SystemManager.Instance?.PlayBuzzerSE();
    }
    
    private void OnLineBreak()
    {
        currentPhase = FishingPhase.Failed;
        EmitSignal(SignalName.LineBroke);
        SystemManager.Instance?.PlayBuzzerSE();
    }
    
    private void CompleteFishing()
    {
        // Add fish to inventory
        if (currentFish != null)
        {
            // Record catch in FishingManager
            FishingManager.Instance?.OnFishCaught(currentFish);
            
            EmitSignal(SignalName.FishCaught, currentFish);
            GD.Print($"Caught {currentFish.FishName}!");
        }
        
        character.ChangeState(CharacterStateType.Idle);
    }
    #endregion
    
    #region Animation
    public override string GetAnimationName()
    {
        string direction = character.CurrentDirection switch
        {
            OverworldCharacter.Direction.Down => "down",
            OverworldCharacter.Direction.Up => "up",
            OverworldCharacter.Direction.Left => "left",
            OverworldCharacter.Direction.Right => "right",
            _ => "down"
        };
        
        return currentPhase switch
        {
            FishingPhase.Casting => $"fishing_cast_{direction}",
            FishingPhase.Waiting => $"fishing_wait_{direction}",
            FishingPhase.Hooked => $"fishing_wait_{direction}", // Could add special hooked animation
            FishingPhase.Reeling => $"fishing_reel_{direction}",
            FishingPhase.Success => $"fishing_success_{direction}",
            FishingPhase.Failed => $"fishing_fail_{direction}",
            _ => $"fishing_wait_{direction}"
        };
    }
    #endregion
}