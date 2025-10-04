// Quests/QuestGivers.cs
using Godot;
using System;
using System.Collections.Generic;

namespace EchoesAcrossTime.Quests
{
    /// <summary>
    /// NPC that can give quests
    /// </summary>
    public partial class NPCQuestGiver : Node2D
    {
        [ExportGroup("Quest Giver Info")]
        [Export] public string NPCId { get; set; } = "";
        [Export] public string NPCName { get; set; } = "";
        [Export] public Godot.Collections.Array<QuestData> QuestsToGive { get; set; }
        
        [ExportGroup("Indicators")]
        [Export] public bool ShowQuestIndicator { get; set; } = true;
        [Export] public Texture2D QuestAvailableIcon { get; set; } // "!" icon
        [Export] public Texture2D QuestInProgressIcon { get; set; } // "?" icon
        [Export] public Texture2D QuestCompleteIcon { get; set; } // "✓" icon
        
        private Sprite2D questIndicator;
        private Area2D interactionArea;
        private bool playerInRange = false;
        
        public override void _Ready()
        {
            QuestsToGive = new Godot.Collections.Array<QuestData>();
            
            // Create interaction area
            interactionArea = new Area2D();
            interactionArea.Name = "InteractionArea";
            AddChild(interactionArea);
            
            var shape = new CollisionShape2D();
            var circle = new CircleShape2D { Radius = 64f };
            shape.Shape = circle;
            interactionArea.AddChild(shape);
            
            interactionArea.BodyEntered += OnPlayerEntered;
            interactionArea.BodyExited += OnPlayerExited;
            
            // Create quest indicator
            if (ShowQuestIndicator)
            {
                questIndicator = new Sprite2D();
                questIndicator.Position = new Vector2(0, -40);
                AddChild(questIndicator);
                UpdateQuestIndicator();
            }
            
            // Listen for quest updates
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.QuestStarted += (_) => UpdateQuestIndicator();
                QuestManager.Instance.QuestCompleted += (_) => UpdateQuestIndicator();
                QuestManager.Instance.QuestObjectiveUpdated += (_, _, _, _) => UpdateQuestIndicator();
            }
        }
        
        private void OnPlayerEntered(Node2D body)
        {
            if (body.IsInGroup("player"))
            {
                playerInRange = true;
                ShowInteractionPrompt();
            }
        }
        
