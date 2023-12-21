using Godot;
using System;
using System.Linq;
using System.Collections.Generic;


public class Tips {
    static public List<Tips> tips = new();
    static public Tips active;

    public string label;
    public string title;
    public string description;

    public Tips(string label, string title, string description) {
        this.label = label;
        this.title = title;
        this.description = description;
    }

    static public bool Exists(string label) {
        return tips.Exists(t => t.label == label);
    }

    static public Tips Find(string label) {
        return tips.Find(t => t.label == label);
    }

    static public Tips Register(string label, string title, string description) {
        var tip = new Tips(label, title, description);
        tips.Add(tip);
        return tip;
    }

    static public void Clear() {
        active = null;
    }

    static public Tips SetTip(string label) {
        Clear();
        active = Find(label);
        return active;
    }
}