using Godot;
using System;
using System.Collections.Generic;

namespace EchoesAcrossTime.UI
{
    /// <summary>
    /// Manages multiple retro screen effects with easy switching and combination
    /// </summary>
    public partial class RetroEffectsManager : CanvasLayer
    {
        public enum RetroEffectType
        {
            None,
            // CRT removed - uses screen_texture which doesn't work in AutoLoad
            Pixelation,
            ColorPalette,
            VHS,
            Dithering,
            Gameboy,
            RGBGlitch,
            ArcadeCRT,
            Scanlines  // NEW: Simple scanlines effect
        }

        [ExportGroup("Active Effects")]
        [Export] public RetroEffectType PrimaryEffect { get; set; } = RetroEffectType.None;
        [Export] public bool EnableSecondaryEffect { get; set; } = false;
        [Export] public RetroEffectType SecondaryEffect { get; set; } = RetroEffectType.None;

        [ExportGroup("Effect Settings")]
        [Export] public bool Enabled { get; set; } = true;

        // Individual effect rects
        private Dictionary<RetroEffectType, ColorRect> effectRects = new Dictionary<RetroEffectType, ColorRect>();
        private Dictionary<RetroEffectType, ShaderMaterial> shaderMaterials = new Dictionary<RetroEffectType, ShaderMaterial>();

        // Shader paths (CRT removed)
        private readonly Dictionary<RetroEffectType, string> shaderPaths = new Dictionary<RetroEffectType, string>
        {
            // CRT removed - requires screen_texture sampling
            { RetroEffectType.Pixelation, "res://UI/Materials/RetroPixelation.gdshader" },
            { RetroEffectType.ColorPalette, "res://UI/Materials/RetroPalette.gdshader" },
            { RetroEffectType.VHS, "res://UI/Materials/RetroVHS.gdshader" },
            { RetroEffectType.Dithering, "res://UI/Materials/RetroDither.gdshader" },
            { RetroEffectType.Gameboy, "res://UI/Materials/RetroGameboy.gdshader" },
            { RetroEffectType.RGBGlitch, "res://UI/Materials/RetroGlitch.gdshader" },
            { RetroEffectType.ArcadeCRT, "res://UI/Materials/RetroArcade.gdshader" },
            { RetroEffectType.Scanlines, "res://UI/Materials/RetroScanlines.gdshader" }
        };

        public override void _Ready()
        {
            GD.Print("=== RetroEffectsManager Initializing ===");
            
            // CRITICAL: Set CanvasLayer properties for AutoLoad
            // Use a high but not too high layer - render AFTER content
            Layer = 100; // High enough to be on top, not so high it renders too early
            FollowViewportEnabled = true; // Follow the viewport (essential for AutoLoad)
            
            GD.Print($"Layer set to: {Layer}");
            GD.Print($"FollowViewportEnabled: {FollowViewportEnabled}");
            
            InitializeEffects();
            UpdateActiveEffects();
            
            // Connect to viewport size changed signal
            GetViewport().SizeChanged += OnViewportSizeChanged;
            
            GD.Print("=== RetroEffectsManager Ready ===");
        }
        
        private void OnViewportSizeChanged()
        {
            // Update all effect rects when viewport size changes
            var newSize = GetViewport().GetVisibleRect().Size;
            GD.Print($"Viewport resized to: {newSize.X}x{newSize.Y}");
            
            foreach (var rect in effectRects.Values)
            {
                rect.SetSize(newSize);
            }
        }

