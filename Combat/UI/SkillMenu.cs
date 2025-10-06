using Godot;
using System.Linq;
using EchoesAcrossTime.Combat;

namespace EchoesAcrossTime.Combat.UI
{
    public partial class SkillMenu : Control
    {
        public SkillData SelectedSkill { get; private set; }
        public bool WasCancelled { get; private set; }
        
        private BattleMember currentActor;
        private ItemList skillList;
        private Label skillNameLabel;
        private Label skillDescriptionLabel;
        private Label skillCostLabel;
        private Button confirmButton;
        private Button cancelButton;
        
        public override void _Ready()
        {
            BuildMenu();
            Hide();
        }
        
        private void BuildMenu()
        {
            var panel = new PanelContainer();
            panel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
            panel.CustomMinimumSize = new Vector2(600, 400);
            AddChild(panel);
            
            var margin = new MarginContainer();
            margin.AddThemeConstantOverride("margin_left", 20);
            margin.AddThemeConstantOverride("margin_top", 20);
            panel.AddChild(margin);
            
            var vbox = new VBoxContainer();
            margin.AddChild(vbox);
            
            var title = new Label();
            title.Text = "SELECT SKILL";
            title.AddThemeFontSizeOverride("font_size", 24);
            vbox.AddChild(title);
            
            skillList = new ItemList();
            skillList.CustomMinimumSize = new Vector2(0, 200);
            skillList.ItemSelected += OnSkillListItemSelected;
            vbox.AddChild(skillList);
            
            skillNameLabel = new Label();
            vbox.AddChild(skillNameLabel);
            
            skillDescriptionLabel = new Label();
            vbox.AddChild(skillDescriptionLabel);
            
            skillCostLabel = new Label();
            vbox.AddChild(skillCostLabel);
            
            var buttonBox = new HBoxContainer();
            vbox.AddChild(buttonBox);
            
            confirmButton = new Button();
            confirmButton.Text = "Confirm";
            confirmButton.Pressed += OnConfirmPressed;
            buttonBox.AddChild(confirmButton);
            
            cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Pressed += OnCancelPressed;
            buttonBox.AddChild(cancelButton);
        }
        
        public void ShowMenu(BattleMember actor)
        {
            currentActor = actor;
            WasCancelled = false;
            SelectedSkill = null;
            PopulateSkills();
            Show();
            
            if (skillList.ItemCount > 0)
            {
                skillList.Select(0);
                OnSkillListItemSelected(0);
            }
        }
        
        private void PopulateSkills()
        {
            skillList.Clear();
            
            if (currentActor == null) return;
            
            // FIXED: Use GetEquippedSkills() instead of KnownSkills
            var skills = currentActor.Stats.Skills.GetEquippedSkills();
            
            foreach (var skill in skills)
            {
                string displayText = $"{skill.DisplayName} ({skill.MPCost} MP)";
                // FIXED: CurrentMP is directly on CharacterStats, not in BattleStats
                bool canAfford = currentActor.Stats.CurrentMP >= skill.MPCost;
                
                skillList.AddItem(displayText);
                int index = skillList.ItemCount - 1;
                skillList.SetItemMetadata(index, skill);
                
                if (!canAfford)
                {
                    skillList.SetItemDisabled(index, true);
                }
            }
        }
        
        private void OnSkillListItemSelected(long index)
        {
            var skill = skillList.GetItemMetadata((int)index).As<SkillData>();
            if (skill == null) return;
            
            skillNameLabel.Text = skill.DisplayName;
            skillDescriptionLabel.Text = skill.Description;
            skillCostLabel.Text = $"MP Cost: {skill.MPCost} | Power: {skill.BasePower}";
        }
        
        private void OnConfirmPressed()
        {
            if (skillList.GetSelectedItems().Length == 0) return;
            
            int selectedIndex = skillList.GetSelectedItems()[0];
            SelectedSkill = skillList.GetItemMetadata(selectedIndex).As<SkillData>();
            WasCancelled = false;
            Hide();
        }
        
        private void OnCancelPressed()
        {
            WasCancelled = true;
            SelectedSkill = null;
            Hide();
        }
    }
}