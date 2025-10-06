using Godot;
using System.Collections.Generic;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime.Combat.UI
{
    public partial class PartyStatusDisplay : Control
    {
        private List<PartyMemberUI> memberDisplays = new List<PartyMemberUI>();
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
            CreatePartyDisplays();
            ConnectMemberSignals();
        }
        
        private void ConnectMemberSignals()
        {
            var party = battleManager.GetPlayerParty();
            
            foreach (var member in party)
            {
                // Connect to HP changes
                member.Stats.HPChanged += (oldHP, newHP, maxHP) => 
                {
                    UpdateMemberHP(member, oldHP, newHP, maxHP);
                };
                
                // Connect to MP changes
                member.Stats.MPChanged += (oldMP, newMP, maxMP) => 
                {
                    UpdateMemberMP(member, oldMP, newMP, maxMP);
                };
                
                // Connect to death
                member.Stats.Death += () => 
                {
                    OnMemberDeath(member);
                };
            }
            
            GD.Print($"Connected HP/MP signals for {party.Count} party members");
        }
        
        private void UpdateMemberHP(BattleMember member, int oldHP, int newHP, int maxHP)
        {
            var memberUI = memberDisplays.Find(m => m.Member == member);
            if (memberUI == null) return;
            
            // Update bar value smoothly
            var tween = CreateTween();
            tween.TweenProperty(memberUI.HPBar, "value", newHP, 0.3);
            
            // Update text
            memberUI.HPText.Text = $"{newHP}/{maxHP}";
            
            // Calculate damage/healing
            int change = newHP - oldHP;
            
            if (change < 0)
            {
                // Took damage - flash red and show damage number
                FlashColor(memberUI.HPBar, Colors.Red);
                ShowDamageNumber(memberUI.Container, Mathf.Abs(change), Colors.Red);
                
                // Change bar color based on HP percentage
                float hpPercent = (float)newHP / maxHP;
                if (hpPercent <= 0.25f)
                {
                    memberUI.HPBar.Modulate = Colors.DarkRed; // Critical HP
                }
                else if (hpPercent <= 0.5f)
                {
                    memberUI.HPBar.Modulate = Colors.Orange; // Low HP
                }
            }
            else if (change > 0)
            {
                // Healed - flash green and show healing number
                FlashColor(memberUI.HPBar, Colors.Green);
                ShowDamageNumber(memberUI.Container, change, Colors.Green, "+");
                
                // Restore bar color if healed above threshold
                float hpPercent = (float)newHP / maxHP;
                if (hpPercent > 0.5f)
                {
                    memberUI.HPBar.Modulate = Colors.White;
                }
            }
        }
        
        private void UpdateMemberMP(BattleMember member, int oldMP, int newMP, int maxMP)
        {
            var memberUI = memberDisplays.Find(m => m.Member == member);
            if (memberUI == null) return;
            
            // Update bar value smoothly
            var tween = CreateTween();
            tween.TweenProperty(memberUI.MPBar, "value", newMP, 0.3);
            
            // Update text
            memberUI.MPText.Text = $"{newMP}/{maxMP}";
            
            // Calculate change
            int change = newMP - oldMP;
            
            if (change < 0)
            {
                // Used MP - flash blue
                FlashColor(memberUI.MPBar, Colors.Blue);
            }
            else if (change > 0)
            {
                // Restored MP - flash cyan
                FlashColor(memberUI.MPBar, Colors.Cyan);
                ShowDamageNumber(memberUI.Container, change, Colors.Cyan, "+");
            }
        }
        
        private void OnMemberDeath(BattleMember member)
        {
            var memberUI = memberDisplays.Find(m => m.Member == member);
            if (memberUI == null) return;
            
            // Gray out the display
            memberUI.Container.Modulate = new Color(0.5f, 0.5f, 0.5f);
            
            // Show death indicator
            var deathLabel = new Label();
            deathLabel.Text = "DEAD";
            deathLabel.AddThemeFontSizeOverride("font_size", 24);
            deathLabel.Modulate = Colors.Red;
            deathLabel.HorizontalAlignment = HorizontalAlignment.Center;
            memberUI.Container.AddChild(deathLabel);
            
            GD.Print($"{member.Stats.CharacterName} has been defeated!");
        }
        
        private void FlashColor(Control control, Color color)
        {
            var originalColor = control.Modulate;
            
            var tween = CreateTween();
            tween.TweenProperty(control, "modulate", color, 0.1);
            tween.TweenProperty(control, "modulate", originalColor, 0.3);
        }
        
        private void ShowDamageNumber(Control parent, int amount, Color color, string prefix = "")
        {
            var damageLabel = new Label();
            damageLabel.Text = $"{prefix}{amount}";
            damageLabel.AddThemeFontSizeOverride("font_size", 32);
            damageLabel.Modulate = color;
            damageLabel.Position = new Vector2(150, 30); // Center of member display
            damageLabel.ZIndex = 100;
            parent.AddChild(damageLabel);
            
            // Animate floating upward and fading
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(damageLabel, "position:y", damageLabel.Position.Y - 50, 1.0);
            tween.TweenProperty(damageLabel, "modulate:a", 0.0, 1.0);
            tween.Chain().TweenCallback(Callable.From(() => damageLabel.QueueFree()));
        }
        
        private void CreatePartyDisplays()
        {
            foreach (var display in memberDisplays)
            {
                display.Container.QueueFree();
            }
            memberDisplays.Clear();
            
            var party = battleManager.GetPlayerParty();
            
            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 10);
            AddChild(vbox);
            
            foreach (var member in party)
            {
                var memberUI = CreateMemberDisplay(member);
                memberDisplays.Add(memberUI);
                vbox.AddChild(memberUI.Container);
            }
        }
        
        private PartyMemberUI CreateMemberDisplay(BattleMember member)
        {
            var container = new PanelContainer();
            container.CustomMinimumSize = new Vector2(300, 100);
            
            var margin = new MarginContainer();
            margin.AddThemeConstantOverride("margin_left", 10);
            margin.AddThemeConstantOverride("margin_right", 10);
            margin.AddThemeConstantOverride("margin_top", 10);
            margin.AddThemeConstantOverride("margin_bottom", 10);
            container.AddChild(margin);
            
            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 5);
            margin.AddChild(vbox);
            
            var nameLabel = new Label();
            nameLabel.Text = member.Stats.CharacterName;
            nameLabel.AddThemeFontSizeOverride("font_size", 20);
            vbox.AddChild(nameLabel);
            
            // HP Bar
            var hpContainer = new HBoxContainer();
            vbox.AddChild(hpContainer);
            
            var hpLabel = new Label();
            hpLabel.Text = "HP:";
            hpLabel.CustomMinimumSize = new Vector2(40, 0);
            hpContainer.AddChild(hpLabel);
            
            var hpBar = new ProgressBar();
            hpBar.MinValue = 0;
            hpBar.MaxValue = member.Stats.MaxHP;
            hpBar.Value = member.Stats.CurrentHP;
            hpBar.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            hpBar.CustomMinimumSize = new Vector2(0, 20);
            hpBar.ShowPercentage = false;
            hpContainer.AddChild(hpBar);
            
            var hpText = new Label();
            hpText.Text = $"{member.Stats.CurrentHP}/{member.Stats.MaxHP}";
            hpText.CustomMinimumSize = new Vector2(80, 0);
            hpText.HorizontalAlignment = HorizontalAlignment.Right;
            hpContainer.AddChild(hpText);
            
            // MP Bar
            var mpContainer = new HBoxContainer();
            vbox.AddChild(mpContainer);
            
            var mpLabel = new Label();
            mpLabel.Text = "MP:";
            mpLabel.CustomMinimumSize = new Vector2(40, 0);
            mpContainer.AddChild(mpLabel);
            
            var mpBar = new ProgressBar();
            mpBar.MinValue = 0;
            mpBar.MaxValue = member.Stats.MaxMP;
            mpBar.Value = member.Stats.CurrentMP;
            mpBar.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            mpBar.CustomMinimumSize = new Vector2(0, 20);
            mpBar.ShowPercentage = false;
            mpContainer.AddChild(mpBar);
            
            var mpText = new Label();
            mpText.Text = $"{member.Stats.CurrentMP}/{member.Stats.MaxMP}";
            mpText.CustomMinimumSize = new Vector2(80, 0);
            mpText.HorizontalAlignment = HorizontalAlignment.Right;
            mpContainer.AddChild(mpText);
            
            return new PartyMemberUI
            {
                Member = member,
                Container = container,
                NameLabel = nameLabel,
                HPBar = hpBar,
                HPText = hpText,
                MPBar = mpBar,
                MPText = mpText
            };
        }
        
        private class PartyMemberUI
        {
            public BattleMember Member;
            public PanelContainer Container;
            public Label NameLabel;
            public ProgressBar HPBar;
            public Label HPText;
            public ProgressBar MPBar;
            public Label MPText;
        }
    }
}