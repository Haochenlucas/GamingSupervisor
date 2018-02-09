using System;
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
            heroId = parsedReplay.getHerosLowercase()[GUISelection.heroName.ToLower()];

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
                        break;
                    case "DOTA_GAMERULES_STATE_PRE_GAME":
                    case "DOTA_GAMERULES_STATE_GAME_IN_PROGRESS":
                        replayStarted = true;
                        for (int i = 0; i < 5; i++)
                        {
                            overlay.ClearMessage(i);
                        }
                        HandleGamePlay();
                        break;
                    default:
                        replayStarted = true;
                        break;
                }

                if (!keepLooping)
                {
                    break;
                }

                ShowHints();

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

        private void ShowHints()
        {
            overlay.ShowMessage();
        }

        private void HandleHeroSelection()
        {
            counter_pick_logic cp = new counter_pick_logic(GUISelection.replayDataFolderLocation);
            cp.readTeam();
            int[,] table = cp.selectTable();
            string heroname = GUISelection.heroName;
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
            for(int i = 0; i< 30; i++)
            {
                if(table[i,3] == team_side)
                {
                    heroID id = new heroID();
                    Dictionary<int, string> id_string = id.getHeroID();
                    string name = id_string[table[i, 0]];
                    name = String.Join("", name.Split(' '));
                    int index_id = parsedReplay.getHerosLowercase()[name.ToLower()];
                    teamHeroIds.Add(index_id);
                }
            }
            int ticLast = 0;
            int ticNext = Int32.MaxValue;
            int mark_index = 0;
            int shuangla = 0;
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
            double[] hpToSend = new double[5] { 0, 0, 0, 0, 0 };
            if (CurrentTick - parsedReplay.getOffSet() < 0)
            {
                int cur_tic_fake = 0;
                health = (int)parsedData[cur_tic_fake, heroId, 0];                
                hpToSend[0] = health;
                for (int i = 0; i < 4; i ++)
                {
                    hpToSend[i] = parsedData[cur_tic_fake, teamHeroIds[i - 1], 0];
                }
            }
            health = (int)parsedData[CurrentTick - parsedReplay.getOffSet(), heroId, 0];
            //if (health < 470)
            if (true)
            {
                overlay.ToggleGraphForHeroHP();
                overlay.AddHPs(hpToSend);
                //overlay.ShowMessage("Health is low, retreat");
                overlay.AddRetreatMessage("Tick " + CurrentTick + " Health " + health + " " + parsedReplay.getOffSet(), "");
                //Console.WriteLine("Tick " + CurrentTick + " Health " + health + " " + parsedReplay.getOffSet());
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
