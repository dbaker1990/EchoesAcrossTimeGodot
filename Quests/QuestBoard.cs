// Quests/QuestBoard.cs
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EchoesAcrossTime.Quests
{
    /// <summary>
    /// Quest board in the game world where players can pick up quests
    /// </summary>
    public partial class QuestBoard : Node2D
    {
        [ExportGroup("Quest Board Settings")]
        [Export] public string BoardName { get; set; } = "Guild Quest Board";
        [Export] public Godot.Collections.Array<QuestData> AvailableQuests { get; set; }
        [Export] public bool ShowNewQuestIndicator { get; set; } = true;
        
        [ExportGroup("Interaction")]
        [Export] public float InteractionRange { get; set; } = 64f;
        
        private Area2D interactionArea;
        private Sprite2D sprite;
        private AnimatedSprite2D newQuestIndicator;
        private bool playerInRange = false;
        
        public override void _Ready()
        {
            AvailableQuests = new Godot.Collections.Array<QuestData>();
            
            // Create sprite (placeholder)
            sprite = new Sprite2D();
            AddChild(sprite);
            // TODO: Set actual quest board sprite
            
            // Create interaction area
            interactionArea = new Area2D();
            interactionArea.Name = "InteractionArea";
            AddChild(interactionArea);
            
            var collisionShape = new CollisionShape2D();
            var circleShape = new CircleShape2D();
            circleShape.Radius = InteractionRange;
            collisionShape.Shape = circleShape;
            interactionArea.AddChild(collisionShape);
            
            // Connect area signals
            interactionArea.BodyEntered += OnBodyEntered;
            interactionArea.BodyExited += OnBodyExited;
            
            // Create new quest indicator
            if (ShowNewQuestIndicator)
            {
                CreateNewQuestIndicator();
            }
            
            UpdateNewQuestIndicator();
        }
        
        private void CreateNewQuestIndicator()
        {
            newQuestIndicator = new AnimatedSprite2D();
            newQuestIndicator.Position = new Vector2(0, -40); // Above the board
            AddChild(newQuestIndicator);
            
            // Create simple animation (you can replace with actual animated sprite)
            var frames = new SpriteFrames();
            frames.AddAnimation("default");
            // TODO: Add actual exclamation mark frames
            newQuestIndicator.SpriteFrames = frames;
            newQuestIndicator.Play("default");
            newQuestIndicator.Hide();
        }
        
        private void OnBodyEntered(Node2D body)
        {
            if (body.IsInGroup("player"))
            {
                playerInRange = true;
                ShowInteractionPrompt();
            }
        }
        
        private void OnBodyExited(Node2D body)
        {
            if (body.IsInGroup("player"))
            {
                playerInRange = false;
                HideInteractionPrompt();
            }
        }
        
        public override void _Input(InputEvent @event)
        {
            if (playerInRange && @event.IsActionPressed("ui_accept"))
            {
                OpenQuestBoard();
                GetViewport().SetInputAsHandled();
            }
        }
        
        private void OpenQuestBoard()
        {
            var ui = GetTree().Root.GetNode<QuestBoardUI>("%QuestBoardUI");
            if (ui != null)
            {
                ui.OpenBoard(this);
            }
            else
            {
                // Fallback: create UI on the fly
                ShowQuestBoardDialog();
            }
        }
        
        private void ShowQuestBoardDialog()
        {
            var dialog = new AcceptDialog();
            dialog.Title = BoardName;
            
            var vbox = new VBoxContainer();
            
            var availableQuests = GetAvailableQuests();
            
            if (availableQuests.Count == 0)
            {
                var label = new Label();
                label.Text = "No quests available at this time.";
                vbox.AddChild(label);
            }
            else
            {
                foreach (var quest in availableQuests)
                {
                    var questButton = new Button();
                    questButton.Text = quest.QuestName;
                    questButton.Pressed += () => AcceptQuest(quest);
                    vbox.AddChild(questButton);
                }
            }
            
            dialog.AddChild(vbox);
            GetTree().Root.AddChild(dialog);
            dialog.PopupCentered();
        }
        
        public void AcceptQuest(QuestData quest)
        {
            if (QuestManager.Instance != null)
            {
                bool success = QuestManager.Instance.StartQuest(quest.QuestId);
                if (success)
                {
                    ShowQuestAcceptedMessage(quest);
                    UpdateNewQuestIndicator();
                }
            }
        }
        
        private void ShowQuestAcceptedMessage(QuestData quest)
        {
            // TODO: Show fancy quest accepted popup
            GD.Print($"Accepted quest: {quest.QuestName}");
        }
        
        public List<QuestData> GetAvailableQuests()
        {
            if (QuestManager.Instance == null)
                return new List<QuestData>();
            
            var available = new List<QuestData>();
            
            foreach (var quest in AvailableQuests)
            {
                if (quest == null) continue;
                
                // Skip if already active or completed
                if (QuestManager.Instance.IsQuestActive(quest.QuestId))
                    continue;
                if (QuestManager.Instance.IsQuestCompleted(quest.QuestId))
                    continue;
                
                // Check if quest is available (story flags, chapter, etc.)
                var switches = GetCurrentSwitches();
                int currentChapter = GetCurrentChapter();
                
                if (quest.IsAvailable(currentChapter, switches))
                {
                    available.Add(quest);
                }
            }
            
            return available;
        }
        
        private void UpdateNewQuestIndicator()
        {
            if (newQuestIndicator == null) return;
            
            var available = GetAvailableQuests();
            newQuestIndicator.Visible = available.Count > 0;
        }
        
        private void ShowInteractionPrompt()
        {
            // TODO: Show "Press E to interact" prompt
            GD.Print($"Press E to view {BoardName}");
        }
        
        private void HideInteractionPrompt()
        {
            // TODO: Hide interaction prompt
        }
        
        private Dictionary<string, bool> GetCurrentSwitches()
        {
            var switches = new Dictionary<string, bool>();
            if (GameManager.Instance?.CurrentSave != null)
            {
                foreach (var kvp in GameManager.Instance.CurrentSave.Switches)
                {
                    switches[kvp.Key] = kvp.Value;
                }
            }
            return switches;
        }
        
        private int GetCurrentChapter()
        {
            // Get from save data or game manager
            return GameManager.Instance?.CurrentSave?.Variables.GetValueOrDefault("current_chapter", 1) as int? ?? 1;
        }
    }
    
    /// <summary>
    /// Quest board UI (optional - for more detailed interface)
    /// </summary>
    public partial class QuestBoardUI : Control
    {
        [Export] private VBoxContainer questListContainer;
        [Export] private Panel questPreviewPanel;
        [Export] private Label questTitleLabel;
        [Export] private Label questDescriptionLabel;
        [Export] private VBoxContainer objectivesContainer;
        [Export] private VBoxContainer rewardsContainer;
        [Export] private Button acceptButton;
        [Export] private Button closeButton;
        
        private QuestBoard currentBoard;
        private QuestData selectedQuest;
        
        public override void _Ready()
        {
            acceptButton?.Connect("pressed", new Callable(this, nameof(OnAcceptPressed)));
            closeButton?.Connect("pressed", new Callable(this, nameof(OnClosePressed)));
            Hide();
        }
        
        public void OpenBoard(QuestBoard board)
        {
            currentBoard = board;
            RefreshQuestList();
            Show();
        }
        
        private void RefreshQuestList()
        {
            ClearContainer(questListContainer);
            
            if (currentBoard == null) return;
            
            var quests = currentBoard.GetAvailableQuests();
            
            if (quests.Count == 0)
            {
                var label = new Label();
                label.Text = "No quests available";
                questListContainer.AddChild(label);
                return;
            }
            
            foreach (var quest in quests)
            {
                var button = new Button();
                button.Text = quest.QuestName;
                button.Pressed += () => SelectQuest(quest);
                questListContainer.AddChild(button);
            }
        }
        
        private void SelectQuest(QuestData quest)
        {
            selectedQuest = quest;
            DisplayQuestPreview(quest);
        }
        
        private void DisplayQuestPreview(QuestData quest)
        {
            if (questPreviewPanel == null) return;
            
            questPreviewPanel.Show();
            questTitleLabel.Text = quest.QuestName;
            questDescriptionLabel.Text = quest.Description;
            
            // Show objectives
            ClearContainer(objectivesContainer);
            foreach (var objective in quest.Objectives)
            {
                var label = new Label();
                label.Text = $"○ {objective.Description}";
                if (objective.RequiredCount > 1)
                {
                    label.Text += $" (0/{objective.RequiredCount})";
                }
                objectivesContainer.AddChild(label);
            }
            
            // Show rewards
            ClearContainer(rewardsContainer);
            if (quest.Type == QuestType.SideQuest)
            {
                var rewardsTitle = new Label();
                rewardsTitle.Text = "Rewards:";
                rewardsTitle.AddThemeColorOverride("font_color", Colors.Gold);
                rewardsContainer.AddChild(rewardsTitle);
                
                if (quest.GoldReward > 0)
                {
                    var label = new Label();
                    label.Text = $"• {quest.GoldReward} Gold";
                    rewardsContainer.AddChild(label);
                }
                
                if (quest.ExpReward > 0)
                {
                    var label = new Label();
                    label.Text = $"• {quest.ExpReward} EXP";
                    rewardsContainer.AddChild(label);
                }
                
                foreach (var itemReward in quest.ItemRewards)
                {
                    var item = GameManager.Instance?.Database?.GetItem(itemReward.ItemId);
                    if (item != null)
                    {
                        var label = new Label();
                        label.Text = $"• {item.DisplayName} x{itemReward.Quantity}";
                        rewardsContainer.AddChild(label);
                    }
                }
            }
        }
        
        private void OnAcceptPressed()
        {
            if (selectedQuest != null && currentBoard != null)
            {
                currentBoard.AcceptQuest(selectedQuest);
                RefreshQuestList();
                questPreviewPanel?.Hide();
            }
        }
        
        private void OnClosePressed()
        {
            Hide();
        }
        
        private void ClearContainer(VBoxContainer container)
        {
            if (container == null) return;
            foreach (Node child in container.GetChildren())
            {
                child.QueueFree();
            }
        }
    }
}