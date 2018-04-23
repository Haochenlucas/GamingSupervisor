using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;



namespace replayParse
{
    public class item_info
    {
        private string[,] item_table_info = new string[157, 120]; // start at [3,1] with ID of first item.
        private int[,] item_KB = new int[116, 3];
        public item_info()
        {

            string s = Path.Combine(Environment.CurrentDirectory, "Properties/item_info.txt");
            string[] item_Lines = System.IO.File.ReadAllLines(s);
            int startline = 3;
            foreach (string line in item_Lines)
            {
                string[] words = line.Split('*');
                for( int i  = 0; i< words.Length; i++)
                {
                    item_table_info[startline, i+1] = words[i];
                }
                startline++;
            }



            s = Path.Combine(Environment.CurrentDirectory, "Properties/hero_item.txt");
            string[] lines = System.IO.File.ReadAllLines(s);

            for( int i = 0; i < lines.Length; i ++ )
            {
                if (lines[i].Contains("["))
                {
                    string[] three_items = lines[i].Split('[');
                    three_items[1] = three_items[1].Remove(three_items[1].Length-1);
                    string[] three_item_cur = three_items[1].Split(' ');
                    item_KB[i + 1, 0] = int.Parse(three_item_cur[0]);
                    item_KB[i + 1, 1] = int.Parse(three_item_cur[1]);
                    item_KB[i + 1, 2] = int.Parse(three_item_cur[2]);
                }
            }
        }

        public string[,] get_Info_Table()
        {
            return item_table_info;
        }

        public Dictionary<int,int> item_suggestion(int money, string dataFolderLocation, string myHero)
        {
            Dictionary<int, int> item_ID = new Dictionary<int, int>();
            heroID hid = new heroID();
            Dictionary<int, string> ID_table = hid.getHeroID(); // key is ID, value is hero_name;
            Dictionary<string, int> hero_table = hid.getIDfromLowercaseHeroname(); // key is hero_name, value is ID;
            int firstTick;
            int lastTick;
            string timePath = dataFolderLocation + "time.txt";
            string combatPath = dataFolderLocation + "combat.txt";

            List<String> timeLines = new List<String>(System.IO.File.ReadAllLines(timePath));
            Int32.TryParse(timeLines.First().Split(' ')[0], out firstTick);
            Int32.TryParse(timeLines.Last().Split(' ')[0], out lastTick);
            List<String> combatLines = new List<String>(System.IO.File.ReadAllLines(combatPath));
            List<List<String>> teamfight = new List<List<string>>();
            Dictionary<int, List<Tuple<String, String, String>>> tickInfo = new Dictionary<int, List<Tuple<string, string, string>>>();
            int currInd = 0;
            TimeSpan prevTime = new TimeSpan();
            TimeSpan thirty = TimeSpan.FromSeconds(30);
            string heroPattern = "hero.*hero";

            foreach (var line in combatLines)
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
                            teamfight[currInd].Add(contents[2] + " " + contents[3]);
                        }

                    }
                }
            }
            int oneThird = (2 * firstTick + lastTick) / 3;
            int twoThird = (firstTick + 2*lastTick) / 3;
            foreach (var kills in teamfight)
            {
                tickInfo[(int)Double.Parse(kills[0])] = new List<Tuple<string, string, string>>();
                for (int i = 1; i < kills.Count; i++)
                {
                    string[] cont = kills[i].Split(new char[] { ' ' });
                    string killed = ID_table[hero_table[ConvertedHeroName.Get(cont[0])]];
                    string killer = ID_table[hero_table[ConvertedHeroName.Get(cont[1])]];
                    if (killed == myHero)
                    {
                        int itemInfo;
                        int killedID = hero_table[ConvertedHeroName.Get(killed)];
                        int curtick = (int)(Double.Parse(kills[0]));
                        if(curtick<= oneThird)
                        {
                            itemInfo = item_KB[killedID, 0];
                        }
                        else if (twoThird > curtick && curtick > oneThird)
                        {
                            itemInfo = item_KB[killedID, 1];
                        }
                        else
                        {
                            itemInfo = item_KB[killedID, 2];
                        }
                        item_ID.Add((int)(Double.Parse(kills[0])), itemInfo);
                    }
                }
            }
            return item_ID;
        }



        public int item_suggestion_for_live(List<string> items,  string myHero)
        {
            Dictionary<int, int> item_ID = new Dictionary<int, int>();
            heroID hid = new heroID();
            Dictionary<int, string> ID_table = hid.getHeroID(); // key is ID, value is hero_name;
            Dictionary<string, int> hero_table = hid.getIDfromLowercaseHeroname(); // key is hero_name, value is ID;
            int hero_cur_id = hero_table[myHero];
            List<int> itemsID = new List<int>();
            string item_name = "";
            string item_lower = "";
            foreach(string s in items)
            {
                item_name = s.Replace("item","");
                item_name = item_name.Replace("_", "");
                if (item_name.Contains("travel"))
                {
                    item_name = "bootsoftravel";
                }
                else if (item_name.Contains("cyclone"))
                {
                    item_name = "eul";
                }
                for (int i = 3; i< 156; i++)
                {
                    item_lower = item_table_info[i,2].ToLower();
                    item_lower = item_lower.Replace(" ", "");
                    if (item_lower.Contains(item_name))
                    {
                        itemsID.Add(i-2);
                    }
                }
            }

            for(int i = 0; i< 3; i++)
            {
                if (!itemsID.Contains(item_KB[hero_cur_id, i]))
                {
                    return item_KB[hero_cur_id, i];
                }
            }
            return 121;
        }
    }

}
