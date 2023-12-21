using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

// NOTE Отключили в релизе из-за того, что не успели сделать систему сохранений
public class GalleryState: Fstm.State {
    public Rules rules;

    public Gallery gallery;

    public GalleryState(Rules rules): base(GenericStatesEnum.Gallery) {
        this.rules = rules;
        gallery = rules.GetNode<Gallery>("/root/Node3D/Gallery");
    }

    public override void OnEnter() {
        // NOTE
        // gallery.Add("cat1");
        // gallery.Add("cat2");
        Show();
    }

    public override void OnExit() {
        Hide();
    }

    public GalleryState Show() {
        gallery.Show();
        return this;
    }
    
    public GalleryState Hide() {
        gallery.Hide();
        return this;
    }
}