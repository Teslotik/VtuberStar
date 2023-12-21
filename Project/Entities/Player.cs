using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using Builder;
using Bonus;

public class Player {
    static Random random = new Random();
    static public Player active;

    public string name = "Anonymous";

    public int gaming = 0;
    public int stamina = 0;
    public int cooking = 0;
    public int charisma = 0;
    public int creativity = 0;
    public int intelligence = 0;
    public int balance = 0;

    // Общие
    public double mood = 1.0;
    public double luck = 0.0;
    // Модификаторы характеристик
    public double gamingModifier = 1.0;
    public double staminaModifier = 1.0;
    public double cookingModifier = 1.0;
    public double charismaModifier = 1.0;
    public double creativityModifier = 1.0;
    public double intelligenceModifier = 1.0;
    public double balanceModifier = 1.0;
    // Модификаторы игры
    // public int bonusCount = 5;
    // public double bonusChance = 0.5;
    public int minigameCount = 5;
    public double minigameChance = 0.5;
    // public double scoresMultiplier = 1.0;
    // public double minigameDuration = 5.0;
    public int actionsCount = 3;
    public double actionsChance = 1.0;

    public Player() {
        active = this;
    }

    public Godot.Collections.Dictionary Save() {
        var dictionary = new Godot.Collections.Dictionary();
        dictionary["name"] = name;

        dictionary["gaming"] = gaming;
        dictionary["stamina"] = stamina;
        dictionary["cooking"] = cooking;
        dictionary["charisma"] = charisma;
        dictionary["creativity"] = creativity;
        dictionary["intelligence"] = intelligence;
        dictionary["balance"] = balance;
        
        dictionary["mood"] = mood;
        dictionary["luck"] = luck;
        
        dictionary["gamingModifier"] = gamingModifier;
        dictionary["staminaModifier"] = staminaModifier;
        dictionary["cookingModifier"] = cookingModifier;
        dictionary["charismaModifier"] = charismaModifier;
        dictionary["creativityModifier"] = creativityModifier;
        dictionary["intelligenceModifier"] = intelligenceModifier;
        dictionary["balanceModifier"] = balanceModifier;
        
        dictionary["minigameCount"] = minigameCount;
        dictionary["minigameChance"] = minigameChance;
        
        dictionary["actionsCount"] = actionsCount;
        dictionary["actionsChance"] = actionsChance;
        return dictionary;
    }

    public void Load(Godot.Collections.Dictionary data) {
        name = data["name"].AsString();

        gaming = data["gaming"].AsInt32();
        stamina = data["stamina"].AsInt32();
        cooking = data["cooking"].AsInt32();
        charisma = data["charisma"].AsInt32();
        creativity = data["creativity"].AsInt32();
        intelligence = data["intelligence"].AsInt32();
        balance = data["balance"].AsInt32();
        
        mood = data["mood"].AsDouble();
        luck = data["luck"].AsDouble();
        
        gamingModifier = data["gamingModifier"].AsDouble();
        staminaModifier = data["staminaModifier"].AsDouble();
        cookingModifier = data["cookingModifier"].AsDouble();
        charismaModifier = data["charismaModifier"].AsDouble();
        creativityModifier = data["creativityModifier"].AsDouble();
        intelligenceModifier = data["intelligenceModifier"].AsDouble();
        balanceModifier = data["balanceModifier"].AsDouble();
        
        minigameCount = data["minigameCount"].AsInt32();
        minigameChance = data["minigameChance"].AsDouble();
        
        actionsCount = data["actionsCount"].AsInt32();
        actionsChance = data["actionsChance"].AsDouble();
    }

    public double Scale(double value) {
        return value * mood * (random.NextSingle() < luck? 1.5: 1.0);
    }

    public int GetScores() {
        return gaming + stamina + cooking + charisma + creativity + intelligence;
    }

    public Player Clear() {
        name = "Anonymous";
        gaming = 0;
        stamina = 0;
        cooking = 0;
        charisma = 0;
        creativity = 0;
        intelligence = 0;
        balance = 0;
        return this;
    }

    public Player Restore() {
        mood = 1.0;
        luck = 1.0;

        gamingModifier = 1.0;
        staminaModifier = 1.0;
        cookingModifier = 1.0;
        charismaModifier = 1.0;
        creativityModifier = 1.0;
        intelligenceModifier = 1.0;
        balanceModifier = 1.0;

        // bonusCount = 5;
        // bonusChance = 0.5;
        minigameCount = 5;
        minigameChance = 0.5;
        // scoresMultiplier = 1.0;
        // minigameDuration = 5.0;
        actionsCount = 3;
        actionsChance = 1.0;
        return this;
    }
}