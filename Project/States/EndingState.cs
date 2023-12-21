using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class EndingState: Fstm.State {
    public Rules rules;

    public Control page;
    public Info info;
    public TextureButton button;

    public EndingState(Rules rules): base(GenericStatesEnum.Ending) {
        this.rules = rules;
        
        page = rules.GetNode<Control>("/root/Node3D/Ending");
        info = page.GetNode<Info>("Info");
        button = page.GetNode<TextureButton>("Button");
        
        button.ButtonUp += () => {
            rules.machine.SetState(rules.menuState);
        };
    }

    public override void OnEnter() {
        Show();
        info.Image("Ending", 0, true, false);
    }

    public override void OnExit() {
        info.Clear();
        Hide();
    }

    public EndingState Show() {
        page.Show();
        return this;
    }
    
    public EndingState Hide() {
        page.Hide();
        return this;
    }
}