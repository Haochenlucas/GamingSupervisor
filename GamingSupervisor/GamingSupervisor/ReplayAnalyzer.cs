using System;
using replayParse;
using System.Threading;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GamingSupervisor
{
    class ReplayAnalyzer : Analyzer
    {
        private HeroParser heroData;
        private ReplayHeroID heroIDData;
        private List<int> teamHeroIds = new List<int>(4);
        private ReplayTick replayTick;
        private ReplayHighlights replayHighlights;

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

        private int CurrentTick
        {
            get { lock (tickLock) { return currentTick; } }
            set { lock (tickLock) { currentTick = value; } }
        }

        private int heroID;


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
            string instru_OpenReplay = "Step 1: Click Watch on the top.\nStep 2: Click Downloads\nStep 3: The replay you selected is\n     " + System.IO.Path.GetFileNameWithoutExtension(GUISelection.fileName) + ", click Watch to start.";
            overlay.Intructions_setup(instru_OpenReplay);
            while (!announcer.waitForReplayToStart())
            {
                if (Terminate)
                {
                    overlay.Clear();
                    return;
                }

                // draw instruction to watch the replay in dota2 client
                overlay.ShowInstructionMessage();
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
                        replayStarted = true;
                        for (int i = 0; i < 5; i++)
                        {
                            overlay.ClearMessage(i);
                        }
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
            // TODO:

            //// if first 10 sec, show camer setting instruction
            //if (announcer.GetCurrentGameTime() < 10)
            //{
            //    string instru_OpenReplay = "Step 1: Change the camera mode to Hero Chase\nStep 2: Click on the hero you choose.";
            //    overlay.Intructions_setup(instru_OpenReplay);
            //    overlay.ShowInstructionMessage();
            //}
            Console.WriteLine("announcer.GetCurrentGameTime(): " + announcer.GetCurrentGameTime());
             if (announcer.GetCurrentGameTime() >= 750 && announcer.GetCurrentGameTime() <= 760)
            {
                string temp = "Lycan is a remarkable pusher who can wear down buildings and force enemies to react quickly to his regular tower onslaughts; as towers melt incredibly fast under Lycan's and his units' pressure, boosted by their canine Feral Impulse. His only contribution to full-on team fights will be the bonus damage he grants with Howl to his allies, his allies' summons, his owns summons, and himself, as well as his formidable physical attacks. Else he can surge out of the woods for a quick gank or push after he transformed with Shapeshift, moving at a haste speed of 650. Finally, good players will make the best usage of his Summon Wolves ability and scout the enemies' position while remaining undetected with invisibility at level 4.";

                temp = Regex.Replace(temp, ".{50}", "$0\n");
                // after that, show hero information
                overlay.AddHeroInfoMessage(temp, "");
            }
            else
            {
                overlay.ClearMessage(6);
            }

            // Add item suggestion
            //if (announcer.GetCurrentGameTime() >= 1380 && announcer.GetCurrentGameTime() <= 1390)
            //{
            //    overlay.AddItemSuggestionMessage("Necronomicon", "");
            //}

            int health = 0;

            //int maxHealth = 0;

            double[] hpToSend = new double[5] { 0, 0, 0, 0, 0 };

            Console.WriteLine(CurrentTick + " getting health " + heroID);
            health = heroData.getHealth(CurrentTick, heroID);
            Console.WriteLine("Tick " + CurrentTick + " Health " + health);
            //maxHealth = (int)heroData.getMaxHealth(CurrentTick, heroID);
            //if (health <= 600)

            hpToSend[0] = health;
            for (int i = 0; i < 4; i++)
            {
                hpToSend[i + 1] = heroData.getHealth(CurrentTick, teamHeroIds[i]);
            }

            overlay.ToggleGraphForHeroHP();
            overlay.AddHPs(hpToSend);
            overlay.AddHp(hpToSend[0]);

            if (health < 600)
            {
                overlay.AddRetreatMessage("Low health warning! " + "Current Health: " + health, "");

            }
            else
            {
                overlay.ClearMessage(7);
            }
        }

        private void tickCallback(object sender, EventArgs e)
        {
            CurrentTick++;
        }
    }
}