// Audio/AudioManager.cs
using Godot;
using System;
using System.Threading.Tasks;

namespace EchoesAcrossTime
{
    /// <summary>
    /// Manages background music and sound effects
    /// </summary>
    public partial class AudioManager : Node
    {
        public static AudioManager Instance { get; private set; }
        
        [ExportGroup("Audio Players")]
        [Export] public AudioStreamPlayer BGMPlayer { get; set; }
        [Export] public AudioStreamPlayer BGSPlayer { get; set; } // Background Sound
        [Export] public AudioStreamPlayer SEPlayer { get; set; }  // Sound Effects
        [Export] public AudioStreamPlayer MEPlayer { get; set; }  // Music Effects (jingles)
        
        [ExportGroup("Settings")]
        [Export] public float MasterVolume { get; set; } = 1.0f;
        [Export] public float BGMVolume { get; set; } = 1.0f;
        [Export] public float SEVolume { get; set; } = 1.0f;
        
        private AudioStream currentBGM;
        private float currentBGMPosition = 0f;
        
        public override void _Ready()
        {
            if (Instance != null)
            {
                QueueFree();
                return;
            }
            
            Instance = this;
            
            // Create audio players if not assigned
            if (BGMPlayer == null)
            {
                BGMPlayer = new AudioStreamPlayer { Name = "BGMPlayer" };
                AddChild(BGMPlayer);
            }
            
            if (BGSPlayer == null)
            {
                BGSPlayer = new AudioStreamPlayer { Name = "BGSPlayer" };
                AddChild(BGSPlayer);
            }
            
            if (SEPlayer == null)
            {
                SEPlayer = new AudioStreamPlayer { Name = "SEPlayer" };
                AddChild(SEPlayer);
            }
            
            if (MEPlayer == null)
            {
                MEPlayer = new AudioStreamPlayer { Name = "MEPlayer" };
                AddChild(MEPlayer);
            }
            
            GD.Print("AudioManager initialized");
        }
        
        /// <summary>
        /// Play background music
        /// </summary>
        public void PlayBGM(AudioStream bgm, float volume = 0f, float fadeInDuration = 0f)
        {
            if (bgm == null || BGMPlayer == null) return;
            
            // If same BGM is playing, don't restart
            if (currentBGM == bgm && BGMPlayer.Playing)
                return;
            
            currentBGM = bgm;
            BGMPlayer.Stream = bgm;
            BGMPlayer.VolumeDb = volume;
            
            if (fadeInDuration > 0)
            {
                BGMPlayer.VolumeDb = -80f; // Start silent
                BGMPlayer.Play();
                
                var tween = CreateTween();
                tween.TweenProperty(BGMPlayer, "volume_db", volume, fadeInDuration);
            }
            else
            {
                BGMPlayer.Play();
            }
            
            GD.Print($"Playing BGM: {bgm.ResourcePath}");
        }
        
        /// <summary>
        /// Stop background music
        /// </summary>
        public async Task StopBGM(float fadeOutDuration = 0f)
        {
            if (BGMPlayer == null) return;
            
            if (fadeOutDuration > 0)
            {
                float startVolume = BGMPlayer.VolumeDb;
                var tween = CreateTween();
                tween.TweenProperty(BGMPlayer, "volume_db", -80f, fadeOutDuration);
                await ToSignal(tween, Tween.SignalName.Finished);
            }
            
            BGMPlayer.Stop();
            currentBGM = null;
        }
        
        /// <summary>
        /// Pause BGM
        /// </summary>
        public void PauseBGM()
        {
            if (BGMPlayer != null && BGMPlayer.Playing)
            {
                currentBGMPosition = BGMPlayer.GetPlaybackPosition();
                BGMPlayer.Stop();
            }
        }
        
        /// <summary>
        /// Resume BGM
        /// </summary>
        public void ResumeBGM()
        {
            if (BGMPlayer != null && currentBGM != null)
            {
                BGMPlayer.Play();
                BGMPlayer.Seek(currentBGMPosition);
            }
        }
        
        /// <summary>
        /// Play background sound (looping ambient sound)
        /// </summary>
        public void PlayBGS(AudioStream bgs, float volume = 0f)
        {
            if (bgs == null || BGSPlayer == null) return;
            
            BGSPlayer.Stream = bgs;
            BGSPlayer.VolumeDb = volume;
            BGSPlayer.Play();
        }
        
        /// <summary>
        /// Stop background sound
        /// </summary>
        public void StopBGS(float fadeOutDuration = 0f)
        {
            if (BGSPlayer == null) return;
            
            if (fadeOutDuration > 0)
            {
                var tween = CreateTween();
                tween.TweenProperty(BGSPlayer, "volume_db", -80f, fadeOutDuration);
                tween.TweenCallback(Callable.From(() => BGSPlayer.Stop()));
            }
            else
            {
                BGSPlayer.Stop();
            }
        }
        
        /// <summary>
        /// Play sound effect
        /// </summary>
        public void PlaySoundEffect(AudioStream se, float volume = 0f)
        {
            if (se == null || SEPlayer == null) return;
            
            // For overlapping sound effects, you might want multiple AudioStreamPlayers
            // For now, this just plays on the main SE player
            SEPlayer.Stream = se;
            SEPlayer.VolumeDb = volume;
            SEPlayer.Play();
        }
        
        /// <summary>
        /// Play music effect (jingle that pauses BGM)
        /// </summary>
        public async Task PlayME(AudioStream me, float volume = 0f)
        {
            if (me == null || MEPlayer == null) return;
            
            // Pause BGM
            PauseBGM();
            
            // Play ME
            MEPlayer.Stream = me;
            MEPlayer.VolumeDb = volume;
            MEPlayer.Play();
            
            // Wait for ME to finish
            await ToSignal(MEPlayer, AudioStreamPlayer.SignalName.Finished);
            
            // Resume BGM
            ResumeBGM();
        }
        
        /// <summary>
        /// Set master volume
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            MasterVolume = Mathf.Clamp(volume, 0f, 1f);
            AudioServer.SetBusVolumeDb(0, Mathf.LinearToDb(MasterVolume));
        }
        
        /// <summary>
        /// Set BGM volume
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            BGMVolume = Mathf.Clamp(volume, 0f, 1f);
            if (BGMPlayer != null)
            {
                BGMPlayer.VolumeDb = Mathf.LinearToDb(BGMVolume);
            }
        }
        
        /// <summary>
        /// Set SE volume
        /// </summary>
        public void SetSEVolume(float volume)
        {
            SEVolume = Mathf.Clamp(volume, 0f, 1f);
            if (SEPlayer != null)
            {
                SEPlayer.VolumeDb = Mathf.LinearToDb(SEVolume);
            }
        }
    }
}