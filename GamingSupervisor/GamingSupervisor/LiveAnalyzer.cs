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
        private static item_info i_info = new item_info();
        private string[,] item_Info_Table = i_info.get_Info_Table();
        private hero_intro hero_Intro = new hero_intro();

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

            while (gsi.GameState == "Undefined" || gsi.GameState == null || gsi.GameState == "")
            {
                if (!IsDotaRunning())
                {
                    overlay.Clear();
                    Console.WriteLine("Dota ended");
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

                    overlay.UpdateWindowHandler();
                    overlay.Intructions_setup("Start a game");
                    overlay.ShowInstructionMessage(positionX, positionY, visualCustomizeHandle);
                }
                else
                {
                    overlay.Clear();
                }
            }

            overlay.Clear();

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
                                junglingBox.Visibility = Visibility.Hidden;
                                retreatBox.Visibility = Visibility.Hidden;
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
                                junglingBox.Visibility = Visibility.Hidden;
                                retreatBox.Visibility = Visibility.Hidden;
                            });
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
                        break;
                    case "DOTA_GAMERULES_STATE_STRATEGY_TIME":
                        if (lastGameState != "DOTA_GAMERULES_STATE_STRATEGY_TIME")
                        {
                            if (playerName == null)
                                playerName = GetPlayerName();

                            lastGameState = "DOTA_GAMERULES_STATE_STRATEGY_TIME";

                            consoleData.StopHeroSelectionParsing();
                        }

                        overlay.ClearHeroSuggestion();
                        overlay.Clear();
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
                                healthGraphsBox.Visibility = Visibility.Hidden;
                                itemBox.Visibility = Visibility.Visible;
                                junglingBox.Visibility = Visibility.Hidden;
                                retreatBox.Visibility = Visibility.Visible;
                            });

                        }
                        
                        SendCommandsToDota();

                        bool isHealthGraphsBoxVisible = true;
                        bool isItemSuggestionsBoxVisible = true;
                        bool isRetreatBoxVisible = true;
                        System.Windows.Application.Current.Dispatcher.Invoke(
                            () =>
                            {
                                isHealthGraphsBoxVisible = healthGraphsBox.IsOverlayVisible;
                                isItemSuggestionsBoxVisible = itemBox.IsOverlayVisible;
                                isRetreatBoxVisible = retreatBox.IsOverlayVisible;
                            });

                        //if (isHealthGraphsBoxVisible)
                            //overlay.ShowHealthGraphs();
                        //else
                        overlay.HideHealthGraphs();
                        overlay.ToggleHighlight(false);

                        if (isItemSuggestionsBoxVisible)
                            overlay.ShowItemSuggestions();
                        else
                            overlay.HideItemSuggestions();

                        if (isRetreatBoxVisible)
                            overlay.ShowRetreat();
                        else
                            overlay.HideRetreat();

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

        private long timeSinceItemSuggestion_ms = 0;
        private long timeSinceheroInfo_ms = 0;
        private int itemflag = 0;
        private int heroInfoflag = 0;
        private void HandleGamePlay()
        {
            long item_now_ms = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string name = "";
            if (heroInfoflag == 0)
            {
                heroInfoflag = 1;
                timeSinceheroInfo_ms = item_now_ms;
            }
            if (item_now_ms - timeSinceheroInfo_ms <= 20000 && heroInfoflag == 1)
            {
                Dictionary<int, string> hero_Intro_Dic = hero_Intro.getHeroIntro();
                name = gsi.Name.Replace("hero_","*");
                string[] namestr1 = name.Split('*');
                name = string.Join("", namestr1[namestr1.Length-1].Split(new string[] { "_" }, StringSplitOptions.None));
                name = ConvertedHeroName.Get(name);
                string hero_Intro_String = hero_Intro_Dic[hero_table[name]];
                overlay.AddHeroInfoMessage(hero_Intro_String, "");
            }
            else
            {
                overlay.ClearHeroInfo();
            }
            



            // placeholders
            double[] hpToSend = new double[5] { 0, 0, 0, 0, 0 };
            double[] maxHpToSend = new double[5] { 0, 0, 0, 0, 0 };

            int healthPercent = gsi.HealthPercent;

            overlay.AddHPs(hpToSend, maxHpToSend);
            overlay.AddHp(hpToSend[0]);


            if (gsi.Health < (gsi.MaxHealth - 430) && gsi.Health != 0)
            {
                overlay.AddRetreatMessage("Low health warning! " + "Current Health: " + gsi.Health, "exclamation_mark");
            }
            else
            {
                overlay.ClearRetreat();
            }


            // For suggestion for tango and Healing_Salve.
            if (gsi.Health != 0 && gsi.Health < (gsi.MaxHealth - 130))
            {
                if (gsi.Health < (gsi.MaxHealth - 430) && gsi.Health != 0 && gsi.Items.Contains("item_flask"))
                {
                    long now_ms = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    if (itemflag == 0 || itemflag == 1)
                    {
                        itemflag = 2;
                        timeSinceItemSuggestion_ms = now_ms;
                    }
                    if (now_ms - timeSinceItemSuggestion_ms <= 10000)
                    {
                        string item_name = item_Info_Table[8 + 2, 2];
                        string item_tip = item_Info_Table[8 + 2, 117];
                        string item_content;
                        if (item_tip == " 0")
                        {
                            item_content = item_name + ":\n This is a good choice.";
                        }
                        else
                        {
                            item_content = item_name + ":\n " + item_tip;
                        }
                        if (itemflag == 2)
                        {
                            item_name = item_name.TrimStart(' ');
                            string item_img = item_name.Replace(" ", "_");
                            overlay.AddItemSuggestionMessage(item_content, item_img + "_icon");
                        }

                    }
                    else
                    {
                        overlay.ClearItemSuggestion();
                    }

                }
                else if (gsi.Items.Contains("item_tango"))
                {
                    long now_ms = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    if (itemflag == 0 || itemflag == 2)
                    {
                        itemflag = 1;
                        timeSinceItemSuggestion_ms = now_ms;
                    }
                    if (now_ms - timeSinceItemSuggestion_ms <= 10000)
                    {
                        string item_name = item_Info_Table[9 + 2, 2];
                        string item_tip = item_Info_Table[9 + 2, 117];
                        string item_content;
                        if (item_tip == " 0")
                        {
                            item_content = item_name + ":\nThis is a good choice.";
                        }
                        else
                        {
                            item_content = item_name + ":\n " + item_tip;
                        }
                        if(itemflag == 1)
                        {
                            item_name = item_name.TrimStart(' ');
                            string item_img = item_name.Replace(" ", "_");
                            overlay.AddItemSuggestionMessage(item_content, item_img + "_icon");
                        }
                        
                    }
                    else
                    {
                        overlay.ClearItemSuggestion();
                    }
                }
            }
            if (gsi.Health == 0|| gsi.Health == gsi.MaxHealth)
            {
                itemflag = 0;
                overlay.ClearItemSuggestion();
            }


            // item suggestion to buy something
            int item_suggestion_flag = 0;
            name = gsi.Name.Replace("hero_", "*");
            string[] namestr = name.Split('*');
            name = string.Join("", namestr[namestr.Length - 1].Split(new string[] { "_" }, StringSplitOptions.None));
            name = ConvertedHeroName.Get(name);
            if (gsi.Health == 0)
            {
                int item_id = i_info.item_suggestion_for_live(gsi.Items, name);
                long now_ms = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                if (item_suggestion_flag == 0)
                {
                    item_suggestion_flag = 1;
                    timeSinceItemSuggestion_ms = now_ms;
                }
                if (now_ms - timeSinceItemSuggestion_ms <= 10000)
                {
                    string item_name = item_Info_Table[item_id + 2, 2];
                    string item_tip = item_Info_Table[item_id + 2, 117];
                    string item_content;
                    if (item_tip == " 0")
                    {
                        item_content = item_name + ":\n This is a good choice.";
                    }
                    else
                    {
                        item_content = item_name + ":\n " + item_tip;
                    }
                    if (item_suggestion_flag == 1)
                    {
                        item_name = item_name.TrimStart(' ');
                        string item_img = item_name.Replace(" ", "_");
                        overlay.AddItemSuggestionMessage(item_content, item_img + "_icon");
                    }

                }
                else
                {
                    overlay.ClearItemSuggestion();
                    item_suggestion_flag = 0;
                }
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
