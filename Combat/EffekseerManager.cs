using Godot;
using System;
using System.Collections.Generic;

namespace EchoesAcrossTime.Combat
{
    /// <summary>
    /// Manages Effekseer particle effects for battle
    /// Handles both targeted effects and full-screen effects
    /// </summary>
    public partial class EffekseerManager : Node2D
    {
        #region Exports
        [ExportGroup("References")]
        [Export] public Node2D BattlefieldNode { get; set; }
        [Export] public CanvasLayer EffectsCanvasLayer { get; set; }
        
        [ExportGroup("Screen Settings")]
        [Export] public Vector2 ScreenSize { get; set; } = new Vector2(1920, 1080);
        [Export] public Vector2 ScreenCenter { get; set; } = new Vector2(960, 540);
        
        [ExportGroup("Effect Scaling")]
        [Export] public float DefaultEffectScale { get; set; } = 1.0f;
        [Export] public float ScreenWideEffectScale { get; set; } = 2.0f;
        #endregion

        #region Effect Tracking
        private Dictionary<string, Node> activeEffects = new();
        private Queue<Node> effectPool = new();
        #endregion

        #region Initialization
        public override void _Ready()
        {
            // Auto-create CanvasLayer if not assigned
            if (EffectsCanvasLayer == null)
            {
                EffectsCanvasLayer = new CanvasLayer();
                EffectsCanvasLayer.Name = "EffectsOverlay";
                EffectsCanvasLayer.Layer = 100; // High layer for screen effects
                AddChild(EffectsCanvasLayer);
            }
            
            // Get battlefield reference if not assigned
            if (BattlefieldNode == null)
            {
                BattlefieldNode = GetParent<Node2D>();
            }
        }
        #endregion

        #region Effect Spawning - Targeted (World Space)
        
        /// <summary>
        /// Play effect at a specific world position (e.g., on a character)
        /// </summary>
        public Node PlayEffectAtPosition(string effectPath, Vector2 worldPosition, float scale = 1.0f, float duration = 2.0f)
        {
            var effect = LoadEffect(effectPath);
            if (effect == null) return null;
            
            // Add to battlefield (world space)
            BattlefieldNode.AddChild(effect);
            
            // Set position and scale
            if (effect is Node2D node2D)
            {
                node2D.GlobalPosition = worldPosition;
                node2D.Scale = Vector2.One * scale * DefaultEffectScale;
            }
            
            // Auto-cleanup after duration
            GetTree().CreateTimer(duration).Timeout += () => CleanupEffect(effect);
            
            return effect;
        }
        
        /// <summary>
        /// Play effect attached to a sprite (follows the sprite)
        /// </summary>
        public Node PlayEffectOnSprite(string effectPath, Node2D sprite, Vector2 offset = default, float scale = 1.0f, float duration = 2.0f)
        {
            var effect = LoadEffect(effectPath);
            if (effect == null) return null;
            
            // Add as child of sprite (will follow it)
            sprite.AddChild(effect);
            
            if (effect is Node2D node2D)
            {
                node2D.Position = offset;
                node2D.Scale = Vector2.One * scale * DefaultEffectScale;
            }
            
            // Auto-cleanup
            GetTree().CreateTimer(duration).Timeout += () => CleanupEffect(effect);
            
            return effect;
        }
        
        #endregion

        #region Effect Spawning - Screen-Wide (Canvas Layer)
        
        /// <summary>
        /// Play FULL SCREEN effect (ultimate attacks, screen transitions, etc.)
        /// Uses CanvasLayer for true screen coverage
        /// </summary>
        public Node PlayScreenWideEffect(string effectPath, float scale = 1.0f, float duration = 3.0f, Vector2? customPosition = null)
        {
            var effect = LoadEffect(effectPath);
            if (effect == null) return null;
            
            // Add to canvas layer (screen space)
            EffectsCanvasLayer.AddChild(effect);
            
            if (effect is Node2D node2D)
            {
                // Position at screen center (or custom position)
                node2D.Position = customPosition ?? ScreenCenter;
                
                // Apply screen-wide scaling
                node2D.Scale = Vector2.One * scale * ScreenWideEffectScale;
                
                // Optional: Set Z-index to ensure it's on top
                node2D.ZIndex = 1000;
            }
            
            // Auto-cleanup
            GetTree().CreateTimer(duration).Timeout += () => CleanupEffect(effect);
            
            return effect;
        }
        
        /// <summary>
        /// Play effect that covers entire screen from top to bottom
        /// Perfect for beam attacks, screen wipes, etc.
        /// </summary>
        public Node PlayVerticalScreenEffect(string effectPath, float width = 1.5f, float duration = 2.5f)
        {
            return PlayScreenWideEffect(
                effectPath, 
                scale: width, 
                duration: duration,
                customPosition: ScreenCenter
            );
        }
        
