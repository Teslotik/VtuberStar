using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bonus {
    public delegate void EffectApplied(GameState state);
    public delegate bool FilterCallback(Effect effect);
    public delegate Control FactoryCallback(Effect effect);

    public partial class Bonus: VBoxContainer {
        static Random random = new Random();
        static public List<Effect> effects = new();

        public Godot.Collections.Array Save() {
            return Variant.From(effects.Where(effect => effect.IsActive()).Select(effect => effect.label).ToArray()).AsGodotArray();
        }

        public void Load(Godot.Collections.Array data) {
            foreach (var item in data) {
                var effect = effects.Find(effect => effect.label == item.AsString());
                AddChild(effect.Instance());
            }
        }

        public Bonus Clear() {
            foreach (var effect in effects) {
                if (effect.IsActive()) effect.node.QueueFree();
                effect.node = null;
            }
            return this;
        }

        public bool HasEffects() {
            return effects.Any();
        }

        public Effect Find(string label) {
            return effects.Find(state => state.label == label);
        }

        static public void Register(string label, string text, Effect.Type type, EffectApplied apply) {
            var wrapper = new Effect(label, text, type, effect => {
                var label = ResourceLoader.Load<PackedScene>("res://Prefabs/effect.tscn").Instantiate<Label>();
                label.Text = text;
                return label;
            }, apply);
            effects.Add(wrapper);
        }

        public Effect Add(string label) {
            return Add(effect => effect.label == label);
        }

        public Effect Add(FilterCallback filter) {
            var effect = effects.Find(effect => filter(effect) && !effect.IsActive());
            Add(effect);
            return effect;
        }

        public Bonus Add(Effect effect) {
            if (effect.IsActive()) return this;
            AddChild(effect.Instance());
            GD.Print("added effect");
            return this;
        }

        public List<Effect> GetActive() {
            return effects.FindAll(effect => effect.IsActive());
        }

        public Bonus Apply(GameState state) {
            effects.ForEach(effect => {
                if (effect.IsActive()) effect.apply(state);
            });
            return this;
        }

        public Bonus RandomEffect(Effect.Type type) {
            var unused = effects.FindAll(effect => effect.type == type && !effect.IsActive());
            if (!unused.Any()) return this;
            var effect = unused[random.Next(unused.Count)];
            Add(effect);
            return this;
        }
    }

    public class Effect {
        public enum Type {
            Positive,
            Negative,
            Neutral
        }

        public string label;
        public string text;
        public EffectApplied apply;
        public Control node;
        public FactoryCallback factory;
        public Type type;

        public bool IsActive() {
            return node != null;
        }

        public Effect(string label, string text, Type type, FactoryCallback factory, EffectApplied apply) {
            this.label = label;
            this.text = text;
            this.type = type;
            this.factory = factory;
            this.apply = apply;
        }

        public Control Instance() {
            if (IsActive()) return node;
            node = factory(this);
            return node;
        }
    }
}