using Godot;
using System;
using System.Collections.Generic;

namespace ChoiceMiniGame {
    public delegate Button FactoryCallback();
    public delegate void InteractCallback(Button button);
    
    public partial class ChoiceMiniGame: Panel {
        static Random random = new Random();
        static public ChoiceMiniGame active;

        public List<FactoryCallback> factories = new List<FactoryCallback>();
        public Label label;
        public VBoxContainer container;

        public string title = "Title";
        public bool isCorrect = false;
        public ResultCallback onFinish;

        public void Finish() {
            GD.Print(isCorrect? "You win": "You lose");
            onFinish(isCorrect);
        }

        public void AddChoice(string label, string text, InteractCallback callback) {
            factories.Add(() => {
                Button button = (Button)ResourceLoader.Load<PackedScene>("res://Activities/Choice/choice_button.tscn").Instantiate();
                button.Text = text;
                button.Pressed += () => callback(button);
                return button;
            });
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready() {
            active = this;
            container = GetChild<VBoxContainer>(0);
            label = (Label)GetNode("VBoxContainer/Label");
            foreach (var factory in factories) {
                container.AddChild(factory());
            }
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta) {
            active = this;
            label.Text = title;
        }
    }
}