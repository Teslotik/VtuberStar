using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using Builder;
using Bonus;

public class GameState: Fstm.State {
    static Random random = new Random();

    public Rules rules;
    public MarginContainer interactive;
    public bool isActive = false;   // TODO костыль - избавиться
    public bool isPointingObject = false;
    public bool isPointingUi = false;
    public bool isTutorial = false;
    public Vector2 tipPosition = new();

    public Label actions;
    public CameraController cameraController;
    public Narration.Narration narration;
    public Info illustration;
    public Info info;
    public Timeline timeline;
    public Bonus.Bonus bonus;
    public Progress progress;
    public Activity.Activity activity;
    public TextureButton endDay;
    public Items items;
    public Floating floating;
    public NinePatchRect tip;
    public HSlider volumeSlider;
    public SubViewport gameView;

    // State
    public Node3D scene;
    public NarrationBuilder builder = new();
    public Nfa.Machine machine = new();
    public Player player = new();
    public int scores = 0;      // очки за последнюю последовательность мини игр

    public GameState SetHudVisibility(bool visible) {
        timeline.Visible = visible;
        // Items
        rules.GetNode<Control>("/root/Node3D/Control/MarginContainer3/Container/HBoxContainer/PanelContainer").Visible = visible;
        // Bonuses
        rules.GetNode<Control>("/root/Node3D/Control/MarginContainer3/Container/HBoxContainer/PanelContainer2").Visible = visible;
        bonus.Visible = visible;
        progress.Visible = visible;
        endDay.Visible = visible;
        return this;
    }

