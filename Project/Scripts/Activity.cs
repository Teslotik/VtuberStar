using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Activity {
    public delegate void FactoryCallback(Activity activity);

    public partial class Activity: Panel {
        public Dictionary<string, FactoryCallback> factories = new();
        public List<Node> activities = new();

        public void Register(string label, FactoryCallback factory) {
            factories.Add(label, factory);
        }

        public Activity Create(string label) {
            factories[label](this);
            return this;
        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready() {

        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta) {
            
        }
    }
}