using Godot;
using System;
using System.Collections.Generic;
using EchoesAcrossTime.Combat;
using EchoesAcrossTime.Database;
using EchoesAcrossTime.Managers;

namespace EchoesAcrossTime.Bestiary
{
    /// <summary>
    /// Displays popup notifications when new enemies are discovered
    /// Similar to "New Enemy Registered!" notifications in JRPGs
    /// </summary>
    public partial class BestiaryNotification : Control
    {
        [Export] private Panel notificationPanel;
        [Export] private Label titleLabel;
        [Export] private Label enemyNameLabel;
        [Export] private TextureRect enemyIcon;
        [Export] private AnimationPlayer animationPlayer;
        [Export] private Timer displayTimer;
        
        [Export] private float displayDuration = 3.0f;
        [Export] private Color discoveryColor = Colors.Gold;
        
        private Queue<NotificationData> notificationQueue = new();
        private bool isShowingNotification = false;
        
        private class NotificationData
        {
            public string EnemyId;
            public CharacterData EnemyData;
            public string Message;
        }
        
        public override void _Ready()
        {
            // Setup
            if (notificationPanel != null)
                notificationPanel.Hide();
            
            if (displayTimer == null)
            {
                displayTimer = new Timer();
                AddChild(displayTimer);
                displayTimer.WaitTime = displayDuration;
                displayTimer.OneShot = true;
                displayTimer.Timeout += OnDisplayTimerTimeout;
            }
            
            // Connect to bestiary events
            if (BestiaryManager.Instance != null)
            {
                BestiaryManager.Instance.EnemyDiscovered += OnEnemyDiscovered;
                BestiaryManager.Instance.WeaknessDiscovered += OnWeaknessDiscovered;
                BestiaryManager.Instance.NewSkillDiscovered += OnSkillDiscovered;
            }
        }
        
        #region Event Handlers
        private void OnEnemyDiscovered(string enemyId, CharacterData enemyData)
        {
            QueueNotification(new NotificationData
            {
                EnemyId = enemyId,
                EnemyData = enemyData,
                Message = "New Enemy Registered!"
            });
        }
        
        private void OnWeaknessDiscovered(string enemyId, ElementType weakness)
        {
            var enemyData = BestiaryManager.Instance?.GetEnemyData(enemyId);
            if (enemyData == null) return;
            
            QueueNotification(new NotificationData
            {
                EnemyId = enemyId,
                EnemyData = enemyData,
                Message = $"Weakness Discovered: {weakness}!"
            });
        }
        
        private void OnSkillDiscovered(string enemyId, string skillId)
        {
            var enemyData = BestiaryManager.Instance?.GetEnemyData(enemyId);
            var skill = GameManager.Instance?.Database?.GetSkill(skillId);
            
            if (enemyData == null || skill == null) return;
            
            QueueNotification(new NotificationData
            {
                EnemyId = enemyId,
                EnemyData = enemyData,
                Message = $"New Skill Learned: {skill.DisplayName}!"
            });
        }
        #endregion
        
        #region Notification Display
        private void QueueNotification(NotificationData data)
        {
            notificationQueue.Enqueue(data);
            
            if (!isShowingNotification)
            {
                ShowNextNotification();
            }
        }
        
        private void ShowNextNotification()
        {
            if (notificationQueue.Count == 0)
            {
                isShowingNotification = false;
                return;
            }
            
            isShowingNotification = true;
            var data = notificationQueue.Dequeue();
            
            // Update UI
            if (titleLabel != null)
            {
                titleLabel.Text = data.Message;
                titleLabel.AddThemeColorOverride("font_color", discoveryColor);
            }
            
            if (enemyNameLabel != null)
            {
                enemyNameLabel.Text = data.EnemyData.DisplayName;
            }
            
            if (enemyIcon != null && data.EnemyData.BattlePortrait != null)
            {
                enemyIcon.Texture = data.EnemyData.BattlePortrait;
            }
            
            // Show panel
            if (notificationPanel != null)
                notificationPanel.Show();
            
            // Play animation
            if (animationPlayer != null && animationPlayer.HasAnimation("show"))
            {
                animationPlayer.Play("show");
            }
            else
            {
                // Fallback: simple fade in
                if (notificationPanel != null)
                {
                    notificationPanel.Modulate = new Color(1, 1, 1, 0);
                    var tween = CreateTween();
                    tween.TweenProperty(notificationPanel, "modulate:a", 1.0f, 0.3f);
                }
            }
            
            // Play sound effect
            SystemManager.Instance?.PlaySystemSE(SystemSoundEffect.UseItem);
            
            // Start timer
            displayTimer?.Start();
        }
        
        private void OnDisplayTimerTimeout()
        {
            HideNotification();
        }
        
        private void HideNotification()
        {
            // Play hide animation
            if (animationPlayer != null && animationPlayer.HasAnimation("hide"))
            {
                animationPlayer.Play("hide");
                animationPlayer.AnimationFinished += OnHideAnimationFinished;
            }
            else
            {
                // Fallback: simple fade out
                if (notificationPanel != null)
                {
                    var tween = CreateTween();
                    tween.TweenProperty(notificationPanel, "modulate:a", 0.0f, 0.3f);
                    tween.TweenCallback(Callable.From(() =>
                    {
                        notificationPanel.Hide();
                        ShowNextNotification();
                    }));
                }
            }
        }
        
        private void OnHideAnimationFinished(StringName animName)
        {
            if (animName == "hide")
            {
                animationPlayer.AnimationFinished -= OnHideAnimationFinished;
                
                if (notificationPanel != null)
                    notificationPanel.Hide();
                
                ShowNextNotification();
            }
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// Manually trigger a custom notification
        /// </summary>
        public void ShowCustomNotification(string enemyId, string message)
        {
            var enemyData = BestiaryManager.Instance?.GetEnemyData(enemyId);
            if (enemyData == null) return;
            
            QueueNotification(new NotificationData
            {
                EnemyId = enemyId,
                EnemyData = enemyData,
                Message = message
            });
        }
        
        /// <summary>
        /// Clear all pending notifications
        /// </summary>
        public void ClearQueue()
        {
            notificationQueue.Clear();
        }
        #endregion
    }
}