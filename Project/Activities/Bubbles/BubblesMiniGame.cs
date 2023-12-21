using Godot;
using System;
using System.Collections.Generic;

namespace BubblesMiniGame {
    public delegate Bubble FactoryCallback(Factory factory);
    public delegate void InteractCallback(Bubble bubble);

    public partial class BubblesMiniGame: Panel {
        static Random random = new Random();
        static public BubblesMiniGame active;

        public Panel container;

        public List<Factory> factories = new List<Factory>();
        public List<Bubble> bubbles = new List<Bubble>();

        public ResultCallback onFinish;
        public double winRate = 3.0;
        public int remained = 10;
        public int killed = 0;
        public int dead = 0;

        public void Register(string label, double spawnChance, double remaining, double cooldown, FactoryCallback factory, InteractCallback interact, InteractCallback die) {
            factories.Add(new Factory(label, spawnChance, remaining, cooldown, factory, interact, die));
        }

        public void Unregister(string label) {
            factories.RemoveAll(t => t.label == label);
        }

        public void Finish() {

        }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready() {
            active = this;
            container = this;
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta) {
            active = this;
            foreach (var factory in factories) factory.elapsed += delta;
            // Обходим все фабрики и вызываем перезарядившиеся
            for (int i = 0; i < factories.Count; ++i) {
                if (remained <= 0) {
                    onFinish?.Invoke(dead == 0? true: killed / dead >= winRate);
                    // QueueFree();
                    break;
                }
                Bubble bubble = factories[random.Next(factories.Count)].Instance();
                if (bubble == null) continue;
                remained--;
                bubbles.Add(bubble);
                Rect2 zone = container.GetRect();
                bubble.SetPosition(new Vector2(
                    random.Next((int)(-zone.Size.X / 2), (int)(zone.Size.X / 2 - 60.0)) ,
                    random.Next((int)(-zone.Size.Y / 2), (int)(zone.Size.Y / 2 - 60.0)) 
                ));
                container.AddChild(bubble);
                break;
            }
        }
    }

    // TODO переделать на просто callback
    public class Factory {
        static Random random = new Random();

        public string label;
        public FactoryCallback factory;
        public InteractCallback interact;
        public InteractCallback die;
        public double spawnChance;
        public double remaining;
        public double cooldown;
        public double elapsed = 0;

        public Factory(string label, double spawnChance, double remaining, double cooldown, FactoryCallback factory, InteractCallback interact, InteractCallback die) {
            this.label = label;
            this.spawnChance = spawnChance;
            this.remaining = remaining;
            this.cooldown = cooldown;
            this.factory = factory;
            this.interact = interact;
            this.die = die;
        }

        public Bubble Instance() {
            if (elapsed < cooldown) return null;
            if (random.NextSingle() > spawnChance) return null;
            Bubble bubble = factory(this);
            bubble.interact = interact;
            bubble.die = die;
            bubble.spawnChance = spawnChance;
            bubble.remaining = remaining;
            elapsed = 0;
            return bubble;
        }
    }
}