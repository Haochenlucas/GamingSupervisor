using System;
using replayParse;
using System.Threading;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Forms;
using System.Diagnostics;

namespace GamingSupervisor
{
    class ReplayAnalyzer : Analyzer
    {
        private HeroParser heroData;
        private ReplayHeroID heroIDData;
        private List<int> teamHeroIds = new List<int>(4);
        private List<int> teamIDGraph = new List<int>();
        private ReplayTick replayTick;
        private ReplayHighlights replayHighlights;
        private int closestGaming = 0;
        private LastHitCalculator lastHitCalculator;
        private Retreat retreat;

        private ReplayStartAnnouncer announcer = null;

        private System.Timers.Timer tickTimer;
        private readonly object tickLock = new object();
        private int currentTick;

        // the object for the selection analyzer.
        private static counter_pick_logic cp = new counter_pick_logic(GUISelection.replayDataFolderLocation);
        private static heroID h_ID = new heroID();

        private int[,] table = cp.selectTable();
        private Dictionary<string, int> hero_table = h_ID.getIDHero();
        private Dictionary<string, int> lower_hero_table = h_ID.getIDfromLowercaseHeroname();
        private Dictionary<int, string> ID_table = h_ID.getHeroID();
        private hero_intro hero_Intro = new hero_intro();
        private int[] enemiesHeroID;

        private JungleCampData JungleCamps = new JungleCampData();
        private static item_info i_info = new item_info();
        private string[,] item_Info_Table = i_info.get_Info_Table();
        private Dictionary<int, int> i_suggestion = i_info.item_suggestion(0, GUISelection.replayDataFolderLocation, GUISelection.heroName);


        private float screen_width = Screen.PrimaryScreen.Bounds.Width;
        private float screen_height = Screen.PrimaryScreen.Bounds.Height;
        private int itemflag = 0;
        private int item_Time_Mark = 0;
        private int team_side;
        private int[,] suggestiontable;
        private int[,] table_checkmark;

        // Remove after game time is available
        private Stopwatch fakeGameTime= new Stopwatch();
        private int CurrentTick
        {
            get { lock (tickLock) { return currentTick; } }
            set { lock (tickLock) { currentTick = value; } }
        }

        private readonly int heroID;
        private bool ranHeroSelectionAtLeastOnce = false; // Hero selection has code other parts depend on


        public ReplayAnalyzer() : base()
        {
            tickTimer = new System.Timers.Timer(1000.0 / 30.0);
            tickTimer.Elapsed += new System.Timers.ElapsedEventHandler(tickCallback);

            heroData = new HeroParser(GUISelection.replayDataFolderLocation);
            heroIDData = new ReplayHeroID(GUISelection.replayDataFolderLocation);
            replayTick = new ReplayTick(GUISelection.replayDataFolderLocation);
            replayHighlights = new ReplayHighlights(GUISelection.replayDataFolderLocation, GUISelection.heroName);
            replayHighlights.ConstructTickInfo(replayTick);
            lastHitCalculator = new LastHitCalculator();
            retreat = new Retreat();
            
            heroID = heroIDData.getHeroID(GUISelection.heroName);
            string heroname = GUISelection.heroName;

            team_side = 0;
            for (int i = 0; i < table.Length / 4; i++)
            {
                if (table[i, 0] == hero_table[heroname])
                {
                    team_side = table[i, 2];
                }
            }
            suggestiontable = cp.suggestionTable_1(team_side, 3);
            table_checkmark = cp.checkMark();
        }

