using System;
using System.Linq;
using System.Collections.Generic;
using Godot;

// TODO
namespace Saves {
    public class SaveManager {
        // public 

        public Rules rules;
        // public string path;  // TODO
        // public saves;    // TODO

        public SaveManager(Rules rules, string path = null) {
            this.rules = rules;
            // this.path = path;
        }

        public void Save(string label) {
            GD.Print($"=== Saving: {label} ===");
            using var file = FileAccess.Open($"user://save_{label}.json", FileAccess.ModeFlags.Write);
            Godot.Collections.Dictionary dictionary = new Godot.Collections.Dictionary();
            
            dictionary.Add("tags", Tag.Save());

            file.StoreString(Json.Stringify(dictionary, "    ", false));
        }

        public void Load(string label) {
            GD.Print($"=== Loading: {label} ===");
            using var file = FileAccess.Open($"user://save_{label}.json", FileAccess.ModeFlags.Read);
            var dictionary = Json.ParseString(file.GetAsText()).AsGodotDictionary();
            
            Tag.Load(dictionary["tags"].AsGodotArray());
        }
    }

    // class JsonManager: SaveManager {

    //     // public void Save(FileAccess file) {
    //     //     // using var file = FileAccess.Open()
    //     // }
    // }

    // class Snapshot {
    //     public void RegisterEntry() {

    //     }
    // }
}