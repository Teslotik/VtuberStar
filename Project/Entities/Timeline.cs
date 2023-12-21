using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public delegate void TimeAlarmCallback(TimeEvent e);
public delegate bool TimeFilterCallback(TimeEvent e);

public partial class Timeline: TextureRect {
    static Random random = new Random();
    static public Color currentColor = new Color(1.0f, 0.141f, 0.098f, 1.0f);   // красный
    static public Color focusColor = new Color(0.29f, 0.38f, 0.827f, 1.0f);     // синий
    [Signal]public delegate void TimelineChangedEventHandler(Timeline timeline);
    
    public HBoxContainer container;
    public ScrollContainer scroll;

    public List<TimeEvent> events = new List<TimeEvent>();
    public bool isFreezed = false;
    public int elapsed = 0;
    private int time = -1;  // TODO -1 - костыль, чтобы Next() сработал правильно первый раз
    public TimeEvent current;

    public Godot.Collections.Dictionary Save() {
        var dictionary = new Godot.Collections.Dictionary();
        // var events = new Godot.Collections.Array();
        dictionary["current"] = current.label;
        
        foreach (var @event in events) {
            var data = new Godot.Collections.Dictionary();
            // NOTE что делать с callback'ами?
            // data[""]
        }
        return dictionary;
    }

    public void Load(Godot.Collections.Dictionary data) {

    }

    // ---------------------------------- Utils ----------------------------------

    public void Place(TimeEvent e, int time) {
        if (time >= 30) return;
        TimeEvent legacy = events.Find(e => e.end == time);
        if (legacy != null) container.RemoveChild(legacy.node);
        events.RemoveAll(e => e.end == time);

        events.Add(e);
        events.Sort((e1, e2) => e1.end.CompareTo(e2.end));
        container.AddChild(e.node);
        container.MoveChild(e.node, events.IndexOf(e));
    }

    public int GetTime() {
        return time;
    }

    // ---------------------------------- Factory ----------------------------------

    public TimeEvent Mark(int time) {
        TimeEvent mark = new TimeEvent("mark", null, TimeEvent.Type.Mark, time, time, e => {});
        mark.node = (Control)ResourceLoader.Load<PackedScene>("res://Prefabs/time_marker.tscn").Instantiate();
        mark.node.GetNode<Label>("Label").Text = $"{time + 1}";
        Place(mark, time);
        return mark;
    }

    public TimeEvent Event(string label, string text, int end, TimeAlarmCallback alarm) {
        TimeEvent e = new TimeEvent(label, text, TimeEvent.Type.Event, time, end, alarm);
        e.node = (Control)ResourceLoader.Load<PackedScene>("res://Prefabs/time_event.tscn").Instantiate();
        e.node.GetNode<Label>("Label").Text = e.text;
        Place(e, end);
        return e;
    }

    public TimeEvent Event(string label, string text) {
        var end = events.Find(e => e.end > time && e.type == TimeEvent.Type.Mark).end;
        TimeEvent e = new TimeEvent(label, text, TimeEvent.Type.Event, time, end, timeline => {});
        e.node = (Control)ResourceLoader.Load<PackedScene>("res://Prefabs/time_event.tscn").Instantiate();
        e.node.GetNode<Label>("Label").Text = e.text;
        Place(e, end);
        return e;
    }

    public void Unregister(string label) {
        events.RemoveAll(e => e.label == label);
    }

    public void Unregister(TimeEvent e) {
        events.Remove(e);
    }

    // ----------------------------------  ----------------------------------

    private int focus = 0;
    public void Focus(int time) {
        focus = this.time;
        this.time = time;
        events.Find(e => e.end == time).Select(focusColor);
    }

    public void RestoreFocus() {
        this.time = focus;
        events.ForEach(e => e.Unselect());
        events.Find(e => e.end == time).Select(currentColor);
    }

    // public void Step() {
    //     if (isFreezed) return;
    //     foreach (var e in events) {
    //         if (time < e.end) continue;
    //         e.onAlarm(e);
    //     }
    //     // TODO Unregister
    //     events.RemoveAll(e => e.isCancelled);
    // }

    public void Next(Predicate<TimeEvent> match) {
        var e = events.Find(match);
        if (e == null) return;
        elapsed = e.end - time;
        time = e.end;
        current = e;
        current.Select(currentColor);
        EmitSignal(SignalName.TimelineChanged, this);
        e.onAlarm(e);
        // TODO Unregister
        events.RemoveAll(e => e.isCancelled);
    }

    public void Next() {
        Next(e => e.end > time && e.type != TimeEvent.Type.Mark);
    }

    public void SetTime(int time) {
        Next(e => e.end == time);
    }

    public void Clear() {
        for (int i = 0; i < 30; ++i) {
            Mark(i);
        }
        time = -1;
        elapsed = 0;
        isFreezed = false;
    }

    public bool HasEvent(int time) {
        return events.Exists(e => e.end == time && e.type != TimeEvent.Type.Mark);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        scroll = GetChild<ScrollContainer>(0);
        container = scroll.GetChild<HBoxContainer>(0);

        Clear();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        if (!events.Any()) return;
        TimeEvent current = events.Find(e => e.end > time);
        if (current == null) current = events.Last();

        double offset = current.node.GetTransform().Origin.X - this.GetRect().GetCenter().X;
        scroll.ScrollHorizontal = (int)Mix(scroll.ScrollHorizontal, offset, 2.0 * delta);
    }

    static double Mix(double v0, double v1, double f) {
        return v0 + f * (v1 - v0);
    }
}



public class TimeEvent {
    public enum Type {
        Mark,
        Event,
        Secret
    }

    public string label;
    public string text;
    public Type type;
    public bool isFinished = false;
    public bool isCancelled = false;
    public int start = 0;
    public int end = 0;
    public Control node;
    public TimeAlarmCallback onAlarm;
    
    public TimeEvent(string label, string text, Type type, int start, int end, TimeAlarmCallback alarm) {
        this.label = label;
        this.text = text;
        this.type = type;
        this.start = start;
        this.end = end;
        this.onAlarm = alarm;
    }

    public TimeEvent Select(Color color) {
        if (node != null) node.Modulate = color;
        return this;
    }

    public TimeEvent Unselect() {
        if (node != null) node.Modulate = new Color(1, 1, 1, 1);
        return this;
    }
}