        public override void Start()
        {
            if (announcer == null)
            {
                announcer = new ReplayStartAnnouncer();
            }

            overlay = OverlaySingleton.Instance;

            CurrentTick = 0;

            while (!announcer.isReplayStarted())
            {
                if (!IsDotaRunning())
                {
                    overlay.Clear();
                    Console.WriteLine("Dota ended");
                    return;
                }

                if (Terminate)
                {
                    overlay.Clear();
                    return;
                }

                bool isInitialInstructionsBoxVisible = true;
                System.Windows.Application.Current.Dispatcher.Invoke(
                    () =>
                    {
                        isInitialInstructionsBoxVisible = initialInstructionsBox.IsOverlayVisible;
                    });

                if (isInitialInstructionsBoxVisible)
                {
                    double positionX = 0;
                    double positionY = 0;
                    GetBoxPosition(initialInstructionsBox, out positionX, out positionY);
                    // draw instruction to watch the replay in dota2 client
                    overlay.UpdateWindowHandler();
                    string instru_OpenReplay = "1: Click Watch on the top.\n2: Click Downloads\n3: Click Watch to start\n   the replay "
                        + System.IO.Path.GetFileNameWithoutExtension(GUISelection.fileName);
                    overlay.Intructions_setup(instru_OpenReplay);
                    overlay.ShowInstructionMessage(positionX, positionY, visualCustomizeHandle);
                }
                else
                {
                    overlay.Clear();
                }
            }

            overlay.Clear();

            tickTimer.Start();

            int lastGameTime = announcer.GetCurrentGameTime();
            int currentGameTime = 0;
            int lastTickWhenGameTimeChanged = 0;
            bool replayStarted = false;
            bool keepLooping = true;

            CurrentTick = replayTick[announcer.GetCurrentGameTime()];
            lastTickWhenGameTimeChanged = CurrentTick;

            Console.WriteLine("Currently analyzing...");
            string lastGameState = "";
            while (keepLooping)
            {
                overlay.UpdateWindowHandler();
                if (!IsDotaRunning())
                {
                    overlay.Clear();
                    Console.WriteLine("Dota ended");
                    return;
                }

                if (Terminate)
                {
                    overlay.Clear();
                    return;
                }

                switch (announcer.GetCurrentGameState())
                {
                    case null:
                    case "":
                    case "Undefined":
                        if (lastGameState != "Undefined")
                        {
                            lastGameState = "Undefined";
                            System.Windows.Application.Current.Dispatcher.Invoke(
                            () =>
                            {
                                initialInstructionsBox.Visibility = Visibility.Hidden;
                                heroSelectionBox.Visibility = Visibility.Hidden;
                                highlightBarBox.Visibility = Visibility.Hidden;
                                healthGraphsBox.Visibility = Visibility.Hidden;
                                itemBox.Visibility = Visibility.Hidden;
                                junglingBox.Visibility = Visibility.Hidden;
                                retreatBox.Visibility = Visibility.Hidden;
                            });
                        }
                        if (replayStarted)
                        {
                            tickTimer.Stop();
                            keepLooping = false;
                        }
                        break;
                    case "DOTA_GAMERULES_STATE_HERO_SELECTION":
                        if (lastGameState != "DOTA_GAMERULES_STATE_HERO_SELECTION")
                        {
                            lastGameState = "DOTA_GAMERULES_STATE_HERO_SELECTION";
                            System.Windows.Application.Current.Dispatcher.Invoke(
                            () =>
                            {
                                initialInstructionsBox.Visibility = Visibility.Hidden;
                                heroSelectionBox.Visibility = Visibility.Visible;
                                highlightBarBox.Visibility = Visibility.Hidden;
                                healthGraphsBox.Visibility = Visibility.Hidden;
                                itemBox.Visibility = Visibility.Hidden;
                                junglingBox.Visibility = Visibility.Hidden;
                                retreatBox.Visibility = Visibility.Hidden;
                            });

                            replayStarted = true;
                        }

                        bool isHeroSelectionBoxVisible = true;
                        System.Windows.Application.Current.Dispatcher.Invoke(
                            () =>
                            {
                                isHeroSelectionBoxVisible = heroSelectionBox.IsOverlayVisible;
                            });

                        HandleHeroSelection();

                        if (isHeroSelectionBoxVisible)
                        {
                            ShowHeroSelectionSuggestions();
                        }
                        else
                        {
                            overlay.Clear();
                        }

                        if (!ranHeroSelectionAtLeastOnce)
                            ranHeroSelectionAtLeastOnce = true;

                        break;
                    case "DOTA_GAMERULES_STATE_STRATEGY_TIME":
                    case "DOTA_GAMERULES_STATE_WAIT_FOR_MAP_TO_LOAD":
                        if (lastGameState != "DOTA_GAMERULES_STATE_WAIT_FOR_MAP_TO_LOAD")
                        {
                            if (!ranHeroSelectionAtLeastOnce)
                            {
                                ranHeroSelectionAtLeastOnce = true;
                                HandleHeroSelection();
                            }

                            overlay.ClearHeroSuggestion();

                            lastGameState = "DOTA_GAMERULES_STATE_WAIT_FOR_MAP_TO_LOAD";
                            overlay.Clear();
                            replayStarted = true;
                        }
                        break;
                    case "DOTA_GAMERULES_STATE_PRE_GAME":
                    case "DOTA_GAMERULES_STATE_GAME_IN_PROGRESS":
                        if (!fakeGameTime.IsRunning)
                        {
                            fakeGameTime.Start();
                        }
                        
                        if (lastGameState != "DOTA_GAMERULES_STATE_GAME_IN_PROGRESS")
                        {
                            if (!ranHeroSelectionAtLeastOnce)
                            {
                                ranHeroSelectionAtLeastOnce = true;
                                HandleHeroSelection();
                            }
                            lastGameState = "DOTA_GAMERULES_STATE_GAME_IN_PROGRESS";

                            overlay.ClearHeroSuggestion();

                            System.Windows.Application.Current.Dispatcher.Invoke(
                            () =>
                            {
                                initialInstructionsBox.Visibility = Visibility.Hidden;
                                heroSelectionBox.Visibility = Visibility.Hidden;
                                highlightBarBox.Visibility = Visibility.Visible;
                                healthGraphsBox.Visibility = Visibility.Visible;
                                itemBox.Visibility = Visibility.Visible;
                                junglingBox.Visibility = Visibility.Visible;
                                retreatBox.Visibility = Visibility.Visible;
                            });
                            replayStarted = true;
                        }

                        bool isHighlightBarBoxVisible = true;
                        bool isHealthGraphsBoxVisible = true;
                        bool isItemSuggestionsBoxVisible = true;
                        bool isJunglingBoxVisible = true;
                        bool isRetreatVisible = true;
                        System.Windows.Application.Current.Dispatcher.Invoke(
                            () =>
                            {
                                isHighlightBarBoxVisible = highlightBarBox.IsOverlayVisible;
                                isHealthGraphsBoxVisible = healthGraphsBox.IsOverlayVisible;
                                isItemSuggestionsBoxVisible = itemBox.IsOverlayVisible;
                                isJunglingBoxVisible = junglingBox.IsOverlayVisible;
                                isRetreatVisible = retreatBox.IsOverlayVisible;
                            });

                        SetEnemiesHeroIDs();

                        if (isHighlightBarBoxVisible)
                            HandleHighlight();
                        else
                            overlay.ToggleHighlight(false);

                        if (isHealthGraphsBoxVisible)
                            overlay.ShowHealthGraphs();
                        else
                            overlay.HideHealthGraphs();

                        if (isItemSuggestionsBoxVisible)
                            overlay.ShowItemSuggestions();
                        else
                            overlay.HideItemSuggestions();

                        if (isJunglingBoxVisible)
                            overlay.ShowJungleStacking();
                        else
                            overlay.HideJungleStacking();

                        if (isRetreatVisible)
                            overlay.ShowRetreat();
                        else
                            overlay.HideRetreat();

                        HandleGamePlay();
                        UpdateInGameOverlay();
                        break;
                    default:
                        replayStarted = true;
                        break;
                }

                if (!keepLooping)
                {
                    break;
                }

                currentGameTime = announcer.GetCurrentGameTime();
                if (currentGameTime < 1)
                {
                    break;
                }
                if (currentGameTime != lastGameTime)
                {
                    lastGameTime = currentGameTime;
                    CurrentTick = replayTick[announcer.GetCurrentGameTime()];
                    lastTickWhenGameTimeChanged = CurrentTick;

                    if (!tickTimer.Enabled)
                    {
                        tickTimer.Start();
                    }
                }
                else if (CurrentTick - lastTickWhenGameTimeChanged >= 45 /*ticks*/)
                {
                    // Give ~1.5sec for game time to change before assuming game is paused )
                    CurrentTick = replayTick[currentGameTime];

                    if (tickTimer.Enabled)
                    {
                        tickTimer.Stop();
                    }
                }
            }

            overlay.Clear();
            System.Windows.Application.Current.Dispatcher.Invoke(() => { visualCustomize.CloseWindow(); });
            Console.WriteLine("Replay stopped!");
        }

