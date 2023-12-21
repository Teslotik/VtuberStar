using Godot;
using System;

namespace LevelingMiniGame {
    public partial class Level: VBoxContainer {
        public ProgressBar progress;
        public Button button;

        public OverflowCallback overflow;
        public double viscosity = 1.0;
        public double value = 1.0;
        public double scale = 100.0;

        public void Increase(double value) {
            this.value = Math.Clamp(this.value + value * viscosity, 0.0, 1.0);
        }

        public bool IsOverflow() {
            return value <= 0.0 || value >= 1.0;
        }

        public void Finish() {

        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready() {
            progress = (ProgressBar)GetNode("ProgressBar");
            button = (Button)GetNode("Button");
            button.ButtonDown += () => {
                Increase(0.2);
            };
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta) {
            progress.Value = value * scale;
            Increase(-0.3 * delta);
            button.Text = $"{(int)(value * scale)}%";
        }
    }
}