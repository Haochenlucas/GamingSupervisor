﻿using System;
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
            manager = new OverlayManager(VS_HWND, out overlay, out d2d);

            /*
            string timePath = @"E:\University\2017 Second Half aka Fall\CS 4000 Senior Project\GamingSupervisor\GamingSupervisor\GamingSupervisor\Parser\3716503818\time.txt";

            List<String> timeLines = new List<String>(System.IO.File.ReadAllLines(timePath));
            Double.TryParse(timeLines.First().Split(' ')[2], out double firstTick);
            Double.TryParse(timeLines.Last().Split(' ')[2], out double totalTick);
            

            string combatPath = @"E:\University\2017 Second Half aka Fall\CS 4000 Senior Project\GamingSupervisor\GamingSupervisor\GamingSupervisor\Parser\3716503818\combat.txt";

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
            
            */

            //Thread.Sleep(2000);
            // Control FPS

            

            Stopwatch watch = new Stopwatch();
            d2d.SetupHintSlots();
            
            watch.Start();
            string instru_OpenReplay = "Step 1: Click Watch on the top.\nStep 2: Click Downloads\nStep 3: The replay you selected is\n        "
                 + ", click Watch to start.\n\nHint: Hover over the X icon for 2 seconds\n        to close";
            d2d.Intructions_setup(instru_OpenReplay);
            while (true)
            {
                if (watch.ElapsedMilliseconds < 15)
                {
                    continue;
                }

                // Low health
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

                //d2d.HeroSelectionHints(messages, imgName);
                string temp = "Lycan is a remarkable pusher who can wear down buildings and force enemies to react quickly to his regular tower onslaughts; as towers melt incredibly fast under Lycan's and his units' pressure, boosted by their canine Feral Impulse. His only contribution to full-on team fights will be the bonus damage he grants with Howl to his allies, his allies' summons, his owns summons, and himself, as well as his formidable physical attacks. Else he can surge out of the woods for a quick gank or push after he transformed with Shapeshift, moving at a haste speed of 650. Finally, good players will make the best usage of his Summon Wolves ability and scout the enemies' position while remaining undetected with invisibility at level 4.";

                d2d.HeroInfoHints(temp, "");
                d2d.ItemSelectionHints("You need to wear boots", "Boots_of_Speed_icon");
                //d2d.ToggleHightlight(true);
                //d2d.UpdateHighlightTime(tickInfo, (int)totalTick);

                //d2d.SelectedHeroSuggestion(38, Cursor.Position.Y);

                if (Control.ModifierKeys == Keys.Alt)
                {
                    //d2d.HeroSelection_Draw(VS_HWND, overlay);
                    //d2d.Intructions_Draw(VS_HWND, overlay);
                }
                else
                {
                    //d2d.Ingame_Draw(VS_HWND, overlay);
                }
                //Console.WriteLine(Direct2DRenderer.hits.hero_selection_1);

                
                watch.Restart();
            }
        }
    }
}