    public GameState(Rules rules): base(GenericStatesEnum.Game) {
        this.rules = rules;
        actions = rules.GetNode<Label>("/root/Node3D/Control/MarginContainer3/Container/Progress/HBoxContainer/Actions");
        timeline = rules.GetNode<Timeline>("/root/Node3D/Control/MarginContainer3/Container/Timeline");
        narration = rules.GetNode<Narration.Narration>("/root/Node3D/Control/MarginContainer3/Container/PlayerState/HBoxContainer/Narration");
        bonus = rules.GetNode<Bonus.Bonus>("/root/Node3D/Control/MarginContainer3/Container/HBoxContainer/PanelContainer/MarginContainer/Bonus");
        illustration = rules.GetNode<Info>("/root/Node3D/Control/Illustration");
        info = rules.GetNode<Info>("/root/Node3D/Control/Info");
        progress = rules.GetNode<Progress>("/root/Node3D/Control/MarginContainer3/Container/Progress");
        interactive = rules.GetNode<MarginContainer>("/root/Node3D/Control/Interactive");
        activity = rules.GetNode<Activity.Activity>("/root/Node3D/Control/Interactive/AspectRatioContainer/Activity");
        items = rules.GetNode<Items>("/root/Node3D/Control/MarginContainer3/Container/HBoxContainer/PanelContainer2/Items");
        endDay = rules.GetNode<TextureButton>("/root/Node3D/Control/EndDay");
        cameraController = rules.GetNode<CameraController>("/root/Node3D/GameView/Marker3D");
        floating = rules.GetNode<Floating>("/root/Node3D/Control/Floating");
        tip = rules.GetNode<NinePatchRect>("/root/Node3D/Control/Tip");
        volumeSlider = rules.GetNode<HSlider>("/root/Node3D/Control/MarginContainer3/Container/PlayerState/SoundVolume");
        gameView = rules.GetNode<SubViewport>("/root/Node3D/GameView");

        // Биндимся
        volumeSlider.ValueChanged += (v) => {
            rules.music.VolumeDb = Rules.VolumeToDb((float)v);
        };

        // Обрабатываем перемещение и нажатие мышки
        cameraController.Updated += delta => {
            var mousePosition = rules.GetViewport().GetMousePosition();

            // Подсказки при наведении на объекты
            isPointingObject = false;
            if (cameraController.pointing != null && cameraController.pointing.Any()) {
                var target = ((PhysicsBody3D)cameraController.pointing["collider"])?.GetParent();
                string tipLabel = null;
                
                if (target != null) {
                    if (target.HasMeta(Metastrings.Tag)) tipLabel = target.GetMeta(Metastrings.Tag).AsString();
                    else if (target.HasMeta(Metastrings.Tip)) tipLabel = target.GetMeta(Metastrings.Tip).AsString();
                    else if (target.HasMeta(Metastrings.Minigame)) tipLabel = target.GetMeta(Metastrings.Minigame).AsString();
                }
                
                if (tipLabel != null && Tips.Exists(tipLabel)) {
                    isPointingObject = true;
                    var data = Tips.SetTip(tipLabel);
                    tipPosition = mousePosition;
                    tip.GetNode<Label>("MarginContainer/VBoxContainer/Title").Text = data.title;
                    tip.GetNode<Label>("MarginContainer/VBoxContainer/Description").Text = data.description;
                }
            }
            // Подсказки при наведении на интерфейс
            isPointingUi = false;
            if (Items.pointing != null && Tips.Exists(Items.pointing.Name)) {
                isPointingUi = true;
                var data = Tips.SetTip(Items.pointing.Name);
                tipPosition = mousePosition;
                tip.GetNode<Label>("MarginContainer/VBoxContainer/Title").Text = data.title;
                tip.GetNode<Label>("MarginContainer/VBoxContainer/Description").Text = data.description;
            }
            

            var isIntaractable = machine.HasState("Interactable");
            var screenRect = rules.GetNode<TextureRect>("/root/Node3D/Control/ScreenContainer/Screen").GetRect();
            if (isIntaractable) {
                var pos = new Vector2(
                    (float)Rules.Lerp(screenRect.Position.X, 0, screenRect.End.X, 1280, mousePosition.X),
                    (float)Rules.Lerp(screenRect.Position.Y, 0, screenRect.End.Y, 720, mousePosition.Y)
                );

                // var pos = cameraController.GetViewport().GetMousePosition();
                var space = cameraController.GetWorld3D().DirectSpaceState;
                var from = cameraController.camera.ProjectRayOrigin(pos);
                var param = PhysicsRayQueryParameters3D.Create(from, from + cameraController.camera.ProjectRayNormal(pos) * 1000.0f);
                cameraController.pointing = space.IntersectRay(param);
                // GD.Print(pos, cameraController.pointing);
            }

            // Обрабатываем клики и перемещения
            Vector2 current = rules.GetViewport().GetMousePosition();
            if (Input.IsMouseButtonPressed(MouseButton.Left)) {
                cameraController.isClicked = true;
                cameraController.selected = null;
                // Добавляем порог дельты перемещения мышки, после которого считается, что камера перемещается
                if (current.DistanceSquaredTo(cameraController.mouse) > cameraController.dragThreshold) cameraController.isDragging = true;
                // Перемещаем камеру вдоль двух осей (локальной X и локальной Y) на дельту перемещения мыши
                if (machine.HasState(GenericStatesEnum.Movable) && screenRect.HasPoint(mousePosition)) {
                    cameraController.Position += (cameraController.camera.Transform.Basis.X * (cameraController.mouse.X - current.X) + cameraController.camera.Transform.Basis.Y * (current.Y - cameraController.mouse.Y)) * cameraController.sensivity;
                }
            } else {
                // Выделение объектов
                if (!cameraController.isDragging && cameraController.isClicked) {
                    cameraController.selected = cameraController.pointing;
                    rules.EmitSignal(Rules.SignalName.Clicked, cameraController);
                }

                cameraController.isDragging = false;
                cameraController.isClicked = false;

                // Возвращаем камеру ближе к началам координат, если игрок отвёл её далеко
                if (cameraController.Position.Length() > cameraController.radius) {
                    cameraController.Position = cameraController.Position.Lerp(cameraController.Position.Normalized() * (float)cameraController.radius, (float)(4.0f * delta));
                }
            }
            cameraController.mouse = current;
        };

        rules.Clicked += camera => {
            // if (rules.machine.current != this) return;
            if (!isActive) return;
            if (!machine.HasState(GenericStatesEnum.Interactable)) return;
            if (player.actionsCount <= 0) return;

            if (!camera.selected.Any()) return;
            PhysicsBody3D collider = (PhysicsBody3D)camera.selected["collider"];
            var target = collider.GetParent();
            
            // GD.Print("Clicked", target);

            // Пузырьки с мыслями, получение бонусов
            if (typeof(Character.Character).IsInstanceOfType(target)) {
                var character = collider.GetParent<Character.Character>();
                if (character.HasThoughts()) {
                    var thought = Thought.Find(character.text);
                    GD.Print($"Thoughts: {thought.label}");
                    thought?.apply(thought);
                    character.RemoveThoughts();
                    IncreaseActionsCount(-1);
                    return;
                }
            }

            // Запуск мини игр
            if (target.HasMeta(Metastrings.Minigame)) {
                machine.RemoveState(GenericStatesEnum.Interactable);
                machine.RemoveState(GenericStatesEnum.Movable);
                scores = 0;
                interactive.Show();
                var type = target.GetMeta(Metastrings.Minigame).AsString();
                GD.Print($"Minigame: {type}");
                // Creating list of possible mini games
                // NOTE Здесь можно установить последовательность мини игр для одной активности
                List<string> games = new();
                games = new List<string>(){MinigamesEnum.Bubbles, MinigamesEnum.Leveling, MinigamesEnum.Sequencing};
                var progress = interactive.GetNode<ProgressBar>("AspectRatioContainer/Activity/MarginContainer/ActivityProgress");
                
                // Создаём корневое состояние мини-игр
                var label = $"Activity{random.Next()}";
                machine.State(label, state => {
                    GD.Print("Activity started");
                    machine.isFreezed = true;
                });
                // Генерируем последовательность мини-игр
                for (int i = 0; i < player.minigameCount; ++i) {
                    double ratio = (double)i / player.minigameCount;
                    // Показываем прогресс и запускаем следующую мини-игру
                    builder.Then(state => {
                        progress.Value = ratio * 100;
                        Tasks.active.Task(1.5, task => {
                            machine.Step(true);
                        });
                    });
                    // С некоторым шансом создаём мини-игру
                    builder.Then(state => {
                        if (random.NextSingle() > player.minigameChance) {
                            machine.Step(true);
                            GD.Print("Minigame skipped");
                            return;
                        }
                        GD.Print("Minigame started");
                        var name = games[random.Next(games.Count)];
                        GD.Print("Minigame: ", name);
                        activity.Create(name);
                    });
                }
                // Возвращаем отображение
                builder.Then(state => {
                    GD.Print("Activity finished");
                    machine.isFreezed = false;
                    var scale = 0.25;
                    if (type == "Gaming") {
                        player.gaming += (int)(scores * ScoresEnum.Much * scale);
                        player.charisma += (int)(scores * ScoresEnum.Few * scale);
                    } else if (type == "Manga") {
                        player.intelligence += (int)(scores * ScoresEnum.Normal * scale);
                        player.creativity += (int)(scores * ScoresEnum.Few * scale);
                    } else if (type == "Yoga") {
                        player.stamina += (int)(scores * ScoresEnum.Much * scale);
                    } else if (type == "Drawing") {
                        player.creativity += (int)(scores * ScoresEnum.Normal * scale);
                        player.stamina += (int)(scores * ScoresEnum.Normal * scale);
                    } else if (type == "Digital") {
                        player.creativity += (int)(scores * ScoresEnum.Much * scale);
                    } else if (type == "Books") {
                        player.creativity += (int)(scores * ScoresEnum.Much * scale);
                        player.intelligence += (int)(scores * ScoresEnum.Much * scale);
                    } else if (type == "Stove") {
                        player.cooking += (int)(scores * ScoresEnum.Much * scale);
                        player.stamina += (int)(scores * ScoresEnum.Few * scale);
                    } else if (type == "Mirror") {
                        player.charisma += (int)(scores * ScoresEnum.Much * scale);
                        player.creativity += (int)(scores * ScoresEnum.Few * scale);
                    } else if (type == "Cookbooks") {
                        player.cooking += (int)(scores * ScoresEnum.Few * scale);
                        player.intelligence += (int)(scores * ScoresEnum.Few * scale);
                    }
                    interactive.Hide();
                    machine.AddState(GenericStatesEnum.Interactable);
                    machine.AddState(GenericStatesEnum.Movable);
                });
                builder.build(machine);
                machine.AddState(label);
                machine.Step(true);
                IncreaseActionsCount(-1);
                return;
            }

            // Получение предметов
            if (target.HasMeta(Metastrings.Tag)) {
                var tag = target.GetMeta(Metastrings.Tag).AsString();
                if (!Tag.Find(tag).canTake(Tag.Find(tag))) return;
                GD.Print($"Tag: {tag}");
                Tag.Apply(tag);
                target.QueueFree();
                cameraController.pointing.Clear();
                IncreaseActionsCount(-1);
                return;
            }

            // Диалог
            if (target.HasMeta(Metastrings.Dialogue)) {
                machine.RemoveState(GenericStatesEnum.Interactable);
                var dialogue = target.GetMeta(Metastrings.Dialogue).AsString();
                target.RemoveMeta(Metastrings.Dialogue);
                GD.Print($"Dialogue: {dialogue}");
                machine.AddState(dialogue);
                IncreaseActionsCount(-1);
                return;
            }

            // Случайная реплика (при клике на 3d персонажа)
            // NOTE Не используется
            // if (target.HasMeta(Metastrings.Replica)) {
            //     var replica = target.GetMeta(Metastrings.Replica).AsString();
            //     GD.Print($"Replica: {replica}");
            //     // TODO "Replica" + replica
            //     machine.AddState(replica + random.Next(3));
            //     IncreaseActionsCount(-1);
            //     return;
            // }

            // Выбор    // NOTE Что это???
            if (target.HasMeta(Metastrings.Choice)) {
                var choice = target.GetMeta(Metastrings.Choice).AsString();
                GD.Print($"Choice: {choice}");
                machine.AddState(choice);
                target.QueueFree();
                cameraController.pointing.Clear();
                IncreaseActionsCount(-1);
                return;
            }
        };

        // timeline.TimelineChanged += timeline => {
        // };
        
        rules.Clicked += camera => {
            // if (rules.machine.current != this) return;
            if (!isActive) return;
            machine.Step();
        };

        // --- фаза 1 - начало дня (запускается автоматически после завершения дня) ---
        machine.State(GenericStatesEnum.NextDay, state => {
            GD.Print("\n---------- Next day ----------");
            rules.saveManager.Save("last");
            // сброс интерфейсов
            info.Clear();
            // сброс бонусов
            GD.Print("Restoring bonuses");
            player.Restore();
            foreach (var tag in Tag.tags){
                if (!tag.isTemporary) continue;
                tag.isApplied = false;
                items.RemoveItem(tag.label);
            }
            SetActionsCount(player.actionsCount);
            // таймлайн смещается на следующее событие
            GD.Print("Moving to the next timeline event");
            timeline.Next(e => e.end > timeline.GetTime() && e.type != TimeEvent.Type.Mark);
            // даются рандомные (лужа) бонусы
            GD.Print("Generating random bonuses");
            builder.ApplyEffect(bonus, 2, 0.5);
            // LEGACY - использовалось в демо
            // даются периодические (работа) бонусы
            // GD.Print("Applying tasks bonuses");
            // открывается локция
            GD.Print($"Location opened: {timeline.current.label}");
            machine.AddState(timeline.current.label);
            if (scene != null) gameView.RemoveChild(scene);
            scene = ResourceLoader.Load<PackedScene>($"res://Scenes/{timeline.current.label}.tscn").Instantiate<Node3D>();
            gameView.AddChild(scene);
            // LEGACY - использовалось в демо
            // // декорации
            // GD.Print("Generating decorations");
            // персонажи - запускают активности при клике
            // GD.Print("Generating persons");
            // var noel = Person.Find("Noel").Instance();
            // noel.node.Position = new Vector3(-5, 1, -9);
            // noel.node.SetMeta("dialogue", "Welcome" + CharactersEnum.Noel);
            // noel.node.SetMeta("replica", "Replica" + CharactersEnum.Noel);
            // предметы - запускают дополнительные активности при клике
            GD.Print("Generating items");
        });

        endDay.ButtonUp += () => {
            OnEndDay();
            // if (!machine.HasState("Interactable")) return;
            // GD.Print("Finishing day");
            // GD.Print("Interpretation");
            // GD.Print("Adding tasks");
            // // даётся выбор позволяющий интерпретировать действия персонажа и получить очки с учётом бонусов (работает как модификаторы)
            // // добавляются задачи
            // machine.RemoveState("NextDay");
            // machine.AddState(timeline.current.label + "End");
        };
    }

