using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace replayParse
{
    public class ReplayHighlights
    {
        public int firstTick;
        public float lastTick;
        public Dictionary<int, Tuple<String, List<Tuple<String, String, String>>>> tickInfo;
        private Dictionary<int, List<Tuple<String, String, String>>> listInfo;

        public ReplayHighlights(string dataFolderLocation, string myHero)
        {
            heroID hid = new heroID();
            Dictionary<int, string> ID_table = hid.getHeroID(); // key is ID, value is hero_name;
            Dictionary<string, int> hero_table = hid.getIDfromLowercaseHeroname(); // key is hero_name, value is ID;

            string timePath = dataFolderLocation + "time.txt";
            string combatPath = dataFolderLocation + "combat.txt";

            List<String> timeLines = new List<String>(System.IO.File.ReadAllLines(timePath));
            Int32.TryParse(timeLines.First().Split(' ')[2], out this.firstTick);
            float.TryParse(timeLines.Last().Split(' ')[2], out this.lastTick);

            List<String> combatLines = new List<String>(System.IO.File.ReadAllLines(combatPath));
            List<List<String>> killLines = GetTeamfight(combatLines);
            this.tickInfo = new Dictionary<int, Tuple<String, List<Tuple<String, String, String>>>>();
            listInfo = new Dictionary<int, List<Tuple<string, string, string>>>();

            foreach (var kills in killLines)
            {
                listInfo[(int)Double.Parse(kills[0])] = new List<Tuple<string, string, string>>();
                for (int i = 1; i < kills.Count; i++)
                {
                    string[] cont = kills[i].Split(new char[] { ' ' });
                    string killed = ID_table[hero_table[ConvertedHeroName.Get(cont[0])]];
                    string killer = ID_table[hero_table[ConvertedHeroName.Get(cont[1])]];
                    
                    string color = "we";
                    if (killed == myHero)
                    {
                        color = "R";
                    }
                    else if (killer == myHero)
                    {
                        color = "G";
                    }
                    listInfo[(int)Double.Parse(kills[0])].Add(new Tuple<string, string, string>(killer, killed, color));
                }
            }
        }     
        
        public static void ConstructTickInfo()
        {

        }


        /** Finds teamfights, returns them in a list with each item being a teamfight
         Each teamfight starts with a time, then followed by strings of "killed killer"
            **/
        private static List<List<String>> GetTeamfight(List<String> lines)
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
    }
}
