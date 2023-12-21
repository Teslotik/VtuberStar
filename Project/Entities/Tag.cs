using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public delegate void TagApplied(Tag tag);
public delegate bool CanTake(Tag tag);
public class Tag {
    static public List<Tag> tags = new();

    public string label;
    public TagApplied apply;
    public CanTake canTake;
    public bool isApplied = false;
    public bool isTemporary = false;

    public Tag(string label, bool isTemporary, TagApplied apply, CanTake canTake) {
        this.label = label;
        this.isTemporary = isTemporary;
        this.apply = apply;
        this.canTake = canTake;
    }

    public static Godot.Collections.Array Save() {
        return Variant.From(tags.Where(tag => tag.isApplied).Select(tag => tag.label).ToArray()).AsGodotArray();
    }

    public static void Load(Godot.Collections.Array data) {
        foreach (var item in data) {
            var tag = tags.Find(tag => tag.label == item.AsString());
            tag.isApplied = true;
            // TODO Вынести этот костыль
            Rules.active.gameState.items.Add(tag.label);
        }
    }

    static public Tag Find(string label) {
        return tags.Find(t => t.label == label);
    }

    static public bool IsApplied(string label) {
        return tags.Find(t => t.label == label).isApplied;
    }

    static public Tag Apply(string label) {
        var tag = tags.Find(t => t.label == label);
        if (tag.isApplied) return tag;
        tag.isApplied = true;
        tag.apply(tag);
        // TODO Вынести этот костыль
        if (tag.isApplied) Rules.active.gameState.items.Add(tag.label);
        return tag;
    }

    static public Tag Register(string label, TagApplied apply, bool isTemporary = false) {
        var tag = new Tag(label, isTemporary, apply, tag => true);
        tags.Add(tag);
        return tag;
    }

    static public Tag Register(string label, TagApplied apply, CanTake canTake, bool isTemporary = false) {
        var tag = new Tag(label, isTemporary, apply, canTake);
        tags.Add(tag);
        return tag;
    }

    static public Tag Register(string label, bool isTemporary = false) {
        var tag = new Tag(label, isTemporary, tag => {}, tag => true);
        tags.Add(tag);
        return tag;
    }
}