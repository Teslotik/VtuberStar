using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fstm {
    public delegate void MachineChanged(Machine fstm);
    public delegate void StateChanged(State state);
    public delegate bool CanEnter(State other);

    public class Machine {
        static Random random = new Random();

        public List<State> states = new();
        public State current;
        public State created;
        public bool isFreezed = false;

        public State Find(string label) {
            return states.Find(state => state.label == label);
        }

        public Machine State(State state) {
            states.Add(state);
            current ??= state;
            created = state;
            return this;
        }

        // public Machine Then(string label, StateChanged onEnter = null, StateChanged onExit = null) {
        //     var prev = created;
        //     State(label, onEnter, onExit);
        //     prev.canEnter.Add(state => state.label == label);
        //     return this;
        // }

        // public Machine Then(StateChanged onEnter = null, StateChanged onExit = null) {
        //     var prev = created;
        //     State(null, onEnter, onExit);
        //     var current = created;
        //     prev.canEnter.Add(state => state == current);
        //     return this;
        // }

        public Machine Transition(string from, CanEnter rule) {
            Find(from).canEnter.Add(rule);
            return this;
        }

        public Machine RemoveTransitions(string label) {
            Find(label).canEnter.Clear();
            return this;
        }

        public Machine SetState(string state) {
            current.OnExit();
            current = Find(state);
            current.OnEnter();
            return this;
        }

        public Machine SetState(State state) {
            current?.OnExit();
            current = state;
            current?.OnEnter();
            return this;
        }

        public Machine ReplaceState(string state) {
            current = Find(state);
            return this;
        }

        public Machine RemoveState(string label) {
            states.RemoveAll(state => state.label == label);
            return this;
        }

        // TODO
        public void Store() {

        }

        public void Load() {
            
        }

        public void ToDrawio() {
            
        }

        public Machine Step(bool hard = false) {
            if (isFreezed && !hard) return this;
            foreach (var state in states) {
                if (!current.canEnter.Any(rule => rule(state))) continue;
                SetState(state);
                break;
            }
            return this;
        }
    }

    public abstract class State {
        public string label;
        public List<CanEnter> canEnter = new();

        public State(string label) {
            this.label = label;
        }

        public virtual void OnEnter() {

        }

        public virtual void OnExit() {

        }

        public virtual void _Process(double delta) {

        }
    }
}