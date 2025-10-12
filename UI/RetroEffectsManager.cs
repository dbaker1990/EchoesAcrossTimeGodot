// UI/RetroEffectsManager.cs
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
            CRT,
            Pixelation,
            ColorPalette,
            VHS,
            Dithering,
            Gameboy,
            RGBGlitch,
            ArcadeCRT
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

        // Shader paths
        private readonly Dictionary<RetroEffectType, string> shaderPaths = new Dictionary<RetroEffectType, string>
        {
            { RetroEffectType.CRT, "res://UI/Materials/CRT.gdshader" },
            { RetroEffectType.Pixelation, "res://UI/Materials/RetroPixelation.gdshader" },
            { RetroEffectType.ColorPalette, "res://UI/Materials/RetroPalette.gdshader" },
            { RetroEffectType.VHS, "res://UI/Materials/RetroVHS.gdshader" },
            { RetroEffectType.Dithering, "res://UI/Materials/RetroDither.gdshader" },
            { RetroEffectType.Gameboy, "res://UI/Materials/RetroGameboy.gdshader" },
            { RetroEffectType.RGBGlitch, "res://UI/Materials/RetroGlitch.gdshader" },
            { RetroEffectType.ArcadeCRT, "res://UI/Materials/RetroArcade.gdshader" }
        };

        public override void _Ready()
        {
            InitializeEffects();
            UpdateActiveEffects();
        }

        private void InitializeEffects()
        {
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
                rect.SetAnchorsPreset(Control.LayoutPreset.FullRect);

                if (shaderPaths.ContainsKey(effectType))
                {
                    var shader = GD.Load<Shader>(shaderPaths[effectType]);
                    if (shader != null)
                    {
                        var material = new ShaderMaterial { Shader = shader };
                        rect.Material = material;
                        shaderMaterials[effectType] = material;
                    }
                    else
                    {
                        GD.PrintErr($"Failed to load shader for {effectType} at {shaderPaths[effectType]}");
                    }
                }

                effectRects[effectType] = rect;
            }
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
            PrimaryEffect = effect;
            EnableSecondaryEffect = false;
            UpdateActiveEffects();
        }

        /// <summary>
        /// Combine two effects (e.g., CRT + Pixelation)
        /// </summary>
        public void SetCombinedEffect(RetroEffectType primary, RetroEffectType secondary)
        {
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

        public void SetCRTParameters(float scanlineStrength, float curvature)
        {
            var material = GetEffectMaterial(RetroEffectType.CRT);
            if (material != null)
            {
                material.SetShaderParameter("scanline_strength", scanlineStrength);
                material.SetShaderParameter("curvature", curvature);
            }
        }

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
                    SetColorDepth(32);
                    break;

                case "crt_classic":
                    SetEffect(RetroEffectType.CRT);
                    SetCRTParameters(0.3f, 0.05f);
                    break;

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