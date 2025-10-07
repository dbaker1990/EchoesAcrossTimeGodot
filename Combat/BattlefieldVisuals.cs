using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Database;

namespace EchoesAcrossTime.Combat
{
    public partial class BattlefieldVisuals : Node2D
    {
        #region Signals
        [Signal] public delegate void AnimationStartedEventHandler(string animName, string memberName);
        [Signal] public delegate void AnimationFinishedEventHandler(string animName, string memberName);
        [Signal] public delegate void HitTimingReachedEventHandler(string attackerName, string targetName);
        [Signal] public delegate void MovementCompletedEventHandler(string memberName);
        #endregion

        #region Node References
        private Node2D partySpritesContainer;
        private Node2D enemySpritesContainer;
        private Node2D effectsContainer;
        #endregion

        #region Sprite Lists
        private List<AnimatedSprite2D> partySprites = new();
        private List<AnimatedSprite2D> enemySprites = new();
        private Dictionary<BattleMember, AnimatedSprite2D> memberToSpriteMap = new();
        private Dictionary<AnimatedSprite2D, Vector2> spriteHomePositions = new();
        #endregion

        #region Default Formations
        private readonly Vector2[] DefaultPartyPositions = {
            new Vector2(1400, 400),  // Character 1 - front right
            new Vector2(1450, 500),  // Character 2 - back right
            new Vector2(1350, 500),  // Character 3 - back left  
            new Vector2(1400, 600)   // Character 4 - far back
        };

        private readonly Vector2[] DefaultEnemyPositions = {
            new Vector2(400, 300),   // Enemy 1 - front
            new Vector2(500, 400),   // Enemy 2 - middle
            new Vector2(400, 500),   // Enemy 3 - back
            new Vector2(300, 400)    // Enemy 4 - side
        };
        #endregion

        public override void _Ready()
        {
            partySpritesContainer = GetNode<Node2D>("PartySprites");
            enemySpritesContainer = GetNode<Node2D>("EnemySprites");
            
            // Create effects container if it doesn't exist
            if (!HasNode("Effects"))
            {
                effectsContainer = new Node2D { Name = "Effects" };
                AddChild(effectsContainer);
            }
            else
            {
                effectsContainer = GetNode<Node2D>("Effects");
            }
        }

        #region Setup Methods
        /// <summary>
        /// Set up party member sprites with custom or default positions
        /// </summary>
        public void SetupPartySprites(List<BattleMember> party, Vector2[] customPositions = null)
        {
            ClearSprites(partySprites, memberToSpriteMap);
            
            var positions = customPositions ?? DefaultPartyPositions;
            
            for (int i = 0; i < party.Count && i < positions.Length; i++)
            {
                var sprite = CreateBattleSprite(party[i], positions[i]);
                partySpritesContainer.AddChild(sprite);
                partySprites.Add(sprite);
                memberToSpriteMap[party[i]] = sprite;
                spriteHomePositions[sprite] = positions[i];
            }
        }

        /// <summary>
        /// Set up enemy sprites with custom or default positions
        /// </summary>
        public void SetupEnemySprites(List<BattleMember> enemies, Vector2[] customPositions = null)
        {
            ClearSprites(enemySprites, memberToSpriteMap);
            
            var positions = customPositions ?? DefaultEnemyPositions;
            
            for (int i = 0; i < enemies.Count && i < positions.Length; i++)
            {
                var sprite = CreateBattleSprite(enemies[i], positions[i]);
                enemySpritesContainer.AddChild(sprite);
                enemySprites.Add(sprite);
                memberToSpriteMap[enemies[i]] = sprite;
                spriteHomePositions[sprite] = positions[i];
                
                // Flip enemies to face party
                sprite.FlipH = false;
            }
        }

        private void ClearSprites(List<AnimatedSprite2D> spriteList, Dictionary<BattleMember, AnimatedSprite2D> map)
        {
            foreach (var sprite in spriteList)
            {
                if (IsInstanceValid(sprite))
                    sprite.QueueFree();
            }
            spriteList.Clear();
            
            // Clean up map entries
            var keysToRemove = new List<BattleMember>();
            foreach (var kvp in map)
            {
                if (spriteList.Contains(kvp.Value))
                    keysToRemove.Add(kvp.Key);
            }
            foreach (var key in keysToRemove)
                map.Remove(key);
        }

