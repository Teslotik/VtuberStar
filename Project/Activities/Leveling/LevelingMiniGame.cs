using Godot;
using System;
using System.Collections.Generic;

namespace LevelingMiniGame {
    public delegate Level FactoryCallback();
    public delegate void OverflowCallback(Level level);

    public partial class LevelingMiniGame: Panel {
        static Random random = new Random();

        public List<FactoryCallback> factories = new List<FactoryCallback>();
        public HBoxContainer container;

        public double winDeviation = 0.1;
        public ResultCallback onFinish;

        public void AddLeveling(string label, double value, double viscosity, OverflowCallback overflow) {
            factories.Add(() => {
                Level level = (Level)ResourceLoader.Load<PackedScene>("res://Activities/Leveling/level.tscn").Instantiate();
                level.value = value;
                level.viscosity = viscosity;
                level.overflow = overflow;
                return level;
            });
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready() {
            container = GetNode<HBoxContainer>("HBoxContainer");
            foreach (var factory in factories) {
                container.AddChild(factory());
            }
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta) {
            var fits = true;
            foreach (var child in container.GetChildren()) {
                var level = (Level)child;
                if (Math.Abs(level.value - 0.5) > winDeviation) fits = false;
                if (level.IsOverflow()) {
                    onFinish?.Invoke(false);
                    return;
                }
            }
            if (fits) {
                onFinish?.Invoke(true);
                // QueueFree();
            }
        }
    }
}