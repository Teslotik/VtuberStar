using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class AboutJamSate: Fstm.State {
    public Rules rules;

    public Control page;
    public TextureButton info;
    public TextureButton back;

    public AboutJamSate(Rules rules): base(GenericStatesEnum.AboutJam) {
        this.rules = rules;
        page = rules.GetNode<Control>("/root/Node3D/AboutJam");
        info = page.GetNode<TextureButton>("Info");
        back = page.GetNode<TextureButton>("Back");

        back.ButtonUp += () => {
            rules.machine.SetState(GenericStatesEnum.MainMenu);
        };
    }

    public override void OnEnter() {
        Show();
    }

    public override void OnExit() {
        Hide();
    }

    public AboutJamSate Show() {
        page.Show();
        return this;
    }
    
    public AboutJamSate Hide() {
        page.Hide();
        return this;
    }
}