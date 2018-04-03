using replayParse;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace GamingSupervisor
{
    class LiveAnalyzer : Analyzer
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        private GameStateIntegration gsi;
        private DotaConsoleParser consoleData;

        // the object for the selection analyzer.
        private static counter_pick_logic cp = new counter_pick_logic(GUISelection.replayDataFolderLocation);
        private static heroID h_ID = new heroID();
        private int[,] table = cp.selectTable();
        private Dictionary<string, int> hero_table = h_ID.getIDHero();
        private Dictionary<int, string> ID_table = h_ID.getHeroID();
        private List<int> teamHeroIds = new List<int>(4);
        private List<int> teamIDGraph = new List<int>();
        private ReplayHeroID heroIDData;


        public LiveAnalyzer() : base()
        {
            consoleData = new DotaConsoleParser();
        }

        public override void Start()
        {
            Console.WriteLine("Started live...");

            overlay = OverlaySingleton.Instance;
            gsi = GameStateIntegrationSingleton.Instance;

            gsi.StartListener();

            overlay.Intructions_setup("Start a game");

            while (gsi.GameState == "Undefined" || gsi.GameState == null || gsi.GameState == "")
            {
                if (!IsDotaRunning())
                {
                    overlay.Clear();
                    Console.WriteLine("Dota ended");
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

                overlay.ShowInstructionMessage(positionX, positionY, visualCustomizeHandle);

                Thread.Sleep(10);
            }

            bool gameStarted = false;
            bool keepLooping = true;

            Console.WriteLine("Currently analyzing...");
            while (keepLooping)
            {
                if (!IsDotaRunning())
                {
                    overlay.Clear();
                    Console.WriteLine("Dota ended");
                    return;
                }

                string lastGameState = "";
                switch (gsi.GameState)
                {
                    case null:
                    case "":
                    case "Undefined":
                        if (gameStarted)
                        {
                            keepLooping = false;
                        }
                        lastGameState = "Undefined";
                        break;
                    case "DOTA_GAMERULES_STATE_HERO_SELECTION":
                        gameStarted = true;
                        if (lastGameState != "DOTA_GAMERULES_STATE_HERO_SELECTION")
                        {
                            lastGameState = "DOTA_GAMERULES_STATE_HERO_SELECTION";
                            consoleData.StartHeroSelectionParsing();
                        }
                        HandleHeroSelection();
                        break;
                    case "DOTA_GAMERULES_STATE_PRE_GAME":
                    case "DOTA_GAMERULES_STATE_GAME_IN_PROGRESS":
                        lastGameState = "DOTA_GAMERULES_STATE_GAME_IN_PROGRESS";
                        gameStarted = true;

                        //overlay.ClearHeroSuggestion();
                        HandleGamePlay();
                        overlay.ShowIngameMessage();
                        break;
                    default:
                        lastGameState = "Other";
                        gameStarted = true;
                        break;
                }

                if (!keepLooping)
                {
                    break;
                }

                Thread.Sleep(10);
            }

            overlay.Clear();

            Console.WriteLine("Game stopped!");
        }

        private long timeSinceLastKeyPress_ms = 0;
        private void SendCommandsToDota()
        {
            long now_ms = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (now_ms - timeSinceLastKeyPress_ms >= 500)
            {
                timeSinceLastKeyPress_ms = now_ms;
                if (GetForegroundWindow() == overlay.GetOverlayHandle())
                    SendKeys.SendWait("{F12}");
            }
        }

        private void HandleGamePlay()
        {
            SendCommandsToDota();

            // placeholders
            double[] hpToSend = new double[5] { 0, 0, 0, 0, 0 };
            double[] maxHpToSend = new double[5] { 0, 0, 0, 0, 0 };

            int healthPercent = gsi.HealthPercent;
            
            overlay.AddHPs(hpToSend, maxHpToSend);
            overlay.AddHp(hpToSend[0]);

            if (true)//healthPercent < 25)
            {
                overlay.AddRetreatMessage("Health " + gsi.Health, "");

            }
            else
            {
                //overlay.ClearMessage(7);
            }
        }

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
            int[,] suggestiontable = cp.suggestionTable_1(team_side, 3);
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
    }
}
