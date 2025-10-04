using Godot;
using System.Collections.Generic;

namespace EchoesAcrossTime.Quests
{
    public partial class QuestNotificationManager : Control
    {
        private Queue<string> notifications = new Queue<string>();
        private bool isShowing = false;
        
        public override void _Ready()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.QuestStarted += (id) => ShowNotification($"New Quest: {id}");
                QuestManager.Instance.QuestCompleted += (id) => ShowNotification($"Quest Complete: {id}");
            }
            
            GD.Print("QuestNotificationManager initialized");
        }
        
        private void ShowNotification(string message)
        {
            notifications.Enqueue(message);
            if (!isShowing)
                DisplayNext();
        }
        
        private async void DisplayNext()
        {
            if (notifications.Count == 0)
            {
                isShowing = false;
                return;
            }
            
            isShowing = true;
            string message = notifications.Dequeue();
            
            var label = new Label();
            label.Text = message;
            label.AddThemeFontSizeOverride("font_size", 24);
            AddChild(label);
            
            await ToSignal(GetTree().CreateTimer(3.0f), SceneTreeTimer.SignalName.Timeout);
            
            label.QueueFree();
            DisplayNext();
        }
    }
}