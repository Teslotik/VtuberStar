using System;
using System.Collections.Generic;
using System.Linq;
using Bonus;
using Godot;
using Nfa;

namespace Builder {
    public class NfaBuilder {
        public List<State> states = new();

        public NfaBuilder build(Machine machine, bool clear = true, bool remove = true) {
            states.ForEach(state => state.group = machine.last.group);
            var last = machine.last;
            foreach (var state in states) {
                var from = last;
                machine.Transition(machine => {
                    if (!machine.current.Contains(from)) return false;
                    machine.ReplaceState(from, state);
                    if (state.next) machine.Step();
                    return true;
                });
                last = state;
                machine.states.Add(state);
            }
            if (remove) {
                var onExit = last.onExit;
                var excess = states.ToList();
                // machine.RemoveState(machine.last);   // NOTE удаление первого состояния
                last.onExit = s => {
                    excess.ForEach(s => machine.RemoveState(s));
                    onExit?.Invoke(last);
                };
            }
            if (clear) states.Clear();
            return this;
        }

        public NfaBuilder Then(StateCallback onEnter = null, StateCallback onExit = null, StateCallback onUpdate = null, bool next = false) {
            var state = new State(onEnter, onExit, onUpdate, next);
            states.Add(state);
            return this;
        }

        public NfaBuilder Action(StateCallback onEnter = null, StateCallback onExit = null, int repeat = 1, bool next = true) {
            for (int i = 0; i < repeat; ++i) {
                Then(onEnter, onExit, null, next);
            }
            return this;
        }

        // Удаляется сразу после добавления после вызова callback'а
        public NfaBuilder Disappearing(StateCallback onEnter) {

            return this;
        }
    }


    public class NarrationBuilder: NfaBuilder {
        static Random random = new Random();

        public NarrationBuilder ApplyEffect(Bonus.Bonus bonus, int repeat = 1, double chance = 1.0, Effect.Type type = Effect.Type.Positive) {
            for (int i = 0; i < repeat; ++i) {
                if (random.NextSingle() > chance) continue;
                bonus.RandomEffect(type);
            }
            return this;
        }

        public NarrationBuilder Monologue(Narration.Narration narration, string text) {
            Then(state => {
                narration.Monologue(string.Format(text, Player.active.name));
            });
            return this;
        }

        public NarrationBuilder Dialogue(Narration.Narration narration, string title, string text) {
            Then(state => {
                narration.Titled(string.Format(title, Player.active.name), string.Format(text, Player.active.name));
            });
            return this;
        }
    }
}