using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chat {
    public delegate void EnteredCallback(string text);

    public partial class Chat: Panel {
        public VBoxContainer container;
        public LineEdit field;

        public List<Control> messages = new();

        static public ResultCallback onFinish;
        static public EnteredCallback onEnter;

        public Chat Clear() {
            if (messages.Any()) {
                foreach (var message in messages) {
                    container.RemoveChild(message);
                }
            }
            messages.Clear();
            return this;
        }

        public Chat Message(string title, string message) {
            var label = new Label();
            label.Text = $"{title}: {message}";
            label.AutowrapMode = TextServer.AutowrapMode.Word;
            label.Modulate = new Color(0, 0, 0);
            messages.Add(label);
            container.AddChild(label);
            return this;
        }

        public Chat Hyperlink(string message, EnteredCallback clicked) {
            var button = new Button();
            button.Text = $"{message}";
            button.ButtonUp += () => clicked(message);
            messages.Add(button);
            container.AddChild(button);
            return this;
        }

        private void onTextEntered(string text) {
            field.Clear();
            field.Hide();
            onEnter?.Invoke(text);
            onEnter = null;
        }
        public Chat Enter(string placeholder, EnteredCallback onEnter) {
            field.Show();
            field.PlaceholderText = placeholder;
            Chat.onEnter = onEnter;
            return this;
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready() {
            container = GetNode<VBoxContainer>("MarginContainer/VBoxContainer/ScrollContainer/Container");
            field = GetNode<LineEdit>("MarginContainer/VBoxContainer/Field");
            field.Hide();
            field.TextSubmitted += onTextEntered;
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta) {
            
        }
    }

}