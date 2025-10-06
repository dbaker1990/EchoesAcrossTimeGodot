using Godot;
using System.Collections.Generic;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime.Combat.UI
{
    public partial class EnemyStatusDisplay : Control
    {
        private List<EnemyUI> enemyDisplays = new List<EnemyUI>();
        private BattleManager battleManager;
        
        public override void _Ready()
        {
            battleManager = GetNode<BattleManager>("/root/BattleScene/BattleManager");
            
            if (battleManager != null)
            {
                battleManager.BattleStarted += OnBattleStarted;
            }
        }
        
        private void OnBattleStarted()
        {
            CreateEnemyDisplays();
            ConnectEnemySignals();
        }
        
        private void ConnectEnemySignals()
        {
            var enemies = battleManager.GetEnemyParty();
            
            foreach (var enemy in enemies)
            {
                // Connect to HP changes
                enemy.Stats.HPChanged += (oldHP, newHP, maxHP) => 
                {
                    UpdateEnemyHP(enemy, oldHP, newHP, maxHP);
                };
                
                // Connect to death
                enemy.Stats.Death += () => 
                {
                    OnEnemyDeath(enemy);
                };
            }
            
            GD.Print($"Connected HP signals for {enemies.Count} enemies");
        }
        
        private void UpdateEnemyHP(BattleMember enemy, int oldHP, int newHP, int maxHP)
        {
            var enemyUI = enemyDisplays.Find(e => e.Enemy == enemy);
            if (enemyUI == null) return;
            
            // Update bar smoothly
            var tween = CreateTween();
            tween.TweenProperty(enemyUI.HPBar, "value", newHP, 0.3);
            
            // Calculate damage
            int damage = oldHP - newHP;
            
            if (damage > 0)
            {
                // Took damage - show damage number
                ShowDamageNumber(enemyUI.Container, damage, Colors.White);
                
                // Flash the enemy
                FlashEnemy(enemyUI);
            }
            else if (damage < 0)
            {
                // Healed
                ShowDamageNumber(enemyUI.Container, Mathf.Abs(damage), Colors.Green, "+");
            }
            
            // Update knockdown indicator
            if (enemy.IsKnockedDown)
            {
                enemyUI.KnockdownLabel.Visible = true;
            }
            else
            {
                enemyUI.KnockdownLabel.Visible = false;
            }
        }
        
        private void OnEnemyDeath(BattleMember enemy)
        {
            var enemyUI = enemyDisplays.Find(e => e.Enemy == enemy);
            if (enemyUI == null) return;
            
            // Death animation - fade out and fall
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(enemyUI.Container, "modulate:a", 0.0, 0.5);
            tween.TweenProperty(enemyUI.Container, "position:y", 
                enemyUI.Container.Position.Y + 50, 0.5);
            tween.Chain().TweenCallback(Callable.From(() => 
            {
                enemyUI.Container.QueueFree();
                enemyDisplays.Remove(enemyUI);
            }));
            
            GD.Print($"{enemy.Stats.CharacterName} defeated!");
        }
        
        private void FlashEnemy(EnemyUI enemyUI)
        {
            var tween = CreateTween();
            tween.TweenProperty(enemyUI.Container, "modulate", Colors.Red, 0.1);
            tween.TweenProperty(enemyUI.Container, "modulate", Colors.White, 0.3);
        }
        
        private void ShowDamageNumber(Control parent, int amount, Color color, string prefix = "")
        {
            var damageLabel = new Label();
            damageLabel.Text = $"{prefix}{amount}";
            damageLabel.AddThemeFontSizeOverride("font_size", 48);
            damageLabel.Modulate = color;
            damageLabel.Position = new Vector2(50, -20);
            damageLabel.ZIndex = 100;
            
            // Add outline for visibility
            damageLabel.AddThemeColorOverride("font_outline_color", Colors.Black);
            damageLabel.AddThemeConstantOverride("outline_size", 3);
            
            parent.AddChild(damageLabel);
            
            // Animate
            var tween = CreateTween();
            tween.SetParallel(true);
            
            // Float up
            tween.TweenProperty(damageLabel, "position:y", damageLabel.Position.Y - 80, 1.2);
            
            // Scale punch effect
            tween.TweenProperty(damageLabel, "scale", Vector2.One * 1.5f, 0.1);
            tween.TweenProperty(damageLabel, "scale", Vector2.One, 0.2).SetDelay(0.1);
            
            // Fade out
            tween.TweenProperty(damageLabel, "modulate:a", 0.0, 0.5).SetDelay(0.7);
            
            tween.Chain().TweenCallback(Callable.From(() => damageLabel.QueueFree()));
        }
        
        private void CreateEnemyDisplays()
        {
            foreach (var display in enemyDisplays)
            {
                display.Container.QueueFree();
            }
            enemyDisplays.Clear();
            
            var enemies = battleManager.GetEnemyParty();
            
            int index = 0;
            foreach (var enemy in enemies)
            {
                var enemyUI = CreateEnemyDisplay(enemy, index);
                enemyDisplays.Add(enemyUI);
                AddChild(enemyUI.Container);
                index++;
            }
        }
        
        private EnemyUI CreateEnemyDisplay(BattleMember enemy, int index)
        {
            var container = new Control();
            container.Position = new Vector2(600 + (index * 200), 200);
            container.CustomMinimumSize = new Vector2(150, 80);
            
            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 5);
            container.AddChild(vbox);
            
            var nameLabel = new Label();
            nameLabel.Text = enemy.Stats.CharacterName;
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            vbox.AddChild(nameLabel);
            
            var hpBar = new ProgressBar();
            hpBar.MinValue = 0;
            hpBar.MaxValue = enemy.Stats.MaxHP;
            hpBar.Value = enemy.Stats.CurrentHP;
            hpBar.CustomMinimumSize = new Vector2(150, 15);
            hpBar.ShowPercentage = false;
            vbox.AddChild(hpBar);
            
            var knockdownLabel = new Label();
            knockdownLabel.Text = "DOWN!";
            knockdownLabel.AddThemeFontSizeOverride("font_size", 18);
            knockdownLabel.Modulate = Colors.Yellow;
            knockdownLabel.Visible = false;
            knockdownLabel.HorizontalAlignment = HorizontalAlignment.Center;
            vbox.AddChild(knockdownLabel);
            
            return new EnemyUI
            {
                Enemy = enemy,
                Container = container,
                NameLabel = nameLabel,
                HPBar = hpBar,
                KnockdownLabel = knockdownLabel
            };
        }
        
        private class EnemyUI
        {
            public BattleMember Enemy;
            public Control Container;
            public Label NameLabel;
            public ProgressBar HPBar;
            public Label KnockdownLabel;
        }
    }
}