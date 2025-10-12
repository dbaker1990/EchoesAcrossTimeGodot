using Godot;
using System;
using EchoesAcrossTime.Managers;

namespace EchoesAcrossTime.Environment
{
    /// <summary>
    /// Visual weather system that handles particles, fog, and effects
    /// Attach this to a Node2D in your overworld scene
    /// Should be a child of the Camera2D so it moves with the player
    /// </summary>
    public partial class WeatherSystem : Node2D
    {
        // Weather particle nodes (will be created in scene)
        private GpuParticles2D rainParticles;
        private GpuParticles2D snowParticles;
        
        // Fog overlay
        private ColorRect fogOverlay;
        
        // Lightning effect
        private ColorRect lightningFlash;
        private Timer lightningTimer;
        private AudioStreamPlayer lightningSound;
        
        // References
        private Camera2D camera;
        
        // Settings
        [Export] public Vector2 ParticleEmissionSize { get; set; } = new Vector2(2000, 100);
        [Export] public Color FogColor { get; set; } = new Color(0.8f, 0.8f, 0.8f, 0.4f);
        [Export] public float LightningMinInterval { get; set; } = 3.0f;
        [Export] public float LightningMaxInterval { get; set; } = 8.0f;
        
        public override void _Ready()
        {
            // Get or create camera reference
            camera = GetParent<Camera2D>();
            if (camera == null)
            {
                GD.PrintErr("[WeatherSystem] Must be child of Camera2D!");
                return;
            }
            
            SetupWeatherNodes();
            
            // Connect to weather manager
            if (WeatherManager.Instance != null)
            {
                WeatherManager.Instance.WeatherChanged += OnWeatherChanged;
                OnWeatherChanged((int)WeatherManager.Instance.CurrentWeather);
            }
            
            GD.Print("[WeatherSystem] Initialized and connected to camera");
        }
        
        private void SetupWeatherNodes()
        {
            // Create Rain Particles
            rainParticles = CreateRainParticles();
            AddChild(rainParticles);
            
            // Create Snow Particles
            snowParticles = CreateSnowParticles();
            AddChild(snowParticles);
            
            // Create Fog Overlay
            fogOverlay = CreateFogOverlay();
            AddChild(fogOverlay);
            
            // Create Lightning Effect
            lightningFlash = CreateLightningFlash();
            AddChild(lightningFlash);
            
            // Create Lightning Timer
            lightningTimer = new Timer();
            lightningTimer.OneShot = true;
            lightningTimer.Timeout += OnLightningTimerTimeout;
            AddChild(lightningTimer);
            
            // Create Lightning Sound (optional)
            lightningSound = new AudioStreamPlayer();
            lightningSound.VolumeDb = -5.0f;
            AddChild(lightningSound);
        }
        
        private GpuParticles2D CreateRainParticles()
        {
            var particles = new GpuParticles2D();
            particles.Name = "RainParticles";
            particles.Amount = 500;
            particles.Lifetime = 2.0f;
            particles.Preprocess = 0.5f;
            particles.SpeedScale = 1.0f;
            particles.Emitting = false;
            particles.ZIndex = 100;
            
            // Create process material
            var material = new ParticleProcessMaterial();
            
            // Emission
            material.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box;
            material.EmissionBoxExtents = new Vector3(ParticleEmissionSize.X / 2, ParticleEmissionSize.Y / 2, 0);
            
            // Gravity (downward)
            material.Gravity = new Vector3(0, 2000, 0);
            
            // Direction
            material.Direction = new Vector3(0, 1, 0);
            material.Spread = 5.0f;
            material.InitialVelocityMin = 500.0f;
            material.InitialVelocityMax = 800.0f;
            
            // Appearance
            material.ScaleMin = 2.0f;
            material.ScaleMax = 2.0f;
            
            particles.ProcessMaterial = material;
            
            // Create simple white line texture
            particles.Texture = CreateRainTexture();
            
            return particles;
        }
        
        private GpuParticles2D CreateSnowParticles()
        {
            var particles = new GpuParticles2D();
            particles.Name = "SnowParticles";
            particles.Amount = 300;
            particles.Lifetime = 5.0f;
            particles.Preprocess = 1.0f;
            particles.SpeedScale = 1.0f;
            particles.Emitting = false;
            particles.ZIndex = 100;
            
            // Create process material
            var material = new ParticleProcessMaterial();
            
            // Emission
            material.EmissionShape = ParticleProcessMaterial.EmissionShapeEnum.Box;
            material.EmissionBoxExtents = new Vector3(ParticleEmissionSize.X / 2, ParticleEmissionSize.Y / 2, 0);
            
            // Gravity (slower downward)
            material.Gravity = new Vector3(0, 200, 0);
            
            // Direction with spread
            material.Direction = new Vector3(0, 1, 0);
            material.Spread = 15.0f;
            material.InitialVelocityMin = 50.0f;
            material.InitialVelocityMax = 100.0f;
            
            // Turbulence for flutter effect
            material.TurbulenceEnabled = true;
            material.TurbulenceNoiseStrength = 2.0f;
            material.TurbulenceNoiseScale = 5.0f;
            material.TurbulenceInfluenceMin = 0.1f;
            material.TurbulenceInfluenceMax = 0.3f;
            
            // Appearance
            material.ScaleMin = 3.0f;
            material.ScaleMax = 3.0f;
            
            particles.ProcessMaterial = material;
            
            // Create simple white dot texture
            particles.Texture = CreateSnowTexture();
            
            return particles;
        }
        
