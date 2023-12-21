using Godot;
using System;

// NOTE не используется и не работает - заменено на спрайты в 3D из-за неочивидного механизмма работы якорей
public partial class Parallax: Control {
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        var viewport = GetViewport().GetVisibleRect();
        var mouse = GetViewport().GetMousePosition().Clamp(Vector2.Zero, viewport.Size);
        foreach (var child in GetChildren()) {
            var parent = (Control)child;
            var node = (Control)child.GetChild(0);

            var depth = node.GetMeta("depth").AsSingle();

            var factor = depth > 0? (1 / depth) * (1 / depth): 0.0f;

        }
    }
}
