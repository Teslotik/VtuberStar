using Godot;
using System;

public partial class Floating: Control {
    public Control floating;
    public string label;
    
    [Signal]public delegate void ChangedEventHandler(Floating floating);
    public delegate void Clicked(string label);

    private Clicked onClick = null;

    public Floating Clear() {
        if (!HasFloating()) return this;
        // RemoveChild(floating);
        floating.QueueFree();   // NOTE
        floating = null;
        return this;
    }

    public bool HasFloating() {
        return floating != null;
    }

    public Floating Appear(string label, string image, Clicked clicked = null) {
        Clear();
        this.label = label;
        this.floating = ResourceLoader.Load<PackedScene>("res://Prefabs/floating_character.tscn").Instantiate<Control>();
        var floating = this.floating.GetNode<TextureButton>("Texture");
        floating.TextureNormal = ResourceLoader.Load<Texture2D>($"res://Floating/{image}.png");
        AnimationPlayer player = floating.GetNode<AnimationPlayer>("AnimationPlayer");
        player.Play("Appearing");
        AddChild(this.floating);
        floating.GetNode<TextureRect>("ExclamationMark").Visible = clicked != null;
        onClick = clicked;
        if (clicked != null) {
            floating.ButtonUp += OnClick;
        }
        EmitSignal(SignalName.Changed, this);
        return this;
    }

    public void OnClick() {
        onClick?.Invoke(label);
        onClick = null;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        
    }
}
