using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace replayParse
{
    public class counter_pick_logic
    {
        public static double[,] matrix_info = new double[116, 116];
        // the hero_ID_Client_Team is all pick and ban hero, 
        // the second dimension first column is about hero_id,the second_column is about hero client id, the third_column is about team side, the fourth_column is about tic.
        // team side(1: ban , 2: team 1; 3 :team 2;).
        public static int[,] hero_ID_Client_Team = new int[30,4];

        public counter_pick_logic()
        {
            counterpick_info cp_info = new counterpick_info();
            matrix_info = cp_info.getCounterTable();
        }

        public int[] logic_counter(int[] hero_sequence, int[] ban_list)
        {
            Dictionary<double, int> five_picks = new Dictionary<double, int>();
            int[] five_hero_array = new int[5];
            int length = hero_sequence.Length;
            int count = 1;
            int flag = 0;
            double lowest_fact = 100;
            double[] counterFact = new double[5];
            while (count <= 115)
            {
                if (hero_sequence.Contains(count) || ban_list.Contains(count))
                {
                    count++;
                    continue;
                }    
                double sum1 = 0;
                for (int i = 0; i < length; i++)
                {
                    sum1 = sum1 + matrix_info[hero_sequence[i], count];
                }
                if (flag < 5)
                {
                    five_picks.Add(sum1, count);
                    if (sum1 < lowest_fact)
                        lowest_fact = sum1;
                    flag++;
                }
                else if (flag >= 5)
                {
                    if (sum1 > lowest_fact)
                    {
                        five_picks.Remove(lowest_fact);
                        five_picks.Add(sum1, count);
                        lowest_fact = five_picks.Min(i =>i.Key);
                    }
                }
                count++;
            }
            var list = five_picks.Keys.ToList();
            list.Sort();

            // Loop through keys.
            int index = 4;
            foreach (var key in list)
            {
                five_hero_array[index] = five_picks[key];
                index--;
            }

            return five_hero_array;
        }

        public void readTeam()
        {
            //count the hero name
            int count = 0;
            //count the hero team
            int count1 = 0;
            string s = Path.Combine(Environment.CurrentDirectory, @"..\..\..\GamingSupervisor\Parser\info.txt");
            //s = @"C: \Users\dominate\Desktop\GamingSupervisor\GamingSupervisor\GamingSupervisor\Parser\replay.txt";
            string[] lines = System.IO.File.ReadAllLines(s);
            heroIDClient ID_client = new heroIDClient();
            Dictionary<string, int> clientID_Dic = ID_client.getIDHero();
            Dictionary<int, string> clientHero_Dic = ID_client.getHeroID();

            heroID ID_hero = new heroID();
            Dictionary<string, int> ID_Dic = ID_hero.getIDHero();
            Dictionary<int, string> Hero_Dic = ID_hero.getHeroID();
            var caseInsensitiveDictionary = new Dictionary<string, int>(
                        StringComparer.OrdinalIgnoreCase);
            foreach (string key in clientID_Dic.Keys)
            {
                int value1 = 0;
                clientID_Dic.TryGetValue(key, out value1);
                if (value1 > 0)
                {
                    caseInsensitiveDictionary.Add(key, value1);
                }
            }
            foreach (string line in lines)
            {
                if (line.Contains("hero_name:"))
                {
                    string[] words = line.Split(' ');
                    if (words[words.Length-1].Contains("npc_dota_hero"))
                    {
                        string[] substrings = Regex.Split(words[words.Length - 1], "hero_");
                        var r = new Regex(@"
                            (?<=[A-Z])(?=[A-Z][a-z]) |
                             (?<=[^A-Z])(?=[A-Z]) |
                             (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);
                        string name = r.Replace(substrings[1], " ");
                        name = string.Join("", name.Split(new string[] { "_" }, StringSplitOptions.None));
                        name = name.TrimEnd( new Char[] { '"', ' '});
                        int value_cur = 0;
                        string hero_name = "";
                        var comparer = StringComparer.OrdinalIgnoreCase;
                        foreach (KeyValuePair<string, int> entry in caseInsensitiveDictionary)
                        {
                            string name1 = entry.Key.ToLower();
                            if (String.Equals(name1, name))
                            {
                                value_cur = caseInsensitiveDictionary[name];
                                hero_name = clientHero_Dic[value_cur];
                                int table_0 = 0;
                                ID_Dic.TryGetValue(hero_name, out table_0);
                                hero_ID_Client_Team[count, 0] = table_0;
                                hero_ID_Client_Team[count, 1] = value_cur;
                                count++;
                            }
                        }
                    }
                }
                if (line.Contains("game_team:"))
                {
                    string[] words = line.Split(' ');
                    int number = 0;
                    if (Int32.TryParse(words[words.Length - 1], out number))
                    {
                        hero_ID_Client_Team[count1, 2] = number;
                        count1++;
                    }
                }
            }

            s = Path.Combine(Environment.CurrentDirectory, @"..\..\..\GamingSupervisor\Parser\selection.txt");
            //s = @"C: \Users\dominate\Desktop\GamingSupervisor\GamingSupervisor\GamingSupervisor\Parser\replay.txt";
            lines = System.IO.File.ReadAllLines(s);
            foreach (string line in lines)
            {
                string[] words = line.Split(' ');
                int tic = Int32.Parse(words[0]);
                int team = 0;
                int heroID = 0;
                if (words[1].Contains('B'))
                {
                    heroID = Int32.Parse(words[2]);
                    string hero_name = "";
                    clientHero_Dic.TryGetValue(heroID, out hero_name);
                    int cur_ID = 0;
                    ID_Dic.TryGetValue(hero_name, out cur_ID);
                    team = 1;
                    hero_ID_Client_Team[count, 1] = heroID;
                    hero_ID_Client_Team[count, 2] = team;
                    hero_ID_Client_Team[count, 0] = cur_ID;
                    hero_ID_Client_Team[count, 3] = tic;
                    count++;
                }
                else
                {
                    heroID = Int32.Parse(words[2]);
                    string hero_name = "";
                    clientHero_Dic.TryGetValue(heroID, out hero_name);
                    int cur_ID = 0;
                    ID_Dic.TryGetValue(hero_name, out cur_ID);
                    for(int i = 0; i < count; i++)
                    {
                        if(hero_ID_Client_Team[i,0] == cur_ID)
                        {
                            hero_ID_Client_Team[i, 3] = tic; 
                        }
                    }
                }

            }
        }

        public int[,] selectTable()
        {
            return hero_ID_Client_Team;
        }
    }
}
