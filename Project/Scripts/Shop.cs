using Godot;
using System;

public partial class Shop: Node3D {
    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        foreach (var child in GetChildren()) {
            var sprite = (Sprite3D)child;
            if (!sprite.HasMeta(Metastrings.Tag)) continue;
            var tag = sprite.GetMeta(Metastrings.Tag).AsString();
            if (Tag.Find(tag).isApplied) {
                sprite.Texture = ResourceLoader.Load<Texture2D>($"res://Icons/Sold.png");
                sprite.RemoveMeta(Metastrings.Tag);
            }
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {

    }
}
