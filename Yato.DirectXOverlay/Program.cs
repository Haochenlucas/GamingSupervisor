using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace Yato.DirectXOverlay
{
    class Program
    {
        static void Main(string[] args)
        {
            // Declare window and overlay object
            OverlayWindow overlay;
            Direct2DRenderer d2d;
            OverlayManager manager;

            // Pass window handle of Dota2 into Initialization function
            if (Process.GetProcessesByName("dota2").Length > 0)
            {
                var dota_HWND = Process.GetProcessesByName("dota2")[0].MainWindowHandle;
                manager = new OverlayManager(dota_HWND, out overlay, out d2d);
            }

            // For test use only. Show overlay on Visual Studio
            var VS_HWND = Process.GetProcessesByName("notepad++")[0].MainWindowHandle;
            manager = new OverlayManager(VS_HWND,out overlay,out d2d);

            #region timeline

            string timePath = @"X:\Documents\GamingSupervisor\GamingSupervisor\GamingSupervisor\Parser\3703866531\time.txt";
            List<String> timeLines = new List<String>(System.IO.File.ReadAllLines(timePath));
            int firstTick = 0;
            Int32.TryParse(timeLines.First().Split(' ')[0], out firstTick);
            int totalTick = 0;
            Int32.TryParse(timeLines.Last().Split(' ')[0], out totalTick);
            
            string statePath = @"X:\Documents\GamingSupervisor\GamingSupervisor\GamingSupervisor\Parser\3703866531\state.txt";
            List<String> stateLines = new List<String>(System.IO.File.ReadAllLines(statePath));
            int initStateTick = 0;
            Int32.TryParse(stateLines.Find(str => str.Contains("[STATE] 4")).Split(' ')[0], out initStateTick);
            int postStateTick = 0;
            Int32.TryParse(stateLines.Find(str => str.Contains("[STATE] 6")).Split(' ')[0], out postStateTick);

            System.Random rand = new System.Random();
            int numHighlights = 10;
            Dictionary<int, List<Tuple<string, string, string>>> randInfo = new Dictionary<int, List<Tuple<string, string, string>>>();
            List<String> myTeam = new List<string>{ "Disruptor", "Storm Spirit", "Tusk", "Brewmaster", "Morphling" };
            List<String> theirTeam = new List<string> { "Witch Doctor", "Dragon Knight", "Gyrocopter", "Invoker", "Rattletrap" };
            String myHero = myTeam[rand.Next(0,5)];
            Console.Write(myHero);

            for (int i = 0; i < numHighlights; i++)
            {
                int currTick = rand.Next(initStateTick, postStateTick);
                randInfo[currTick] = new List<Tuple<string, string, string>>();
                int killCount = rand.Next(1, 10);
                for (int j = 0; j < killCount; j++) {
                    int whoKillWho = rand.Next(2);
                    if (whoKillWho == 1) // myTeam kill theirTeam
                    {
                        string killer = myTeam[rand.Next(5)];
                        string killed = theirTeam[rand.Next(5)];
                        string color = "LG";
                        if (killer == myHero)
                            color = "G";
                        randInfo[currTick].Add(new Tuple<string, string, string>(killer, killed, color));
                    }
                    else
                    {
                        string killer = theirTeam[rand.Next(5)];
                        string killed = myTeam[rand.Next(5)];
                        string color = "LR";
                        if (killed == myHero)
                            color = "R";
                        randInfo[currTick].Add(new Tuple<string, string, string>(killer, killed, color));
                    }
                }
            }

            #endregion

            //Thread.Sleep(2000);
            // Control FPS
            Stopwatch watch = new Stopwatch();
            d2d.SetupHintSlots();
            
            watch.Start();
            while (true)
            {
                if (watch.ElapsedMilliseconds < 15)
                {
                    continue;
                }

                // Low health
                if (true)
                {
                    string[] messages = new string[5];
                    messages[0] = "Abaddon";
                    messages[1] = "Alchemist";
                    messages[2] = "Ancient Apparition";
                    messages[3] = "Anti-mage";
                    messages[4] = "Axe";
                    string[] imgName = new string[5];
                    imgName[0] = "1";
                    imgName[1] = "2";
                    imgName[2] = "3";
                    imgName[3] = "4";
                    imgName[4] = "6";

                    d2d.HeroSelectionHints(messages, imgName);

                    d2d.ToggleHightlight(true);
                    d2d.UpdateHighlightTime(randInfo, totalTick);

                    d2d.Retreat("Run", "");
                    
                    //d2d.SelectedHeroSuggestion(38);
                }
                if (Control.ModifierKeys == Keys.Alt)
                {
                    d2d.low_hp = true;
                    
                    //d2d.DeleteMessage(0);
                    //d2d.ban_and_pick = -5;
                }
                else
                {
                    d2d.low_hp = false;
                }

                d2d.Draw(VS_HWND, overlay);

                watch.Restart();
            }
        }
    }
}