        private void SetEnemiesHeroIDs()
        {
            string heroname = GUISelection.heroName;
            int team_side = 0;
            for (int i = 0; i < table.Length / 4; i++)
            {
                if (table[i, 0] == hero_table[heroname])
                {
                    team_side = table[i, 2];
                    break;
                }
            }
            int enemyTeam;
            if (team_side == 2)
            {
                enemyTeam = 3;
            }
            else
            {
                enemyTeam = 2;
            }
            enemiesHeroID = cp.GetEnemiesHeroID(enemyTeam);
        }

        private void UpdateInGameOverlay()
        { 
            double highlightBarPositionX = 0;
            double highlightBarPositionY = 0;
            GetBoxPosition(highlightBarBox, out highlightBarPositionX, out highlightBarPositionY);
            double highlightBarWidth = 0;
            GetBoxWidth(highlightBarBox, out highlightBarWidth);

            double healthGraphPositionX = 0;
            double healthGraphPositionY = 0;
            GetBoxPosition(healthGraphsBox, out healthGraphPositionX, out healthGraphPositionY);

            double itemPositionX = 0;
            double itemPositionY = 0;
            GetBoxPosition(itemBox, out itemPositionX, out itemPositionY);

            double junglingPositionX = 0;
            double junglingPositionY = 0;
            GetBoxPosition(junglingBox, out junglingPositionX, out junglingPositionY);

            double retreatPositionX = 0;
            double retreatPositionY = 0;
            GetBoxPosition(retreatBox, out retreatPositionX, out retreatPositionY);

            overlay.ShowInGameOverlay(visualCustomizeHandle,
                highlightBarPositionX, highlightBarPositionY,
                healthGraphPositionX, healthGraphPositionY,
                itemPositionX, itemPositionY,
                junglingPositionX, junglingPositionY,
                retreatPositionX, retreatPositionY,
                highlightBarWidth);
        }
        