        private void InitializeEffects()
        {
            GD.Print("Initializing visual effects...");
            int successCount = 0;
            int failCount = 0;
            
            // Get viewport size for proper rect sizing
            var viewportSize = GetViewport().GetVisibleRect().Size;
            GD.Print($"Viewport size: {viewportSize.X}x{viewportSize.Y}");
            
            foreach (var effectType in System.Enum.GetValues<RetroEffectType>())
            {
                if (effectType == RetroEffectType.None) continue;

                var rect = new ColorRect
                {
                    Name = $"{effectType}Rect",
                    MouseFilter = Control.MouseFilterEnum.Ignore,
                    Visible = false
                };
                AddChild(rect);
                
                // Set anchors to fill screen
                rect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
                
                // CRITICAL: Manually set size for AutoLoad CanvasLayers
                rect.SetSize(viewportSize);
                rect.Position = Vector2.Zero;
                
                // Also set offsets to 0 to ensure full coverage
                rect.OffsetLeft = 0;
                rect.OffsetTop = 0;
                rect.OffsetRight = 0;
                rect.OffsetBottom = 0;

                if (shaderPaths.ContainsKey(effectType))
                {
                    var shader = GD.Load<Shader>(shaderPaths[effectType]);
                    if (shader != null)
                    {
                        var material = new ShaderMaterial { Shader = shader };
                        rect.Material = material;
                        shaderMaterials[effectType] = material;
                        successCount++;
                        GD.Print($"  ✓ {effectType} shader loaded - Size: {rect.Size}");
                    }
                    else
                    {
                        GD.PrintErr($"  ✗ Failed to load shader for {effectType} at {shaderPaths[effectType]}");
                        failCount++;
                    }
                }

                effectRects[effectType] = rect;
            }
            
            GD.Print($"Effects initialized: {successCount} loaded, {failCount} failed");
        }

        public override void _Process(double delta)
        {
            UpdateActiveEffects();
        }

        private void UpdateActiveEffects()
        {
            if (!Enabled)
            {
                foreach (var rect in effectRects.Values)
                {
                    rect.Visible = false;
                }
                return;
            }

            // Hide all effects first
            foreach (var effectType in effectRects.Keys)
            {
                effectRects[effectType].Visible = false;
            }

            // Show primary effect
            if (PrimaryEffect != RetroEffectType.None && effectRects.ContainsKey(PrimaryEffect))
            {
                effectRects[PrimaryEffect].Visible = true;
            }

            // Show secondary effect if enabled
            if (EnableSecondaryEffect && SecondaryEffect != RetroEffectType.None && 
                effectRects.ContainsKey(SecondaryEffect))
            {
                effectRects[SecondaryEffect].Visible = true;
            }
        }

        // ===== PUBLIC METHODS =====

        /// <summary>
        /// Switch to a specific retro effect
        /// </summary>
        public void SetEffect(RetroEffectType effect)
        {
            GD.Print($"[RetroEffectsManager] Setting effect to: {effect}");
            PrimaryEffect = effect;
            EnableSecondaryEffect = false;
            UpdateActiveEffects();
            
            // Debug: Check if rect is visible
            if (effect != RetroEffectType.None && effectRects.ContainsKey(effect))
            {
                var rect = effectRects[effect];
                GD.Print($"  Effect rect visible: {rect.Visible}");
                GD.Print($"  Effect rect has material: {rect.Material != null}");
                GD.Print($"  Effect rect size: {rect.Size}");
            }
        }

        /// <summary>
        /// Combine two effects (e.g., CRT + Pixelation)
        /// </summary>
        public void SetCombinedEffect(RetroEffectType primary, RetroEffectType secondary)
        {
            GD.Print($"[RetroEffectsManager] Setting combined effect: {primary} + {secondary}");
            PrimaryEffect = primary;
            SecondaryEffect = secondary;
            EnableSecondaryEffect = true;
            UpdateActiveEffects();
        }

        /// <summary>
        /// Toggle effects on/off
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            GD.Print($"[RetroEffectsManager] Enabled set to: {enabled}");
            Enabled = enabled;
            UpdateActiveEffects();
        }

        /// <summary>
        /// Get the shader material for a specific effect to modify parameters
        /// </summary>
        public ShaderMaterial GetEffectMaterial(RetroEffectType effect)
        {
            return shaderMaterials.ContainsKey(effect) ? shaderMaterials[effect] : null;
        }

        // ===== EFFECT-SPECIFIC PARAMETER SETTERS =====
        // CRT methods removed

        public void SetPixelationSize(int size)
        {
            var material = GetEffectMaterial(RetroEffectType.Pixelation);
            material?.SetShaderParameter("pixel_size", size);
        }

