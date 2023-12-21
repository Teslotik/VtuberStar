using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

class NarrationPlugin: TimePlugin {
    static Random random = new Random();

    Nfa.Machine machine;
    Builder.NarrationBuilder builder;
    Narration.Narration narration;
    Info info;
    Info illustration;
    Timeline timeline;
    Player player;
    Floating floating;
    NinePatchRect tip;

    public Vector3 characterPosition = new Vector3(5.5f, 0.1f, 2.0f);

    public delegate void LocatorCallback();

    override public void OnEnable(Rules rules) {
        machine = rules.gameState.machine;
        builder = rules.gameState.builder;
        narration = rules.gameState.narration;
        info = rules.gameState.info;
        illustration = rules.gameState.illustration;
        timeline = rules.gameState.timeline;
        player = rules.gameState.player;
        floating = rules.gameState.floating;
        tip = rules.gameState.tip;

        // ---------------------------------- Обучение ----------------------------------

        LocatorCallback locator = null;
        rules.Update += delta => locator?.Invoke();
        machine.State(EventsEnum.Tutorial);
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Action(state => rules.gameState.isTutorial = true);
        builder.Then(state => {
            locator = () => rules.gameState.tipPosition = rules.gameState.timeline.GetGlobalRect().GetCenter();
            tip.GetNode<Label>("MarginContainer/VBoxContainer/Title").Text = "Таймлайн";
            tip.GetNode<Label>("MarginContainer/VBoxContainer/Description").Text = @"Тут показан текущий день и возможные события";
        });
        builder.Then(state => {
            locator = () => rules.gameState.tipPosition = rules.gameState.bonus.GetGlobalRect().GetCenter();
            tip.GetNode<Label>("MarginContainer/VBoxContainer/Title").Text = "Эффекты дня";
            tip.GetNode<Label>("MarginContainer/VBoxContainer/Description").Text = @"Эта панель, в которой каждый день будут появляться новые случайные эффекты";
        });
        builder.Then(state => {
            locator = () => rules.gameState.tipPosition = rules.gameState.progress.GetGlobalRect().GetCenter();
            tip.GetNode<Label>("MarginContainer/VBoxContainer/Title").Text = "Характеристики";
            tip.GetNode<Label>("MarginContainer/VBoxContainer/Description").Text = @"Это Ваши навыки, которые Вы должны повышать, чтобы стать самым лучшим VTuber'ом. Для каждого типа стримов нужно качать определённые. Но не стоит забывать и о других";
        });
        builder.Then(state => {
            locator = () => rules.gameState.tipPosition = rules.gameState.items.GetGlobalRect().GetCenter();
            tip.GetNode<Label>("MarginContainer/VBoxContainer/Title").Text = "Предметы";
            tip.GetNode<Label>("MarginContainer/VBoxContainer/Description").Text = @"Здесь будут находиться как подбираемые предметы, так и покупные, которые дают эффекты при получении";
        });
        builder.Then(state => {
            floating.Appear(CharactersEnum.Luca, "Luca_saying");
            locator = () => rules.gameState.tipPosition = rules.gameState.progress.GetNode<Control>("HBoxContainer/Relations").GetGlobalRect().GetCenter();
            tip.GetNode<Label>("MarginContainer/VBoxContainer/Title").Text = "Отношения";
            tip.GetNode<Label>("MarginContainer/VBoxContainer/Description").Text = @"У каждой девочки есть свой показатель отношений к Вам. Общение с ней, а также дарение подарков увеличивает его. Кто знает, чем обернётся эта близость с Вами ¯|_(ツ)_/¯";
        });
        builder.Then(state => {
            floating.Clear();
            locator = () => rules.gameState.tipPosition = rules.gameState.endDay.GetGlobalRect().GetCenter();
            tip.GetNode<Label>("MarginContainer/VBoxContainer/Title").Text = "Завершить день";
            tip.GetNode<Label>("MarginContainer/VBoxContainer/Description").Text = @"Прежде чем завершать день, убедитесь, что вы сделали всё, что хотели, после чего запустится следующее событие";
        });
        builder.Then(state => info.Choice("Выберите сложность", new[]{
            "Сложность: Реальная жизнь"
        }, (index, choice) => {

        }));
        builder.Then(state => {
            locator = () => rules.gameState.tipPosition = rules.gameState.volumeSlider.GetGlobalRect().GetCenter() - new Vector2(0, 20);
            tip.GetNode<Label>("MarginContainer/VBoxContainer/Title").Text = "Игра";
            tip.GetNode<Label>("MarginContainer/VBoxContainer/Description").Text = @"Вы выбрали самую высокую сложность игры. Тут не будет ни настроек, ни сохранений и загрузок. Ну хорошо. Вот Вам ползунок звука";
        });
        builder.Then(state => {
            locator = () => rules.gameState.tipPosition = rules.gameState.illustration.GetGlobalRect().GetCenter();
            tip.GetNode<Label>("MarginContainer/VBoxContainer/Title").Text = "Суть игры";
            tip.GetNode<Label>("MarginContainer/VBoxContainer/Description").Text = @"За 30 игровых дней Вы должны решить, кем станет Ваш главный герой и прокачать навыки в определённой сфере стриминга. Подбирайте и покупайте предметы, ходите на работу и не забывайте общаться с девочками";
        });
        builder.Then(state => {
            locator = () => rules.gameState.tipPosition = rules.gameState.illustration.GetGlobalRect().GetCenter();
            tip.GetNode<Label>("MarginContainer/VBoxContainer/Title").Text = "И это ещё не всё!";
            tip.GetNode<Label>("MarginContainer/VBoxContainer/Description").Text = @"Всего существует 3 варианта развития событий. Вы можете закончить игру с какой-то из девочек. Но! Вы также можете забыть об их существовании и стать самым крутым и без их помощи :) Удачи! P.S. Не выходите из игры, иначе начнёте сначала";
        });
        builder.Action(state => {
            rules.gameState.isTutorial = false;
            locator = null;
            machine.AddState(GenericStatesEnum.Interactable);
        });
        builder.build(machine);


        // ---------------------------------- Дни ----------------------------------

        // Хаб
        machine.State(EventsEnum.Hub);
        builder.Action(state => {
            rules.music.Stream = ResourceLoader.Load<AudioStream>("res://Music/Hub.mp3");
            rules.music.Play();
        });
        builder.build(machine);



        // Работа
        machine.State(EventsEnum.Job);
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Action(state => {
            rules.music.Stream = ResourceLoader.Load<AudioStream>("res://Music/Hub.mp3");
            rules.music.Play();
        });
        builder.Then(state => {
            rules.gameState.SetHudVisibility(false);
            illustration.Image("Job", 0, false, false);
        });
        builder.Action(state => illustration.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.Action(state => machine.AddState(GenericStatesEnum.NextDay));
        builder.Action(state => rules.gameState.SetHudVisibility(true));
        builder.build(machine);

        machine.State(EventsEnum.Job + "End");


        // Магазин
        machine.State(EventsEnum.Shop);
        builder.Action(state => {
            rules.music.Stream = ResourceLoader.Load<AudioStream>("res://Music/Hub.mp3");
            rules.music.Play();
        });
        builder.build(machine);

        machine.State(EventsEnum.Shop + "End");
        builder.Action(state => machine.AddState(GenericStatesEnum.NextDay));
        builder.build(machine);



        // Интро
        machine.State(EventsEnum.Opening);
        // /*
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Action(state => rules.gameState.SetHudVisibility(false));
        builder.Action(state => illustration.Chapter("", "", 0, false, false));
        builder.Monologue(narration, "*Звук будильника*");
        builder.Monologue(narration, "В полудреме я слышу, как звенит будильник.");
        builder.Monologue(narration, "Нужно вставать и делать дела.");
        builder.Monologue(narration, "...");
        builder.Monologue(narration, "Но какие дела могут быть у обычного среднестатистического человека? Встать, умыться, позавтракать, идти на работу.");
        builder.Monologue(narration, "В наше время работать можно не выходя из дома. Чем, собственно, я и занимался на протяжении последних 5 лет.");
        builder.Action(state => illustration.Image("Monitor", 0, false, false));
        builder.Monologue(narration, "Особенно в нашу эпоху, где все удобства мира доступны на расстоянии протянутой руки. Я могу заниматься чем угодно, работать кем угодно и быть счастливым до конца моей жизни.");
        builder.Monologue(narration, ".....");
        builder.Monologue(narration, "Как скучно.");
        builder.Monologue(narration, "У меня с каждым днем растет странное чувство.");
        builder.Monologue(narration, "Чувство, что мне чего - то не хватает. Что - то хочется в жизни.");
        builder.Monologue(narration, "У меня нет каких либо больших амбиций, поэтому большую часть своей жизни я лениво сижу дома.");
        builder.Monologue(narration, "Серые скучные будни мне помогают скоротать стримы моих кумиров.");
        builder.Action(state => illustration.Image("Forum", 0, false, false));
        builder.Monologue(narration, "Vtuber - Это вымышленные персонажи, которые олицетворяют реальных людей.");
        builder.Monologue(narration, "Витуберы обычно записывают видео с помощью своих моделек, стримят и принимают прочие активности в жизни. Обычно реальная жизнь витубера строго охраняется, чтобы личность из ИРЛ не беспокоилась о своей безопасности.");
        builder.Monologue(narration, "Я обожаю проводить время за просмотром их видео или стримов. Это придает мне сил и уверенности.");
        builder.Monologue(narration, "Эти трое. Самые популярные Витуберы всего мира. Именно они радуют мою душу и наполняют мое сердце верой в человечество.");
        // builder.Action(state => illustration.IconsView("", new[]{"LucaStar", "MonphyStar", "RechumoeStar"}));
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_normal"));
        builder.Monologue(narration, "Монфис - Креативная, практичная и яркая звездочка.");
        builder.Action(state => floating.Appear(CharactersEnum.Luca, "Luca_normal"));
        builder.Monologue(narration, "Лука - Обаятельная, гиперактивная и непринужденная леди.");
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_normal"));
        builder.Monologue(narration, "Ретя - Милая, спокойная и ламповая булочка.");
        builder.Action(state => floating.Clear());
        builder.Monologue(narration, "Вся троица состоит в объединении, которое называется FruLive Vtubers.");
        builder.Monologue(narration, "Просмотры их стримов достигают до 500 тысяч человек, а их мерч скупают за секунду.");
        builder.Monologue(narration, "Они занимают топы самых популярных витуберов на планете.");
        builder.Monologue(narration, "Как обычно я сидел на форумах и участвовал в групповом чате.");
        builder.Monologue(narration, "Это были единственные друзья, с которыми я общался.");
        builder.Action(state => illustration.Image("Monitor", 0, false, false));
        builder.Action(state => narration.Clear());
        builder.Then(state => rules.gameState.activity.Create(MinigamesEnum.ChatBefore));
        builder.Action(state => Chat.Chat.onFinish = result => {
            Chat.Chat.onFinish = null;
            rules.gameState.interactive.Hide();
            machine.AddState(EventsEnum.Opening + "Lottery");
            machine.Step();
        });
        builder.build(machine);

        machine.State(EventsEnum.Opening + "Lottery");
        builder.Then(state => illustration.Image("Post", 0, false, false));
        builder.Monologue(narration, "Передо мной возник пост о результатах лотереи.");
        builder.Monologue(narration, "...");
        builder.Monologue(narration, "....");
        builder.Monologue(narration, ".....");
        builder.Monologue(narration, "Я вернулся на экран с групповым чатом.");
        builder.Action(state => narration.Clear());
        builder.Action(state => Chat.Chat.onFinish = result => {
            Chat.Chat.onFinish = null;
            rules.gameState.interactive.Hide();
            narration.Clear();
            machine.AddState(EventsEnum.Opening + "Chance");
            machine.Step();
        });
        builder.Action(state => rules.gameState.activity.Create(MinigamesEnum.ChatAfter));
        builder.build(machine);

        machine.State(EventsEnum.Opening + "Chance");
        builder.Monologue(narration, "Даже не знаю… Мне ничего не хочется менять в жизни. Я привык плыть по течению.");
        builder.Monologue(narration, "Но, с другой стороны, я давно хотел что-то поменять, ибо мне становиться скучно.");
        builder.Action(state => narration.Clear());
        builder.Action(state => info.Choice("Действительно, если так подумать, это отличный шанс. Но хочу ли я этого на самом деле?", new[]{
            "Да", "Нет"
        }, (index, choice) => {
            switch (index) {
                case 0:
                    machine.AddState(EventsEnum.Opening + "Accept");
                    break;
                case 1:
                    machine.AddState(EventsEnum.EndAvito);
                    break;
            }
            // machine.Step();
        }));
        builder.build(machine);

        machine.State(EventsEnum.EndAvito);
        builder.Monologue(narration, "Ай, как же мне лень. Отдам место кому нибудь другому. Ну или продам его на Авито.");
        builder.Action(state => {
            illustration.Clear();
            info.Chapter("", "", 0, false, false);
            // info.Chapter("Конец", "Спасибо за игру", 0, false, false);
        });
        builder.Action(state => Tasks.active.Task(0.01, task => {
            rules.machine.SetState(GenericStatesEnum.Ending);
            rules.machine.Step();
        }));
        builder.build(machine);

        machine.State(EventsEnum.Opening + "Accept");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Monologue(narration, "Пока не попробуешь - не узнаешь. Если не получится - все равно останется хороший опыт от проведенного времени.");
        builder.Monologue(narration, "Открыв страницу и подтвердив свою личность, мне прислали сообщение.");
        builder.Action(state => illustration.Image("Winner", 0, false, false));
        builder.Action(state => narration.Clear());
        builder.Action(state => Tasks.active.Task(3, task => {
            machine.isFreezed = false;
            machine.Step();
        }));
        builder.Action(state => machine.isFreezed = true);
        builder.Monologue(narration, "Я сделал все, что от меня требовали. Осталось только ввести свое имя и войти.");
        builder.Then(state => {
            machine.isFreezed = true;
            info.Registration(name => {
                GD.Print($"Registered as '{name}'");
                player.name = name;
                machine.isFreezed = false;
            });
        });
        builder.Then(state => {
            illustration.Clear();
            info.Clear();
            rules.gameState.SetHudVisibility(true);
        });
        // TODO Правильные floating изображения (с эмоциями)
        builder.Monologue(narration, "Передо мной появилась комната. В ней расположены три двери и светящийся пол с эмблемой Фрулайф. Всё это находилось в небольшом тусклом помещении.");
        builder.Monologue(narration, "Также, я заметил картонные версии девушек в полный рост, стоящих у дверей.");
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_normal"));
        builder.Dialogue(narration, "Монфис", "Тада-а-ам~");
        builder.Monologue(narration, "Из не откуда показалась Монфис и радостно поприветствовала меня.");
        builder.Monologue(narration, "За ней вышли остальные две девушки.");
        builder.Action(state => floating.Appear(CharactersEnum.Luca, "Luca_normal"));
        builder.Dialogue(narration, "Лука", "Не привычно смотреть на саму себя в таком виде.");
        builder.Monologue(narration, "Она указала на картонные 2D изображения у дверей.");
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_normal"));
        builder.Dialogue(narration, "Ретя", "На самом деле, мы не знали как украсить наше приложение… Дом…Игру? Я до сих пор не знаю, как нам его называть. Но Продик сказал: тренировочный лагерь.");
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_normal"));
        builder.Dialogue(narration, "Монфис", "Да-да. Все это еще в сыром виде, наш лагерь еще в процессе создания. Пока что в объединении только мы. Но, со временем, к нам придут множество замечательных людей!");
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_normal"));
        builder.Dialogue(narration, "Ретя", "Тут не так много вещей, но базовые потребности мы уже добавили.");
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_normal"));
        builder.Dialogue(narration, "Монфис", "Ты - наш первый ученик, и возможно, будущий новый участник объединения Фрулайва.");
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_normal"));
        builder.Dialogue(narration, "Ретя", "Мы мечтаем со временем стать больше и завести множество друзей втуберов, живущих и работающих с нами бок о бок.");
        builder.Action(state => floating.Appear(CharactersEnum.Luca, "Luca_normal"));
        builder.Dialogue(narration, "Лука", "Мы первые витуберы, у которых есть виртуальный дом. Так что, радуйся такой возможности. Ты теперь будешь прилагать дюжину усилий, чтобы стать таким же популярными, как и мы.");
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_normal"));
        builder.Dialogue(narration, "Ретя", "Ну не нужно так напрягать его, Лука. {0} нужно привыкнуть и дать время!");
        builder.Action(state => floating.Appear(CharactersEnum.Luca, "Luca_normal"));
        builder.Dialogue(narration, "Лука", "{0}?");
        builder.Dialogue(narration, "Лука", "У тебя очень необычно имя. Или это ник. Я надеюсь ты его поменяешь, когда станешь знаменитым.");
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_normal"));
        builder.Dialogue(narration, "Монфис", "Эй, не суди его по обложке.");
        builder.Action(state => floating.Appear(CharactersEnum.Luca, "Luca_normal"));
        builder.Dialogue(narration, "Лука", "Блах-Блах. Мне нужны действия и результат. Я буду требовать от тебя полной отдачи, дружочек-пирожочек!");
        builder.Monologue(narration, "Я напрягся. До этого момента я не ожидал, что меня ждет. Да что там говорить, я вообще изначально не думал, что выиграю в этой лотереи.");
        builder.Monologue(narration, "Ретя видимо заметила мое напряжение.");
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_normal"));
        builder.Dialogue(narration, "Ретя", "Не волнуйся ты так. Расслабься. Постарайся найти с нами одну волну, и все будет в порядке.");
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_normal"));
        builder.Dialogue(narration, "Монфис", "Для начала, тебе нужно определиться в какой сфере ты хочешь быть втубером.");
        builder.Action(state => floating.Appear(CharactersEnum.Luca, "Luca_normal"));
        builder.Dialogue(narration, "Лука", "Направлений существует неизмеримое количество. Но мы не можем уделять времени всему этому балагану. Тебе выдан всего месяц, а значит ты обойдешься и обычной базой.");
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_normal"));
        builder.Dialogue(narration, "Монфис", "Конечно, потом ты можешь добавлять к себе новые и новые увлечения, но ведь нужно же с чего то начать.");
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_normal"));
        builder.Dialogue(narration, "Ретя", "Я например больше провожу разговорные стримы. Иногда готовлю на них, делясь своим опытом.Также,  мы смотрим фильмы, сериалы и аниме. В моем случае, общение и поддержка очень важна как от меня, так и от зрителей.");
        builder.Action(state => floating.Appear(CharactersEnum.Luca, "Luca_normal"));
        builder.Dialogue(narration, "Лука", "Общение и поддержка нужна всем Ретя. Просто у каждого она в той или иной степени больше или меньше. Но не волнуйся, это не главное.");
        builder.Dialogue(narration, "Лука", "Я больше задрочу в играх. Не важно в каких. Мне все по душе. Но, чаще всего, играю в киберспортивные. Если хочешь быть как я, придется выработать скилл и выносливость, чтобы просидеть кучу времени за игрой.");
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_normal"));
        builder.Dialogue(narration, "Монфис", "Ну а я творческая личность. Я тоже много сижу, но не уделяю спорту большого внимания. Я слишком ленива, и мне бы не хотелось, что бы и ты стал таким. Но, кто знает.");
        builder.Dialogue(narration, "Монфис", "Я много рисую и читаю. Ты тоже можешь выбрать себе эту стезю, занимаясь чтением книг, развивая воображение и рисованием на чем угодно. Развивая навыки.");
        builder.Action(state => floating.Appear(CharactersEnum.Luca, "Luca_normal"));
        builder.Dialogue(narration, "Лука", "Но это лишь наши предпочтения и советы. Ты можешь сам выбрать, чему у нас учиться. Не нужно забывать о всех навыках что помогут тебе в этом нелёгком пути, мой дружочек.");
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_normal"));
        builder.Dialogue(narration, "Ретя", "Я надеюсь тебе у нас понравится.");
        builder.Dialogue(narration, "{0}", "Большое спасибо за такой теплый прием. Я постараюсь вас не подвести!");
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_normal"));
        builder.Dialogue(narration, "Монфис", "Ничего страшного, если ты нас подведешь, тебе всего-то придется иметь дело с нашим гневом.");
        builder.Monologue(narration, "Я нервно сглотнул");
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_normal"));
        builder.Dialogue(narration, "Ретя", "Монфис пытается сказать, что мы бы расстроились, если бы наши старания пропали даром но...");
        builder.Monologue(narration, "Лука перебила Ретю.");
        builder.Action(state => floating.Appear(CharactersEnum.Luca, "Luca_normal"));
        builder.Dialogue(narration, "Лука", "Но это твоя жизнь и тебе жить с последствиями своих УЖАСНЫХ выборов.");
        builder.Monologue(narration, "Последнее она выделила особой интонацией.");
        builder.Monologue(narration, "Затем она улыбнулась уголками губ и произнесла слово по буквам.");
        builder.Action(state => floating.Appear(CharactersEnum.Luca, "Luca_normal"));
        builder.Dialogue(narration, "Лука", "Ш У Т К А");
        builder.Monologue(narration, "Я почесал затылок и кивнул");
        builder.Dialogue(narration, "{0}", "Хорошо. Я постараюсь.");
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_normal"));
        builder.Dialogue(narration, "Монфис", "Вот и отличненько!");
        builder.Monologue(narration, "Девочки пропали ровно так же, как и появились. Скорее всего, они сейчас находятся в своих виртуальных комнатах.");
        builder.Monologue(narration, "Вот оно. Наконец - то в моей жизни случилось что - то особенное. Я смогу изменить ее и стать лучше.");
        builder.Action(state => {
            narration.Clear();
            floating.Clear();
            illustration.Clear();
        });
        builder.Action(state => machine.AddState(EventsEnum.Tutorial));
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Action(state => {
            rules.gameState.isTutorial = true;
            locator = () => rules.gameState.tipPosition = rules.gameState.timeline.GetGlobalRect().GetCenter() + new Vector2(0, 20);
            tip.GetNode<Label>("MarginContainer/VBoxContainer/Title").Text = "События";
            tip.GetNode<Label>("MarginContainer/VBoxContainer/Description").Text = @"Ознакомьтесь с событиями, которые будут ждать Вас в этом месяце";
        });
        // */
        builder.Then(state => {
            // Свидания по пятницам
            timeline.Event(EventsEnum.DateWithLuca, "Свидание", 4, e => {});
            timeline.Event(EventsEnum.DateWithMonphy, "Свидание", 11, e => {});
            timeline.Event(EventsEnum.DateWithRechumoe, "Свидание", 18, e => {});
            // Магазин по субботам
            timeline.Event(EventsEnum.Shop, "Магазин", 5, e => {});
            timeline.Event(EventsEnum.Shop, "Магазин", 12, e => {});
            timeline.Event(EventsEnum.Shop, "Магазин", 19, e => {});
            timeline.Event(EventsEnum.Shop, "Магазин", 26, e => {});
            // Работа по средам
            for (int day = 2; day < 30; day += 7) {
                timeline.Event(EventsEnum.Job, "Работа", day, e => {
                    player.balance += timeline.elapsed * 500;
                    GD.Print("Balance up");
                });
            }
            // Концовка
            timeline.Event(EventsEnum.End, "Конец", 29, e => {});

            machine.isFreezed = true;

            timeline.Focus(4);
            Tasks.active.Task(3, task => {
                timeline.RestoreFocus();
                timeline.Focus(11);
            });
            Tasks.active.Task(6, task => {
                timeline.RestoreFocus();
                timeline.Focus(18);
            });
            Tasks.active.Task(9, task => {
                timeline.RestoreFocus();
                timeline.Focus(29);
            });
            Tasks.active.Task(12, task => {
                timeline.RestoreFocus();
                machine.isFreezed = false;
                rules.gameState.isTutorial = false;
                locator = null;
                machine.Step();
            });
        });
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        // */
        builder.build(machine);

        machine.State(EventsEnum.Opening + "End");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Action(state => ScheduleDate(), null, 1, false);
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);


        // ---------------------------------- Свидания ----------------------------------

        machine.State(EventsEnum.DateWithLuca);
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Action(state => {
            rules.music.Stream = ResourceLoader.Load<AudioStream>("res://Music/Luca.mp3");
            rules.music.Play();
        });
        builder.Monologue(narration, "Я принял видеозвонок от Луки.");
        builder.Action(state => floating.Appear(CharactersEnum.Luca, "Luca_normal"));
        builder.Dialogue(narration, "Лука", "Эй, {0}, ты сейчас сильно занят?");
        builder.Dialogue(narration, "{0}", "Смотря что тебе нужно…..");
        builder.Action(state => floating.Appear(CharactersEnum.Luca, "Luca_normal"));
        builder.Dialogue(narration, "Лука", "Я просто хочу убедиться ,что ты занимаешься делом.");
        builder.Dialogue(narration, "Лука", "Не хочу внезапно узнать потом, что ты провалился на экзамене по приему во втуберство.");
        builder.Dialogue(narration, "{0}", "Что? Экзамен?");
        builder.Action(state => floating.Appear(CharactersEnum.Luca, "Luca_normal"));
        builder.Dialogue(narration, "Лука", "Ой, да не волнуйся. Там ничего сложного нет. Все что тебе нужно, это улучшать определенные навыки. Ты же этим занимаешься, я надеюсь?");
        builder.Dialogue(narration, "{0}", "Ну…. В какой то степени да.");
        builder.Action(state => floating.Appear(CharactersEnum.Luca, "Luca_normal"));
        builder.Dialogue(narration, "Лука", "Ладно, ближе к сути. Ты знаешь что такое Йога?");
        builder.Dialogue(narration, "{0}", "Немного. Люди принимают смешные позы, растягивают свои мышцы и получают умиротворение от медитации.");
        builder.Action(state => floating.Appear(CharactersEnum.Luca, "Luca_normal"));
        builder.Dialogue(narration, "Лука", "Примерно так и есть.");
        builder.Then(state => {
            illustration.Image("DateWithLuca", 0, false, false);
            rules.gameState.SetHudVisibility(false);
            floating.Clear();
            narration.Clear();
        });
        builder.Dialogue(narration, "Лука", "Просто повторяй за мной движения. Не торопись. Здесь важно правильно дышать и не делать резких движений");
        builder.Monologue(narration, "Я не любитель спорта, но не смог отказать Луке.");
        builder.Monologue(narration, "Она выглядит очень спортивной и в хорошей форме. Думаю Йога - не единственное, чем она занимается.");
        builder.Monologue(narration, "Я давно следил за ее творчеством как витубер.");
        builder.Monologue(narration, "Очень целеустремленная и уверенная в себе. Да, она немного требовательная, но это наверное все потому, что она хочет, что бы было все идеально.");
        builder.Monologue(narration, "Даже она может совершать ошибки, но глядя на то, как она яростно преодолевает все невзгоды...");
        builder.Monologue(narration, "Кажется, я смотрел на Луку слишком долго, и она заметила.");
        builder.Action(state => floating.Appear(CharactersEnum.Luca, "Luca_normal"));
        builder.Dialogue(narration, "Лука", "Не пялься так сильно на мою модельку. Она просто повторяет за мной движения в ИРЛ.");
        builder.Dialogue(narration, "{0}", "На самом деле, я восхищаюсь тем, как ты управляешь своим телом.");
        builder.Action(state => floating.Appear(CharactersEnum.Luca, "Luca_normal"));
        builder.Dialogue(narration, "Лука", "Чтобы стримить так много и так часто, нужно обладать хорошей выносливостью. Особенно, когда я играю в киберспортивные игры.");
        builder.Dialogue(narration, "Лука", "Приходится много сидеть, поэтому нужно держать себя в форме.");
        builder.Dialogue(narration, "Лука", "Конечно, еще нужно не забывать о типичных навыках в играх.");
        builder.Dialogue(narration, "Лука", "Даже тонкая настройка в игре поможет тебе выиграть матч.");
        builder.Action(state => floating.Appear(CharactersEnum.Luca, "Luca_normal"));
        builder.Dialogue(narration, "{0}", "Как ты справляешься с токсичными людьми в играх?");
        builder.Action(state => floating.Appear(CharactersEnum.Luca, "Luca_normal"));
        builder.Dialogue(narration, "Лука", "Хах. Они будут везде, и не только в играх. Зачем тратить на них время, если можно потратить на саморазвитие или игры. Тоже и с хейтерами.");
        builder.Dialogue(narration, "Лука", "Хоть я и играю в киберспортивные игры, и бываю груба, но я не позволяю себе сказать лишнего.");
        builder.Dialogue(narration, "Лука", "Особенно, когда ты медийная личность. Очень важно следить за тем, что ты говоришь.");
        builder.Dialogue(narration, "{0}", "Я ожидал, что мы будем играть в игры с тобой, но вместо этого - занимаюсь йогой. Я думаю, это лучший день в моей жизни.");
        builder.Monologue(narration, "Лука мило фыркнула что - то под нос вставая в очередную позу и мы продолжили.");
        builder.Monologue(narration, "Пока она беседовала со мной, я наблюдал и повторял за ней.");
        builder.Monologue(narration, "После такой физиотерапии я полон энергии и сил.");
        builder.Monologue(narration, "И кажется немного сблизился с Лукой.");
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => illustration.Clear());
        builder.Action(state => rules.gameState.SetHudVisibility(true));
        builder.Action(state => player.stamina += 25);
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(EventsEnum.DateWithLuca + "End");
        builder.Action(state => machine.AddState(GenericStatesEnum.NextDay));
        builder.build(machine);