        private AnimatedSprite2D CreateBattleSprite(BattleMember member, Vector2 position)
        {
            var sprite = new AnimatedSprite2D
            {
                Position = position,
                Name = $"Sprite_{member.Stats.CharacterName}"
            };
            
            if (member.SourceData != null && member.SourceData.HasBattleSprites())
            {
                sprite.SpriteFrames = member.SourceData.BattleSpriteFrames;
                
                // Apply scale from CharacterData
                float scale = member.SourceData.BattleScale;
                sprite.Scale = new Vector2(scale, scale);
                
                // Apply offset
                sprite.Offset = member.SourceData.BattleSpriteOffset;
                
                // Play idle animation
                PlayAnimation(sprite, GetAnimationName(member, "idle"));
            }
            else
            {
                GD.PrintErr($"No battle sprite for {member.Stats.CharacterName}");
                // Create placeholder
                CreatePlaceholderSprite(sprite, member);
            }
            
            return sprite;
        }

        private void CreatePlaceholderSprite(AnimatedSprite2D sprite, BattleMember member)
        {
            // This creates a simple colored square as a fallback
            var frames = new SpriteFrames();
            frames.AddAnimation("idle");
            
            // You could create a simple texture here or just leave it empty
            sprite.SpriteFrames = frames;
            // Blue for player-controlled, Red for enemies
            sprite.Modulate = member.IsPlayerControlled ? Colors.Blue : Colors.Red;
        }
        #endregion

        #region Animation Playback
        /// <summary>
        /// Play any animation for a battle member
        /// </summary>
        public void PlayMemberAnimation(BattleMember member, string animationName)
        {
            var sprite = GetSpriteForMember(member);
            if (sprite == null) return;

            string actualAnimName = GetAnimationName(member, animationName);
            PlayAnimation(sprite, actualAnimName);
            
            EmitSignal(SignalName.AnimationStarted, animationName, member.Stats.CharacterName);
        }

        private void PlayAnimation(AnimatedSprite2D sprite, string animName)
        {
            if (sprite?.SpriteFrames == null) return;
            
            if (sprite.SpriteFrames.HasAnimation(animName))
            {
                sprite.Play(animName);
            }
            else if (sprite.SpriteFrames.HasAnimation("idle"))
            {
                sprite.Play("idle");
                GD.PushWarning($"Animation '{animName}' not found, playing idle");
            }
        }

        private string GetAnimationName(BattleMember member, string defaultName)
        {
            // Check if member has custom animation data
            if (member.SourceData?.BattleAnimations != null)
            {
                var animData = member.SourceData.BattleAnimations;
                
                return defaultName.ToLower() switch
                {
                    "idle" => animData.IdleAnimation,
                    "ready" => animData.ReadyAnimation,
                    "attack" => animData.AttackAnimation,
                    "cast" => animData.CastAnimation,
                    "item" => animData.ItemAnimation,
                    "defend" => animData.DefendAnimation,
                    "evade" => animData.EvadeAnimation,
                    "hit" => animData.HitAnimation,
                    "critical_hit" => animData.CriticalHitAnimation,
                    "miss" => animData.MissAnimation,
                    "heal" => animData.HealAnimation,
                    "buff" => animData.BuffAnimation,
                    "debuff" => animData.DebuffAnimation,
                    "death" => animData.DeathAnimation,
                    "victory" => animData.VictoryAnimation,
                    "defeat" => animData.DefeatAnimation,
                    _ => defaultName
                };
            }
            
            return defaultName;
        }
        #endregion