        public void SetColorDepth(int depth)
        {
            var material = GetEffectMaterial(RetroEffectType.ColorPalette);
            material?.SetShaderParameter("color_depth", depth);
        }

        public void SetVHSDistortion(float distortion, float noise)
        {
            var material = GetEffectMaterial(RetroEffectType.VHS);
            if (material != null)
            {
                material.SetShaderParameter("distortion_strength", distortion);
                material.SetShaderParameter("noise_strength", noise);
            }
        }

        public void SetRGBGlitchIntensity(float intensity, float separation)
        {
            var material = GetEffectMaterial(RetroEffectType.RGBGlitch);
            if (material != null)
            {
                material.SetShaderParameter("glitch_intensity", intensity);
                material.SetShaderParameter("separation_amount", separation);
            }
        }

        public void SetScanlinesParameters(float strength, float count)
        {
            var material = GetEffectMaterial(RetroEffectType.Scanlines);
            if (material != null)
            {
                material.SetShaderParameter("scanline_strength", strength);
                material.SetShaderParameter("scanline_count", count);
            }
        }

        public void SetDitheringParameters(float strength, int size)
        {
            var material = GetEffectMaterial(RetroEffectType.Dithering);
            if (material != null)
            {
                material.SetShaderParameter("dither_strength", strength);
                material.SetShaderParameter("dither_size", size);
            }
        }

        // CRT methods removed - filter not supported in AutoLoad

        // ===== PRESETS =====

        public void ApplyPreset(string presetName)
        {
            switch (presetName.ToLower())
            {
                case "nes":
                    SetEffect(RetroEffectType.ColorPalette);
                    SetColorDepth(64);
                    break;

                case "gameboy":
                    SetEffect(RetroEffectType.Gameboy);
                    break;
                    
                case "snes":
                    SetEffect(RetroEffectType.Scanlines);
                    SetScanlinesParameters(0.3f, 480.0f);
                    break;
                    
                case "snes_dither":
                    SetCombinedEffect(RetroEffectType.Dithering, RetroEffectType.Scanlines);
                    SetDitheringParameters(0.5f, 2);
                    SetScanlinesParameters(0.2f, 480.0f);
                    break;

                case "vhs_horror":
                    SetEffect(RetroEffectType.VHS);
                    SetVHSDistortion(0.05f, 0.3f);
                    break;

                case "arcade":
                    SetCombinedEffect(RetroEffectType.ArcadeCRT, RetroEffectType.ColorPalette);
                    SetColorDepth(256);
                    break;

                case "glitch_city":
                    SetCombinedEffect(RetroEffectType.RGBGlitch, RetroEffectType.Pixelation);
                    SetPixelationSize(4);
                    SetRGBGlitchIntensity(0.8f, 0.03f);
                    break;

                case "pixel_perfect":
                    SetCombinedEffect(RetroEffectType.Pixelation, RetroEffectType.ColorPalette);
                    SetPixelationSize(3);
                    SetColorDepth(256);
                    break;

                // CRT presets removed

                default:
                    SetEffect(RetroEffectType.None);
                    break;
            }
        }

        // ===== SPECIAL EFFECTS =====

        /// <summary>
        /// Trigger a temporary glitch effect
        /// </summary>
        public async void TriggerGlitchBurst(float duration = 0.3f)
        {
            var previousEffect = PrimaryEffect;
            var previousSecondary = EnableSecondaryEffect;

            SetEffect(RetroEffectType.RGBGlitch);
            SetRGBGlitchIntensity(1.0f, 0.05f);

            await ToSignal(GetTree().CreateTimer(duration), SceneTreeTimer.SignalName.Timeout);

            PrimaryEffect = previousEffect;
            EnableSecondaryEffect = previousSecondary;
            UpdateActiveEffects();
        }

        /// <summary>
        /// Fade between two effects
        /// </summary>
        public async void TransitionEffect(RetroEffectType from, RetroEffectType to, float duration = 1.0f)
        {
            // Simple crossfade by switching (could be enhanced with alpha blending)
            SetEffect(from);
            await ToSignal(GetTree().CreateTimer(duration / 2), SceneTreeTimer.SignalName.Timeout);
            SetEffect(to);
        }
    }
}