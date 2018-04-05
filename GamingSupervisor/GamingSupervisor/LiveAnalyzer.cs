using replayParse;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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
        private Dictionary<string, int> hero_table = h_ID.getIDfromLowercaseHeroname();
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

        /*
         * take real time picks and give 5 suggested hero.
         */
        private void HandleHeroSelection()
        {
            string[] heroesSelected = consoleData.HeroesSelected;
            int team_side = 0;

            if (gsi.Name == "null")
            {
                string teamname = gsi.Team;
               
                if (teamname == "Dire")
                {
                    team_side = 3;
                }
                else if (teamname == "Radiant")
                {
                    team_side = 2;
                }
                if (team_side != 0)
                {
                    int[] hero_sequence_enemy = new int[5];
                    int[] hero_sequence_teammate = new int[5];
                    int flagEnemy = 0;
                    int flagTeam = 0;
                    int heroID;
                    string curHeroName;
                    for (int i = 0; i < 10; i++)
                    {
                        if (heroesSelected[i] == "null")
                        {
                            continue;
                        }
                        else
                        {
                            curHeroName = heroesSelected[i];
                            var r = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z]) | (?<=[^A-Z])(?=[A-Z]) | (?<=[A-Za-z])(?=[^A-Za-z])",
            RegexOptions.IgnorePatternWhitespace);
                            string name = r.Replace(curHeroName, "");
                            name = string.Join("", name.Split(new string[] { "_" }, StringSplitOptions.None));
                            name = name.ToLower();
                            if (name.Contains("never"))
                            {
                                name = "shadowfiend";
                            }
                            if (name.Contains("obsidian"))
                            {
                                name = "outworlddevourer";
                            }
                            if (name.Contains("wisp"))
                            {
                                name = "io";
                            }
                            if (name.Contains("magnataur"))
                            {
                                name = "magnus";
                            }
                            if (name.Contains("treant"))
                            {
                                name = "treantprotector";
                            }
                            if (name.Contains("Rattletrap"))
                            {
                                name = "Clockwerk";
                            }
                            if (name.Contains("skele"))
                            {
                                name = "wraithking";
                            }
                            heroID = hero_table[name];

                        }

                        if (i < 5 && team_side == 2)
                        {
                            hero_sequence_teammate[flagTeam] = heroID;
                            flagTeam++;
                        }
                        else if (i < 5 && team_side == 3)
                        {
                            hero_sequence_enemy[flagEnemy] = heroID;
                            flagEnemy++;
                        }
                        else if (i >= 5 && team_side == 2)
                        {
                            hero_sequence_enemy[flagEnemy] = heroID;
                            flagEnemy++;
                        }
                        else if (i >= 5 && team_side == 3)
                        {
                            hero_sequence_teammate[flagTeam] = heroID;
                            flagTeam++;
                        }
                    }
                    string[] heroes = new string[5];
                    string[] heroesimg = new string[5];
                    int diff = 3;
                    int[] sugHeroID = counter_pick_logic.logic_counter_1(hero_sequence_enemy, hero_sequence_teammate, new int[0], diff);


                    // Check mark update version;
                    //int counter = 0;
                    //if (CurrentTick > table_checkmark[counter, 0] && CurrentTick < table_checkmark[counter, 1])
                    //{
                    //    overlay.XorCheck(table_checkmark[counter, 2]);
                    //    index = index - 1;
                    //    counter++;
                    //}
                    //else
                    //{
                    //    overlay.XorCheck(0);
                    //}

                    for (int j = 0; j < 5; j++)
                    {
                        heroesimg[j] = sugHeroID[j].ToString();
                        heroes[j] = ID_table[sugHeroID[j]];
                    }

                    overlay.AddHeroesSuggestionMessage(heroes, heroesimg);
                }
                else
                {
                    Console.WriteLine(teamname);
                }
            }
            else
            {
                Console.WriteLine("Picked"+ gsi.Name);
            }
            
        }
    }
}
