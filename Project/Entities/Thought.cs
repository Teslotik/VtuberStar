using Godot;
using System;
using System.Linq;
using System.Collections.Generic;


public delegate void ThoughtApplied(Thought thought);
public class Thought {
    static public List<Thought> thoughts = new();

    public string label;
    public ThoughtApplied apply;
    // public string text;

    public Thought(string label, ThoughtApplied apply) {
        this.label = label;
        // this.text = text;
        this.apply = apply;
    }

    static public Thought Find(string label) {
        return thoughts.Find(t => t.label == label);
    }

    static public Thought Register(string label, ThoughtApplied apply) {
        var thought = new Thought(label, apply);
        thoughts.Add(thought);
        return thought;
    }
}