        private void OnPlayerExited(Node2D body)
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
                InteractWithNPC();
                GetViewport().SetInputAsHandled();
            }
        }
        
        private void InteractWithNPC()
        {
            // Notify quest manager that we talked to this NPC
            QuestManager.Instance?.OnNPCTalked(NPCId);
            
            // Check if NPC has quests to give or turn in
            var questToComplete = GetQuestToComplete();
            if (questToComplete != null)
            {
                ShowQuestCompletionDialog(questToComplete);
                return;
            }
            
            var questToGive = GetQuestToGive();
            if (questToGive != null)
            {
                OfferQuest(questToGive);
                return;
            }
            
            // No quests - just dialogue
            ShowNormalDialogue();
        }
        
        private QuestData GetQuestToGive()
        {
            if (QuestManager.Instance == null) return null;
            
            var switches = GetCurrentSwitches();
            int chapter = GetCurrentChapter();
            
            foreach (var quest in QuestsToGive)
            {
                if (quest == null) continue;
                
                // Skip if already active or completed
                if (QuestManager.Instance.IsQuestActive(quest.QuestId)) continue;
                if (QuestManager.Instance.IsQuestCompleted(quest.QuestId)) continue;
                
                // Check availability
                if (quest.IsAvailable(chapter, switches))
                {
                    return quest;
                }
            }
            
            return null;
        }
        
        private QuestData GetQuestToComplete()
        {
            if (QuestManager.Instance == null) return null;
            
            foreach (var quest in QuestsToGive)
            {
                if (quest == null) continue;
                
                // Check if this is an active quest from this NPC
                if (!QuestManager.Instance.IsQuestActive(quest.QuestId)) continue;
                
                // Check if objectives are complete
                var progress = QuestManager.Instance.GetQuestProgress(quest.QuestId);
                if (quest.AreAllObjectivesComplete(progress))
                {
                    return quest;
                }
            }
            
            return null;
        }
        
        private void OfferQuest(QuestData quest)
        {
            var dialog = new ConfirmationDialog();
            dialog.Title = NPCName;
            dialog.DialogText = $"{quest.Description}\n\nAccept this quest?";
            
            dialog.Confirmed += () => 
            {
                QuestManager.Instance?.StartQuest(quest.QuestId);
                UpdateQuestIndicator();
            };
            
            GetTree().Root.AddChild(dialog);
            dialog.PopupCentered();
        }
        
        private void ShowQuestCompletionDialog(QuestData quest)
        {
            var dialog = new AcceptDialog();
            dialog.Title = NPCName;
            
            var vbox = new VBoxContainer();
            
            var label = new Label();
            label.Text = $"You've completed the quest!\n\n{quest.QuestName}";
            vbox.AddChild(label);
            
            // Show rewards
            if (quest.Type == QuestType.SideQuest)
            {
                var rewardsLabel = new Label();
                rewardsLabel.Text = "\nRewards:";
                rewardsLabel.AddThemeColorOverride("font_color", Colors.Gold);
                vbox.AddChild(rewardsLabel);
                
                if (quest.GoldReward > 0)
                {
                    var gold = new Label();
                    gold.Text = $"• {quest.GoldReward} Gold";
                    vbox.AddChild(gold);
                }
                
                if (quest.ExpReward > 0)
                {
                    var exp = new Label();
                    exp.Text = $"• {quest.ExpReward} EXP";
                    vbox.AddChild(exp);
                }
                
                foreach (var item in quest.ItemRewards)
                {
                    var itemData = GameManager.Instance?.Database?.GetItem(item.ItemId);
                    if (itemData != null)
                    {
                        var itemLabel = new Label();
                        itemLabel.Text = $"• {itemData.DisplayName} x{item.Quantity}";
                        vbox.AddChild(itemLabel);
                    }
                }
            }
            
            dialog.Confirmed += () => 
            {
                QuestManager.Instance?.CompleteQuest(quest.QuestId);
                UpdateQuestIndicator();
            };
            
            dialog.AddChild(vbox);
            GetTree().Root.AddChild(dialog);
            dialog.PopupCentered();
        }
        
        private void ShowNormalDialogue()
        {
            // TODO: Integrate with dialogue system
            var dialog = new AcceptDialog();
            dialog.Title = NPCName;
            dialog.DialogText = "Hello there!";
            GetTree().Root.AddChild(dialog);
            dialog.PopupCentered();
        }
        
        private void UpdateQuestIndicator()
        {
            if (questIndicator == null) return;
            
            // Check for quest to complete (highest priority)
            if (GetQuestToComplete() != null)
            {
                questIndicator.Texture = QuestCompleteIcon;
                questIndicator.Show();
                return;
            }
            
            // Check for quest to give
            if (GetQuestToGive() != null)
            {
                questIndicator.Texture = QuestAvailableIcon;
                questIndicator.Show();
                return;
            }
            
            // Check if NPC has an active quest
            bool hasActiveQuest = false;
            if (QuestManager.Instance != null)
            {
                foreach (var quest in QuestsToGive)
                {
                    if (quest != null && QuestManager.Instance.IsQuestActive(quest.QuestId))
                    {
                        hasActiveQuest = true;
                        break;
                    }
                }
            }
            
            if (hasActiveQuest)
            {
                questIndicator.Texture = QuestInProgressIcon;
                questIndicator.Show();
            }
            else
            {
                questIndicator.Hide();
            }
        }
        
        private void ShowInteractionPrompt()
        {
            // TODO: Show interaction prompt
            GD.Print($"Press E to talk to {NPCName}");
        }
        
        private void HideInteractionPrompt()
        {
            // TODO: Hide prompt
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
            return GameManager.Instance?.CurrentSave?.Variables.GetValueOrDefault("current_chapter", 1) as int? ?? 1;
        }
    }
    
    /// <summary>
    /// Readable file/document that gives a quest
    /// </summary>
    public partial class FileQuestGiver : Node2D
    {
        [ExportGroup("File Info")]
        [Export] public string FileName { get; set; } = "Mysterious Letter";
        [Export(PropertyHint.MultilineText)] public string FileContent { get; set; } = "";
        [Export] public QuestData QuestToGive { get; set; }
        
        [ExportGroup("Interaction")]
        [Export] public bool OneTimeUse { get; set; } = true;
        
        private bool hasBeenRead = false;
        private Area2D interactionArea;
        private bool playerInRange = false;
        
        public override void _Ready()
        {
            // Create interaction area
            interactionArea = new Area2D();
            AddChild(interactionArea);
            
            var shape = new CollisionShape2D();
            var rect = new RectangleShape2D { Size = new Vector2(32, 32) };
            shape.Shape = rect;
            interactionArea.AddChild(shape);
            
            interactionArea.BodyEntered += OnPlayerEntered;
            interactionArea.BodyExited += OnPlayerExited;
        }
        
        private void OnPlayerEntered(Node2D body)
        {
            if (body.IsInGroup("player"))
            {
                playerInRange = true;
                if (!hasBeenRead || !OneTimeUse)
                {
                    ShowInteractionPrompt();
                }
            }
        }
        
        private void OnPlayerExited(Node2D body)
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
                if (!hasBeenRead || !OneTimeUse)
                {
                    ReadFile();
                    GetViewport().SetInputAsHandled();
                }
            }
        }
        
        private void ReadFile()
        {
            hasBeenRead = true;
            
            var dialog = new AcceptDialog();
            dialog.Title = FileName;
            
            var vbox = new VBoxContainer();
            
            // Show file content
            var contentLabel = new Label();
            contentLabel.Text = FileContent;
            contentLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            contentLabel.CustomMinimumSize = new Vector2(400, 0);
            vbox.AddChild(contentLabel);
            
            // If quest attached, offer it
            if (QuestToGive != null && !QuestManager.Instance.IsQuestActive(QuestToGive.QuestId))
            {
                var questLabel = new Label();
                questLabel.Text = $"\n[New Quest: {QuestToGive.QuestName}]";
                questLabel.AddThemeColorOverride("font_color", Colors.Gold);
                vbox.AddChild(questLabel);
                
                dialog.Confirmed += () => 
                {
                    QuestManager.Instance?.StartQuest(QuestToGive.QuestId);
                };
            }
            
            dialog.AddChild(vbox);
            GetTree().Root.AddChild(dialog);
            dialog.PopupCentered();
            
            if (OneTimeUse)
            {
                HideInteractionPrompt();
            }
        }
        
        private void ShowInteractionPrompt()
        {
            GD.Print($"Press E to read {FileName}");
        }
        
        private void HideInteractionPrompt()
        {
            // TODO: Hide prompt
        }
    }
}