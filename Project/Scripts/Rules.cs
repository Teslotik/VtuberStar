using Bonus;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Builder;
using Saves;

public delegate void ResultCallback(bool result);

public partial class Rules: Node {
    static Random random = new Random();
    static public Rules active;

    public List<TimePlugin> plugins = new();
    public Fstm.Machine machine = new();

    public AudioStreamPlayer music;

    // States
    public LaunchState launchState;
    public GameState gameState;
    public MenuState menuState;
    public GalleryState galleryState;
    public AboutJamSate aboutJamState;
    public DisclaimerState disclaimerState;
    public EndingState endingState;

    public SaveManager saveManager;

    [Signal]public delegate void ClickedEventHandler(CameraController controller);
    [Signal]public delegate void UpdateEventHandler(double delta);


    // Called when the node enters the scene tree for the first time.
    public override void _Ready() {
        DisplayServer.WindowSetMinSize(new Vector2I(1600, 900));

        active = this;

        music = GetNode<AudioStreamPlayer>("/root/Node3D/Music");

        launchState = new(this);
        gameState = new(this);
        menuState = new(this);
        galleryState = new(this);
        aboutJamState = new(this);
        disclaimerState = new(this);
        endingState = new(this);

        launchState.Hide();
        gameState.Hide();
        menuState.Hide();
        galleryState.Hide();
        aboutJamState.Hide();
        disclaimerState.Hide();
        endingState.Hide();

        saveManager = new SaveManager(this, "saves");

        // -------------------------- Регистрация --------------------------

        // Регистрация плагинов
        plugins.Add(new MinigamesPlugin());
        plugins.Add(new NarrationPlugin());

        // Регистрация персонажей
        Person.Register(CharactersEnum.Noel, person => {
            person.node = ResourceLoader.Load<PackedScene>("res://Prefabs/character.tscn").Instantiate<Character.Character>();
            GetNode("/root/Node3D").AddChild(person.node);
        });
        Person.Register(CharactersEnum.Monphy, person => {});
        Person.Register(CharactersEnum.Luca, person => {});
        Person.Register(CharactersEnum.Rechumoe, person => {});
        
        // Регистрация бонусов
        // Положительные
        Bonus.Bonus.Register(BonusesEnum.Donated, "Пожертвовал на благотворительность (Харизма +)", Effect.Type.Positive, state => state.player.charismaModifier *= 1.4);
        Bonus.Bonus.Register(BonusesEnum.Fed, "Накормил бездомного кота (Харизма +)", Effect.Type.Positive, state => state.player.charismaModifier *= 1.4);
        Bonus.Bonus.Register(BonusesEnum.Thinking, "Перед сном подумал о смысле жизни (Интеллект, Творчество +)", Effect.Type.Positive, state => {
            state.player.intelligenceModifier *= 1.4;
            state.player.creativityModifier *= 1.4;
        });
        Bonus.Bonus.Register(BonusesEnum.Clutch, "Затащил катку в соло (Геймерство, Творчество +)", Effect.Type.Positive, state => {
            state.player.gamingModifier *= 1.4;
            state.player.charismaModifier *= 1.4;
        });
        Bonus.Bonus.Register(BonusesEnum.Compliment, "Хороший комментарий в интернете обо мне (Геймерство, Харизма +)", Effect.Type.Positive, state => {
            state.player.gamingModifier *= 1.4;
            state.player.charismaModifier *= 1.4;
        });
        Bonus.Bonus.Register(BonusesEnum.GoodDream, "Приснился хороший сон (Творчество, Харизма +)", Effect.Type.Positive, state => {
            state.player.creativityModifier *= 1.4;
            state.player.charismaModifier *= 1.4;
        });
        Bonus.Bonus.Register(BonusesEnum.Delicious, "Приготовил отличный завтрак (Кулинария, Выносливость +)", Effect.Type.Positive, state => {
            state.player.cookingModifier *= 1.4;
            state.player.staminaModifier *= 1.4;
        });
        Bonus.Bonus.Register(BonusesEnum.Assistance, "Весь день переводил бабушек через улицу (Выносливость, Харизма +)", Effect.Type.Positive, state => {
            state.player.staminaModifier *= 1.4;
            state.player.charismaModifier *= 1.4;
        });
        Bonus.Bonus.Register(BonusesEnum.Recipe, "Вспомнил семейный тайный рецепт (Кулинария, Интеллект +)", Effect.Type.Positive, state => {
            state.player.cookingModifier *= 1.4;
            state.player.intelligenceModifier *= 1.4;
        });
        Bonus.Bonus.Register(BonusesEnum.Puzzle, "Решил головоломку (Интеллект +)", Effect.Type.Positive, state => state.player.intelligenceModifier *= 1.4);
        Bonus.Bonus.Register(BonusesEnum.CaughtedBus, "Успел на последний автобус (Выносливость +)", Effect.Type.Positive, state => state.player.staminaModifier *= 1.4);
        Bonus.Bonus.Register(BonusesEnum.WithHello, "Поздоровался с начальником вне работы (Харизма +)", Effect.Type.Positive, state => state.player.charismaModifier *= 1.4);
        Bonus.Bonus.Register(BonusesEnum.Training, "Тренировал скилл (Геймерство +)", Effect.Type.Positive, state => state.player.gamingModifier *= 1.4);
        Bonus.Bonus.Register(BonusesEnum.Fantasized, "Фантазировал о своей лучшей жизни (Творчество +)", Effect.Type.Positive, state => state.player.creativityModifier *= 1.4);
        Bonus.Bonus.Register(BonusesEnum.Horoscope, "Хороший день по гороскопу (Все +)", Effect.Type.Positive, state => {
            state.player.gamingModifier *= 1.4;
            state.player.staminaModifier *= 1.4;
            state.player.cookingModifier *= 1.4;
            state.player.charismaModifier *= 1.4;
            state.player.creativityModifier *= 1.4;
            state.player.intelligenceModifier *= 1.4;
        });
        // Отрицательные
        Bonus.Bonus.Register(BonusesEnum.Wet, "Промок от дождя (Все -)", Effect.Type.Negative, state => {
            state.player.gamingModifier /= 1.4;
            state.player.staminaModifier /= 1.4;
            state.player.cookingModifier /= 1.4;
            state.player.charismaModifier /= 1.4;
            state.player.creativityModifier /= 1.4;
            state.player.intelligenceModifier /= 1.4;
        });
        Bonus.Bonus.Register(BonusesEnum.Nightmare, "Приснился страшный сон (Творчество, Харизма -)", Effect.Type.Negative, state => {
            state.player.creativityModifier /= 1.4;
            state.player.charismaModifier /= 1.4;
        });
        Bonus.Bonus.Register(BonusesEnum.Spoiler, "Увидел концовку любимого аниме (Харизма -)", Effect.Type.Negative, state => state.player.charismaModifier /= 1.4);
        Bonus.Bonus.Register(BonusesEnum.Cut, "Порезался пока учился готовить (Кулинария, Интеллект -)", Effect.Type.Negative, state => {
            state.player.cookingModifier /= 1.4;
            state.player.intelligenceModifier /= 1.4;
        });
        Bonus.Bonus.Register(BonusesEnum.BadInternet, "Весь день не работал нормально интернет (Геймерство -)", Effect.Type.Negative, state => state.player.gamingModifier /= 1.4);
        Bonus.Bonus.Register(BonusesEnum.Toxic, "Плохой комментарий в интернете обо мне (Геймерство, Харизма -)", Effect.Type.Negative, state => {
            state.player.charismaModifier /= 1.4;
            state.player.gamingModifier /= 1.4;
        });
        Bonus.Bonus.Register(BonusesEnum.BurntBreakfast, "Сгорел завтрак (Кулинария, Выносливость -)", Effect.Type.Negative, state => {
            state.player.cookingModifier /= 1.4;
            state.player.staminaModifier /= 1.4;
        });
        Bonus.Bonus.Register(BonusesEnum.Stumbled, "Споткнулся из-за развязанных шнурков (Выносливость, Интеллект -)", Effect.Type.Negative, state => {
            state.player.staminaModifier /= 1.4;
            state.player.intelligenceModifier /= 1.4;
        });
        Bonus.Bonus.Register(BonusesEnum.Minibus, "Постеснялся попросить остановиться на остановке (Харизма, Интеллект -)", Effect.Type.Negative, state => {
            state.player.charismaModifier /= 1.4;
            state.player.intelligenceModifier /= 1.4;
        });
        Bonus.Bonus.Register(BonusesEnum.MissedTheBus, "Не успел на последний автобус (Выносливость -)", Effect.Type.Negative, state => state.player.staminaModifier /= 1.4);
        Bonus.Bonus.Register(BonusesEnum.OverexposedDumplings, "Забыл о том, что варились пельмешки (Кулинария, Интеллект -)", Effect.Type.Negative, state => {
            state.player.cookingModifier /= 1.4;
            state.player.intelligenceModifier /= 1.4;
        });
        Bonus.Bonus.Register(BonusesEnum.DidnSleep, "Всю ночь играл в игру (Выносливость, Харизма -)", Effect.Type.Negative, state => {
            state.player.staminaModifier /= 1.4;
            state.player.charismaModifier /= 1.4;
        });
        Bonus.Bonus.Register(BonusesEnum.Overdue, "Выпил просроченное молоко (Интеллект, Кулинария -)", Effect.Type.Negative, state => {
            state.player.intelligenceModifier /= 1.4;
            state.player.cookingModifier /= 1.4;
        });
        Bonus.Bonus.Register(BonusesEnum.WithoutHello, "Не поздоровался с начальником вне работы (Харизма -)", Effect.Type.Negative, state => state.player.charismaModifier /= 1.4);

        // Регистрация тегов (предметов)
        // Переходы на другие локации
        // Tag.Register(TagsEnum.Hub);
        // Tag.Register(TagsEnum.Girls);
        // Обычные предметы
        Tag.Register(TagsEnum.Ticket, tag => gameState.player.gaming += (int)(ScoresEnum.Few * gameState.player.gamingModifier), true);
        Tag.Register(TagsEnum.Crosswords, tag => gameState.player.intelligence += (int)(ScoresEnum.Few * gameState.player.intelligenceModifier), true);
        Tag.Register(TagsEnum.Cookbook, tag => gameState.player.cooking += (int)(ScoresEnum.Few * gameState.player.cookingModifier), true);
        Tag.Register(TagsEnum.Dumbbells, tag => gameState.player.stamina += (int)(ScoresEnum.Few * gameState.player.staminaModifier), true);
        Tag.Register(TagsEnum.Sketchbook, tag => gameState.player.creativity += (int)(ScoresEnum.Few * gameState.player.creativityModifier), true);
        // Кино
        Tag.Register(TagsEnum.CookingShow, tag => gameState.player.cooking += (int)(ScoresEnum.Few * gameState.player.cookingModifier), true);
        Tag.Register(TagsEnum.EsportsFestival, tag => gameState.player.gaming += (int)(ScoresEnum.Few * gameState.player.gamingModifier), true);
        Tag.Register(TagsEnum.RomanticMovie, tag => gameState.player.charisma += (int)(ScoresEnum.Few * gameState.player.charismaModifier), true);
        Tag.Register(TagsEnum.MartialArtsMovie, tag => gameState.player.stamina += (int)(ScoresEnum.Few * gameState.player.staminaModifier), true);
        Tag.Register(TagsEnum.ChessTournament, tag => gameState.player.intelligence += (int)(ScoresEnum.Few * gameState.player.intelligenceModifier), true);
        Tag.Register(TagsEnum.SnatchTheMovie, tag => gameState.player.creativity += (int)(ScoresEnum.Few * gameState.player.creativityModifier), true);
        // Подарки
        Tag.Register(TagsEnum.Toy, tag => {
            Person.Find(CharactersEnum.Rechumoe).relation += ScoresEnum.Much;
            gameState.player.balance -= 600;
            gameState.machine.AddState(EventsEnum.ToyPresent);
        }, tag => gameState.player.balance >= 600);
        Tag.Register(TagsEnum.Energy, tag => {
            Person.Find(CharactersEnum.Monphy).relation += ScoresEnum.Much;
            gameState.player.balance -= 600;
            gameState.machine.AddState(EventsEnum.EnergyPresent);
        }, tag => gameState.player.balance >= 600);
        Tag.Register(TagsEnum.Beer, tag => {
            Person.Find(CharactersEnum.Luca).relation += ScoresEnum.Much;
            gameState.player.balance -= 600;
            gameState.machine.AddState(EventsEnum.BeerPresent);
        }, tag => gameState.player.balance >= 600);
        // Достижения
        Tag.Register(TagsEnum.Gaming, tag => {
            gameState.player.gaming += (int)(ScoresEnum.Bought * gameState.player.gamingModifier);
            gameState.player.balance -= 300;
        }, tag => gameState.player.balance >= 300);
        Tag.Register(TagsEnum.Stamina, tag => {
            gameState.player.stamina += (int)(ScoresEnum.Bought * gameState.player.staminaModifier);
            gameState.player.balance -= 300;
        }, tag => gameState.player.balance >= 300);
        Tag.Register(TagsEnum.Cooking, tag => {
            gameState.player.cooking += (int)(ScoresEnum.Bought * gameState.player.cookingModifier);
            gameState.player.balance -= 300;
        }, tag => gameState.player.balance >= 300);
        Tag.Register(TagsEnum.Charisma, tag => {
            gameState.player.charisma += (int)(ScoresEnum.Bought * gameState.player.charismaModifier);
            gameState.player.balance -= 300;
        }, tag => gameState.player.balance >= 300);
        Tag.Register(TagsEnum.Creativity, tag => {
            gameState.player.creativity += (int)(ScoresEnum.Bought * gameState.player.creativityModifier);
            gameState.player.balance -= 300;
        }, tag => gameState.player.balance >= 300);
        Tag.Register(TagsEnum.Intelligence, tag => {
            gameState.player.intelligence += (int)(ScoresEnum.Bought * gameState.player.intelligenceModifier);
            gameState.player.balance -= 300;
        }, tag => gameState.player.balance >= 300);

        // Регистрация мыслей
        Thought.Register("Скучно", thought => gameState.bonus.Add("Bored"));

        // Регистрация описаний
        // Предметы
        Tips.Register(TagsEnum.Ticket, "Флаер", "Сходите в аркадные игровые автоматы (Геймерство +)");
        Tips.Register(TagsEnum.Crosswords, "Газета", "В газете Вы видите любимый раздел с кроссвордами (Интеллект +)");
        Tips.Register(TagsEnum.Cookbook, "Кулинарная книга", "Похоже здесь много рецептов... (Кулинария +)");
        Tips.Register(TagsEnum.Dumbbells, "Гантеля", "12 кг (Выносливость +)");
        Tips.Register(TagsEnum.Sketchbook, "Скетчбук", "Скетчбук, каранаш и ластик (Творчество +)");
        Tips.Register(TagsEnum.CookingShow, "Билет в кино", "CINEMA\nTICKET\n(Случайная характеристика +)");
        Tips.Register(TagsEnum.EsportsFestival, "Билет в кино", "CINEMA\nTICKET\n(Случайная характеристика +)");
        Tips.Register(TagsEnum.RomanticMovie, "Билет в кино", "CINEMA\nTICKET\n(Случайная характеристика +)");
        Tips.Register(TagsEnum.MartialArtsMovie, "Билет в кино", "CINEMA\nTICKET\n(Случайная характеристика +)");
        Tips.Register(TagsEnum.ChessTournament, "Билет в кино", "CINEMA\nTICKET\n(Случайная характеристика +)");
        Tips.Register(TagsEnum.SnatchTheMovie, "Билет в кино", "CINEMA\nTICKET\n(Случайная характеристика +)");
        Tips.Register(TagsEnum.Toy, "Любимая игрушка Рёты", "(Отношение с Рётой +)");
        Tips.Register(TagsEnum.Energy, "Куча энергетиков", "(Отношение с Монфис +)");
        Tips.Register(TagsEnum.Beer, "Пивасик", "(Отношение с Лукой +)");
        Tips.Register(TagsEnum.Gaming, "Контроллер", "(Геймерство +)");
        Tips.Register(TagsEnum.Stamina, "Мяч", "(Выносливость +)");
        Tips.Register(TagsEnum.Cooking, "Яблоко", "(Кулинария +)");
        Tips.Register(TagsEnum.Charisma, "Звезда", "(Харизма +)");
        Tips.Register(TagsEnum.Creativity, "Кисть", "(Творчество +)");
        Tips.Register(TagsEnum.Intelligence, "Книга", "(Интеллект +)");
        // Мини игры
        Tips.Register("Gaming", "Компьютер", "Геймерство, Харизма +");
        Tips.Register("Manga", "Манга", "Интеллект, Творчество +");
        Tips.Register("Yoga", "Коврик для йоги", "Выносливость +");
        Tips.Register("Drawing", "Мольберт", "Творчество, Выносливость +");
        Tips.Register("Digital", "Графический планшет", "Творчество +");
        Tips.Register("Books", "Книги", "Творчество, Харизма +");
        Tips.Register("Stove", "Готовка", "Кулинария, Выносливость +");
        Tips.Register("Mirror", "Зеркало", "Харизма, Творчество +");
        Tips.Register("Cookbooks", "Кулинарные книги", "Кулинария, Интеллект +");

        // Регистрация активностей

        // Регистрация событий
        // timeline.Event("Opening");
        // timeline.Event("Room", 0, e => Goto(e.label));

        // Регистрация состояний
        // Nfa
        gameState.machine.State(GenericStatesEnum.Interactable);
        gameState.machine.State(GenericStatesEnum.Movable);
        // Fstm
        machine.State(launchState);
        machine.State(gameState);
        machine.State(menuState);
        machine.State(galleryState);
        machine.State(aboutJamState);
        machine.State(disclaimerState);
        machine.State(endingState);

        // --- Инициализация логики ---
        
        // LEGACY - использовалось в демо
        // Случайные мысли
        // Tasks.active.Task(5, task => {
        //     task.isCancelled = false;
        //     var person = Person.persons[random.Next(Person.persons.Count)];
        //     person?.node?.ShowThoughts(Thought.thoughts[random.Next(Thought.thoughts.Count)].label);
        // });

        // --- Инициализация данных ---
        foreach (var plugin in plugins) {
            plugin.OnEnable(this);
        }

        // -------------------------- Запуск --------------------------

        machine.SetState(launchState);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        machine.current._Process(delta);
        EmitSignal(SignalName.Update, delta);
    }