        private void ShowHeroSelectionSuggestions()
        {
            double positionX = 0;
            double positionY = 0;
            GetBoxPosition(heroSelectionBox, out positionX, out positionY);
            
            overlay.ShowDraftMessage(positionX, positionY, visualCustomizeHandle);
        }

        /*
         * This function is to logic of what to draw in selection mode.
         */
        private void HandleHeroSelection()
        {
            for (int i = 0; i < 30; i++)
            {
                if (table[i, 2] == team_side)
                {
                    teamIDGraph.Add(table[i, 0]);
                    heroID id = new heroID();
                    Dictionary<int, string> id_string = id.getHeroID();
                    string name = id_string[table[i, 0]];
                    int index_id = heroIDData.getHeroID(name);
                    if (!teamHeroIds.Contains(index_id))
                        teamHeroIds.Add(index_id);
                }
            }
            int ticLast = 0;
            int ticNext = Int32.MaxValue;
            int mark_index = 0;
            int index = 0;
            while (mark_index < 25 && suggestiontable[mark_index, 0] < CurrentTick)
            {
                if (suggestiontable[mark_index, 0] == 0 && suggestiontable[mark_index, 1] == 0)
                {
                    break;
                }
                mark_index++;
            }

            if (mark_index > 0 && mark_index < 25)
            {
                ticNext = suggestiontable[mark_index, 0];
                ticLast = suggestiontable[mark_index - 1, 0];
                index = mark_index - 1;
            }
            else if (mark_index == 0)
            {
                ticNext = suggestiontable[mark_index, 0];
                ticLast = ticNext;
                index = mark_index;
            }
            else
            {
                ticNext = suggestiontable[mark_index - 1, 0];
                ticLast = ticNext;
                index = mark_index - 1;
            }

            string[] heroes = new string[5];
            string[] heroesimg = new string[5];

            int counter = 0;
            if (CurrentTick > table_checkmark[counter, 0] && CurrentTick < table_checkmark[counter, 1])
            {
                overlay.XorCheck(table_checkmark[counter, 2]);
                index = index - 1;
                counter++;
            }
            else
            {
                overlay.XorCheck(0);
            }

            for (int j = 1; j < 6; j++)
            {
                heroesimg[j - 1] = suggestiontable[index, j].ToString();
                heroes[j - 1] = ID_table[suggestiontable[index, j]];
            }
            
            overlay.AddHeroesSuggestionMessage(heroes, heroesimg);
        }

