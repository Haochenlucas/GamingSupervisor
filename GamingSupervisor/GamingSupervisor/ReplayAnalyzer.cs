﻿using System;
using replayParse;
using System.Threading;
using System.Collections.Generic;

namespace GamingSupervisor
{
    class ReplayAnalyzer
    {
        private replay_version01 parsedReplay;
        private double[,,] parsedData;
        private int heroId;
        private List<int> teamHeroIds = new List<int>(4);
        private ReplayTick replayTick;

        private ReplayStartAnnouncer announcer = null;
        private static Overlay overlay = null;

        private System.Timers.Timer tickTimer;
        private readonly object tickLock = new object();
        private int currentTick;

        // the object for the selection analyzer.
        private static counter_pick_logic cp = new counter_pick_logic(GUISelection.replayDataFolderLocation);
        private static heroID h_ID = new heroID();
        private int[,] table = cp.selectTable();
        private Dictionary<string, int> hero_table = h_ID.getIDHero();
        private Dictionary<int, string> ID_table = h_ID.getHeroID();

        private int CurrentTick
        {
            get { lock (tickLock) { return currentTick; } }
            set { lock (tickLock) { currentTick = value; } }
        }

        public ReplayAnalyzer()
        {
            tickTimer = new System.Timers.Timer(1000.0 / 30.0);
            tickTimer.Elapsed += new System.Timers.ElapsedEventHandler(tickCallback);

            parsedReplay = new replay_version01(GUISelection.replayDataFolderLocation);
            parsedData = parsedReplay.getReplayInfo();
            string name = GUISelection.heroName.ToLower().Replace(" ", "");
            heroId = parsedReplay.getHerosLowercase()[name];

            replayTick = new ReplayTick(GUISelection.replayDataFolderLocation);
        }

        public void Start()
        {
            if (announcer == null)
            {
                announcer = new ReplayStartAnnouncer();
            }

            CurrentTick = 0;
            announcer.waitForReplayToStart();
            tickTimer.Start();

            if (overlay == null)
            {
                overlay = new Overlay();
            }

            int lastGameTime = announcer.GetCurrentGameTime();
            int currentGameTime = 0;
            bool replayStarted = false;
            bool keepLooping = true;

            CurrentTick = replayTick[announcer.GetCurrentGameTime()];

            Console.WriteLine("Currently analyzing...");
            while (keepLooping)
            {
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
                        replayStarted = true;
                        for (int i = 0; i < 5; i++)
                        {
                            overlay.ClearMessage(i);
                        }
                        HandleGamePlay();
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
                }
            }

            overlay.Clear();

            Console.WriteLine("Replay stopped!");
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
            int[,] suggestiontable = cp.suggestionTable(team_side);
            int[,] table_checkmark = cp.checkMark();
            for (int i = 0; i < 30; i++)
            {
                if (table[i, 2] == team_side)
                {
                    heroID id = new heroID();
                    Dictionary<int, string> id_string = id.getHeroID();
                    string name = id_string[table[i, 0]];
                    name = String.Join("", name.Split(new string[] { " " }, StringSplitOptions.None));
                    int index_id = parsedReplay.getHerosLowercase()[name.ToLower()];
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
                heroes[j - 1] = ID_table[suggestiontable[index, j]] + announcer.GetCurrentGameTime().ToString();
            }
            
            overlay.AddHeroesSuggestionMessage(heroes, heroesimg);
        }

        private void HandleGamePlay()
        {
            int health = 0;

            int maxHealth = 0;

            double[] hpToSend = new double[5] { 0, 0, 0, 0, 0 };

            if (CurrentTick - parsedReplay.getOffSet() < 0)
            {
                int cur_tic_fake = 0;
                health = (int)parsedData[cur_tic_fake, heroId, 0];
                hpToSend[0] = health;
                for (int i = 0; i < 4; i++)
                {
                    hpToSend[i + 1] = parsedData[cur_tic_fake, teamHeroIds[i], 0];
                }
            }
            health = (int)parsedData[CurrentTick - parsedReplay.getOffSet(), heroId, 0];

            maxHealth = (int)parsedData[CurrentTick - parsedReplay.getOffSet(), heroId, 9];
            //if (health <= 600)

            hpToSend[0] = health;
            for (int i = 0; i < 4; i++)
            {
                hpToSend[i + 1] = parsedData[CurrentTick - parsedReplay.getOffSet(), teamHeroIds[i], 0];
            }
            if (health < 600)
            {
                overlay.AddRetreatMessage("Tick " + CurrentTick + " Health " + health + " " + parsedReplay.getOffSet(), "");

            }
            else
            {
                overlay.ClearMessage(7);
            }
            
                overlay.ToggleGraphForHeroHP();
                overlay.AddHPs(hpToSend);
                overlay.AddHp(hpToSend[0]);
                //overlay.ShowMessage("Health is low, retreat");
                //Console.WriteLine("Tick " + CurrentTick + " Health " + health + " " + parsedReplay.getOffSet());
        }

        private void tickCallback(object sender, EventArgs e)
        {
            CurrentTick++;
        }
    }
}