    // ---------------------------------- Utils ----------------------------------

    static public double Mix(double v0, double v1, double f) {
        return v0 + f * (v1 - v0);
    }

    static public double Lerp(double x0, double y0, double x1, double y1, double f) {
        return y0 + ((y1 - y0) / (x1 - x0)) * (f - x0);
    }

    public T Select<T>(params T[] values) {
        return values[random.Next(values.Length)];
    }

    public static float VolumeToDb(float v) {
        // NOTE подобрано экспериментально
        return (v - 70.0f) / 2.0f;
    }

    public static float DbToVolume(float v) {
        return v * 2.0f + 70.0f;
    }

    // ---------------------------------- Delegates ----------------------------------


    // TODO
    // public bool HasGame() {
    //     return false;
    // }

    public void Simulate() {
        
    }

    // ---------------------------------- Factory ----------------------------------

    // LEGACY - Использовалось в Демо
    public ChoiceMiniGame.ChoiceMiniGame ChoiceMiniGame() {
        ChoiceMiniGame.ChoiceMiniGame game = (ChoiceMiniGame.ChoiceMiniGame)ResourceLoader.Load<PackedScene>("res://Activities/Choice/choice_mini_game.tscn").Instantiate();
        game.title = "Любимая группа Noel";
        game.AddChoice("NozieMC", "Noze MC", button => {
            game.Finish();
        });
        game.AddChoice("Menson", "Menson", button => {
            game.Finish();
        });
        game.AddChoice("LinkinPark", "Linkin Park", button => {
            game.Finish();
        });
        game.AddChoice("Obereg", "Оберег", button => {
            game.isCorrect = true;
            game.Finish();
        });
        return game;
    }
}