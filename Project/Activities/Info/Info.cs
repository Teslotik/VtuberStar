using Godot;
using System;

public delegate void ChoiceCallback(int index, string choice);
public delegate void RegistrationCallback(string label);

public partial class Info: Control {
    public Control pane;

    public void Clear() {
        if (!HasScreen()) return;
        RemoveChild(pane);
        pane = null;
        Hide();
    }

    public bool HasScreen() {
        return pane != null;
    }

    public void Chapter(string title, string text, double duration, bool fadeIn = true, bool fadeOut = true) {
        Clear();
        Show();
        pane = ResourceLoader.Load<PackedScene>("res://Activities/Info/chapter.tscn").Instantiate<Panel>();
        pane.GetNode<Label>("Container/Title").Text = title;
        pane.GetNode<Label>("Container/Text").Text = text;
        AnimationPlayer animation = pane.GetNode<AnimationPlayer>("AnimationPlayer");
        if (fadeIn) animation.Play("FadeIn");
        if (fadeOut) {
            Tasks.active.Task(duration, task => {
                animation.Play("FadeOut");
                // Clear();
            });
        }
        AddChild(pane);
    }

    public void Choice(string title, string[] choices, ChoiceCallback onSelected) {
        Clear();
        Show();
        pane = ResourceLoader.Load<PackedScene>("res://Activities/Info/choice.tscn").Instantiate<Panel>();
        pane.GetNode<Label>("Container/Label").Text = title;
        var container = pane.GetNode<VBoxContainer>("Container");
        for (int i = 0; i < choices.Length; ++i) {
            var index = i;
            var choice = choices[index];
            var button = new Button();
            button.Text = choice;
            button.ButtonDown += () => {
                GD.Print($"Selected: {choice}");
                onSelected(index, choice);
                Clear();
            };
            container.AddChild(button);
        }
        AddChild(pane);
    }

    public void IconsChoice(string title, string[] characters, ChoiceCallback onSelected) {
        Clear();
        Show();
        pane = ResourceLoader.Load<PackedScene>("res://Activities/Info/icons_choice.tscn").Instantiate<Panel>();
        pane.GetNode<Label>("VBoxContainer/Label").Text = title;
        var container = pane.GetNode<HBoxContainer>("VBoxContainer/Container");
        for (int i = 0; i < characters.Length; ++i) {
            var index = i;
            var choice = characters[index];
            var button = new TextureButton();
            button.TextureNormal = ResourceLoader.Load<Texture2D>($"res://Icons/{choice}.png");
            button.TextureHover = ResourceLoader.Load<Texture2D>($"res://Icons/{choice}_pressed.png");  // TODO _hovered
            button.TexturePressed = ResourceLoader.Load<Texture2D>($"res://Icons/{choice}_pressed.png");
            button.IgnoreTextureSize = true;
            button.StretchMode = TextureButton.StretchModeEnum.Scale;
            button.CustomMinimumSize = new Vector2(300, 300);
            button.ButtonUp += () => {
                GD.Print($"Selected: {choice}");
                onSelected(index, choice);
                Clear();
            };
            container.AddChild(button);
        }
        AddChild(pane);
    }

    public void IconsView(string title, string[] characters) {
        Clear();
        Show();
        pane = ResourceLoader.Load<PackedScene>("res://Activities/Info/icons_choice.tscn").Instantiate<Panel>();
        pane.GetNode<Label>("VBoxContainer/Label").Text = title;
        var container = pane.GetNode<HBoxContainer>("VBoxContainer/Container");
        for (int i = 0; i < characters.Length; ++i) {
            var index = i;
            var choice = characters[index];
            var button = new TextureButton();
            button.TextureNormal = ResourceLoader.Load<Texture2D>($"res://Icons/{choice}.png");
            button.IgnoreTextureSize = true;
            button.StretchMode = TextureButton.StretchModeEnum.Scale;
            button.CustomMinimumSize = new Vector2(300, 300);
            container.AddChild(button);
        }
        AddChild(pane);
    }

    public void Registration(RegistrationCallback onComplete) {
        Clear();
        Show();
        pane = ResourceLoader.Load<PackedScene>("res://Activities/Info/registration.tscn").Instantiate<Panel>();
        var input = pane.GetNode<LineEdit>("Container/Input");
        var button = pane.GetNode<Button>("Container/Button");
        button.ButtonUp += () => {
            if (input.Text.Length < 1) return;
            onComplete(input.Text);
            Clear();
        };
        AddChild(pane);
    }

    public void Image(string image, double duration, bool fadeIn = true, bool fadeOut = true) {
        Clear();
        Show();
        pane = ResourceLoader.Load<PackedScene>("res://Activities/Info/image.tscn").Instantiate<Panel>();
        pane.GetNode<TextureRect>("Image").Texture = ResourceLoader.Load<Texture2D>($"res://Background/{image}.png");
        AnimationPlayer animation = pane.GetNode<AnimationPlayer>("AnimationPlayer");
        if (fadeIn) animation.Play("FadeIn");
        if (fadeOut) {
            Tasks.active.Task(duration, task => {
                animation.Play("FadeOut");
                // Clear();
            });
        }
        AddChild(pane);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        
    }
}
