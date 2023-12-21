using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Narration {
    public partial class Narration: Control {
        public Control dialogue;

        public void Clear() {
            if (!HasDialogue()) return;
            RemoveChild(dialogue);
            dialogue = null;
        }

        public bool HasDialogue() {
            return dialogue != null;
        }

        public void Monologue(string text) {
            Clear();
            dialogue = ResourceLoader.Load<PackedScene>("res://Activities/Narration/narrator.tscn").Instantiate<PanelContainer>();
            dialogue.GetNode<RichTextLabel>("Container/Text").Text = text;
            dialogue.GetNode<MarginContainer>("Container/DialogueName").Hide();
            AddChild(dialogue);
        }

        public void Titled(string title, string text) {
            Clear();
            dialogue = ResourceLoader.Load<PackedScene>("res://Activities/Narration/narrator.tscn").Instantiate<PanelContainer>();
            dialogue.GetNode<RichTextLabel>("Container/Text").Text = text;
            dialogue.GetNode<Label>("Container/DialogueName/Name").Text = title;

            // dialogue = ResourceLoader.Load<PackedScene>("res://Activities/Narration/titled.tscn").Instantiate<PanelContainer>();
            // dialogue.GetNode<RichTextLabel>("Container/VBoxContainer/Title").Text = title;
            // dialogue.GetNode<RichTextLabel>("Container/VBoxContainer/Text").Text = text;
            AddChild(dialogue);
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready() {

        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta) {

        }
    }
}