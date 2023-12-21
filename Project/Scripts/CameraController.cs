using Godot;
using System;
using Godot.Collections;
using System.Linq;

public partial class CameraController: Marker3D {
    [Signal]public delegate void UpdatedEventHandler(double delta);

    public Vector2 mouse;
    public float sensivity = 0.03f;
    public double radius = 0.0;
    public bool isDragging = false;
    public bool isClicked = false;

    [Export]public float dragThreshold = 5;

    public Camera3D camera;
    public Dictionary pointing;
    public Dictionary selected;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        camera = GetNode<Camera3D>("Camera3D");
        mouse = GetViewport().GetMousePosition();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        EmitSignal(SignalName.Updated, delta);
    }
}
