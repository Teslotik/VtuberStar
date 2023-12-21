using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Nfa {
    public delegate bool TransitionCallback(Machine machine);
    public delegate void StateCallback(State state);

    public class Machine {
        public List<State> states = new();
        public List<State> current = new();
        public State last;
        public List<TransitionCallback> transitions = new();

        public bool isFreezed = false;

        public Machine State(string label, StateCallback onEnter = null, StateCallback onExit = null, StateCallback onUpdate = null, bool next = false) {
            var state = new State(label, onEnter, onExit, onUpdate, next);
            states.Add(state);
            last = state;
            return this;
        }

        public Machine Transition(TransitionCallback transition) {
            transitions.Add(transition);
            return this;
        }

        // Delegates
        // Используйте эти методы для того, чтобы сработали Callback'и состояний

        public Machine Clear() {
            current.Clear();
            return this;
        }

        public Machine SetState(string label) {
            current.ForEach(state => state.onExit?.Invoke(state));
            current.Clear();
            AddState(label);
            return this;
        }

        public Machine ReplaceState(State from, State to) {
            current.Remove(from);
            from.onExit?.Invoke(from);
            current.Add(to);
            to.onEnter?.Invoke(to);
            return this;
        }

        public Machine AddState(string label) {
            var state = states.Find(state => state.label == label);
            current.Add(state);
            GD.Print($"::: State: {label} {state} :::");
            state.onEnter?.Invoke(state);
            last = state;
            return this;
        }

        public Machine FilterState(Predicate<State> match) {
            current.ForEach(state => {
                if (match(state)) state.onExit?.Invoke(state);
            });
            current.RemoveAll(match);
            return this;
        }

        public Machine RemoveState(string label) {
            foreach (var state in current) {
                if (state.label == label) state.onExit?.Invoke(state);
            }
            current.RemoveAll(state => state.label == label);
            return this;
        }

        public Machine RemoveState(State state) {
            state.onExit?.Invoke(state);
            current.Remove(state);
            return this;
        }

        public bool HasState(string label) {
            if (!states.Exists(s => s.label == label)) throw new Exception($"State '{label}' does not exists");
            return current.Exists(state => state.label == label);
        }
        
        public Machine Step(bool hard = false) {
            if (isFreezed && !hard) return this;
            foreach (var rule in transitions) {
                if (!rule(this)) continue;
                break;
            }
            return this;
        }

        public void _Process(double delta) {
            foreach (var state in current) {
                state.onUpdate?.Invoke(state);
            }
        }
    }

    public class State {
        public string label;
        public string group;
        public bool next;
        public StateCallback onEnter;
        public StateCallback onExit;
        public StateCallback onUpdate;

        public State(string label, StateCallback onEnter, StateCallback onExit, StateCallback onUpdate, bool next) {
            this.label = label;
            group = label;
            this.next = next;
            this.onEnter = onEnter;
            this.onExit = onExit;
            this.onUpdate = onUpdate;
        }

        public State(StateCallback onEnter, StateCallback onExit, StateCallback onUpdate, bool next) {
            this.next = next;
            this.onEnter = onEnter;
            this.onExit = onExit;
            this.onUpdate = onUpdate;
        }
    }
}