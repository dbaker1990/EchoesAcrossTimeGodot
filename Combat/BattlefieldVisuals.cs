using Godot;
using System.Collections.Generic;
using EchoesAcrossTime.Combat;

public partial class BattlefieldVisuals : Node2D
{
    private Node2D partySpritesContainer;
    private Node2D enemySpritesContainer;
    
    private List<AnimatedSprite2D> partySprites = new();
    private List<AnimatedSprite2D> enemySprites = new();
    
    public override void _Ready()
    {
        partySpritesContainer = GetNode<Node2D>("PartySprites");
        enemySpritesContainer = GetNode<Node2D>("EnemySprites");
    }
    
    /// <summary>
    /// Set up party member sprites
    /// </summary>
    public void SetupPartySprites(List<BattleMember> party)
    {
        // Clear existing
        foreach (var sprite in partySprites)
            sprite.QueueFree();
        partySprites.Clear();
        
        // Create new sprites
        Vector2[] positions = {
            new Vector2(1600, 300),
            new Vector2(1600, 450),
            new Vector2(1600, 600),
            new Vector2(1600, 750)
        };
        
        for (int i = 0; i < party.Count && i < positions.Length; i++)
        {
            var sprite = CreateBattleSprite(party[i], positions[i]);
            partySpritesContainer.AddChild(sprite);
            partySprites.Add(sprite);
        }
    }
    
    /// <summary>
    /// Set up enemy sprites
    /// </summary>
    public void SetupEnemySprites(List<BattleMember> enemies)
    {
        // Clear existing
        foreach (var sprite in enemySprites)
            sprite.QueueFree();
        enemySprites.Clear();
        
        // Create new sprites with varied positioning
        Vector2[] positions = {
            new Vector2(400, 350),
            new Vector2(600, 450),
            new Vector2(500, 600),
            new Vector2(700, 500)
        };
        
        for (int i = 0; i < enemies.Count && i < positions.Length; i++)
        {
            var sprite = CreateBattleSprite(enemies[i], positions[i]);
            enemySpritesContainer.AddChild(sprite);
            enemySprites.Add(sprite);
        }
    }
    
    private AnimatedSprite2D CreateBattleSprite(BattleMember member, Vector2 position)
    {
        var sprite = new AnimatedSprite2D();
        sprite.Position = position;
        
        // FIXED: Access SourceData instead of Stats.CharacterData
        // Load sprite frames from CharacterData if available
        if (member.SourceData != null && member.SourceData.HasBattleSprites())
        {
            sprite.SpriteFrames = member.SourceData.BattleSpriteFrames;
            sprite.Play("idle"); // Default animation
        }
        else
        {
            // Fallback: Create a colored rectangle if no sprite available
            GD.PrintErr($"No battle sprite for {member.Stats.CharacterName}");
        }
        
        // Scale appropriately
        sprite.Scale = new Vector2(2, 2); // Adjust as needed
        
        return sprite;
    }
    
    /// <summary>
    /// Get sprite for a specific party member
    /// </summary>
    public AnimatedSprite2D GetPartySprite(int index)
    {
        if (index >= 0 && index < partySprites.Count)
            return partySprites[index];
        return null;
    }
    
    /// <summary>
    /// Get sprite for a specific enemy
    /// </summary>
    public AnimatedSprite2D GetEnemySprite(int index)
    {
        if (index >= 0 && index < enemySprites.Count)
            return enemySprites[index];
        return null;
    }
    
    /// <summary>
    /// Play attack animation for a member
    /// </summary>
    public async void PlayAttackAnimation(BattleMember attacker, BattleMember target, List<BattleMember> party, List<BattleMember> enemies)
    {
        var attackerSprite = GetSpriteForMember(attacker, party, enemies);
        var targetSprite = GetSpriteForMember(target, party, enemies);
        
        if (attackerSprite == null) return;
        
        // Play attack animation
        if (attackerSprite.SpriteFrames.HasAnimation("attack"))
            attackerSprite.Play("attack");
        
        // Move toward target
        Vector2 originalPos = attackerSprite.Position;
        Vector2 targetPos = targetSprite != null ? targetSprite.Position : originalPos;
        Vector2 approachPos = originalPos.Lerp(targetPos, 0.3f);
        
        // Tween to approach
        var tween = CreateTween();
        tween.TweenProperty(attackerSprite, "position", approachPos, 0.2f);
        await ToSignal(tween, Tween.SignalName.Finished);
        
        // Return to original position
        tween = CreateTween();
        tween.TweenProperty(attackerSprite, "position", originalPos, 0.2f);
        await ToSignal(tween, Tween.SignalName.Finished);
        
        // Return to idle
        if (attackerSprite.SpriteFrames.HasAnimation("idle"))
            attackerSprite.Play("idle");
    }
    
    /// <summary>
    /// Show damage effect on sprite
    /// </summary>
    public void ShowDamageEffect(BattleMember target, List<BattleMember> party, List<BattleMember> enemies)
    {
        var sprite = GetSpriteForMember(target, party, enemies);
        if (sprite == null) return;
        
        // Flash red
        var tween = CreateTween();
        tween.TweenProperty(sprite, "modulate", Colors.Red, 0.1f);
        tween.TweenProperty(sprite, "modulate", Colors.White, 0.2f);
    }
    
    /// <summary>
    /// Show knocked down effect
    /// </summary>
    public void ShowKnockedDown(BattleMember target, List<BattleMember> party, List<BattleMember> enemies)
    {
        var sprite = GetSpriteForMember(target, party, enemies);
        if (sprite == null) return;
        
        // Rotate sprite to indicate knocked down
        var tween = CreateTween();
        tween.TweenProperty(sprite, "rotation_degrees", 90f, 0.3f);
    }
    
    /// <summary>
    /// Stand up from knocked down
    /// </summary>
    public void RecoverFromKnockedDown(BattleMember target, List<BattleMember> party, List<BattleMember> enemies)
    {
        var sprite = GetSpriteForMember(target, party, enemies);
        if (sprite == null) return;
        
        var tween = CreateTween();
        tween.TweenProperty(sprite, "rotation_degrees", 0f, 0.3f);
    }
    
    /// <summary>
    /// Get sprite for any battle member
    /// </summary>
    public AnimatedSprite2D GetSpriteForMember(BattleMember member, List<BattleMember> party, List<BattleMember> enemies)
    {
        // Check if in party
        int partyIndex = party.IndexOf(member);
        if (partyIndex >= 0)
            return GetPartySprite(partyIndex);
        
        // Check if in enemies
        int enemyIndex = enemies.IndexOf(member);
        if (enemyIndex >= 0)
            return GetEnemySprite(enemyIndex);
        
        return null;
    }
}