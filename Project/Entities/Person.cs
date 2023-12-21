using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

// TODO Поместить в namespace и переименовать на FactoryCallback
// TODO В Character.cs
public delegate void PersonFactoryCallback(Person person);
public class Person {
    static public List<Person> persons = new();

    public string name;
    public int relation = 0;    // отношения ГГ с персонажем
    public Character.Character node;
    public PersonFactoryCallback factory;
    // public double difficult = 5;    // количесто мини-игр?
    // public Fstm.Fstm behavior = new();

    public Person(string name, PersonFactoryCallback factory) {
        this.name = name;
        this.factory = factory;
    }

    public static Godot.Collections.Array Save() {
        var data = new Godot.Collections.Array();
        foreach (var person in persons) {
            var dictionary = new Godot.Collections.Dictionary();
            dictionary["name"] = person.name;
            dictionary["relation"] = person.relation;
            data.Add(dictionary);
        }
        return data;
    }

    public static void Load(Godot.Collections.Array data) {
        foreach (var item in data) {
            var dictionary = item.AsGodotDictionary();
            var person = persons.Find(person => person.name == dictionary["name"].AsString());
            person.relation = dictionary["relation"].AsInt32();
        }
    }

    static public Person Register(string name, PersonFactoryCallback factory) {
        Person person = new Person(name, factory);
        persons.Add(person);
        return person;
    }

    static public Person Find(string name) {
        return persons.Find(p => p.name == name);
    }

    public bool IsActive() {
        return node != null;
    }

    public Person Instance() {
        if (IsActive()) return this;
        factory?.Invoke(this);
        return this;
    }
}