        private ColorRect CreateFogOverlay()
        {
            var fog = new ColorRect();
            fog.Name = "FogOverlay";
            fog.Color = FogColor;
            fog.Modulate = new Color(1, 1, 1, 0); // Start invisible
            fog.ZIndex = 90;
            
            // Size to cover viewport
            fog.Size = new Vector2(1920, 1080);
            fog.Position = new Vector2(-960, -540); // Center on camera
            
            return fog;
        }
        
        private ColorRect CreateLightningFlash()
        {
            var flash = new ColorRect();
            flash.Name = "LightningFlash";
            flash.Color = Colors.White;
            flash.Modulate = new Color(1, 1, 1, 0); // Start invisible
            flash.ZIndex = 110;
            
            // Size to cover viewport
            flash.Size = new Vector2(1920, 1080);
            flash.Position = new Vector2(-960, -540); // Center on camera
            
            return flash;
        }
        
        private Texture2D CreateRainTexture()
        {
            // Create a simple white vertical line
            var image = Image.CreateEmpty(2, 8, false, Image.Format.Rgba8);
            image.Fill(Colors.White);
            return ImageTexture.CreateFromImage(image);
        }
        
        private Texture2D CreateSnowTexture()
        {
            // Create a simple white circle
            var image = Image.CreateEmpty(4, 4, false, Image.Format.Rgba8);
            image.Fill(Colors.White);
            return ImageTexture.CreateFromImage(image);
        }
        
        private void OnWeatherChanged(int weatherType)
        {
            var weather = (WeatherManager.WeatherType)weatherType;
            GD.Print($"[WeatherSystem] Updating visual effects for: {weather}");
            
            // Stop all effects first
            StopAllWeather();
            
            // Start appropriate effects
            switch (weather)
            {
                case WeatherManager.WeatherType.Clear:
                    // Nothing to do
                    break;
                    
                case WeatherManager.WeatherType.Rain:
                    StartRain(false);
                    break;
                    
                case WeatherManager.WeatherType.HeavyRain:
                    StartRain(true);
                    break;
                    
                case WeatherManager.WeatherType.Snow:
                    StartSnow(false);
                    break;
                    
                case WeatherManager.WeatherType.HeavySnow:
                    StartSnow(true);
                    break;
                    
                case WeatherManager.WeatherType.Fog:
                    StartFog();
                    break;
                    
                case WeatherManager.WeatherType.Storm:
                    StartStorm();
                    break;
                    
                case WeatherManager.WeatherType.Blizzard:
                    StartBlizzard();
                    break;
            }
        }
        
        private void StopAllWeather()
        {
            rainParticles.Emitting = false;
            snowParticles.Emitting = false;
            lightningTimer.Stop();
            
            // Fade out fog
            var tween = CreateTween();
            tween.TweenProperty(fogOverlay, "modulate:a", 0.0f, 1.0f);
        }
        
        private void StartRain(bool heavy)
        {
            rainParticles.Amount = heavy ? 800 : 500;
            rainParticles.Emitting = true;
            
            // Adjust rain speed
            var material = rainParticles.ProcessMaterial as ParticleProcessMaterial;
            if (material != null)
            {
                material.InitialVelocityMin = heavy ? 800.0f : 500.0f;
                material.InitialVelocityMax = heavy ? 1200.0f : 800.0f;
            }
        }
        
        private void StartSnow(bool heavy)
        {
            snowParticles.Amount = heavy ? 500 : 300;
            snowParticles.Emitting = true;
            
            // Adjust snow speed
            var material = snowParticles.ProcessMaterial as ParticleProcessMaterial;
            if (material != null)
            {
                material.InitialVelocityMin = heavy ? 80.0f : 50.0f;
                material.InitialVelocityMax = heavy ? 150.0f : 100.0f;
            }
        }
        
        private void StartFog()
        {
            var tween = CreateTween();
            tween.TweenProperty(fogOverlay, "modulate:a", 0.6f, 2.0f);
        }
        
        private void StartStorm()
        {
            StartRain(true);
            
            // Light fog
            var tween = CreateTween();
            tween.TweenProperty(fogOverlay, "modulate:a", 0.3f, 2.0f);
            
            // Start lightning
            ScheduleNextLightning();
        }
        
        private void StartBlizzard()
        {
            StartSnow(true);
            
            // Heavy fog
            var tween = CreateTween();
            tween.TweenProperty(fogOverlay, "modulate:a", 0.5f, 2.0f);
        }
        
        private void ScheduleNextLightning()
        {
            if (!WeatherManager.Instance.IsStormy())
                return;
            
            float interval = (float)GD.RandRange(LightningMinInterval, LightningMaxInterval);
            lightningTimer.Start(interval);
        }
        
        private void OnLightningTimerTimeout()
        {
            TriggerLightning();
            ScheduleNextLightning();
        }
        
        private void TriggerLightning()
        {
            // Quick flash effect
            var tween = CreateTween();
            tween.TweenProperty(lightningFlash, "modulate:a", 1.0f, 0.05f);
            tween.TweenProperty(lightningFlash, "modulate:a", 0.0f, 0.15f);
            
            // Play thunder sound if available
            if (lightningSound.Stream != null)
            {
                lightningSound.Play();
            }
            
            GD.Print("[WeatherSystem] ⚡ Lightning strike!");
        }
        
        public override void _ExitTree()
        {
            if (WeatherManager.Instance != null)
            {
                WeatherManager.Instance.WeatherChanged -= OnWeatherChanged;
            }
        }
    }
}