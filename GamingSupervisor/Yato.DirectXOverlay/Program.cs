using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace Yato.DirectXOverlay
{
    class Program
    {
        // Finds teamfights, returns them in a list with each item being a teamfight
        // Each teamfight starts with a time, then followed by strings of "killed killer"
        public static List<List<String>> GetTeamfight(List<String> lines)
        {
            List<List<String>> teamfight = new List<List<string>>();
            int currInd = 0;
            TimeSpan prevTime = new TimeSpan();
            TimeSpan thirty = TimeSpan.FromSeconds(30);
            string heroPattern = "hero.*hero";

            foreach (var line in lines)
            {
                if (line.Contains("[KILL]"))
                {
                    if (Regex.IsMatch(line, heroPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        int filler = 1;

                        if (teamfight.Count < currInd + 1)
                            teamfight.Add(new List<String>());

                        List<String> contents = new List<String>(line.Split(new char[] { ' ' }));
                        if (prevTime == new TimeSpan())
                            prevTime = TimeSpan.FromSeconds(Double.Parse(contents[0]));

                        TimeSpan currTime = TimeSpan.FromSeconds(Double.Parse(contents[0]));

                        if (prevTime == currTime)
                        {
                            teamfight[currInd].Add(contents[0]);
                            //teamfight[currInd].Add(currTime.ToString(@"hh\:mm\:ss"));
                            teamfight[currInd].Add(contents[2] + " " + contents[3]);                   
                        }
                        else if (prevTime.Add(thirty) > currTime)
                        {
                            teamfight[currInd].Add(contents[2] + " " + contents[3]);
                        }
                        else
                        {
                            currInd++;
                            teamfight.Add(new List<String>());
                            prevTime = currTime;
                            teamfight[currInd].Add(contents[0]);
                            //teamfight[currInd].Add(currTime.ToString(@"hh\:mm\:ss"));
                            teamfight[currInd].Add(contents[2] + " " + contents[3]);
                        }
                        
                    }
                }
            }
            return teamfight;
        }

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

            string timePath = @"C:\Users\J\Documents\CS_Senior_Project\GamingSupervisor\GamingSupervisor\Parser\3690751201\time.txt";
            List<String> timeLines = new List<String>(System.IO.File.ReadAllLines(timePath));
            Double.TryParse(timeLines.First().Split(' ')[2], out double firstTick);
            Double.TryParse(timeLines.Last().Split(' ')[2], out double totalTick);
            
            string combatPath = @"C:\Users\J\Documents\CS_Senior_Project\GamingSupervisor\GamingSupervisor\Parser\3690751201\combat.txt";
            List<String> combatLines = new List<String>(System.IO.File.ReadAllLines(combatPath));
            List<List<String>> killLines = GetTeamfight(combatLines);
            String myHero = "npc_dota_hero_storm_spirit";
            Dictionary<int, List<Tuple<String, String, String>>> tickInfo = new Dictionary<int, List<Tuple<string, string, string>>>();

            foreach (var kills in killLines)
            {
                tickInfo[(int)Double.Parse(kills[0])] = new List<Tuple<string, string, string>>();
                for (int i = 1; i < kills.Count; i++)
                {
                    string[] cont = kills[i].Split(new char[] { ' ' });
                    string color = "we";
                    if (cont[0] == myHero)
                    {
                        color = "R";
                    }
                    else if (cont[1] == myHero)
                    {
                        color = "G";
                    }
                    tickInfo[(int)Double.Parse(kills[0])].Add(new Tuple<string, string, string>(cont[1], cont[0], color));
                }
            }

            

           

            #endregion

            //Thread.Sleep(2000);
            // Control FPS
            Stopwatch watch = new Stopwatch();
            d2d.SetupHintSlots();
            
            watch.Start();
            d2d.Intructions_setup("");
            d2d.HeroIntro_setup(38);
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
                    d2d.UpdateHighlightTime(tickInfo, (int)totalTick);

                    d2d.Retreat("Run", "");
                    
                    d2d.SelectedHeroSuggestion(38, 500);
                }
                //if (Control.ModifierKeys == Keys.Alt)
                {
                    //d2d.Ingame_Draw(VS_HWND, overlay);
                    d2d.HeroInfo_Draw(VS_HWND, overlay);
                }
                //else
                {
                    //d2d.Intructions_Draw(VS_HWND, overlay);
                }

                watch.Restart();
            }
        }
    }
}
