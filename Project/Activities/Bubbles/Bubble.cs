using Godot;
using System;

namespace BubblesMiniGame {
    public partial class Bubble: TextureButton {
        public InteractCallback interact;
        public InteractCallback die;
        public double spawnChance;
        public double remaining;

        public void Destroy() {
            QueueFree();
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready() {
            ButtonDown += onButtonDown;
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta) {
            remaining -= delta;
            if (remaining <= 0.0) die(this);
        }

        public void onButtonDown() {
            interact(this);
        }
    }
}