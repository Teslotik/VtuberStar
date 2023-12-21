using Godot;
using System;
using System.Collections.Generic;

namespace SequencingMiniGame {
    public partial class SequencingMiniGame: Panel {
        static Random random = new Random();
        static public SequencingMiniGame active;

        public HBoxContainer container;

        public ResultCallback onFinish;

        public int count = 5;
        public int next = 0;

        public bool IsNext(int index) {
            return index == next;
        }

        public bool IsWin() {
            return next >= count;
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready() {
            active = this;
            container = (HBoxContainer)GetNode("HBoxContainer");
            for (int i = 0; i < count; ++i) {
                SequentButton button = (SequentButton)ResourceLoader.Load<PackedScene>("res://Activities/Sequencing/sequent_button.tscn").Instantiate();
                button.index = i;
                button.Text = $"{button.index + 1}";
                button.Pressed += () => {
                    if (!IsNext(button.index)) onFinish?.Invoke(false);
                    button.QueueFree();
                    next++;
                    if (IsWin()) onFinish?.Invoke(true);
                };
                container.AddChild(button);
            }
            // Перемешиваем детей
            for (int i = 0; i < count * 3; ++i)
                container.MoveChild(container.GetChild(0), random.Next(count));
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta) {
            active = this;
        }
    }
}
