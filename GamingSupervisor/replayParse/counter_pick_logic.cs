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
        private string dataFolderLocation;

        public static double[,] matrix_info = new double[116, 116];
        // the hero_ID_Client_Team is all pick and ban hero, 
        // the second dimension first column is about hero_id,the second_column is about hero client id, the third_column is about team side, the fourth_column is about tic.
        // team side(0: (ban from team 1), 1: (ban from team 2) , 2: (pick from team 1), 3: (pick from team 2)).
        public static int[,] hero_ID_Client_Team = new int[30,4];

        public counter_pick_logic(string dataFolderLocation)
        {
            this.dataFolderLocation = dataFolderLocation;

            counterpick_info cp_info = new counterpick_info();
            matrix_info = cp_info.getCounterTable();
        }

        

        public void readTeam()
        {
            //count the hero name
            int count = 0;
            //count the hero team
            int count1 = 0;
            string s = dataFolderLocation + "info.txt";

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
            int mode = 0;
            int ban = 0;
            foreach (string line in lines)
            {
                if(mode == 1)
                {
                    if (ban ==1)
                    {
                        if (line.Contains("team"))
                        {
                            mode = 2;
                            string[] words = line.Split(' ');
                            int team_number = -1;
                            if (int.TryParse(words[words.Length - 1], out team_number))
                            {
                                if(team_number == 2)
                                {
                                    hero_ID_Client_Team[count, 2] = 0;
                                }
                                else
                                {
                                    hero_ID_Client_Team[count, 2] = 1;                  
                                }
                            }

                        }
                    }
                    else
                    {
                        if (line.Contains("team"))
                        {
                            mode = 2;
                            string[] words = line.Split(' ');
                            int team_number = -1;
                            if (int.TryParse(words[words.Length - 1], out team_number))
                            {
                                if (team_number == 2)
                                {
                                    hero_ID_Client_Team[count, 2] = 2;
                                }
                                else
                                {
                                    hero_ID_Client_Team[count, 2] = 3;
                                }
                            }

                        }
                    }
                }
                else if (line.Contains("is_pick:"))
                {
                    mode = 1;
                    string[] words = line.Split(' ');
                    if (words[words.Length-1].Contains("false"))
                    {
                        ban = 1;
                    }
                }
                else if( mode == 2)
                {
                    if (line.Contains("hero_id:"))
                    {
                        mode = 0;
                        string[] words = line.Split(' ');
                        int hero_number = -1;
                        if (int.TryParse(words[words.Length - 1], out hero_number))
                        {
                            hero_ID_Client_Team[count, 1] = hero_number;
                            string hero_name = clientHero_Dic[hero_number];
                            int hero_id_cur = ID_Dic[hero_name];
                            hero_ID_Client_Team[count, 0] = hero_id_cur;
                            count++;
                            ban = 0;
                        }
                    }
                }
            }

            s = dataFolderLocation + "selection.txt";

            lines = System.IO.File.ReadAllLines(s);
            foreach (string line in lines)
            {
                string[] words = line.Split(' ');
                int tic = Int32.Parse(words[0]);
                int team = 0;
                int heroID = 0;
                heroID = Int32.Parse(words[2]);
                string hero_name = "";
                clientHero_Dic.TryGetValue(heroID, out hero_name);
                int cur_ID = 0;
                ID_Dic.TryGetValue(hero_name, out cur_ID);
                for (int i = 0; i < count; i++)
                {
                    if (hero_ID_Client_Team[i, 0] == cur_ID)
                    {
                        hero_ID_Client_Team[i, 3] = tic;
                    }
                }
                //if (words[1].Contains('B'))
                //{
                //    heroID = Int32.Parse(words[2]);
                //    string hero_name = "";
                //    clientHero_Dic.TryGetValue(heroID, out hero_name);
                //    int cur_ID = 0;
                //    ID_Dic.TryGetValue(hero_name, out cur_ID);
                //    team = 1;
                //    hero_ID_Client_Team[count, 1] = heroID;
                //    hero_ID_Client_Team[count, 2] = team;
                //    hero_ID_Client_Team[count, 0] = cur_ID;
                //    hero_ID_Client_Team[count, 3] = tic;
                //}
                //else
                //{
                //    heroID = Int32.Parse(words[2]);
                //    string hero_name = "";
                //    clientHero_Dic.TryGetValue(heroID, out hero_name);
                //    int cur_ID = 0;
                //    ID_Dic.TryGetValue(hero_name, out cur_ID);
                //    for(int i = 0; i < count; i++)
                //    {
                //        if(hero_ID_Client_Team[i,0] == cur_ID)
                //        {
                //            hero_ID_Client_Team[i, 3] = tic; 
                //        }
                //    }
                //}

            }
        }

        public int[,] selectTable()
        {
            return hero_ID_Client_Team;
        }

        /*
         * team_name is come from the selectionTable. So proper input is 2 and 3.
         * suggestionTable: second dim: first column : tic, second to six column is hero_id.
         */
        public int[,] suggestionTable(int team_name)
        {
            int[,] table_suggestion = new int[25,6];
            int table_count = 0;
            if (team_name == 2)
            {
                int[] pick_list = new int[5];
                int pick_count = 0;
                int[] ban_list = new int[17];
                int ban_count = 0;
                for (int i = 0; i < hero_ID_Client_Team.Length/4; i++)
                {
                    if (hero_ID_Client_Team[i,1] != 0)
                    {
                        int tic = hero_ID_Client_Team[i, 3];
                        table_suggestion[table_count, 0] = tic;
                        if (hero_ID_Client_Team[i, 2] <= 2)
                        {
                            ban_list[ban_count] = hero_ID_Client_Team[i, 0];
                            ban_count++;
                        }
                        else
                        {
                            pick_list[pick_count] = hero_ID_Client_Team[i, 0];
                            pick_count++;
                        }
                        int[] suggest_cur = logic_counter(pick_list, ban_list);
                        for(int j = 1; j < 6; j++)
                        {
                            table_suggestion[table_count, j] = suggest_cur[j - 1];
                        }
                        table_count++;
                    }
                    else
                    {
                        break;
                    }

                }

            }
            else
            {
                int[] pick_list = new int[5];
                int pick_count = 0;
                int[] ban_list = new int[17];
                int ban_count = 0;
                for (int i = 0; i < hero_ID_Client_Team.Length / 4; i++)
                {
                    if (hero_ID_Client_Team[i, 1] != 0)
                    {
                        int tic = hero_ID_Client_Team[i, 3];
                        table_suggestion[table_count, 0] = tic;
                        if (hero_ID_Client_Team[i, 2] < 2 || hero_ID_Client_Team[i, 2] == 3)
                        {
                            ban_list[ban_count] = hero_ID_Client_Team[i, 0];
                            ban_count++;
                        }
                        else
                        {
                            pick_list[pick_count] = hero_ID_Client_Team[i, 0];
                            pick_count++;
                        }
                        int[] suggest_cur = logic_counter(pick_list, ban_list);
                        for (int j = 1; j < 6; j++)
                        {
                            table_suggestion[table_count, j] = suggest_cur[j - 1];
                        }
                        table_count++;
                    }
                    else
                    {
                        break;
                    }

                }

            }
            return table_suggestion;
        }

        public static int[] logic_counter(int[] hero_sequence, int[] ban_list)
        {
            Dictionary<double, int> five_picks = new Dictionary<double, int>();
            Dictionary<double, int> porper_pick = new Dictionary<double, int>();
            porper_pick.Add(1, 91);
            porper_pick.Add(1.4, 46);
            porper_pick.Add(1.6, 87);
            porper_pick.Add(1.8, 105);
            porper_pick.Add(2, 13);
            porper_pick.Add(1.7, 15);
            porper_pick.Add(1.2, 20);
            int[] five_hero_array = new int[5];
            int length = hero_sequence.Length;
            int count = 1;
            int flag = 0;
            double lowest_fact = 100;
            double[] counterFact = new double[5];
            if (hero_sequence.Sum() == 0)
            {
                foreach(KeyValuePair<double,int> pair in porper_pick)
                {
                    if (!ban_list.Contains(pair.Value))
                    {
                        five_picks.Add(pair.Key,pair.Value);
                    }
                    if(five_picks.Count == 5)
                    {
                        break;
                    }
                }
            }
            else
            {
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
                        if (five_picks.ContainsKey(sum1))
                        {
                            sum1 = sum1 + 0.001;
                        }
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
                            if (five_picks.ContainsKey(sum1))
                            {
                                sum1 = sum1 + 0.001;
                            }
                            five_picks.Add(sum1, count);
                            lowest_fact = five_picks.Min(i => i.Key);
                        }
                    }
                    count++;
                }
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
    }
}
