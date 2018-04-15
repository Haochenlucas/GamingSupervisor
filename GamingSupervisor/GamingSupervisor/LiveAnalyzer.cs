using Dota2Api;
using replayParse;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace GamingSupervisor
{
    class LiveAnalyzer : Analyzer
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int SetForegroundWindow(IntPtr point);
        
        [DllImport("user32.dll")]
        static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, uint wParam, int lParam);

        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        private const int KEYEVENTF_EXTENDEDKEY = 0x0001; // Key down flag
        private const int KEYEVENTF_KEYUP = 0x0002; // Key up flag

        private const ushort WM_KEYDOWN = 0x0100;
        private const ushort WM_KEYUP = 0x0101;

        private const int VK_F11 = 0x7A; // F11 key code
        private const int VK_OEM_5 = 0xDC;
        private const int VK_OEM_3 = 0xC0;
        private const int VK_HOME = 0x24;
        private const int VK_A = 0x41;

        private GameStateIntegration gsi;
        private DotaConsoleParser consoleData;

        // the object for the selection analyzer.
        private static counter_pick_logic cp = new counter_pick_logic();
        private static heroID h_ID = new heroID();

        private int[,] table = cp.selectTable();
        private Dictionary<string, int> hero_table = h_ID.getIDfromLowercaseHeroname();
        private Dictionary<int, string> ID_table = h_ID.getHeroID();
        private List<int> teamHeroIds = new List<int>(4);
        private List<int> teamIDGraph = new List<int>();

        private string playerName = null;


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

            if (gsi.GameState == "Undefined" || gsi.GameState == null || gsi.GameState == "")
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
                GetBoxPosition(initialInstructionsBox, out positionX, out positionY);

                overlay.ShowInstructionMessage(positionX, positionY, visualCustomizeHandle);

                Thread.Sleep(10);
            }

            overlay.HideInitialInstructions();

            bool keepLooping = true;

            Console.WriteLine("Currently analyzing...");
            string lastGameState = "";
            while (keepLooping)
            {
                if (!IsDotaRunning())
                {
                    overlay.Clear();
                    Console.WriteLine("Dota ended");
                    return;
                }

                switch (gsi.GameState)
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
                            });
                        }
                        break;
                    case "DOTA_GAMERULES_STATE_HERO_SELECTION":
                        if (lastGameState != "DOTA_GAMERULES_STATE_HERO_SELECTION")
                        {
                            lastGameState = "DOTA_GAMERULES_STATE_HERO_SELECTION";
                            consoleData.StartHeroSelectionParsing();

                            System.Windows.Application.Current.Dispatcher.Invoke(
                            () =>
                            {
                                initialInstructionsBox.Visibility = Visibility.Hidden;
                                heroSelectionBox.Visibility = Visibility.Visible;
                                highlightBarBox.Visibility = Visibility.Hidden;
                                healthGraphsBox.Visibility = Visibility.Hidden;
                                itemBox.Visibility = Visibility.Hidden;
                            });
                        }
                        HandleHeroSelection();
                        ShowHeroSelectionSuggestions();
                        break;
                    case "DOTA_GAMERULES_STATE_STRATEGY_TIME":
                        if (lastGameState != "DOTA_GAMERULES_STATE_STRATEGY_TIME")
                        {
                            if (playerName == null)
                                playerName = GetPlayerName();

                            lastGameState = "DOTA_GAMERULES_STATE_STRATEGY_TIME";

                            consoleData.StopHeroSelectionParsing();

                            overlay.ClearHeroSuggestion();
                            overlay.Clear();
                        }
                        break;
                    case "DOTA_GAMERULES_STATE_WAIT_FOR_MAP_TO_LOAD":
                        break;
                    case "DOTA_GAMERULES_STATE_PRE_GAME":
                    case "DOTA_GAMERULES_STATE_GAME_IN_PROGRESS":
                        if (lastGameState != "DOTA_GAMERULES_STATE_GAME_IN_PROGRESS")
                        {
                            lastGameState = "DOTA_GAMERULES_STATE_GAME_IN_PROGRESS";

                            consoleData.StartHeroDataParsing();

                            System.Windows.Application.Current.Dispatcher.Invoke(
                            () =>
                            {
                                initialInstructionsBox.Visibility = Visibility.Hidden;
                                heroSelectionBox.Visibility = Visibility.Hidden;
                                highlightBarBox.Visibility = Visibility.Hidden;
                                healthGraphsBox.Visibility = Visibility.Visible;
                                itemBox.Visibility = Visibility.Visible;
                            });
                        }
                        
                        SendCommandsToDota();

                        HandleGamePlay();
                        HandleHeroGraph();

                        UpdateInGameOverlay();
                        break;
                    default:
                        lastGameState = "Other";
                        break;
                }

                if (!keepLooping)
                {
                    break;
                }

                Thread.Sleep(10);
            }

            overlay.Clear();
            System.Windows.Application.Current.Dispatcher.Invoke(() => { visualCustomize.CloseWindow(); });
            Console.WriteLine("Game stopped!");
        }

        private void ShowHeroSelectionSuggestions()
        {
            double positionX = 0;
            double positionY = 0;
            GetBoxPosition(heroSelectionBox, out positionX, out positionY);

            overlay.ShowDraftMessage(positionX, positionY, visualCustomizeHandle);
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

            overlay.ShowInGameOverlay(visualCustomizeHandle,
                highlightBarPositionX, highlightBarPositionY,
                healthGraphPositionX, healthGraphPositionY,
                itemPositionX, itemPositionY,
                highlightBarWidth);
        }

    private long timeSinceLastKeyPress_ms = 0;
        private void SendCommandsToDota()
        {
            long now_ms = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (now_ms - timeSinceLastKeyPress_ms >= 500)
            {
                timeSinceLastKeyPress_ms = now_ms;
                
                IntPtr dotaHandle = Process.GetProcessesByName("dota2")[0].MainWindowHandle;
                if (GetForegroundWindow() == dotaHandle)
                {
                    //Console.WriteLine(dotaHandle + " " + Process.GetProcessesByName("dota2")[0].Handle + " " + Process.GetProcessesByName("dota2").Length);
                    //SetActiveWindow(dotaHandle);
                    //SetForegroundWindow(Process.GetProcessesByName("dota2")[0].Handle);
                    //SendKeys.SendWait("{F11}");
                    //SimWinInput.SimKeyboard.Press(VK_F11);
                    keybd_event(VK_HOME, 0, 0, (UIntPtr)0);
                    Thread.Sleep(100);
                    keybd_event(VK_HOME, 0, KEYEVENTF_KEYUP, (UIntPtr)0);

                    /*INPUT inp[2] = { 0 };
                    inp[0].type = INPUT_KEYBOARD;
                    inp[0].ki.wVk = VK_F5;
                    inp[1].type = INPUT_KEYBOARD;
                    inp[1].ki.wVk = VK_F5;
                    inp[1].ki.dwFlags = KEYEVENTF_KEYUP;

                    SendInput(2, inp, sizeof(INPUT));*/
                }
            }
        }

        private void HandleGamePlay()
        {
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

            if (gsi.Name != "null")
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
                            name = ConvertedHeroName.Get(name);
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
                    //Console.WriteLine(teamname);
                }
            }
            else
            {
                //Console.WriteLine("Picked"+ gsi.Name);
            }

        }

        void HandleHeroGraph()
        {
            Player[] heroes = consoleData.Heroes;
            foreach (var hero in heroes)
                if (hero.HeroName == "")
                    return;

            List<double> teamHealth = new List<double> { 0, 0, 0, 0, 0 };
            List<double> teamMaxHealth = new List<double> { 0, 0, 0, 0, 0 };

            List<int> heroIDs = new List<int>();

            teamHealth.Add(gsi.Health);
            teamMaxHealth.Add(gsi.MaxHealth);

            heroIDs.Add(hero_table[ConvertedHeroName.Get(gsi.Name)]);

            int indexStart = 0;
            string playerTeam = gsi.Team;
            switch (playerTeam) // Player always 0, Radiant next, Dire last
            {
                case "Radiant":
                    indexStart = 1;
                    break;
                case "Dire":
                    indexStart = 6;
                    break;
            }

            foreach (string name in heroID.heroName)
            {
                if (name == null)
                    continue;

                for (int i = indexStart; i < indexStart + 4; i++)
                {
                    if (ConvertedHeroName.Get(name).Contains(ConvertedHeroName.Get(heroes[i].HeroName)))
                    {
                        teamHealth.Add(heroes[i].Health);
                        teamMaxHealth.Add(heroes[i].MaxHealth);
                        
                        heroIDs.Add(hero_table[ConvertedHeroName.Get(name)]);

                        break;
                    }
                }
            }

            overlay.ToggleGraphForHeroHP();
            overlay.AddHeroGraphIcons(heroIDs);
            overlay.AddHPs(teamHealth.ToArray(), teamMaxHealth.ToArray());
            overlay.AddHp(teamHealth[0]);
        }

        private string GetPlayerName()
        {
            return WaitForGetPlayerNameAsync().Result;
        }

        private async Task<string> WaitForGetPlayerNameAsync()
        {
            // Need to use api handler because game state integration is returning wrong player name in bot games
            using (ApiHandler api = new ApiHandler("8BFC2C10E3D1E95B85DCF6AAD861782D"))
            {
                var summary = await api.GetPlayerSummary(new string[] { gsi.SteamID });
                return summary.Players[0].DisplayName;
            }
        }
    }
}