        machine.State(EventsEnum.DateWithMonphy);
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Action(state => {
            rules.music.Stream = ResourceLoader.Load<AudioStream>("res://Music/Monphy.mp3");
            rules.music.Play();
        });
        builder.Monologue(narration, "Я принял видеозвонок от Монфис.");
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_normal"));
        builder.Dialogue(narration, "Монфис", "Здарова, {0} не хочешь попрактиковаться в рисовании?");
        builder.Dialogue(narration, "{0}", "Ого, я бы не отказался. Но с чего ты вдруг так внезапно?");
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_normal"));
        builder.Dialogue(narration, "Монфис", "Ты что, совсем не читал мелким шрифтом в договоре?");
        builder.Monologue(narration, "Я задумчиво начал вспоминать.");
        builder.Monologue(narration, "Монфис раздражительно вздохнула.");
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_normal"));
        builder.Dialogue(narration, "Монфис", "Там сказано, что в конце месяца ты обязан пройти тест на свои навыки.");
        builder.Dialogue(narration, "{0}", "Да. Я читал это.");
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_normal"));
        builder.Dialogue(narration, "Монфис", "Вот и чудненько. Садись и рисуй!");
        builder.Then(state => {
            illustration.Image("DateWithMonphy", 0, false, false);
            rules.gameState.SetHudVisibility(false);
            floating.Clear();
            narration.Clear();
        });
        builder.Dialogue(narration, "{0}", "Но, я сразу говорю, я еще не уверен в том, что у меня хорошо получается….");
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_normal"));
        builder.Dialogue(narration, "Монфис", "Уверен не уверен. Мне без разницы.");
        builder.Dialogue(narration, "Монфис", "Ничто не помогает лучше, как бесконечная практика. Конечно, у тебя не так много свободного времени, как у меня, но практикуясь каждый день, ты сможешь достичь любых высот.");
        builder.Monologue(narration, "Это правда. Сколько бы я не смотрел на то, как рисует Монфис на стримах, не могу перестать удивляться.");
        builder.Monologue(narration, "Она делает это так виртуозно, так легко и быстро. Некоторые фанаты думают, что она начертила пентаграмму, вызвала дьявола и продала душу….");
        builder.Monologue(narration, "Но, я думаю это всего лишь теории. Монфис всегда была упорной.");
        builder.Monologue(narration, "Ленивой. Но упорной по своему.");
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_normal"));
        builder.Dialogue(narration, "Монфис", "Куда ты ведешь линии. Ты что не смотришь на свое построение?");
        builder.Monologue(narration, "Я взглянул на экран графического планшета. Я слишком увлекся тем, что думал о ней и совершил пару ошибок.");
        builder.Dialogue(narration, "{0}", "Точно. И как ты все успеваешь отследить.");
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_normal"));
        builder.Dialogue(narration, "Монфис", "Ты должен успевать и рисовать и следить за чатом. Конечно, есть те, кто приходит и ставит тебя на фон, делая свои дела. Но зачастую, нужно не забывать общаться со зрителями.");
        builder.Dialogue(narration, "Монфис", "Поэтому важно тренировать не только навык рисования и воображения, но и учиться общаться и следить за тем, что люди пишут в чате.");
        builder.Monologue(narration, "Поразительно. Мне потребуется больше времени, чем я думал.");
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_normal"));
        builder.Dialogue(narration, "Монфис", "Когда ты рисуешь, ты должен почувствовать вдохновение. Рисунки без него - просто мусор. Я всегда настраиваюсь перед рисованием. Например, слушаю музыку или читаю мангу или манхву. Воображение поможет тебе, черпая силы отовсюду.");
        builder.Monologue(narration, "Мы просидели так еще немного времени, общаясь на разные темы. Спрашивали что нам нравиться и делились впечатлениями.");
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_normal"));
        builder.Dialogue(narration, "Монфис", "Ого, я и не думала, что тебе такое по душе. Обычно, я ленюсь делать многие вещи, но когда ты делаешь это с кем то - намного веселее.");
        builder.Monologue(narration, "Монфис улыбнулась широкой улыбкой и перевела взгляд на планшет.");
        builder.Monologue(narration, "Мне всегда нравилась эта ее сторона. Она очень простой и обычный по характеру человек.");
        builder.Monologue(narration, "Конечно она возможно и скрывает некоторые чувства. Да и кто будет открывать всё случайным людям.");
        builder.Monologue(narration, "Но я уверен в одном - она обязательно улыбнется всем невзгодам, что её настигнут и справиться!");
        builder.Monologue(narration, "Заметив мой взгляд Монфис немного покраснела.");
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_normal"));
        builder.Dialogue(narration, "Монфис", "Если хочешь рисовать меня, то попросил бы я дала бы референсы.");
        builder.Dialogue(narration, "{0}", "А, прости. Я задумался.");
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_normal"));
        builder.Dialogue(narration, "Монфис", "Вообще, часто пользуйся ими. Они помогут тебе понять строение лучше. Все мы знаем ту поговорку: все начиналось с срисовки.");
        builder.Dialogue(narration, "Монфис", "В творчестве нет определенных правил и рамок. Нет закона, который их прописывает. Ты волен рисовать так как хочешь.");
        builder.Dialogue(narration, "{0}", "Но есть рамки того, что можно а что нельзя рисовать.");
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_normal"));
        builder.Dialogue(narration, "Монфис", "Это да! Не стоит забывать и про это. Но я имею в виду именно процесс. Не дай другим указывать тебе, как нужно рисовать. Найди свой собственный стиль, самурай.");
        builder.Monologue(narration, "После этих слов, Монфис подняла подбородок вверх и поставила руки в боки. Тем самым изображая из себя сенсея.");
        builder.Dialogue(narration, "{0}", "Есть, учитель!");
        builder.Monologue(narration, "Мы еще немного поболтали. Обменялись мнениями и шутками. Вот так просто общаться с ней, мне нравится намного больше. Но не стоит забывать, что мое обучение еще не окончено.");
        builder.Monologue(narration, "Может быть после него нам удастся побыть вдвоем немного больше.");
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => illustration.Clear());
        builder.Action(state => rules.gameState.SetHudVisibility(true));
        builder.Action(state => player.creativity += 25);
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(EventsEnum.DateWithMonphy + "End");
        builder.Action(state => machine.AddState(GenericStatesEnum.NextDay));
        builder.build(machine);



        machine.State(EventsEnum.DateWithRechumoe);
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        // builder.Action(state => {
        //     rules.music.Stream = ResourceLoader.Load<AudioStream>("res://Music/Rechumoe.mp3");
        //     rules.music.Play();
        // });
        builder.Monologue(narration, "Я принял видеозвонок от Рети.");
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_normal"));
        builder.Dialogue(narration, "Ретя", "Эм…Привет {0}!");
        builder.Monologue(narration, "Я улыбнулся. Ретя всегда была для меня нежным лучиком солнца. Каждый раз, когда я приходил на ее стримы, я закутался в плед и пил горячий какао.");
        builder.Monologue(narration, "От нее всегда веет добротой и заботой.");
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_normal"));
        builder.Dialogue(narration, "Ретя", "Ты сейчас не занят? Я бы хотела тебя пригласить со мной что нибудь приготовить.");
        builder.Dialogue(narration, "{0}", "Конечно, я как раз свободен.");
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_normal"));
        builder.Dialogue(narration, "Ретя", "Я хочу быть уверена, что ты готов к выпуску.");
        builder.Dialogue(narration, "{0}", "Выпуску?");
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_normal"));
        builder.Dialogue(narration, "Ретя", "Ну… То есть. Убедиться что ты не завалишь экзамен, который будет в конце месяца.");
        builder.Dialogue(narration, "{0}", "Ах. Это. Не волнуйся, я все сделаю как надо.");
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_normal"));
        builder.Monologue(narration, "Ретя хлопнула ручками от восторга и попросила сменить локацию.");
        builder.Monologue(narration, "Я перенес камеру на кухню. Ретя же в свою очередь подошла к кухонной тумбе.");
        builder.Then(state => {
            illustration.Image("DateWithRechumoe", 0, false, false);
            rules.gameState.SetHudVisibility(false);
            floating.Clear();
            narration.Clear();
        });
        builder.Monologue(narration, "Она стояла в розовом фартуке и улыбалась мне в камеру.");
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_normal"));
        builder.Dialogue(narration, "Ретя", "Итак. Много ты знаешь о кулинарии?");
        builder.Dialogue(narration, "{0}", "Ну, я могу приготовить что-то нормальное. Но не являюсь шеф поваром пятизвездочного ресторана.");
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_normal"));
        builder.Monologue(narration, "Ретя мило хихикнула");
        builder.Dialogue(narration, "Ретя", "Оно и не нужно. Готовка поможет расслабиться. Ничего никуда не убежит. если ты спокойно за всем следишь.");
        builder.Dialogue(narration, "Ретя", "Или наоборот, понимать ингредиенты, правильную температуры и способы смешения помогают тебе развивать разные навыки.");
        builder.Monologue(narration, "Пока она говорила со мной, поднимала разные блюдца и чашки. Затем положила что-то в миску и начала смешивать.");
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_normal"));
        builder.Dialogue(narration, "Ретя", "Обязательно пробуй еду когда готовишь. Ты же не хочешь, чтобы твоя еда была невкусной. Даже если делаешь это не для себя.");
        builder.Dialogue(narration, "Ретя", "Ты же не луковый суп готовишь…");
        builder.Dialogue(narration, "{0}", "Луковый суп?");
        builder.Monologue(narration, "Ретя прикрыла рукой рот и посмеялась.");
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_normal"));
        builder.Dialogue(narration, "Ретя", "Нет-нет. Ничего. Просто кое-кто из моих подруг его готовил.");
        builder.Dialogue(narration, "Ретя", "Так вот. Пробовать еду обязательно надо.");
        builder.Dialogue(narration, "{0}", "Но пока я пробую ее, то могу съесть половину.");
        builder.Monologue(narration, "Посмеялся я про себя, но виду не подал. Лишь улыбнулся.");
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_normal"));
        builder.Dialogue(narration, "Ретя", "Вот поэтому готовить лучше на голодный желудок. Ну… не совсем голодный прямо.. ну.. ты понял.");
        builder.Dialogue(narration, "{0}", "Да-да, не волнуйся. Я понял.");
        builder.Monologue(narration, "Ретя очень добрый человек. Я даже думаю, у нее нет никаких минусов. Но ведь у каждого есть свои потайные проблемы и страхи.");
        builder.Monologue(narration, "Надеюсь Ретя делиться ими с кем - нибудь. Если нет, я готов быть всегда рядом. Если у меня получиться вступить к ним в объединение. Я бы хотел этого...");
        builder.Monologue(narration, "Я всегда видел ее жизнерадостной и веселой. Но никто не знает, что происходит  у не  в душе.");
        builder.Monologue(narration, "Я следил за тем, как она виртуозно управляет всеми приборами на ее кухне, параллельно делая то же самое.");
        builder.Monologue(narration, "Следил я пристально. И именно поэтому заметил, как на ее щеке остался кусочек чего то, что она только что попробовала.");
        builder.Dialogue(narration, "{0}", "У тебя что-то на лице-");
        builder.Monologue(narration, "Я потянул руку к экрану. Сам не понимая зачем. Как только мой палец столкнулся с монитором у щеки, где предположительно был кусочек еды, я машинально потер.");
        builder.Monologue(narration, "......");
        builder.Monologue(narration, "Только спустя секунд 5 я осознал, что только что сделал.");
        builder.Monologue(narration, "Ретя увидев это очень смутилась. Ее щеки залились краской и она тут же вытерла свою щеку.");
        builder.Monologue(narration, "Я смущенно почесал затылок.");
        builder.Dialogue(narration, "{0}", "Ха-ха. Прости, я не подумал. Смешно получилось.");
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_normal"));
        builder.Dialogue(narration, "Ретя", "Д-да. Хи-хихи");
        builder.Monologue(narration, "И ОЧЕНЬ неловко.");
        builder.Monologue(narration, "Пытаясь сменить тему я принялся продолжать готовить.");
        builder.Monologue(narration, "После бурного обсуждения, резки, варки и жарки наконец мы закончили.");
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_normal"));
        builder.Dialogue(narration, "Ретя", "Ну вот. Теперь попробуй что у тебя получилось.");
        builder.Monologue(narration, "Я взял кусочек в рот.");
        builder.Dialogue(narration, "{0}", "ОГО! Я еще никогда ничего такого вкусного не делал! Ретя! Ты волшебница!");
        builder.Monologue(narration, "Ретя мило хихикнула закрыв рот рукой.");
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_normal"));
        builder.Dialogue(narration, "Ретя", "Не я, а рецепты.");
        builder.Monologue(narration, "После этого мы еще немного поболтали и закончили наш кулинарный урок.");
        builder.Monologue(narration, "Кажется я немного сблизился с Ретей.");
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => illustration.Clear());
        builder.Action(state => rules.gameState.SetHudVisibility(true));
        builder.Action(state => player.cooking += 25);
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(EventsEnum.DateWithRechumoe + "End");
        builder.Action(state => machine.AddState(GenericStatesEnum.NextDay));
        builder.build(machine);



        // --- Концовки ---

        machine.State(EventsEnum.End, state => {
            rules.gameState.SetHudVisibility(false);
            var accumulated = player.gaming > 20 && player.stamina > 20 && player.cooking > 20 && player.charisma > 20 && player.creativity > 20 && player.intelligence > 20;
            if (!accumulated) {
                machine.AddState(EventsEnum.EndSingle);
                builder.Action(state => {
                    rules.music.Stream = ResourceLoader.Load<AudioStream>("res://Music/Hub.mp3");
                    rules.music.Play();
                });
            } else if (player.gaming >= 70 && player.stamina >= 70) {
                builder.Action(state => {
                    rules.music.Stream = ResourceLoader.Load<AudioStream>("res://Music/Luca.mp3");
                    rules.music.Play();
                });
                // Геймерская концовка
                if (Person.Find(CharactersEnum.Luca).relation >= 70) {
                    machine.AddState(EventsEnum.EndGamerLuca);
                } else {
                    machine.AddState(EventsEnum.EndSingleGamer);
                }
            } else if (player.cooking >= 70 && player.charisma >= 70) {
                builder.Action(state => {
                    rules.music.Stream = ResourceLoader.Load<AudioStream>("res://Music/Rechumoe.mp3");
                    rules.music.Play();
                });
                // Ламповая концовка
                if (Person.Find(CharactersEnum.Rechumoe).relation >= 70) {
                    machine.AddState(EventsEnum.EndRomanticRechumoe);
                } else {
                    machine.AddState(EventsEnum.EndSweetSingle);
                }
            } else if (player.creativity >= 70 && player.intelligence >= 70) {
                builder.Action(state => {
                    rules.music.Stream = ResourceLoader.Load<AudioStream>("res://Music/Monphy.mp3");
                    rules.music.Play();
                });
                // Творческая концовка
                if (Person.Find(CharactersEnum.Monphy).relation >= 70) {
                    machine.AddState(EventsEnum.EndCreativityMonphy);
                } else {
                    machine.AddState(EventsEnum.EndCreativitySingle);
                }
            } else {
                machine.AddState(EventsEnum.EndSingle);
            }
            machine.Step();
        });

        machine.State(EventsEnum.EndSingle);
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Action(state => illustration.Image(EventsEnum.EndSingle, 0, false, false));
        builder.Then(state => narration.Monologue("Даже не смотря на то, что я старался."));
        builder.Then(state => narration.Monologue("А может и не старался."));
        builder.Then(state => narration.Monologue("А может я вообще ничего толком и не делал."));
        builder.Then(state => narration.Monologue("Что-то из этого точно повлияло. Но теперь я уже не узнаю, какой бы была моя жизнь, если бы я в итоге стал витубером."));
        builder.Then(state => narration.Monologue("Девочки разочаровались во мне. Теперь мне предстоит продолжить свою скучную жизнь."));
        builder.Then(state => narration.Monologue("-Концовка: Не получилось, не фортануло-"));
        builder.Then(state => info.Chapter("-The End-", "", 0, false, false));
        builder.Then(state => info.Choice("Выбор", new[]{ "Попробовать еще раз" }, (index, selected) => {
            rules.machine.SetState(GenericStatesEnum.Ending);
            rules.machine.Step();
        }));
        builder.build(machine);

        machine.State(EventsEnum.EndSingleGamer);
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Action(state => illustration.Chapter("", "", 0, false, false));
        builder.Then(state => narration.Monologue("Это был нелегкий путь, который я смог пройти, преодолев все трудности."));
        builder.Action(state => illustration.Image(EventsEnum.EndSingleGamer, 0, false, false));
        builder.Then(state => narration.Monologue("Самым сложным было научится играть как профессионал и держать форму."));
        builder.Then(state => narration.Monologue("Остальное было не так важно. Меня интересовал только результат. Цифры, статистика и мои заслуги в игре."));
        builder.Then(state => narration.Monologue("Я долго шел к этому. Те люди, что приходили смотреть на мою игру - начинали мной восхищаться и завидовать."));
        builder.Then(state => narration.Monologue("С каждым днем мой онлайн возрастал вдвое. Нет в три раза."));
        builder.Then(state => narration.Monologue("Это был замечательный опыт, который я мог получить, находясь в объединении FruLive"));
        builder.Then(state => narration.Monologue("Как часть их семьи, я продолжал совершенствоваться и быть в топе лучших витуберов мира."));
        builder.Then(state => narration.Monologue("Как сложилась моя судьба дальше? Ну…."));
        builder.Then(state => narration.Monologue("Это уже совсем другая история."));
        builder.Then(state => narration.Monologue("-Концовка: Геймер одиночка-"));
        builder.Then(state => info.Chapter("-The End-", "", 0, false, false));
        builder.Then(state => {
            rules.machine.SetState(GenericStatesEnum.Ending);
            rules.machine.Step();
        });
        builder.build(machine);


        machine.State(EventsEnum.EndGamerLuca);
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Action(state => illustration.Chapter("", "", 0, false, false));
        builder.Then(state => narration.Monologue("Это был нелегкий путь, который я смог пройти, преодолев все трудности."));
        builder.Action(state => illustration.Image(EventsEnum.EndSingleGamer, 0, false, false));
        builder.Then(state => narration.Monologue("Самым сложным было научится играть как профессионал и держать форму."));
        builder.Then(state => narration.Monologue("Остальное было не так важно. Меня интересовал только результат. Цифры, статистика и мои заслуги в игре."));
        builder.Then(state => narration.Monologue("Я долго шел к этому. Те люди, что приходили смотреть на мою игру - начинали мной восхищаться и завидовать."));
        builder.Then(state => narration.Monologue("С каждым днем мой онлайн возрастал вдвое. Нет в три раза."));
        builder.Then(state => narration.Monologue("Это был замечательный опыт, который я мог получить, находясь в объединении FruLive"));
        builder.Then(state => narration.Monologue("Как часть их семьи, я продолжал совершенствоваться и быть в топе лучших витуберов мира."));
        builder.Action(state => illustration.Image(EventsEnum.EndGamerLuca, 0, false, false));
        builder.Then(state => narration.Monologue("Мы с Лукой очень сблизились."));
        builder.Then(state => narration.Monologue("Даже слишком. Настолько, что пришлось скрывать наши отношения."));
        builder.Then(state => narration.Monologue("Для Луки - это было очень важно, ведь она хотела поддерживать свой образ."));
        builder.Then(state => narration.Monologue("Но только я знал, какая она мягкая и милая, когда никто не видит."));
        builder.Then(state => narration.Monologue("Наши совместные игры набирали кучу просмотров, а организаторы чемпионатов по киберспорту приглашали нас на всевозможные турниры."));
        builder.Then(state => narration.Monologue("Ее великолепные навыки в совокупности с моими привели нас на вершину популярности."));
        builder.Then(state => narration.Monologue("Я был очень популярен среди девочек, но Лука мне совсем не ревновала. Она знала о моих истинных чувствах к ней."));
        builder.Then(state => narration.Monologue("Как сложилась моя судьба дальше? Ну…."));
        builder.Then(state => narration.Monologue("Это уже совсем другая история."));
        builder.Then(state => narration.Monologue("-Концовка: Геймер ловелас-"));
        builder.Then(state => info.Chapter("-The End-", "", 0, false, false));
        builder.Then(state => {
            rules.machine.SetState(GenericStatesEnum.Ending);
            rules.machine.Step();
        });
        builder.build(machine);


        machine.State(EventsEnum.EndCreativitySingle);
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Action(state => illustration.Chapter("", "", 0, false, false));
        builder.Then(state => narration.Monologue("Это был нелегкий путь, который я смог пройти, преодолев все трудности."));
        builder.Action(state => illustration.Image(EventsEnum.EndCreativitySingle, 0, false, false));
        builder.Then(state => narration.Monologue("Самым сложным было научится творческим навыкам и развивать свое воображение."));
        builder.Then(state => narration.Monologue("Остальное было не так важно. Меня интересовал только результат."));
        builder.Then(state => narration.Monologue("Количество артов, подборка цвета и стиль - были моим вторым именем."));
        builder.Then(state => narration.Monologue("Я долго шел к этому. Те люди, что приходили смотреть на то, как я рисую - вдохновлялись на собственные достижения."));
        builder.Then(state => narration.Monologue("С каждым днем мой онлайн возрастал вдвое. Нет в три раза."));
        builder.Then(state => narration.Monologue("Это был замечательный опыт, который я мог получить, находясь в объединении FruLive"));
        builder.Then(state => narration.Monologue("Как часть их семьи, я продолжал совершенствоваться и быть в топе лучших витуберов мира."));
        builder.Then(state => narration.Monologue("Как сложилась моя судьба дальше? Ну…."));
        builder.Then(state => narration.Monologue("Это уже совсем другая история."));
        builder.Then(state => narration.Monologue("-Концовка: Творец одиночка-"));
        builder.Then(state => info.Chapter("-The End-", "", 0, false, false));
        builder.Then(state => {
            rules.machine.SetState(GenericStatesEnum.Ending);
            rules.machine.Step();
        });
        builder.build(machine);


        machine.State(EventsEnum.EndCreativityMonphy);
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Action(state => illustration.Chapter("", "", 0, false, false));
        builder.Then(state => narration.Monologue("Это был нелегкий путь, который я смог пройти, преодолев все трудности."));
        builder.Action(state => illustration.Image(EventsEnum.EndCreativitySingle, 0, false, false));
        builder.Then(state => narration.Monologue("Самым сложным было научится творческим навыкам и развивать свое воображение."));
        builder.Then(state => narration.Monologue("Остальное было не так важно. Меня интересовал только результат."));
        builder.Then(state => narration.Monologue("Количество артов, подборка цвета и стиль - были моим вторым именем."));
        builder.Then(state => narration.Monologue("Я долго шел к этому. Те люди, что приходили смотреть на то, как я рисую - вдохновлялись на собственные достижения."));
        builder.Then(state => narration.Monologue("С каждым днем мой онлайн возрастал вдвое. Нет в три раза."));
        builder.Then(state => narration.Monologue("Это был замечательный опыт, который я мог получить, находясь в объединении FruLive"));
        builder.Then(state => narration.Monologue("Как часть их семьи, я продолжал совершенствоваться и быть в топе лучших витуберов мира."));
        builder.Action(state => illustration.Image(EventsEnum.EndCreativityMonphy, 0, false, false));
        builder.Then(state => narration.Monologue("Мы с Монфис очень сблизились."));
        builder.Then(state => narration.Monologue("Даже слишком. Настолько, что пришлось скрывать наши отношения."));
        builder.Then(state => narration.Monologue("Для Монфис, конечно, это было не так важно. Ее не волновало то, что люди думали о нас. Но, на всякий случай, это нужно было для общего спокойствия."));
        builder.Then(state => narration.Monologue("Наши совместные стримы по рисованию смотрело огромное количество человек. Люди заказывали у нас картины и выставляли их у себя в домах."));
        builder.Then(state => narration.Monologue("Ее великолепные навыки в совокупности с моими привели нас на вершину популярности."));
        builder.Then(state => narration.Monologue("Как сложилась моя судьба дальше? Ну…."));
        builder.Then(state => narration.Monologue("Это уже совсем другая история."));
        builder.Then(state => narration.Monologue("-Концовка: Творческая душа-"));
        builder.Then(state => info.Chapter("-The End-", "", 0, false, false));
        builder.Then(state => {
            rules.machine.SetState(GenericStatesEnum.Ending);
            rules.machine.Step();
        });
        builder.build(machine);


        machine.State(EventsEnum.EndSweetSingle);
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Action(state => illustration.Chapter("", "", 0, false, false));
        builder.Then(state => narration.Monologue("Это был нелегкий путь, который я смог пройти, преодолев все трудности."));
        builder.Action(state => illustration.Image(EventsEnum.EndSweetSingle, 0, false, false));
        builder.Then(state => narration.Monologue("Самым сложным было научится готовить, и быть внимательным ко всем в чате."));
        builder.Then(state => narration.Monologue("Остальное было не так важно. Меня интересовало только мое общение с людьми и понимание каждого из них."));
        builder.Then(state => narration.Monologue("Мы занимались просмотром аниме,сериалов и фильмов."));
        builder.Then(state => narration.Monologue("Я долго шел к этому. Не легко было научиться понимать психологию человека."));
        builder.Then(state => narration.Monologue("Те люди, что приходили смотреть вместе со мной или готовить - оставались на долго. А некоторые рекомендовали меня другим."));
        builder.Then(state => narration.Monologue("С каждым днем мой онлайн возрастал вдвое. Нет в три раза."));
        builder.Then(state => narration.Monologue("Это был замечательный опыт, который я мог получить, находясь в объединении FruLive"));
        builder.Then(state => narration.Monologue("Как часть их семьи, я продолжал совершенствоваться и быть в топе лучших витуберов мира."));
        builder.Then(state => narration.Monologue("Как сложилась моя судьба дальше? Ну…."));
        builder.Then(state => narration.Monologue("Это уже совсем другая история."));
        builder.Then(state => narration.Monologue("-Концовка: Романтическое соло-"));
        builder.Then(state => info.Chapter("-The End-", "", 0, false, false));
        builder.Then(state => {
            rules.machine.SetState(GenericStatesEnum.Ending);
            rules.machine.Step();
        });
        builder.build(machine);


        machine.State(EventsEnum.EndRomanticRechumoe);
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Action(state => illustration.Chapter("", "", 0, false, false));
        builder.Then(state => narration.Monologue("Это был нелегкий путь, который я смог пройти, преодолев все трудности."));
        builder.Action(state => illustration.Image(EventsEnum.EndSweetSingle, 0, false, false));
        builder.Then(state => narration.Monologue("Самым сложным было научится готовить, и быть внимательным ко всем в чате."));
        builder.Then(state => narration.Monologue("Остальное было не так важно. Меня интересовало только мое общение с людьми и понимание каждого из них."));
        builder.Then(state => narration.Monologue("Мы занимались просмотром аниме,сериалов и фильмов."));
        builder.Then(state => narration.Monologue("Я долго шел к этому. Не легко было научиться понимать психологию человека."));
        builder.Then(state => narration.Monologue("Те люди, что приходили смотреть вместе со мной или готовить - оставались на долго. А некоторые рекомендовали меня другим."));
        builder.Then(state => narration.Monologue("С каждым днем мой онлайн возрастал вдвое. Нет в три раза."));
        builder.Then(state => narration.Monologue("Это был замечательный опыт, который я мог получить, находясь в объединении FruLive"));
        builder.Then(state => narration.Monologue("Как часть их семьи, я продолжал совершенствоваться и быть в топе лучших витуберов мира."));
        builder.Action(state => illustration.Image(EventsEnum.EndRomanticRechumoe, 0, false, false));
        builder.Then(state => narration.Monologue("Мы с Ретей очень сблизились."));
        builder.Then(state => narration.Monologue("Даже слишком. Настолько, что пришлось скрывать наши отношения."));
        builder.Then(state => narration.Monologue("Для Рети это было немного волнительно. Она стеснялась. И, даже, немного боялась перемен. Но со временем она привыкла."));
        builder.Then(state => narration.Monologue("Как и привыкли к нам другие. Люди поддерживали нас и желали удачи."));
        builder.Then(state => narration.Monologue("Наши совместные стримы получали кучу просмотров. Людям было приятно сидеть с нами и общаться на разные темы.."));
        builder.Then(state => narration.Monologue("Ее милый характер и наша романтическая аура делали стримы все популярней и популярней."));
        builder.Then(state => narration.Monologue("Я даже думал предложить в шутку витуберскую свадьбу, но… пока еще рано."));
        builder.Then(state => narration.Monologue("Как сложилась моя судьба дальше? Ну…."));
        builder.Then(state => narration.Monologue("Это уже совсем другая история."));
        builder.Then(state => narration.Monologue("-Концовка: Романтическое Дуо-"));
        builder.Then(state => info.Chapter("-The End-", "", 0, false, false));
        builder.Then(state => {
            rules.machine.SetState(GenericStatesEnum.Ending);
            rules.machine.Step();
        });
        builder.build(machine);

        // LEGACY Фестиваль
        machine.State(EventsEnum.Festival);


        // ---------------------------------- Комнаты взаимодействия/фарма ----------------------------------

        machine.State(EventsEnum.RoomMonphy);
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Action(state => {
            rules.music.Stream = ResourceLoader.Load<AudioStream>("res://Music/Monphy.mp3");
            rules.music.Play();
        });
        builder.Action(state => GenerateItems(rules.gameState.scene, player.actionsCount));
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_saying", label => {
            machine.RemoveState(GenericStatesEnum.Interactable);
            Person.Find(CharactersEnum.Monphy).relation += ScoresEnum.Replica;
            machine.AddState(Metastrings.Replica + CharactersEnum.Monphy + random.Next(0, 9));
        }));
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(EventsEnum.RoomMonphy + "End");
        builder.Action(state => floating.Clear());
        builder.Then(state => ScheduleDate());
        builder.build(machine);



        machine.State(EventsEnum.RoomLuca);
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Action(state => {
            rules.music.Stream = ResourceLoader.Load<AudioStream>("res://Music/Luca.mp3");
            rules.music.Play();
        });
        builder.Action(state => GenerateItems(rules.gameState.scene, player.actionsCount));
        builder.Action(state => floating.Appear(CharactersEnum.Luca, "Luca_saying", label => {
            machine.RemoveState(GenericStatesEnum.Interactable);
            Person.Find(CharactersEnum.Luca).relation += ScoresEnum.Replica;
            machine.AddState(Metastrings.Replica + CharactersEnum.Luca + random.Next(0, 9));
        }));
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(EventsEnum.RoomLuca + "End");
        builder.Action(state => floating.Clear());
        builder.Then(state => ScheduleDate());
        builder.build(machine);




        machine.State(EventsEnum.RoomRechumoe);
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Action(state => {
            rules.music.Stream = ResourceLoader.Load<AudioStream>("res://Music/Rechumoe.mp3");
            rules.music.Play();
        });
        builder.Action(state => GenerateItems(rules.gameState.scene, player.actionsCount));
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_saying", label => {
            machine.RemoveState(GenericStatesEnum.Interactable);
            Person.Find(CharactersEnum.Rechumoe).relation += ScoresEnum.Replica;
            machine.AddState(Metastrings.Replica + CharactersEnum.Rechumoe + random.Next(0, 8));
        }));
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(EventsEnum.RoomRechumoe + "End");
        builder.Action(state => floating.Clear());
        builder.Then(state => ScheduleDate());
        builder.build(machine);

        // ---------------------------------- Диалоги ----------------------------------

        machine.State(EventsEnum.ToyPresent);
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Action(state => floating.Appear(CharactersEnum.Rechumoe, "Rechumoe_saying"));
        builder.Dialogue(narration, CharactersEnum.Rechumoe, "Это мне? Спасибо \\(ᵔᵕᵔ)/");
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(EventsEnum.EnergyPresent);
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Action(state => floating.Appear(CharactersEnum.Monphy, "Monphy_saying"));
        builder.Dialogue(narration, CharactersEnum.Monphy, "Это мне? Спасибо \\(ᵔᵕᵔ)/");
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(EventsEnum.BeerPresent);
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Action(state => floating.Appear(CharactersEnum.Luca, "Luca_saying"));
        builder.Dialogue(narration, CharactersEnum.Luca, "Это мне? Спасибо \\(ᵔᵕᵔ)/");
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        // ---------------------------------- Рандомные реплики при взаимодействии ----------------------------------

        // --- Monphy ---

        machine.State(Metastrings.Replica + CharactersEnum.Monphy + "0");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Monphy, "- Привет, сегодня ты на удивление вовремя.");
        builder.Dialogue(narration, CharactersEnum.Monphy, "- Мне нравиться твой настрой. Надеюсь ты продолжишь в том же духе.");
        builder.Action(state => Person.Find(CharactersEnum.Monphy).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(Metastrings.Replica + CharactersEnum.Monphy + "1");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Monphy, "- .... А?");
        builder.Dialogue(narration, CharactersEnum.Monphy, "- Что?");
        builder.Dialogue(narration, CharactersEnum.Monphy, "- Прости я задумалась.");
        builder.Dialogue(narration, CharactersEnum.Monphy, "- Мне хочется нарисовать одну картину, но ты сам знаешь как оно бывает.");
        builder.Dialogue(narration, CharactersEnum.Monphy, "- В голове ты видишь одно - на холсте получается другое.");
        builder.Action(state => Person.Find(CharactersEnum.Monphy).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        // TODO Если отношения больше половины
        machine.State(Metastrings.Replica + CharactersEnum.Monphy + "2");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Monphy, "- Хочешь совет?");
        builder.Dialogue(narration, CharactersEnum.Monphy, "- Чтобы стать хорошим художником, нужно обладать навыками творчества и хорошего воображения.");
        builder.Dialogue(narration, CharactersEnum.Monphy, "- Попробуй почитать книги.");
        builder.Action(state => Person.Find(CharactersEnum.Monphy).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        // TODO Если отношения меньше половины но немножко есть
        machine.State(Metastrings.Replica + CharactersEnum.Monphy + "3");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Monphy, "- Не забывай, что одного какого - то определенного навыка не хватит, что - бы стать супер пупер Витубером.");
        builder.Dialogue(narration, CharactersEnum.Monphy, "- Старайся распределить побочные между собой тоже, уделяя вниманию двум основным.");
        builder.Action(state => Person.Find(CharactersEnum.Monphy).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(Metastrings.Replica + CharactersEnum.Monphy + "4");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Monphy, "- Не будешь часто рисовать - не будет прогресса.");
        builder.Dialogue(narration, CharactersEnum.Monphy, "- Я?");
        builder.Dialogue(narration, CharactersEnum.Monphy, "- Ну… Мы сейчас не обо мне говорим!");
        builder.Action(state => Person.Find(CharactersEnum.Monphy).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(Metastrings.Replica + CharactersEnum.Monphy + "5");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Monphy, "Заводи будильник, что-бы у тебя было хоть какое-то расписание.");
        builder.Dialogue(narration, "{0}", "Я?");
        builder.Dialogue(narration, CharactersEnum.Monphy, "Ну…. мы не обо мне говорим!");
        builder.Action(state => Person.Find(CharactersEnum.Monphy).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(Metastrings.Replica + CharactersEnum.Monphy + "6");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Monphy, "-Ля-ля-ля-ля….");
        builder.Dialogue(narration, "{0}", "Кажется она поет какую то песенку.");
        builder.Dialogue(narration, "{0}", "У нее хорошее настроение.");
        builder.Action(state => Person.Find(CharactersEnum.Monphy).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(Metastrings.Replica + CharactersEnum.Monphy + "7");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Monphy, "- Готовка?");
        builder.Dialogue(narration, CharactersEnum.Monphy, "- Ну….");
        builder.Dialogue(narration, CharactersEnum.Monphy, "- Я мало что знаю о ней.");
        builder.Dialogue(narration, CharactersEnum.Monphy, "- Как то раз я просыпала пакет гречки…");
        builder.Action(state => Person.Find(CharactersEnum.Monphy).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(Metastrings.Replica + CharactersEnum.Monphy + "8");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Monphy, "-Как ты думаешь, что лучше: Традишка или Диджитл?");
        builder.Action(state => Person.Find(CharactersEnum.Monphy).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(Metastrings.Replica + CharactersEnum.Monphy + "9");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, "{0}", "(Кажется, Монфис что-то увлеченно рисует. Не буду ее отвлекать.)");
        builder.Action(state => Person.Find(CharactersEnum.Monphy).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        // --- Luca ---
        
        machine.State(Metastrings.Replica + CharactersEnum.Luca + "0");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Luca, "Привет, {0}. Что сегодня будем делать?");
        builder.Action(state => Person.Find(CharactersEnum.Luca).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(Metastrings.Replica + CharactersEnum.Luca + "1");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Luca, "Играть в игры тоже нужно уметь.");
        builder.Dialogue(narration, CharactersEnum.Luca, "Для кого-то это развлечение.");
        builder.Dialogue(narration, CharactersEnum.Luca, "Для кого то работа.");
        builder.Dialogue(narration, CharactersEnum.Luca, "А кто-то - я.");
        builder.Action(state => Person.Find(CharactersEnum.Luca).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(Metastrings.Replica + CharactersEnum.Luca + "2");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Luca, "(Зевок)");
        builder.Dialogue(narration, CharactersEnum.Luca, "Я вчера играла всю ночь в новую игру.");
        builder.Dialogue(narration, CharactersEnum.Luca, "Нужно восстановить режим, иначе я не смогу нормально стримить.");
        builder.Action(state => Person.Find(CharactersEnum.Luca).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(Metastrings.Replica + CharactersEnum.Luca + "3");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Luca, "Эй!");
        builder.Dialogue(narration, CharactersEnum.Luca, "Только не трогай тут ничего, что не касается твоего обучения!");
        builder.Action(state => Person.Find(CharactersEnum.Luca).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        // TODO Если отношения больше половины
        machine.State(Metastrings.Replica + CharactersEnum.Luca + "4");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Luca, "- Совет?");
        builder.Dialogue(narration, CharactersEnum.Luca, "- Зачем мне тебе подсказывать.");
        builder.Dialogue(narration, CharactersEnum.Luca, "- Ты сам должен уже все хорошо понимать.");
        builder.Dialogue(narration, CharactersEnum.Luca, "- Ну ладно…");
        builder.Dialogue(narration, CharactersEnum.Luca, "- Попробуй прокачивать не только навыки в играх но и выносливость.");
        builder.Dialogue(narration, CharactersEnum.Luca, "- Тебе придется много сидеть и играть.");
        builder.Dialogue(narration, CharactersEnum.Luca, "- Но!");
        builder.Dialogue(narration, CharactersEnum.Luca, "- Не забывай,что другие навыки тоже важны.");
        builder.Action(state => Person.Find(CharactersEnum.Luca).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        // TODO Если отношения меньше половины но немножко есть
        machine.State(Metastrings.Replica + CharactersEnum.Luca + "5");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Luca, "- Ты думаешь, что тебе достаточно прокачать пару навыков - и ты станешь крутым втубером?");
        builder.Dialogue(narration, CharactersEnum.Luca, "- Пф-ф-ф.");
        builder.Dialogue(narration, CharactersEnum.Luca, "- Не смеши меня.");
        builder.Dialogue(narration, CharactersEnum.Luca, "- Не забывай, что у тебя есть и другие навыки, которые важны в равной степени.");
        builder.Action(state => Person.Find(CharactersEnum.Luca).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(Metastrings.Replica + CharactersEnum.Luca + "6");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Luca, "Что бы сегодня такого покушать.");
        builder.Dialogue(narration, CharactersEnum.Luca, "Заказать или попросить Ретю приготовить.");
        builder.Action(state => Person.Find(CharactersEnum.Luca).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(Metastrings.Replica + CharactersEnum.Luca + "7");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, "{0}", "(кажется Лука витает в облаках или о чем-то задумалась. Не буду ее беспокоить.");
        builder.Action(state => Person.Find(CharactersEnum.Luca).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(Metastrings.Replica + CharactersEnum.Luca + "8");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Luca, "В здоровом теле - здоровый дух.");
        builder.Dialogue(narration, CharactersEnum.Luca, "Что?");
        builder.Dialogue(narration, CharactersEnum.Luca, "Нет я не верю в призраков.");
        builder.Action(state => Person.Find(CharactersEnum.Luca).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(Metastrings.Replica + CharactersEnum.Luca + "9");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Luca, "И раз. И два.И три.");
        builder.Dialogue(narration, "{0}", "Не буду ее беспокоить.");
        builder.Action(state => Person.Find(CharactersEnum.Luca).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        // --- Rechumoe ---

        machine.State(Metastrings.Replica + CharactersEnum.Rechumoe + "0");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Rechumoe, "Привет, {0}! Мне нравиться то, что ты полон решимости. Постараемся сегодня, хорошо?");
        builder.Action(state => Person.Find(CharactersEnum.Rechumoe).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(Metastrings.Replica + CharactersEnum.Rechumoe + "1");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Rechumoe, "Какой чудесный день.");
        builder.Dialogue(narration, CharactersEnum.Rechumoe, "Может стоит прогуляться.");
        builder.Dialogue(narration, CharactersEnum.Rechumoe, "Жаль у нас тут нет ничего, кроме комнат.");
        builder.Action(state => Person.Find(CharactersEnum.Rechumoe).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(Metastrings.Replica + CharactersEnum.Rechumoe + "2");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Rechumoe, "Очень важно правильно питаться и вовремя ложиться спать.");
        builder.Dialogue(narration, CharactersEnum.Rechumoe, "Я надеюсь ты хорошо питаешься?");
        builder.Action(state => Person.Find(CharactersEnum.Rechumoe).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(Metastrings.Replica + CharactersEnum.Rechumoe + "3");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Rechumoe, "Если хочешь, я могу каждый день тебе готовить…");
        builder.Dialogue(narration, CharactersEnum.Rechumoe, "Ох, правда ты не сможешь есть настоящую еду…");
        builder.Dialogue(narration, CharactersEnum.Rechumoe, "Но ведь времяпровождение  с человеком очень важно!");
        builder.Action(state => Person.Find(CharactersEnum.Rechumoe).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(Metastrings.Replica + CharactersEnum.Rechumoe + "4");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, "{0}", "(Кажеться, Ретя погружена в свои мысли и что-то готвоит на кухне. Не буду ее отвлекать.)");
        builder.Action(state => Person.Find(CharactersEnum.Rechumoe).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(Metastrings.Replica + CharactersEnum.Rechumoe + "5");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, "{0}", "(Кажеться, Ретя о чем то задумалась. Выглядит она… не знаю, грустной?Надеюсь у нее все хорошо.)");
        builder.Action(state => Person.Find(CharactersEnum.Rechumoe).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(Metastrings.Replica + CharactersEnum.Rechumoe + "6");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Rechumoe, "Иногда, мне кажется, что ты единственный, кто меня понимает.");
        builder.Dialogue(narration, CharactersEnum.Rechumoe, "О, привет {0}.");
        builder.Dialogue(narration, CharactersEnum.Rechumoe, "Я?");
        builder.Dialogue(narration, CharactersEnum.Rechumoe, "Я разговаривала со своей игрушкой.");
        builder.Action(state => Person.Find(CharactersEnum.Rechumoe).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(Metastrings.Replica + CharactersEnum.Rechumoe + "7");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Rechumoe, "Когда ты общаешься с чатом, важно помнить, что люди все разные.");
        builder.Dialogue(narration, CharactersEnum.Rechumoe, "Нужно уважать чужое мнение, каким бы оно не было.");
        builder.Dialogue(narration, CharactersEnum.Rechumoe, "Выслушать и рассказать о своем.");
        builder.Action(state => Person.Find(CharactersEnum.Rechumoe).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        machine.State(Metastrings.Replica + CharactersEnum.Rechumoe + "8");
        builder.Action(state => machine.RemoveState(GenericStatesEnum.Interactable));
        builder.Dialogue(narration, CharactersEnum.Rechumoe, "Если ты понимаешь, что тебе трудно будет следить за чатом - ты всегда можешь сделать перерыв.");
        builder.Dialogue(narration, CharactersEnum.Rechumoe, "Не перетрудись.");
        builder.Dialogue(narration, CharactersEnum.Rechumoe, "Хоть стрим и твоя работа, не забывай отдыхать!");
        builder.Action(state => Person.Find(CharactersEnum.Rechumoe).relation += 5);
        builder.Action(state => narration.Clear());
        builder.Action(state => floating.Clear());
        builder.Action(state => machine.AddState(GenericStatesEnum.Interactable));
        builder.build(machine);

        // ---------------------------------- Ежедневные пассивные действия ----------------------------------

    }

    string[] choicesGoToGirls(string title, string[] choices, ChoiceCallback onSelected) {
        var goToMonphy = "Запланировать встречу с Монфис";
        var goToLuca = "Запланировать встречу с Лукой";
        var goToRechumoe = "Запланировать встречу с Рётей";
        
        var variants = choices.ToList();
        variants.Add(goToMonphy);
        variants.Add(goToLuca);
        variants.Add(goToRechumoe);

        info.Choice(title, variants.ToArray(), (index, selected) => {
            if (selected == goToMonphy) {
                timeline.Event(EventsEnum.RoomMonphy, "Монфис");
                machine.AddState("NextDay");
            } else if (selected == goToLuca) {
                timeline.Event(EventsEnum.RoomLuca, "Лука");
                machine.AddState("NextDay");
            } else if (selected == goToRechumoe) {
                timeline.Event(EventsEnum.RoomRechumoe, "Рёта");
                machine.AddState("NextDay");
            } else {
                onSelected(index, selected);
            }
        });
        return null;
    }

    public void ScheduleDate() {
        if (timeline.GetTime() >= 28) {
            machine.AddState("NextDay");
            return;
        }
        info.IconsChoice("Запланировать встречу", new[]{
            "LucaStar", "MonphyStar", "RechumoeStar"
        }, (index, selected) => {
            switch (index) {
                case 0:
                    timeline.Event(EventsEnum.RoomLuca, "Лука");
                    machine.AddState("NextDay");
                    break;
                case 1:
                    timeline.Event(EventsEnum.RoomMonphy, "Монфис");
                    machine.AddState("NextDay");
                    break;
                case 2:
                    timeline.Event(EventsEnum.RoomRechumoe, "Рёта");
                    machine.AddState("NextDay");
                    break;
            }
        });
    }

    public void GenerateItems(Node3D parent, int count) {
        var tags = new[]{
            TagsEnum.Ticket,
            TagsEnum.Crosswords,
            TagsEnum.Cookbook,
            TagsEnum.Dumbbells,
            TagsEnum.Sketchbook,
            "Ticket"
        }.ToList();
        var tickets = new[] {
            TagsEnum.CookingShow,
            TagsEnum.EsportsFestival,
            TagsEnum.RomanticMovie,
            TagsEnum.MartialArtsMovie,
            TagsEnum.ChessTournament,
            TagsEnum.SnatchTheMovie
        }.ToList();
        for (int i = 0; i < Math.Min(count, tags.Count); ++i) {
            if (random.NextSingle() > 0.75) continue;
            var tag = tags[random.Next(0, tags.Count - 1)];
            tags.Remove(tag);
            // Уравниваем вероятность появления одного из билетов по сравнению остатальными предметами
            if (tag == "Ticket") tag = tickets[random.Next(0, tickets.Count - 1)];
            var item = ResourceLoader.Load<PackedScene>("res://Prefabs/item.tscn").Instantiate<Sprite3D>();
            // Координаты и размеры фона на сцене
            item.Position = new Vector3(random.NextSingle() * 12 - 6, 0.25f, random.NextSingle() * 6 - 3);
            item.Texture = ResourceLoader.Load<Texture2D>($"res://Items/{tag}.png");
            item.SetMeta(Metastrings.Tag, tag);
            parent.AddChild(item);
            GD.Print("Generated item: ", tag);
        }
    }

    // LEGACY - использовалось в демо
    // Предметы, хаб
    // string[] choices(string title, string[] choices, ChoiceCallback onSelected) {
    //     var goToHub = "Вернуться в хаб";
    //     var goToFestival = "Пойти в аркадные автоматы";
    //     var goToGirls = "Пойти к VTuber'шам";
    //     var variants = choices.ToList();
    //     if (Tag.Find(TagsEnum.Hub).isApplied) variants.Add(goToHub);
    //     if (Tag.Find(TagsEnum.Ticket).isApplied) variants.Add(goToFestival);
    //     if (Tag.Find(TagsEnum.Girls).isApplied) variants.Add(goToGirls);
    //     // TODO Сделать элегантней - не должно быть проверки с каждым возможным вариантом
    //     info.Choice(title, variants.ToArray(), (index, item) => {
    //         if (item == goToHub) {
    //             timeline.Event(EventsEnum.Hub, "Хаб");
    //             machine.AddState("NextDay");
    //         }
    //         if (item == goToFestival) {
    //             timeline.Event(EventsEnum.Festival, "Фестиваль");
    //             machine.AddState("NextDay");
    //         }
    //         if (item == goToGirls) {
    //             timeline.Event(EventsEnum.Girls, "VTuber'ши");
    //             machine.AddState("NextDay");
    //         }
    //         onSelected(index, item);
    //     });
    //     return null;
    // }
}