        /// <summary>
        /// Play effect that covers entire screen from left to right
        /// </summary>
        public Node PlayHorizontalScreenEffect(string effectPath, float height = 1.5f, float duration = 2.5f)
        {
            return PlayScreenWideEffect(
                effectPath, 
                scale: height, 
                duration: duration,
                customPosition: ScreenCenter
            );
        }
        
        #endregion

        #region Effect Spawning - Multi-Target
        
        /// <summary>
        /// Play effect on multiple targets (all enemies, all party, etc.)
        /// </summary>
        public List<Node> PlayEffectOnMultipleTargets(string effectPath, List<Vector2> positions, float scale = 1.0f, float duration = 2.0f, float staggerDelay = 0.1f)
        {
            var effects = new List<Node>();
            
            for (int i = 0; i < positions.Count; i++)
            {
                int index = i; // Capture for lambda
                
                // Stagger spawns for visual impact
                GetTree().CreateTimer(index * staggerDelay).Timeout += () =>
                {
                    var effect = PlayEffectAtPosition(effectPath, positions[index], scale, duration);
                    if (effect != null)
                        effects.Add(effect);
                };
            }
            
            return effects;
        }
        
        #endregion

        #region Effect Loading and Cleanup
        
        private Node LoadEffect(string effectPath)
        {
            if (string.IsNullOrEmpty(effectPath))
            {
                GD.PrintErr("[EffekseerManager] Empty effect path");
                return null;
            }
            
            // Try to load the effect
            var effectScene = GD.Load<PackedScene>(effectPath);
            if (effectScene == null)
            {
                GD.PrintErr($"[EffekseerManager] Failed to load effect: {effectPath}");
                return null;
            }
            
            var effect = effectScene.Instantiate();
            
            // Store in active effects for tracking
            string id = Guid.NewGuid().ToString();
            activeEffects[id] = effect;
            
            return effect;
        }
        
        private void CleanupEffect(Node effect)
        {
            if (effect == null || !IsInstanceValid(effect)) return;
            
            // Remove from tracking
            foreach (var kvp in activeEffects)
            {
                if (kvp.Value == effect)
                {
                    activeEffects.Remove(kvp.Key);
                    break;
                }
            }
            
            // Queue free
            effect.QueueFree();
        }
        
        /// <summary>
        /// Stop all active effects immediately
        /// </summary>
        public void StopAllEffects()
        {
            foreach (var effect in activeEffects.Values)
            {
                if (IsInstanceValid(effect))
                    effect.QueueFree();
            }
            activeEffects.Clear();
        }
        
        #endregion

        #region Convenience Methods for Common Effects
        
        /// <summary>
        /// Play a hit/impact effect at target
        /// </summary>
        public void PlayHitEffect(Vector2 position, bool isCritical = false)
        {
            string path = isCritical ? "res://Effects/CriticalHit.efkefc" : "res://Effects/NormalHit.efkefc";
            PlayEffectAtPosition(path, position, scale: isCritical ? 1.5f : 1.0f, duration: 0.8f);
        }
        
        /// <summary>
        /// Play elemental skill effect at target
        /// </summary>
        public void PlayElementalEffect(string elementName, Vector2 position, float scale = 1.0f)
        {
            string path = $"res://Effects/Elements/{elementName}.efkefc";
            PlayEffectAtPosition(path, position, scale: scale, duration: 2.0f);
        }
        
        /// <summary>
        /// Play screen-wide ultimate attack effect
        /// </summary>
        public void PlayUltimateEffect(string ultimateName, float duration = 4.0f)
        {
            string path = $"res://Effects/Ultimates/{ultimateName}.efkefc";
            PlayScreenWideEffect(path, scale: 1.0f, duration: duration);
        }
        
        /// <summary>
        /// Play buff/debuff effect on character
        /// </summary>
        public void PlayStatusEffect(Node2D sprite, string statusName, float duration = 2.0f)
        {
            string path = $"res://Effects/Status/{statusName}.efkefc";
            PlayEffectOnSprite(path, sprite, offset: Vector2.Zero, scale: 1.0f, duration: duration);
        }
        
        #endregion

        #region Camera Shake Integration (Optional)
        
        /// <summary>
        /// Play effect with camera shake for more impact
        /// </summary>
        public void PlayEffectWithShake(string effectPath, Vector2 position, float shakeIntensity = 10f, float shakeDuration = 0.3f)
        {
            PlayEffectAtPosition(effectPath, position);
            
            // Call camera shake if you have a camera shake system
            // GetViewport().GetCamera2D()?.Shake(shakeIntensity, shakeDuration);
        }
        
        #endregion
    }
}