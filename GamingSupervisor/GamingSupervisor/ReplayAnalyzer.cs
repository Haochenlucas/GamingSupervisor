using System;
using replayParse;
using System.Threading;
using System.Collections.Generic;

namespace GamingSupervisor
{
    class ReplayAnalyzer
    {
        private GUISelection selection;

        replay_version01 parsedReplay;
        int[,,] parsedData;
        int heroId;

        private ReplayStartAnnouncer announcer = null;
        private Overlay overlay = null;

        private System.Timers.Timer tickTimer;
        private readonly object tickLock = new object();
        private int currentTick;
        private int CurrentTick
        {
            get { lock (tickLock) { return currentTick; } }
            set { lock (tickLock) { currentTick = value; } }
        }

        public ReplayAnalyzer(GUISelection selection)
        {
            this.selection = selection;

            tickTimer = new System.Timers.Timer(1000.0 / 30.0);
            tickTimer.Elapsed += new System.Timers.ElapsedEventHandler(tickCallback);

            parsedReplay = new replay_version01();
            parsedData = parsedReplay.getReplayInfo();
            heroId = parsedReplay.getHeros()[selection.heroName];
        }

        public void Start()
        {
            if (announcer == null)
            {
                announcer = new ReplayStartAnnouncer();
            }
            //CurrentTick = announcer.GetStartTick();
            CurrentTick = 0;
            announcer.waitForReplayToStart();
            tickTimer.Start();

            if (overlay == null)
            {
                overlay = new Overlay();
            }

            int lastGameTime = announcer.GetCurrentGameTime();
            int currentGameTime = 0;
            int lastTickSynced = CurrentTick;
            bool keepLooping = true;
            while (keepLooping)
            {
                if (CurrentTick < 0)
                {
                    tickTimer.Stop();
                    break;
                }
                currentGameTime = announcer.GetCurrentGameTime();
                if (currentGameTime != lastGameTime)
                {
                    int gameTimeChange = currentGameTime - lastGameTime;
                    lastGameTime = currentGameTime;
                    lastTickSynced += 30 * gameTimeChange;
                    CurrentTick = lastTickSynced;
                }
                switch (announcer.GetCurrentGameState())
                {
                    case "Undefined":
                        tickTimer.Stop();
                        keepLooping = false;
                        break;
                    case "DOTA_GAMERULES_STATE_HERO_SELECTION":
                        HandleHeroSelection();
                        break;
                    case "DOTA_GAMERULES_STATE_PRE_GAME":
                    case "DOTA_GAMERULES_STATE_GAME_IN_PROGRESS":
                        for (int i = 0; i < 5; i++)
                        {
                            overlay.ClearMessage(i);
                        }
                        HandleGamePlay();
                        break;
                    default:
                        //Console.WriteLine(announcer.GetCurrentGameState());
                        break;
                }

                ShowHints();

                Thread.Sleep(10);
            }

            overlay.Clear();

            Console.WriteLine("Replay stopped!");
        }

        private void ShowHints()
        {
            overlay.ShowMessage();
        }

        private void HandleHeroSelection()
        {

            counter_pick_logic cp = new counter_pick_logic();
            cp.readTeam();
            int[,] table = cp.selectTable();
            string heroname = selection.heroName;
            heroID h_ID = new heroID();
            Dictionary<string, int> hero_table = h_ID.getIDHero();
            Dictionary<int, string> ID_table = h_ID.getHeroID();
            int team_side = 0;
            for (int i = 0; i < table.Length / 4; i++)
            {
                if (table[i, 0] == hero_table[heroname])
                {
                    team_side = table[i, 2];
                }
            }
            int[,] suggestiontable = cp.suggestionTable(team_side);
            int ticLast = 0;
            int ticNext = Int32.MaxValue;
            int mark_index = 0;
            int shuangla = 0;
            while (mark_index < 25 && suggestiontable[mark_index, 0] < CurrentTick)
            {
                mark_index++;
            }

            if (mark_index > 0 && mark_index < 25)
            {
                ticNext = suggestiontable[mark_index, 0];
                ticLast = suggestiontable[mark_index - 1, 0];
                shuangla = mark_index - 1;
            }
            else if (mark_index == 0)
            {
                ticNext = suggestiontable[mark_index, 0];
                ticLast = ticNext;
                shuangla = mark_index;
            }
            else
            {
                ticNext = suggestiontable[mark_index-1, 0];
                ticLast = ticNext;
                shuangla = mark_index-1;
            }
            
            string[] heroes = new string[5];
            string[] heroesimg = new string[5];

            for (int j = 1; j<6; j++)
            {
                heroesimg[j - 1] = suggestiontable[shuangla, j].ToString();
                heroes[j - 1] = ID_table[suggestiontable[shuangla, j]];
            }
            
             

            overlay.AddHeroesSuggestionMessage(heroes, heroesimg);
        }

        private void HandleGamePlay()
        {
            int health = 0;
            if (CurrentTick - parsedReplay.getOffSet()< 0)
            {
                int cur_tic_fake = 0;
                health = parsedData[cur_tic_fake, heroId, 0];

            }
            health = parsedData[CurrentTick - parsedReplay.getOffSet(), heroId, 0];
            //if (health < 470)
            if (true)
            {
                //overlay.ShowMessage("Health is low, retreat");
                overlay.AddRetreatMessage("Health: " + health, "");
            }
            else
            {
                overlay.Clear();
            }
        }

        private void tickCallback(object sender, EventArgs e)
        {
            CurrentTick++;
        }
    }
}
