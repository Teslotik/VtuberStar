using Builder;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class MinigamesPlugin: TimePlugin {
    static Random random = new Random();

    override public void OnEnable(Rules rules) {
        rules.gameState.activity.Register(MinigamesEnum.Bubbles, activity => {
            BubblesMiniGame.BubblesMiniGame game = ResourceLoader.Load<PackedScene>("res://Activities/Bubbles/bubbles_mini_game.tscn").Instantiate<BubblesMiniGame.BubblesMiniGame>();
            game.onFinish = rules.gameState.OnMinigameFinished;
            game.remained = 20;
            game.Register("True", 1.0, 3.0, 0.4, factory => {
                BubblesMiniGame.Bubble bubble = ResourceLoader.Load<PackedScene>("res://Activities/Bubbles/bubble.tscn").Instantiate<BubblesMiniGame.Bubble>();
                return bubble;
            }, bubble => {
                game.killed++;
                bubble.Destroy();
            }, bubble => {
                game.dead++;
                bubble.Destroy();
            });
            
            game.onFinish = result => {
                activity.RemoveChild(game);
                game.QueueFree();
                rules.gameState.OnMinigameFinished(result);
            };
            activity.AddChild(game);
        });

        rules.gameState.activity.Register(MinigamesEnum.Leveling, activity => {
            LevelingMiniGame.LevelingMiniGame game = ResourceLoader.Load<PackedScene>("res://Activities/Leveling/leveling_mini_game.tscn").Instantiate<LevelingMiniGame.LevelingMiniGame>();
            game.AddLeveling("Cookies", random.Next(30, 70) / 100.0, random.Next(20, 50) / 100.0, level => {
                GD.Print("Finished");
            });
            game.AddLeveling("Cookies", random.Next(30, 70) / 100.0, random.Next(20, 50) / 100.0, level => {
                GD.Print("Finished");
            });
            game.AddLeveling("Cookies", random.Next(30, 70) / 100.0, random.Next(20, 50) / 100.0, level => {
                GD.Print("Finished");
            });
            
            game.onFinish = result => {
                activity.RemoveChild(game);
                game.QueueFree();
                rules.gameState.OnMinigameFinished(result);
            };
            game.winDeviation = 0.1;
            activity.AddChild(game);
        });
        
        rules.gameState.activity.Register(MinigamesEnum.Sequencing, activity => {
            SequencingMiniGame.SequencingMiniGame game = ResourceLoader.Load<PackedScene>("res://Activities/Sequencing/sequencing_mini_game.tscn").Instantiate<SequencingMiniGame.SequencingMiniGame>();
            game.count = random.Next(4, 8);

            game.onFinish = result => {
                activity.RemoveChild(game);
                game.QueueFree();
                rules.gameState.OnMinigameFinished(result);
            };
            activity.AddChild(game);
        });

        // LEGACY - использовалось в демо
        rules.gameState.activity.Register(MinigamesEnum.RoomChoice, activity => {
            ChoiceMiniGame.ChoiceMiniGame game = ResourceLoader.Load<PackedScene>("res://Activities/Choice/choice_mini_game.tscn").Instantiate<ChoiceMiniGame.ChoiceMiniGame>();
            game.title = "Вы чувствуете лёгкую дрожь по телу";
            game.AddChoice("Nope", "Н е  х о ч у!", button => {
                game.isCorrect = true;
                game.Finish();
            });
            game.AddChoice("Go", "Толко вперёд!", button => {
                game.Finish();
            });
            game.onFinish = result => {
                activity.RemoveChild(game);
                game.QueueFree();
                rules.gameState.OnMinigameFinished(result);
            };
            activity.AddChild(game);
        });

        // NOTE Вроде бы не используется - осталось из демо
        rules.gameState.activity.Register(MinigamesEnum.OpeningChoice, activity => {
            ChoiceMiniGame.ChoiceMiniGame game = ResourceLoader.Load<PackedScene>("res://Activities/Choice/choice_mini_game.tscn").Instantiate<ChoiceMiniGame.ChoiceMiniGame>();
            game.title = "В конце дня Вы хорошо проводите время";
            game.AddChoice("Gaming", "Остаток вечера я провёл с девушками за играми! +5 игры", button => {
                game.isCorrect = true;
                rules.gameState.player.gaming += (int)rules.gameState.player.Scale(5);
                game.Finish();
            });
            game.AddChoice("Gaming", "Нужно собрать кристалы! +3 игры", button => {
                game.isCorrect = true;
                rules.gameState.player.gaming += (int)rules.gameState.player.Scale(3);
                game.Finish();
            });
            game.onFinish = result => {
                activity.RemoveChild(game);
                game.QueueFree();
                rules.gameState.OnMinigameFinished(result);
            };
            activity.AddChild(game);
        });


        // NOTE перенести в другой класс?
        // var delay = 0.01;
        // var delay = 0.8;
        var delay = 3;   // TODO
        rules.gameState.activity.Register(MinigamesEnum.ChatBefore, activity => {
            Chat.Chat game = ResourceLoader.Load<PackedScene>("res://Activities/Chat/chat.tscn").Instantiate<Chat.Chat>();
            
            rules.gameState.interactive.Show();
            var machine = new Nfa.Machine();
            var builder = new NfaBuilder();

            machine.State("Chat");
            builder.Then(state => Tasks.active.Task(delay, task => machine.Step()));
            builder.Action(state => game.Message("Тайм", "Ребята, помните лотерею FruLive?"));
            builder.Then(state => Tasks.active.Task(delay, task => machine.Step()));
            builder.Action(state => game.Message("Канди", "Да, еще бы. Получить бесплатный месяц обучение у девочек - об этом мечтают миллионы людей."));
            builder.Then(state => Tasks.active.Task(delay, task => machine.Step()));
            builder.Action(state => game.Message("Тайм", "Сегодня же результаты покажут. Пойду посмотрю."));
            builder.Then(state => game.Enter("Введите что-нибудь", text => machine.Step()));
            builder.Action(state => game.Message("Я", "Не знаю как вы, но я  стал участвовать по приколу. Не думаю, что выиграю. Я не настолько удачлив."));
            builder.Then(state => Tasks.active.Task(delay, task => machine.Step()));
            builder.Action(state => game.Message("Канди", "Я планировал в следующим месяце подавать заявку на вступление… Я долго готовился к этому, и надеюсь, что меня примут"));
            builder.Then(state => Tasks.active.Task(delay, task => machine.Step()));
            builder.Action(state => game.Message("Тайм", "ЛОЛ, ты серьезно думаешь, что туда попасть так просто?"));
            builder.Then(state => Tasks.active.Task(delay, task => machine.Step()));
            builder.Action(state => game.Message("Канди", "Нет, но я прилагаю ОГРОМНЫЕ усилия! Я уверен в своих силах."));
            builder.Then(state => Tasks.active.Task(delay, task => machine.Step()));
            builder.Action(state => game.Message("Тайм", "Погоди, там что-то выложили"));
            builder.Then(state => Tasks.active.Task(delay, task => machine.Step()));
            builder.Action(state => game.Message("Тайм", "..."));
            builder.Then(state => Tasks.active.Task(delay, task => machine.Step()));
            builder.Action(state => game.Message("Тайм", "ОМГ!!!!"));
            builder.Then(state => Tasks.active.Task(delay, task => machine.Step()));
            builder.Action(state => game.Message("Тайм", "Ребята! СРОЧНО ПОСМОТРИТЕ СЮДА!"));
            builder.Action(state => game.Hyperlink(">>Открыть ссылку<<<", message => machine.Step()), null, 1, false);
            builder.Action(state => game.Message("Я", "Нажимая на ссылку, я замечаю пост на официальной странице FruLive"));
            builder.Then(state => Tasks.active.Task(delay, task => machine.Step()));
            builder.Action(state => rules.gameState.illustration.Image("Forum", 0, false, false));
            builder.Action(state => {
                activity.RemoveChild(game);
                game.QueueFree();
                Chat.Chat.onFinish?.Invoke(true);
            });

            builder.build(machine);
            machine.SetState("Chat");
            machine.Step();
            activity.AddChild(game);
            activity.SelfModulate = new Color(0, 0, 0, 0);
        });

        rules.gameState.activity.Register(MinigamesEnum.ChatAfter, activity => {
            Chat.Chat game = ResourceLoader.Load<PackedScene>("res://Activities/Chat/chat.tscn").Instantiate<Chat.Chat>();
            
            var machine = new Nfa.Machine();
            var builder = new NfaBuilder();

            machine.State("Chat");
            builder.Then(state => Tasks.active.Task(delay, task => machine.Step()));
            builder.Action(state => game.Message("Тайм", "Я НЕ ВЕРЮ СВОИМ ГЛАЗАМ!!! ЧЕЛ ТЫ ВЫИГРАЛ!"));
            builder.Then(state => Tasks.active.Task(delay, task => machine.Step()));
            builder.Action(state => game.Message("Канди", "НУ ПОЧЕМУ ОН, А НЕ Я!!!!!"));
            builder.Then(state => Tasks.active.Task(delay, task => machine.Step()));
            builder.Then(state => game.Enter("Введите что-нибудь", text => machine.Step()));
            builder.Action(state => game.Message("Я", "Да ладно, это какая то ошибка. Не может быть."));
            builder.Then(state => Tasks.active.Task(delay, task => machine.Step()));
            builder.Action(state => game.Message("Тайм", "Ты бы лучше радовался. Такой шанс выпадает не часто."));
            builder.Then(state => Tasks.active.Task(delay, task => machine.Step()));
            builder.Action(state => game.Message("Канди", "Ну ничего. Я сам добьюсь всего и без всяких лотерей."));
            builder.Then(state => game.Enter("Введите что-нибудь", text => machine.Step()));
            builder.Action(state => {
                activity.RemoveChild(game);
                activity.SelfModulate = new Color(1, 1, 1, 1);
                game.QueueFree();
                rules.gameState.interactive.Hide();
                Chat.Chat.onFinish?.Invoke(true);
            });
            
            builder.build(machine);
            machine.SetState("Chat");
            machine.Step();
            rules.gameState.interactive.Show();
            activity.AddChild(game);
        });
    }
}