        #region Battle Actions
        /// <summary>
        /// Play complete attack sequence with movement
        /// </summary>
        public async Task PlayAttackSequence(BattleMember attacker, BattleMember target)
        {
            var attackerSprite = GetSpriteForMember(attacker);
            var targetSprite = GetSpriteForMember(target);
            
            if (attackerSprite == null) return;

            var animData = attacker.SourceData?.BattleAnimations;
            Vector2 originalPos = attackerSprite.Position;
            
            // Get movement settings
            float approachDist = animData?.ApproachDistance ?? 50f;
            float approachSpeed = animData?.ApproachSpeed ?? 300f;
            float returnSpeed = animData?.ReturnSpeed ?? 200f;
            float hitDelay = animData?.HitDelay ?? 0.3f;
            bool usesStepForward = animData?.UsesStepForward ?? true;

            // Play attack animation
            PlayAnimation(attackerSprite, GetAnimationName(attacker, "attack"));

            if (usesStepForward && targetSprite != null)
            {
                // Calculate approach position
                Vector2 targetPos = targetSprite.Position;
                Vector2 direction = (targetPos - originalPos).Normalized();
                Vector2 approachPos = originalPos + (direction * approachDist);
                
                // Move toward target
                float approachTime = approachDist / approachSpeed;
                var tween = CreateTween();
                tween.TweenProperty(attackerSprite, "position", approachPos, approachTime);
                await ToSignal(tween, Tween.SignalName.Finished);
            }

            // Wait for hit timing
            await Task.Delay((int)(hitDelay * 1000));
            EmitSignal(SignalName.HitTimingReached, attacker.Stats.CharacterName, target.Stats.CharacterName);

            if (usesStepForward)
            {
                // Return to original position
                float distance = attackerSprite.Position.DistanceTo(originalPos);
                float returnTime = distance / returnSpeed;
                var tween = CreateTween();
                tween.TweenProperty(attackerSprite, "position", originalPos, returnTime);
                await ToSignal(tween, Tween.SignalName.Finished);
            }

            // Return to idle
            PlayAnimation(attackerSprite, GetAnimationName(attacker, "idle"));
            EmitSignal(SignalName.AnimationFinished, "attack", attacker.Stats.CharacterName);
        }

        /// <summary>
        /// Play casting animation (for magic/skills)
        /// </summary>
        public async Task PlayCastSequence(BattleMember caster)
        {
            var sprite = GetSpriteForMember(caster);
            if (sprite == null) return;

            var animData = caster.SourceData?.BattleAnimations;
            float castDuration = animData?.CastDuration ?? 1.0f;

            PlayAnimation(sprite, GetAnimationName(caster, "cast"));
            
            await Task.Delay((int)(castDuration * 1000));
            
            PlayAnimation(sprite, GetAnimationName(caster, "idle"));
            EmitSignal(SignalName.AnimationFinished, "cast", caster.Stats.CharacterName);
        }

        /// <summary>
        /// Play defend animation
        /// </summary>
        public void PlayDefend(BattleMember member)
        {
            var sprite = GetSpriteForMember(member);
            if (sprite == null) return;

            PlayAnimation(sprite, GetAnimationName(member, "defend"));
        }

        /// <summary>
        /// Play item use animation
        /// </summary>
        public async Task PlayItemSequence(BattleMember user)
        {
            var sprite = GetSpriteForMember(user);
            if (sprite == null) return;

            PlayAnimation(sprite, GetAnimationName(user, "item"));
            await Task.Delay(500);
            PlayAnimation(sprite, GetAnimationName(user, "idle"));
        }
        #endregion

        #region Damage Effects
        /// <summary>
        /// Show hit effect on target
        /// </summary>
        public async Task ShowHitEffect(BattleMember target, bool isCritical = false)
        {
            var sprite = GetSpriteForMember(target);
            if (sprite == null) return;

            string animName = isCritical ? "critical_hit" : "hit";
            PlayAnimation(sprite, GetAnimationName(target, animName));

            // Flash effect
            var flashColor = isCritical ? new Color(1, 1, 0) : new Color(1, 0.5f, 0.5f);
            var tween = CreateTween();
            tween.TweenProperty(sprite, "modulate", flashColor, 0.1f);
            tween.TweenProperty(sprite, "modulate", Colors.White, 0.15f);
            
            await Task.Delay(300);
            PlayAnimation(sprite, GetAnimationName(target, "idle"));
        }

