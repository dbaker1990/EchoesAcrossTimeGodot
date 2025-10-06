// Inn/InnKeeperData.cs
using Godot;
using System;
using EchoesAcrossTime.Events;

namespace EchoesAcrossTime.Inn
{
    /// <summary>
    /// Data resource for inn keeper configuration
    /// </summary>
    [GlobalClass]
    public partial class InnKeeperData : Resource
    {
        [Export] public string InnName { get; set; } = "Cozy Inn";
        [Export] public string InnKeeperId { get; set; } = "innkeeper_01";
        [Export] public int RestCost { get; set; } = 50;
        
        [Export(PropertyHint.MultilineText)] 
        public string WelcomeMessage { get; set; } = "Welcome to the inn! A good night's rest costs {cost} gold.";
        
        [Export(PropertyHint.MultilineText)]
        public string NotEnoughMoneyMessage { get; set; } = "I'm sorry, but you don't have enough gold...";
        
        [Export(PropertyHint.MultilineText)]
        public string AfterRestMessage { get; set; } = "Have a good rest!";
        
        [Export] public MessageBoxPosition DialoguePosition { get; set; } = MessageBoxPosition.Bottom;
        
        [Export] public float FadeDuration { get; set; } = 0.5f;
        [Export] public float RestDuration { get; set; } = 3.0f;
        
        /// <summary>
        /// Get welcome message with cost formatted in
        /// </summary>
        public string GetFormattedWelcome()
        {
            return WelcomeMessage.Replace("{cost}", RestCost.ToString());
        }
    }
}