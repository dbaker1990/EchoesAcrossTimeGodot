// UI/CRTEffect.cs
using Godot;
using System;

namespace EchoesAcrossTime.UI
{
    /// <summary>
    /// Applies a retro CRT screen effect over the entire viewport
    /// </summary>
    public partial class CRTEffect : CanvasLayer
    {
        [ExportGroup("CRT Settings")]
        [Export(PropertyHint.Range, "0,1")] public float ScanlineStrength { get; set; } = 0.3f;
        [Export(PropertyHint.Range, "100,1000")] public float ScanlineCount { get; set; } = 400f;
        [Export(PropertyHint.Range, "0,0.3")] public float Curvature { get; set; } = 0.05f;
        [Export(PropertyHint.Range, "0,1")] public float VignetteStrength { get; set; } = 0.4f;
        [Export(PropertyHint.Range, "0,0.01")] public float ChromaticAberration { get; set; } = 0.002f;
        [Export(PropertyHint.Range, "0,0.1")] public float NoiseAmount { get; set; } = 0.02f;
        [Export(PropertyHint.Range, "0.5,1.5")] public float Brightness { get; set; } = 1.0f;
        [Export] public Color PhosphorColor { get; set; } = new Color(1.0f, 0.95f, 0.85f);
        
        [ExportGroup("Flicker")]
        [Export(PropertyHint.Range, "0,10")] public float FlickerSpeed { get; set; } = 2.0f;
        [Export(PropertyHint.Range, "0,0.1")] public float FlickerStrength { get; set; } = 0.02f;
        
        [ExportGroup("Toggle")]
        [Export] public bool Enabled { get; set; } = true;
        
        private ColorRect crtRect;
        private ShaderMaterial shaderMaterial;
        
        public override void _Ready()
        {
            // Create the full-screen rect
            crtRect = new ColorRect
            {
                Name = "CRTRect",
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            AddChild(crtRect);
            crtRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            
            // Load and apply the CRT shader
            var shader = GD.Load<Shader>("res://UI/Materials/CRT.gdshader");
            if (shader != null)
            {
                shaderMaterial = new ShaderMaterial
                {
                    Shader = shader
                };
                crtRect.Material = shaderMaterial;
                UpdateShaderParameters();
            }
            else
            {
                GD.PrintErr("CRT shader not found at res://UI/Materials/CRT.gdshader");
            }
            
            crtRect.Visible = Enabled;
        }
        
        public override void _Process(double delta)
        {
            if (crtRect != null)
            {
                crtRect.Visible = Enabled;
            }
        }
        
        /// <summary>
        /// Updates all shader parameters based on exported properties
        /// </summary>
        public void UpdateShaderParameters()
        {
            if (shaderMaterial == null) return;
            
            shaderMaterial.SetShaderParameter("scanline_strength", ScanlineStrength);
            shaderMaterial.SetShaderParameter("scanline_count", ScanlineCount);
            shaderMaterial.SetShaderParameter("curvature", Curvature);
            shaderMaterial.SetShaderParameter("vignette_strength", VignetteStrength);
            shaderMaterial.SetShaderParameter("chromatic_aberration", ChromaticAberration);
            shaderMaterial.SetShaderParameter("noise_amount", NoiseAmount);
            shaderMaterial.SetShaderParameter("brightness", Brightness);
            shaderMaterial.SetShaderParameter("phosphor_color", new Vector3(
                PhosphorColor.R, PhosphorColor.G, PhosphorColor.B
            ));
            shaderMaterial.SetShaderParameter("flicker_speed", FlickerSpeed);
            shaderMaterial.SetShaderParameter("flicker_strength", FlickerStrength);
        }
        
        /// <summary>
        /// Toggle the CRT effect on/off
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
            if (crtRect != null)
            {
                crtRect.Visible = enabled;
            }
        }
        
        /// <summary>
        /// Preset for subtle CRT effect
        /// </summary>
        public void ApplySubtlePreset()
        {
            ScanlineStrength = 0.15f;
            Curvature = 0.02f;
            VignetteStrength = 0.2f;
            ChromaticAberration = 0.001f;
            NoiseAmount = 0.01f;
            FlickerStrength = 0.01f;
            UpdateShaderParameters();
        }
        
        /// <summary>
        /// Preset for intense retro CRT effect
        /// </summary>
        public void ApplyIntensePreset()
        {
            ScanlineStrength = 0.5f;
            Curvature = 0.1f;
            VignetteStrength = 0.6f;
            ChromaticAberration = 0.005f;
            NoiseAmount = 0.05f;
            FlickerStrength = 0.05f;
            UpdateShaderParameters();
        }
        
        /// <summary>
        /// Animate a glitch effect (for special moments)
        /// </summary>
        public async void TriggerGlitch(float duration = 0.5f)
        {
            float originalNoise = NoiseAmount;
            float originalChroma = ChromaticAberration;
            
            NoiseAmount = 0.2f;
            ChromaticAberration = 0.01f;
            UpdateShaderParameters();
            
            await ToSignal(GetTree().CreateTimer(duration), SceneTreeTimer.SignalName.Timeout);
            
            NoiseAmount = originalNoise;
            ChromaticAberration = originalChroma;
            UpdateShaderParameters();
        }
    }
}