        /// <summary>
        /// Show miss effect
        /// </summary>
        public void ShowMissEffect(BattleMember target)
        {
            var sprite = GetSpriteForMember(target);
            if (sprite == null) return;

            PlayAnimation(sprite, GetAnimationName(target, "miss"));
            
            // Quick dodge animation
            Vector2 originalPos = sprite.Position;
            var tween = CreateTween();
            tween.TweenProperty(sprite, "position", originalPos + new Vector2(20, 0), 0.1f);
            tween.TweenProperty(sprite, "position", originalPos, 0.1f);
        }

        /// <summary>
        /// Show evade effect
        /// </summary>
        public async Task ShowEvadeEffect(BattleMember target)
        {
            var sprite = GetSpriteForMember(target);
            if (sprite == null) return;

            PlayAnimation(sprite, GetAnimationName(target, "evade"));
            
            // Quick backstep
            Vector2 originalPos = sprite.Position;
            var tween = CreateTween();
            tween.TweenProperty(sprite, "position", originalPos + new Vector2(-30, 0), 0.15f);
            tween.TweenProperty(sprite, "position", originalPos, 0.15f);
            
            await Task.Delay(300);
            PlayAnimation(sprite, GetAnimationName(target, "idle"));
        }
        #endregion

        #region Status Effects
        /// <summary>
        /// Show knocked down effect (Persona 5 style)
        /// </summary>
        public void ShowKnockedDown(BattleMember target)
        {
            var sprite = GetSpriteForMember(target);
            if (sprite == null) return;

            // Rotate to show knocked down
            var tween = CreateTween();
            tween.TweenProperty(sprite, "rotation_degrees", 90f, 0.3f);
            
            // Darken sprite
            tween.Parallel().TweenProperty(sprite, "modulate", new Color(0.7f, 0.7f, 0.7f), 0.3f);
        }

        /// <summary>
        /// Recover from knocked down state
        /// </summary>
        public async Task RecoverFromKnockedDown(BattleMember target)
        {
            var sprite = GetSpriteForMember(target);
            if (sprite == null) return;

            var tween = CreateTween();
            tween.TweenProperty(sprite, "rotation_degrees", 0f, 0.3f);
            tween.Parallel().TweenProperty(sprite, "modulate", Colors.White, 0.3f);
            
            await ToSignal(tween, Tween.SignalName.Finished);
            PlayAnimation(sprite, GetAnimationName(target, "idle"));
        }

        /// <summary>
        /// Show healing effect
        /// </summary>
        public async Task ShowHealEffect(BattleMember target)
        {
            var sprite = GetSpriteForMember(target);
            if (sprite == null) return;

            PlayAnimation(sprite, GetAnimationName(target, "heal"));
            
            // Green glow
            var tween = CreateTween();
            tween.TweenProperty(sprite, "modulate", new Color(0.5f, 1, 0.5f), 0.2f);
            tween.TweenProperty(sprite, "modulate", Colors.White, 0.3f);
            
            await Task.Delay(500);
            PlayAnimation(sprite, GetAnimationName(target, "idle"));
        }

        /// <summary>
        /// Show buff effect
        /// </summary>
        public void ShowBuffEffect(BattleMember target)
        {
            var sprite = GetSpriteForMember(target);
            if (sprite == null) return;

            PlayAnimation(sprite, GetAnimationName(target, "buff"));
            
            // Gold glow
            var tween = CreateTween();
            tween.TweenProperty(sprite, "modulate", new Color(1, 1, 0.5f), 0.3f);
            tween.TweenProperty(sprite, "modulate", Colors.White, 0.3f);
        }

        /// <summary>
        /// Show debuff effect
        /// </summary>
        public void ShowDebuffEffect(BattleMember target)
        {
            var sprite = GetSpriteForMember(target);
            if (sprite == null) return;

            PlayAnimation(sprite, GetAnimationName(target, "debuff"));
            
            // Purple darkening
            var tween = CreateTween();
            tween.TweenProperty(sprite, "modulate", new Color(0.7f, 0.5f, 0.7f), 0.3f);
            tween.TweenProperty(sprite, "modulate", Colors.White, 0.3f);
        }

