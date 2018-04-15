using System;
using replayParse;
using System.Threading;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Forms;

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
        private LastHitCalculator lastHitCalculator;

        private ReplayStartAnnouncer announcer = null;

        private System.Timers.Timer tickTimer;
        private readonly object tickLock = new object();
        private int currentTick;

        // the object for the selection analyzer.
        private static counter_pick_logic cp = new counter_pick_logic(GUISelection.replayDataFolderLocation);
        private static heroID h_ID = new heroID();

        private int[,] table = cp.selectTable();
        private Dictionary<string, int> hero_table = h_ID.getIDHero();
        private Dictionary<int, string> ID_table = h_ID.getHeroID();
        private hero_intro hero_Intro = new hero_intro();
        private int[] enemiesHeroID;

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

        private int CurrentTick
        {
            get { lock (tickLock) { return currentTick; } }
            set { lock (tickLock) { currentTick = value; } }
        }

        private readonly int heroID;


        public ReplayAnalyzer() : base()
        {
            tickTimer = new System.Timers.Timer(1000.0 / 30.0);
            tickTimer.Elapsed += new System.Timers.ElapsedEventHandler(tickCallback);

            heroData = new HeroParser(GUISelection.replayDataFolderLocation);
            heroIDData = new ReplayHeroID(GUISelection.replayDataFolderLocation);
            replayTick = new ReplayTick(GUISelection.replayDataFolderLocation);
            replayHighlights = new ReplayHighlights(GUISelection.replayDataFolderLocation, GUISelection.heroName);

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
            string instru_OpenReplay = "1: Click Watch on the top.\n2: Click Downloads\n3: Click Watch to start\n   the replay "
                + System.IO.Path.GetFileNameWithoutExtension(GUISelection.fileName);
            overlay.Intructions_setup(instru_OpenReplay);
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

                double positionX = 0;
                double positionY = 0;
                System.Windows.Application.Current.Dispatcher.Invoke(
                    () =>
                    {
                        positionX = Canvas.GetLeft(initialInstructions) / visualCustomize.ActualWidth * visualCustomize.ScreenWidth;
                        positionY = Canvas.GetTop(initialInstructions) / visualCustomize.ActualHeight * visualCustomize.ScreenHeight;
                    });
                // draw instruction to watch the replay in dota2 client
                overlay.ShowInstructionMessage(positionX, positionY, visualCustomizeHandle);

                Thread.Sleep(10);
            }
            tickTimer.Start();

            int lastGameTime = announcer.GetCurrentGameTime();
            int currentGameTime = 0;
            int lastTickWhenGameTimeChanged = 0;
            bool replayStarted = false;
            bool keepLooping = true;

            CurrentTick = replayTick[announcer.GetCurrentGameTime()];
            lastTickWhenGameTimeChanged = CurrentTick;

            Console.WriteLine("Currently analyzing...");
            while (keepLooping)
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

                Console.WriteLine(announcer.GetCurrentGameState());
                switch (announcer.GetCurrentGameState())
                {
                    case null:
                    case "":
                    case "Undefined":
                        if (replayStarted)
                        {
                            tickTimer.Stop();
                            keepLooping = false;
                        }
                        break;
                    case "DOTA_GAMERULES_STATE_HERO_SELECTION":
                        replayStarted = true;
                        HandleHeroSelection();
                        ShowDraftHints();
                        break;
                    case "DOTA_GAMERULES_STATE_PRE_GAME":
                    case "DOTA_GAMERULES_STATE_GAME_IN_PROGRESS":
                        SetEnemiesHeroIDs();
                        replayStarted = true;
                        overlay.ClearHeroSuggestion();
                        HandleGamePlay();
                        HandleHighlight();
                        ShowIngameHints();
                        break;
                    default:
                        replayStarted = true;
                        break;
                }

                if (!keepLooping)
                {
                    break;
                }

                Thread.Sleep(10);

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

        private void ShowIngameHints()
        {
            overlay.ShowIngameMessage();
        }
        
        private void ShowDraftHints()
        {
            overlay.ShowDraftMessage();
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
            overlay.ToggleHighlight();
            overlay.UpdateHighlight(replayHighlights.tickInfo, replayHighlights.lastTick);
        }

        private void HandleGamePlay()
        {
            // TODO: Set this to be the beginning of the time
            if (announcer.GetCurrentGameTime() >= 750 && announcer.GetCurrentGameTime() <= 760)
            {
                // TODO: Replace with the true intruction
                Dictionary<int, string> hero_Intro_Dic = hero_Intro.getHeroIntro();
                string hero_Intro_String = hero_Intro_Dic[hero_table[GUISelection.heroName]];
                overlay.AddHeroInfoMessage(hero_Intro_String, "");
            }
            else
            {
                overlay.ClearHeroInfo();
            }

            // show on death or enough gold
            //overlay.ClearItemSuggestion();
            //overlay.AddItemSuggestionMessage("Buy this. It is good for you.", "Necronomicon_1_icon");

            // Add item suggestion
            //if (announcer.GetCurrentGameTime() >= 1380 && announcer.GetCurrentGameTime() <= 1390)
            //{
            //    overlay.AddItemSuggestionMessage("Necronomicon", "");
            //}
            
           
            int health = 0;
            int closestTic = 0;
            if (itemflag == 0)
            {
                item_Time_Mark = 0;
                health = heroData.getHealth(CurrentTick, heroID);
                
                foreach (KeyValuePair<int, int> pair in i_suggestion)
                {
                    if (closestTic < pair.Key && pair.Key < CurrentTick)
                    {
                        closestTic = pair.Key;
                    }
                    else if (pair.Key == CurrentTick)
                    {
                        closestTic = pair.Key;
                    }
                    else
                    {
                        itemflag = 1;
                        item_Time_Mark = announcer.GetCurrentGameTime();
                        break;
                    }
                }
            }
            if (itemflag == 1 && announcer.GetCurrentGameTime() >= item_Time_Mark && announcer.GetCurrentGameTime() <= (item_Time_Mark + 10))
            {
                string item_name = item_Info_Table[i_suggestion[closestTic] + 2, 2];
                string item_tip = item_Info_Table[i_suggestion[closestTic] + 2, 117];
                overlay.AddItemSuggestionMessage(item_name, item_tip);
            }
            else
            {
                itemflag = 0;
                overlay.ClearItemSuggestion();
            }



            // bar graph

            double[] hpToSend = new double[5] { 0, 0, 0, 0, 0 };
            double[] maxHpToSend = new double[5] { 0, 0, 0, 0, 0 };

            Console.WriteLine(CurrentTick + " getting health " + heroID);
            health = heroData.getHealth(CurrentTick, heroID);
            Console.WriteLine("Tick " + CurrentTick + " Health " + health);
            //maxHealth = (int)heroData.getMaxHealth(CurrentTick, heroID);
            //if (health <= 600)

            hpToSend[0] = health;
            maxHpToSend[0] = heroData.getMaxHealth(CurrentTick, heroID);
            for (int i = 0; i < 4; i++)
            {
                // Repley start right after hero selection will cause index out of range error
                maxHpToSend[i + 1] = heroData.getMaxHealth(CurrentTick, teamHeroIds[i]);
                hpToSend[i + 1] = heroData.getHealth(CurrentTick, teamHeroIds[i]);
            }

            int closestEnemyID = DrawOnClosestEnemy();

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
            string prim = lastHitCalculator.GetPrimaryAttribute(heroID);
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
            foreach (var c in creeps)
            {
                double cArmor = c.armor;
                double cHp = c.health;
                bool canKill = lastHitCalculator.CanLastHit(baseAtk: minAtk, primaryAtr: attr, armor: cArmor, hpLeft: cHp);

                if (canKill && cHp > 0)
                {
                    overlay.CreepLowEnough();
                }

            }
            overlay.ToggleGraphForHeroHP();
            overlay.AddHeroGraphIcons(teamIDGraph);
            overlay.AddHPs(hpToSend, maxHpToSend);
            overlay.AddHp(hpToSend[0]);

            // The health at the start of the game is 0 so the retreat message will show up
            // TODO: logic
            if (health < 600)
            {
                overlay.AddRetreatMessage("Low health warning! " + "Current Health: " + health, "exclamation_mark");

            }
            else
            {
                overlay.ClearRetreat();
            }
        }

        private int DrawOnClosestEnemy()
        {
            // Get current hero position
            (double x, double y, double z) = heroData.getHeroPosition(CurrentTick+10, heroID);
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
                    enemyHeroPosition = new Tuple<double, double, double>(x_temp - x, y_temp - y, z_temp - z);
                    enemyHeroID = ID;
                }
            }
            if (enemyHeroPosition != null)
            {
                overlay.ShowCloestEnemy(enemyHeroPosition.Item1, enemyHeroPosition.Item2);
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