        private void HandleHighlight()
        {
            overlay.ToggleHighlight(true);
            overlay.UpdateHighlight(replayHighlights.tickInfo, replayHighlights.lastTick);
        }

        private void HandleGamePlay()
        {
            int gameStartTime = replayTick.GameStartTick - 90 * 30;
            
            if (CurrentTick > gameStartTime && CurrentTick < gameStartTime + 300)
            {
                Dictionary<int, string> hero_Intro_Dic = hero_Intro.getHeroIntro();
                string hero_Intro_String = hero_Intro_Dic[hero_table[GUISelection.heroName]];
                overlay.AddHeroInfoMessage(hero_Intro_String, "");
            }
            else
            {
                overlay.ClearHeroInfo();
            }
            
            int currentGameTime = announcer.GetCurrentGameTime();
            // if timer is after the game start and every last 8 sec of a minute
            AnalizeJungleCamps();

            int health = 0;
            if (announcer.GetCurrentGameTime() >= 780)
            {
                
                health = heroData.getHealth(CurrentTick, heroID);
                int gametime = announcer.GetCurrentGameTime();
                if (itemflag == 0 && health == 0)
                {
                    item_Time_Mark = 0;
                    

                    foreach (KeyValuePair<int, int> pair in i_suggestion)
                    {
                        if (closestGaming < pair.Key && pair.Key < CurrentTick/30)
                        {
                            closestGaming = pair.Key;
                        }
                        else if (pair.Key == CurrentTick)
                        {
                            closestGaming = pair.Key;
                        }
                        else
                        {
                            itemflag = 1;
                            item_Time_Mark = announcer.GetCurrentGameTime();
                            break;
                        }
                    }
                }
                if (itemflag == 1 && health ==  0 && announcer.GetCurrentGameTime() >= item_Time_Mark && announcer.GetCurrentGameTime() <= (item_Time_Mark + 10))
                {
                    string item_name = item_Info_Table[i_suggestion[closestGaming] + 2, 2];
                    string item_tip = item_Info_Table[i_suggestion[closestGaming] + 2, 117];
                    string item_content;
                    if (item_tip == " 0")
                    {
                        item_content = item_name + ":\n This item will be a good choice";
                    }
                    else
                    {
                        item_content = item_name + ":\n " + item_tip;
                    }
                    item_name = item_name.TrimStart(' ');
                    string item_img = item_name.Replace(" ", "_");
                    overlay.AddItemSuggestionMessage(item_content, item_img + "_icon");
                }
                else
                {
                    itemflag = 0;
                    overlay.ClearItemSuggestion();
                }

            }           


            // bar graph

            double[] hpToSend = new double[5] { 0, 0, 0, 0, 0 };
            double[] maxHpToSend = new double[5] { 0, 0, 0, 0, 0 };

            //Console.WriteLine(CurrentTick + " getting health " + heroID);
            health = heroData.getHealth(CurrentTick, heroID);
            //Console.WriteLine("Tick " + CurrentTick + " Health " + health);
            //maxHealth = (int)heroData.getMaxHealth(CurrentTick, heroID);
            //if (health <= 600)

            hpToSend[0] = health;

            //maxHpToSend[0] = heroData.getMaxHealth(CurrentTick, heroID);
            for (int i = 0; i < 5; i++)
            {
                heroID id = new heroID();
                Dictionary<int, string> id_string = id.getHeroID();
                string name = id_string[teamIDGraph[i]];
                int index_id = heroIDData.getHeroID(name);

                maxHpToSend[i] = heroData.getMaxHealth(CurrentTick, index_id);
                hpToSend[i] = heroData.getHealth(CurrentTick, index_id);

            }

            int closestEnemyID = DrawOnClosestEnemy();

            int fEID = lower_hero_table[heroIDData.getHeroName(closestEnemyID)];
            int fHID = lower_hero_table[heroIDData.getHeroName(heroID)];

            bool shouldRetreat = heroData.getHealth(CurrentTick, heroID) < 400 ||
                retreat.CreateInput(myID: fHID,
                myLvl: heroData.getLevel(CurrentTick, heroID),
                myHP: health,
                myMana: heroData.getMana(CurrentTick, heroID),
                enemyID: fEID,
                enemyLvl: heroData.getLevel(CurrentTick, closestEnemyID),
                enemyHP: heroData.getHealth(CurrentTick, closestEnemyID),
                enemyMana: heroData.getMana(CurrentTick, closestEnemyID));



            //overlay.ToggleGraphForHeroHP();
            //overlay.AddHPs(hpToSend);
            //overlay.AddHp(hpToSend[0]);

            //double myHp = heroData.getHealth(CurrentTick, heroID);
            //double myMaxHp = heroData.getMaxHealth(CurrentTick, heroID);
            //double myHpPercen = myHp / myMaxHp;
            //int closestHeroId = -1;

            //double min = 0;
            //Get closest hero
            //for (int i = 0; i < 4; i++)
            //{
            //    Tuple<int, int, int> pos = heroData.getHeroPosition(CurrentTick, teamHeroIds[i]);
            //    double dis = Math.Pow((Math.Pow((x - x), 2) + Math.Pow((y - y), 2)), 0.5);
            //    if (dis < min)
            //    {
            //        closestHeroId = i;
            //        min = dis;
            //    }
            //}

            //double closestHp = heroData.getHealth(CurrentTick, closestHeroId);
            //double closestMaxHp = heroData.getMaxHealth(CurrentTick, closestHeroId);
            //double closestHpPercen = closestHp / closestMaxHp;

            List<Creep> creeps = heroData.getLaneCreeps(CurrentTick);
            string prim = lastHitCalculator.GetPrimaryAttribute(fHID);
            double attr = 0;
            switch (prim)
            {
                case "Strength":
                    attr = heroData.getStrength(CurrentTick, heroID);
                    break;
                case "Agility":
                    attr = heroData.getAgility(CurrentTick, heroID);
                    break;
                case "Intelligence":
                    attr = heroData.getIntellect(CurrentTick, heroID);
                    break;
                default:
                    break;
            }
            double minAtk = heroData.getDamageMin(currentTick, heroID);
            List<double> creepX = new List<double>();
            List<double> creepY = new List<double>();
            foreach (var c in creeps)
            {
                double cArmor = c.armor;
                double cHp = c.health;
                bool canKill = lastHitCalculator.CanLastHit(baseAtk: minAtk, primaryAtr: attr, armor: cArmor, hpLeft: cHp);

                if (canKill && cHp > 0)
                {
                    (double x, double y, double z) = heroData.getHeroPosition(CurrentTick + 6, heroID);
                    overlay.CreepLowEnough();
                    double dis = Int32.MaxValue;
                    double temp = Math.Pow((Math.Pow(x - c.x, 2) + Math.Pow(y - c.y, 2)), 0.5);

                    if (temp < dis)
                    {
                        creepX.Add(c.x - x);
                        creepY.Add(c.y - y);
                    }
                }
            }
            overlay.ShowCreep(creepX, creepY);
            overlay.AddHeroGraphIcons(teamIDGraph);
            overlay.AddHPs(hpToSend, maxHpToSend);
            overlay.AddHp(hpToSend[0]);

            // The health at the start of the game is 0 so the retreat message will show up
            // TODO: logic
            if (shouldRetreat && health != 0)
            {
                overlay.AddRetreatMessage("Low health warning! " + "Current Health: " + health, "exclamation_mark");

            }
            else
            {
                overlay.ClearRetreat();
            }
        }