    public override void OnEnter() {
        Show();
        volumeSlider.Value = Rules.DbToVolume(rules.music.VolumeDb);
        rules.music.Stream = ResourceLoader.Load<AudioStream>("res://Music/Hub.mp3");
        rules.music.Play();
        // Запуск
        Tasks.active.Task(0.01, task => {
            machine.AddState(GenericStatesEnum.Interactable);
            machine.AddState(GenericStatesEnum.Movable);
            timeline.Event(EventsEnum.Opening, "Начало");
            // Tag.Apply(TagsEnum.Hub);
            // Tag.Apply(TagsEnum.Girls);
            // timeline.SetTime(0);
            machine.AddState(GenericStatesEnum.NextDay);
            // machine.AddState("Opening");
            machine.Step();
        });
        Tasks.active.Task(0.2, task => isActive = true);
    }

    public override void OnExit() {
        Reset();
        Hide();
        isActive = false;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
        var mousePosition = rules.GetViewport().GetMousePosition();
        
        tip.Visible = (isPointingObject || isPointingUi || isTutorial) && machine.HasState(GenericStatesEnum.Movable);

        // Обновляем координаты подсказки так, чтобы она не вылезала за экран
        if (tip.Visible) {
            tip.Position = new Vector2(
                tipPosition.X > rules.GetViewport().GetVisibleRect().Size.X / 2 ? tipPosition.X - tip.GetRect().Size.X: tipPosition.X,
                tipPosition.Y > rules.GetViewport().GetVisibleRect().Size.Y / 2 ? tipPosition.Y - tip.GetRect().Size.Y: tipPosition.Y
            );
        }

        foreach (var tag in Tag.tags) {
            machine._Process(delta);
            // Завершение дня при заканчивании количества действий
            // NOTE В результате тестирования решили отказаться
            // if (player.actionsCount <= 0 && machine.HasState(GenericStatesEnum.Interactable) && machine.HasState(GenericStatesEnum.NextDay)) {
            //     OnEndDay();
            //     machine.Step(); // TODO вынести из цикла
            // }
            // NOTE вынести из цикла?
            UpdateStats();
        }
    }

