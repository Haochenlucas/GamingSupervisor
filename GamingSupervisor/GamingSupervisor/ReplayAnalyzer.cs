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
        private int[] enemiesHeroID;

        private float screen_width = Screen.PrimaryScreen.Bounds.Width;
        private float screen_height = Screen.PrimaryScreen.Bounds.Height;

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
            string heroname = GUISelection.heroName;

            int team_side = 0;
            for (int i = 0; i < table.Length / 4; i++)
            {
                if (table[i, 0] == hero_table[heroname])
                {
                    team_side = table[i, 2];
                }
            }
            int[,] suggestiontable = cp.suggestionTable_1(team_side,3);
            int[,] table_checkmark = cp.checkMark();
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
                string temp = "Lycan is a remarkable pusher who can wear down buildings and force enemies to react quickly to his regular tower onslaughts; as towers melt incredibly fast under Lycan's and his units' pressure, boosted by their canine Feral Impulse. His only contribution to full-on team fights will be the bonus damage he grants with Howl to his allies, his allies' summons, his owns summons, and himself, as well as his formidable physical attacks. Else he can surge out of the woods for a quick gank or push after he transformed with Shapeshift, moving at a haste speed of 650. Finally, good players will make the best usage of his Summon Wolves ability and scout the enemies' position while remaining undetected with invisibility at level 4.";

                overlay.AddHeroInfoMessage(temp, "");
                overlay.AddItemSuggestionMessage("Buy this. It is good for you.", "Necronomicon_1_icon");
            }
            else
            {
                overlay.ClearItemSuggestion();
                overlay.ClearHeroInfo();
            }

            // Add item suggestion
            //if (announcer.GetCurrentGameTime() >= 1380 && announcer.GetCurrentGameTime() <= 1390)
            //{
            //    overlay.AddItemSuggestionMessage("Necronomicon", "");
            //}

            int health = 0;

            //int maxHealth = 0;

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
            if (heroID >= 0 && heroID <= 4)
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