        /// <summary>
        /// Show death effect
        /// </summary>
        public async Task ShowDeathEffect(BattleMember target)
        {
            var sprite = GetSpriteForMember(target);
            if (sprite == null) return;

            PlayAnimation(sprite, GetAnimationName(target, "death"));
            
            // Fade out
            var tween = CreateTween();
            tween.TweenProperty(sprite, "modulate:a", 0f, 0.5f);
            
            await ToSignal(tween, Tween.SignalName.Finished);
        }
        #endregion

        #region Victory/Defeat
        /// <summary>
        /// Play victory animation for all party members
        /// </summary>
        public void PlayVictoryAnimation()
        {
            foreach (var sprite in partySprites)
            {
                if (IsInstanceValid(sprite))
                {
                    var member = GetMemberForSprite(sprite);
                    if (member != null)
                        PlayAnimation(sprite, GetAnimationName(member, "victory"));
                }
            }
        }

        /// <summary>
        /// Play defeat animation for all party members
        /// </summary>
        public void PlayDefeatAnimation()
        {
            foreach (var sprite in partySprites)
            {
                if (IsInstanceValid(sprite))
                {
                    var member = GetMemberForSprite(sprite);
                    if (member != null)
                        PlayAnimation(sprite, GetAnimationName(member, "defeat"));
                }
            }
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Get sprite for any battle member
        /// </summary>
        public AnimatedSprite2D GetSpriteForMember(BattleMember member)
        {
            if (memberToSpriteMap.TryGetValue(member, out var sprite))
                return sprite;
            return null;
        }

        /// <summary>
        /// Get member for a sprite (reverse lookup)
        /// </summary>
        public BattleMember GetMemberForSprite(AnimatedSprite2D sprite)
        {
            foreach (var kvp in memberToSpriteMap)
            {
                if (kvp.Value == sprite)
                    return kvp.Key;
            }
            return null;
        }

        /// <summary>
        /// Get sprite for a specific party member by index
        /// </summary>
        public AnimatedSprite2D GetPartySprite(int index)
        {
            if (index >= 0 && index < partySprites.Count)
                return partySprites[index];
            return null;
        }

        /// <summary>
        /// Get sprite for a specific enemy by index
        /// </summary>
        public AnimatedSprite2D GetEnemySprite(int index)
        {
            if (index >= 0 && index < enemySprites.Count)
                return enemySprites[index];
            return null;
        }

        /// <summary>
        /// Reset sprite to home position
        /// </summary>
        public void ResetSpritePosition(BattleMember member)
        {
            var sprite = GetSpriteForMember(member);
            if (sprite != null && spriteHomePositions.TryGetValue(sprite, out var homePos))
            {
                sprite.Position = homePos;
            }
        }

        /// <summary>
        /// Return all sprites to idle animation
        /// </summary>
        public void ReturnAllToIdle()
        {
            foreach (var kvp in memberToSpriteMap)
            {
                if (IsInstanceValid(kvp.Value))
                {
                    PlayAnimation(kvp.Value, GetAnimationName(kvp.Key, "idle"));
                    ResetSpritePosition(kvp.Key);
                }
            }
        }

        /// <summary>
        /// Set sprite visibility
        /// </summary>
        public void SetSpriteVisible(BattleMember member, bool visible)
        {
            var sprite = GetSpriteForMember(member);
            if (sprite != null)
                sprite.Visible = visible;
        }

        /// <summary>
        /// Highlight a sprite (for targeting)
        /// </summary>
        public void HighlightSprite(BattleMember member, bool highlight)
        {
            var sprite = GetSpriteForMember(member);
            if (sprite == null) return;

            if (highlight)
            {
                var tween = CreateTween();
                tween.SetLoops();
                tween.TweenProperty(sprite, "modulate", new Color(1.2f, 1.2f, 1.2f), 0.3f);
                tween.TweenProperty(sprite, "modulate", Colors.White, 0.3f);
            }
            else
            {
                sprite.Modulate = Colors.White;
            }
        }
        #endregion
    }
}