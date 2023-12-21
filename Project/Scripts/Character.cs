using Godot;
using System;

// LEGACY - использовалось в демо
// TODO Перенести в папку Entities
namespace Character {
    // public delegate void ThoughtsCallback(Character character);

    public partial class Character: Node3D {
        public Node3D thought;
        public string text;

        public void ShowThoughts(string text) {
            RemoveThoughts();
            this.text = text;
            thought = ResourceLoader.Load<PackedScene>("res://Prefabs/thoughts.tscn").Instantiate<Node3D>();
            thought.GetNode<Label>("SubViewport/Label").Text = text;
            AddChild(thought);
        }

        public void RemoveThoughts() {
            if (!HasThoughts()) return;
            RemoveChild(thought);
            thought = null;
        }

        public bool HasThoughts() {
            return thought != null;
        }
        
        // Called when the node enters the scene tree for the first time.
        public override void _Ready() {
            // ShowThoughts("Скука");
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta) {
            // Sprite3D thoughts = GetNode<Sprite3D>("Thoughts/Sprite3D");
            // thoughts.GetItemRect().HasPoint(Input.)
        }
    }
}