    // ---------------------------------- Game loop ----------------------------------

    public GameState Show() {
        rules.GetNode<Control>("/root/Node3D/Control").Show();
        GD.Print("Game state is now shown");
        return this;
    }
    
    public GameState Hide() {
        rules.GetNode<Control>("/root/Node3D/Control").Hide();
        GD.Print("Game state is now hidden");
        return this;
    }

    public void SetActionsCount(int count) {
        actions.Text = $"Действий: {count}";
    }

    public void IncreaseActionsCount(int count = 1) {
        SetActionsCount(--player.actionsCount);
    }

    public void OnMinigameFinished(bool result) {
        if (result) {
            GD.Print("You win!");
            // builder.ApplyEffect(bonus, 2, 0.5, Effect.Type.Positive);
            scores += ScoresEnum.Minigame;
        } else {
            GD.Print("You lose!");
            // builder.ApplyEffect(bonus, 1, 0.5, Effect.Type.Negative);
            // builder.ApplyEffect(bonus, 1, 0.5, Effect.Type.Neutral);
            scores = Math.Max(scores - ScoresEnum.Minigame, 0);
        }
        machine.Step(true);
    }

    public void UpdateStats() {
        var gaming = progress.GetNode<ProgressBar>("HBoxContainer/Gaming/Progress");
        var stamina = progress.GetNode<ProgressBar>("HBoxContainer/Stamina/Progress");
        var cooking = progress.GetNode<ProgressBar>("HBoxContainer/Cooking/Progress");
        var charisma = progress.GetNode<ProgressBar>("HBoxContainer/Charisma/Progress");
        var creativity = progress.GetNode<ProgressBar>("HBoxContainer/Creativity/Progress");
        var intelligence = progress.GetNode<ProgressBar>("HBoxContainer/Intelligence/Progress");
        var relations = progress.GetNode<HBoxContainer>("HBoxContainer/Relations");
        var balance = progress.GetNode<Label>("HBoxContainer/Balance");
        var playerName = Rules.active.GetNode<Label>("/root/Node3D/Control/MarginContainer3/Container/PlayerState/PlayerName/Name");
        gaming.Value = player.gaming;
        stamina.Value = player.stamina;
        cooking.Value = player.cooking;
        charisma.Value = player.charisma;
        creativity.Value = player.creativity;
        intelligence.Value = player.intelligence;
        // Всплывающий Progress Bar при появлении известного персонажа
        if (floating.HasFloating()) {
            var person = Person.Find(floating.label);
            if (person != null) {
                relations.GetNode<ProgressBar>("Progress").Value = person.relation;
                relations.Show();
            } else {
                relations.Hide();
            }
        } else {
            relations.Hide();
        }
        balance.Text = $"Баланс: {player.balance}";
        playerName.Text = player.name;
    }

    public void OnEndDay() {
        if (!machine.HasState(GenericStatesEnum.Interactable)) return;
        GD.Print("Finishing day");
        GD.Print("Interpretation");
        GD.Print("Adding tasks");
        // LEGACY - использовалось в демо. даётся выбор позволяющий интерпретировать действия персонажа и получить очки с учётом бонусов (работает как модификаторы)
        machine.RemoveState(GenericStatesEnum.NextDay);
        machine.AddState(timeline.current.label + "End");
        bonus.Clear();
    }

    public void Reset() {
        SetHudVisibility(true);
        narration.Clear();
        bonus.Clear();
        illustration.Clear();
        info.Clear();
        // progress.Clear();
        timeline.Clear();
        player.Restore().Clear();

        // actions
        // narration
        // bonus
        // illustration
        // info
        // progress
        // interactive
        // activity
        // items
        // floating
        // tip

        machine.Clear();
    }
}