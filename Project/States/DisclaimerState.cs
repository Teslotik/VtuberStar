using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class DisclaimerState: Fstm.State {
    public Rules rules;

    public Control page;
    public Info info;
    public TextureButton button;

    public DisclaimerState(Rules rules): base(GenericStatesEnum.Disclaimer) {
        this.rules = rules;
        
        page = rules.GetNode<Control>("/root/Node3D/Disclaimer");
        info = page.GetNode<Info>("Info");
        button = page.GetNode<TextureButton>("Button");
        
        button.ButtonUp += () => {
            rules.machine.SetState(rules.gameState);
        };
    }

    public override void OnEnter() {
        Show();
        info.Image("Disclaimer", 0, true, false);
    }

    public override void OnExit() {
        info.Clear();
        Hide();
    }

    public DisclaimerState Show() {
        page.Show();
        return this;
    }
    
    public DisclaimerState Hide() {
        page.Hide();
        return this;
    }
}