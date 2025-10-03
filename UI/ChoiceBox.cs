// UI/ChoiceBox.cs
using Godot;
using System;
using System.Collections.Generic;

namespace EchoesAcrossTime.UI
{
    public partial class ChoiceBox : Control
    {
        [Export] public VBoxContainer ChoiceContainer { get; set; }
        [Export] public PackedScene ChoiceButtonScene { get; set; }
        [Export] public AudioStream SelectSound { get; set; }
        [Export] public AudioStream ConfirmSound { get; set; }
        
        private List<Button> choiceButtons = new List<Button>();
        private int selectedIndex = 0;
        
        [Signal]
        public delegate void ChoiceSelectedEventHandler(int choiceIndex);
        
        public override void _Ready()
        {
            Visible = false;
        }
        
        public void ShowChoices(List<string> choices, int defaultChoice = 0)
        {
            ClearChoices();
            
            for (int i = 0; i < choices.Count; i++)
            {
                var button = CreateChoiceButton(choices[i], i);
                choiceButtons.Add(button);
                ChoiceContainer.AddChild(button);
            }
            
            selectedIndex = defaultChoice;
            if (choiceButtons.Count > 0)
            {
                choiceButtons[selectedIndex].GrabFocus();
            }
            
            Visible = true;
        }
        
        private Button CreateChoiceButton(string text, int index)
        {
            Button button;
            
            if (ChoiceButtonScene != null)
            {
                button = ChoiceButtonScene.Instantiate<Button>();
            }
            else
            {
                button = new Button();
            }
            
            button.Text = text;
            button.Pressed += () => OnChoiceSelected(index);
            
            return button;
        }
        
        private void OnChoiceSelected(int index)
        {
            if (ConfirmSound != null)
            {
                AudioManager.Instance?.PlaySoundEffect(ConfirmSound);
            }
            
            EmitSignal(SignalName.ChoiceSelected, index);
            HideChoiceBox();  // Updated call
        }
        
        private void ClearChoices()
        {
            foreach (var button in choiceButtons)
            {
                button.QueueFree();
            }
            choiceButtons.Clear();
        }
        
        public void HideChoiceBox()  // Renamed from Hide
        {
            Visible = false;
            ClearChoices();
        }
    }
}