        private void AnalizeJungleCamps()
        {
            // Get current hero position
            (double x, double y, double z) = heroData.getHeroPosition(CurrentTick, heroID);
            Tuple<double, double, double> heroPosition = new Tuple<double, double, double>(x, y, z);
            int closestJungleCamp = -1;
            double closest = Int32.MaxValue;
            Tuple<double, double> closestCampPos;

            for (int i = 1; i < 19; i++)
            {
                Tuple<double, double> campPos = JungleCamps.GetCampPos(i);
                double dis = Math.Pow((Math.Pow(x - campPos.Item1, 2) + Math.Pow(y - campPos.Item2, 2)), 0.5);
                if (dis < closest && dis <= 600)
                {
                    closest = dis;
                    closestJungleCamp = i;
                }
            }

            if (closestJungleCamp != -1)
            {
                closestCampPos = JungleCamps.GetCampPos(closestJungleCamp);
                string content = "";
                int totalsecond = (CurrentTick - replayTick.GameStartTick) / 30;
                
                int secondMark = JungleCamps.GetCampSecMark(closestJungleCamp);
                int second = totalsecond % 60;
                int minute = totalsecond / 60 - second;
                //content += "Game Time: " + minute.ToString() + ":" + second.ToString() + "\n";
                int countdown = secondMark - second;
                if (countdown < 6)
                {
                    content += "Count down: " + countdown + "\n";
                    content += JungleCamps.GetDirection(closestJungleCamp);
                    overlay.AddJungleStackingMessage(content, "");
                }
            }
            else
            {
                overlay.ClearJungle();
            }
        }

