using Godot;
using System;
using Godot.Collections;
using System.Linq;

public partial class ParallaxController: Marker3D {
    public Vector2 mouse;
    public float sensivity = 0.002f;

    public Camera3D camera;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        camera = GetNode<Camera3D>("Camera3D");
        mouse = GetViewport().GetMousePosition();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        var viewport = Rules.active.GetViewport();
        var rect = viewport.GetVisibleRect();
        var mouse = viewport.GetMousePosition();
        // Возвращаем камеру ближе к началам координат, если игрок отвёл её далеко
        Position = new Vector3(mouse.X - rect.Size.X / 2, 0, mouse.Y - rect.Size.Y / 2) * sensivity;
    }
}
