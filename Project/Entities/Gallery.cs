using Godot;
using System;

public partial class Gallery: MarginContainer {
	public VBoxContainer container;

	public void Add(string label) {
		var item = ResourceLoader.Load<PackedScene>("res://Prefabs/painting.tscn").Instantiate<TextureRect>();
		item.Texture = ResourceLoader.Load<Texture2D>($"res://Gallery/{label}.png");
		container.AddChild(item);
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready() {
		container = GetNode<VBoxContainer>("MarginContainer/ScrollContainer/Container");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta) {

	}
}
