using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class LaunchState: Fstm.State {
    public Rules rules;

    public Info info;

    public LaunchState(Rules rules): base(GenericStatesEnum.Launch) {
        this.rules = rules;
        info = rules.GetNode<Info>("/root/Node3D/Launch");
    }

    public override void OnEnter() {
        Show();
        info.Image("TimeProject", 2);
        Tasks.active.Task(4, task => {
            rules.machine.SetState(rules.menuState);
        });
    }

    public override void OnExit() {
        Hide();
    }

    public LaunchState Show() {
        info.Show();
        return this;
    }
    
    public LaunchState Hide() {
        info.Hide();
        return this;
    }
}