        private int DrawOnClosestEnemy()
        {
            // Get current hero position
            (double x, double y, double z) = heroData.getHeroPosition(CurrentTick+6, heroID);
            Tuple<double, double, double> heroPosition = new Tuple<double, double, double>(x, y, z);

            // Loop through all enemy heros and find the cloest one
            int enemyHeroID = -1;
            Tuple<double, double, double> enemyHeroPosition = null;
            double dis = Int32.MaxValue;
            int[] enemyHeroIDs;
            if (heroID <= 4)
            {
                enemyHeroIDs = new int[] { 5, 6, 7, 8, 9 };
            }
            else
            {
                enemyHeroIDs = new int[] { 0, 1, 2, 3, 4 };
            }
            foreach (int ID in enemyHeroIDs)
            {
                (double x_temp, double y_temp, double z_temp) = heroData.getHeroPosition(CurrentTick+10, ID);
                double temp = Math.Pow((Math.Pow(x - x_temp, 2) + Math.Pow(y - y_temp, 2)), 0.5);
                if (temp < dis)
                {
                    dis = temp;
                    enemyHeroPosition = new Tuple<double, double, double>(x_temp, y_temp, z_temp);
                    enemyHeroID = ID;
                }
            }
            if (enemyHeroPosition != null)
            {
                overlay.ShowCloestEnemy(enemyHeroPosition.Item1 - x, enemyHeroPosition.Item2 - y);
            }
            else
            {
                throw new Exception("Closet enemy not found.");
            }
            return enemyHeroID;
        }

        private void tickCallback(object sender, EventArgs e)
        {
            CurrentTick++;
        }
    }
}
