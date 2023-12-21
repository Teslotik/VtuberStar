using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public delegate void TaskCallback(Task task);

public partial class Tasks: Node {
    static Random random = new Random();
    static public Tasks active;

    public List<Task> tasks = new List<Task>();
    public Task current;

    // ---------------------------------- Factory ----------------------------------

    public Task Task(string label, double duration, TaskCallback onAlarm) {
        current = new Task(label, duration, onAlarm);
        tasks.Add(current);
        return current;
    }

    public Task Task(double duration, TaskCallback onAlarm) {
        current = new Task(random.Next().ToString(), duration, onAlarm);
        tasks.Add(current);
        return current;
    }

    public void Unregister(string label) {
        tasks.RemoveAll(t => t.label == label);
    }

    public void Unregister(Task task) {
        tasks.Remove(task);
    }

    // ----------------------------------  ----------------------------------

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        active = this;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        active = this;
        // Мы используем копию списка, т.к. могут добавляться задачи по мере обхода таймеров
        // не очень хорошее решение, т.к. требует дополнительной памяти
        foreach (var task in tasks.ToList()) {
            if (task.isFreezed) continue;
            task.elapsed += delta;
            if (task.elapsed < task.duration) continue;
            // if (task.next != null) task.isFreezed = true;
            task.isCancelled = true;
            // GD.Print("Executing task: ", task.label);
            task.onAlarm(task);
            task.elapsed = 0;
            if (task.next == null) continue;
            task.Load(task.next);
        }
        tasks.RemoveAll(t => t.isCancelled);
    }
}


public class Task {
    public string label;
    public double duration;
    public double elapsed = 0;
    public bool isCancelled = false;
    public bool isFreezed = false;
    public TaskCallback onAlarm;
    public Task next;

    public Task(string label, double duration, TaskCallback onAlarm) {
        this.label = label;
        this.duration = duration;
        this.onAlarm = onAlarm;
    }

    public Task Load(Task other) {
        label = other.label;
        duration = other.duration;
        elapsed = other.elapsed;
        isCancelled = other.isCancelled;
        // isFreezed = other.isFreezed;
        onAlarm = other.onAlarm;
        next = other.next;
        return this;
    }

    public Task Then(double duration, TaskCallback onAlarm) {
        var task = new Task(label, duration, onAlarm);
        next = task;
        return task;
    }
}