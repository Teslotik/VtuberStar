using Godot;
using System;
using System.Linq;

public partial class Items: VBoxContainer {
    public static TextureRect pointing;

    public Items Add(string label) {
        var item = new TextureRect();
        item.Texture = ResourceLoader.Load<Texture2D>($"res://Items/{label}.png");
        item.Name = label;
        item.MouseEntered += () => {
            pointing = item;
        };
        item.MouseExited += () => {
            pointing = null;
        };
        AddChild(item);
        return this;
    }

    // LEGACY - добавление предмета на сцену происходит в фабрике GenerateItems в NarrationPlugin
    // public Items Item(string label, Vector3 position, string icon = null) {
    //     icon ??= label;
    //     var item = ResourceLoader.Load<PackedScene>("res://Prefabs/item.tscn").Instantiate<Sprite3D>();
    //     item.Texture = ResourceLoader.Load<Texture2D>($"res://Items/{icon}.png");
    //     item.SetMeta(Metastrings.Tag, label);
    //     item.Position = position;
    //     Rules.active.GetNode("/root/Node3D").AddChild(item);
    //     return this;
    // }

    public Items RemoveItem(string label) {
        var item = GetChildren().ToList().Find(child => child.Name == label);
        if (item == null) return this;
        item.QueueFree();
        return this;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {

    }
}
