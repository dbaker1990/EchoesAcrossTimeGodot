// Events/DialogueData.cs
using Godot;
using System.Collections.Generic;

namespace EchoesAcrossTime.Events
{
    /// <summary>
    /// Stores dialogue text in a structured table format
    /// </summary>
    [GlobalClass]
    public partial class DialogueData : Resource
    {
        [Export] public string DialogueId { get; set; } = "dialogue_001";
        [Export] public string SpeakerName { get; set; } = "";
        [Export] public string PortraitPath { get; set; } = "";
        [Export(PropertyHint.MultilineText)] public string Text { get; set; } = "";
        [Export] public string VoiceClipPath { get; set; } = "";
        [Export] public float DisplayDuration { get; set; } = -1f; // -1 = wait for input
        
        [ExportGroup("Positioning")]
        [Export] public MessageBoxPosition Position { get; set; } = MessageBoxPosition.Bottom;
        [Export] public bool ShowSpeakerName { get; set; } = true;
        [Export] public bool ShowPortrait { get; set; } = true;
        
        [ExportGroup("Animation")]
        [Export] public float TextSpeed { get; set; } = 0.05f; // Seconds per character
        [Export] public bool UseTypewriterEffect { get; set; } = true;
    }

    public enum MessageBoxPosition
    {
        Top,
        Middle,
        Bottom,
        AboveCharacter
    }

    /// <summary>
    /// Collection of dialogue entries - like a conversation
    /// </summary>
    [GlobalClass]
    public partial class DialogueTable : Resource
    {
        [Export] public string TableId { get; set; } = "dialogue_table_001";
        [Export] public Godot.Collections.Array<DialogueData> Entries { get; set; }
        
        public DialogueTable()
        {
            Entries = new Godot.Collections.Array<DialogueData>();
        }
        
        public DialogueData GetEntry(int index)
        {
            if (index >= 0 && index < Entries.Count)
                return Entries[index];
            return null;
        }
        
        public int GetEntryCount() => Entries.Count;
    }
}