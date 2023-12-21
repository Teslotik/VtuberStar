using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class MenuState: Fstm.State {
    public Rules rules;

    public MenuState(Rules rules): base(GenericStatesEnum.MainMenu) {
        this.rules = rules;
        
        var buttons = rules.GetNode<Control>("/root/Node3D/MenuScreen/MarginContainer/VBoxContainer");
        buttons.GetNode<TextureButton>("NewGame").ButtonUp += () => {
            rules.machine.SetState(rules.disclaimerState);
        };
        
        buttons.GetNode<TextureButton>("Gallery").ButtonUp += () => {
            rules.machine.SetState(rules.galleryState);
        };
        
        buttons.GetNode<TextureButton>("AboutJam").ButtonUp += () => {
            rules.machine.SetState(rules.aboutJamState);
        };
        
        buttons.GetNode<TextureButton>("Exit").ButtonUp += () => {
            rules.GetTree().Quit();
        };
    }

    public override void OnEnter() {
        Show();
        rules.music.Stream = ResourceLoader.Load<AudioStream>("res://Music/Menu.mp3");
        rules.music.Play();
    }

    public override void OnExit() {
        Hide();
        // rules.music.Stop();
    }

    public MenuState Show() {
        rules.GetNode<Control>("/root/Node3D/MenuScreen").Show();
        return this;
    }
    
    public MenuState Hide() {
        rules.GetNode<Control>("/root/Node3D/MenuScreen").Hide();
        return this;
    }
}