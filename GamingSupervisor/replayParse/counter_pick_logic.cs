using System;
using System.Collections.Generic;
using System.Linq;

namespace replayParse
{
    public class counter_pick_logic
    {
        private string dataFolderLocation;
        // the matrix_info contains the information we need for counter pick factor.
        public static double[,] matrix_info = new double[116, 116];
        // the hero_ID_Client_Team is all pick and ban hero, 
        // the second dimension first column is about hero_id,the second_column is about hero client id, the third_column is about team side, the fourth_column is about tic.
        // team side(0: (ban from team 1), 1: (ban from team 2) , 2: (pick from team 1), 3: (pick from team 2)).
        public static int[,] hero_ID_Client_Team = new int[30, 4];
        // the table_suggestion is for each ban or pick tic, what heros we suggest to pick.
        // the second dimension first column is tic, 2 to 6 are the fice suggestion heros.
        public static int[,] table_suggestion = new int[25, 6];
        public static int[,] HT_table;

        public counter_pick_logic(string dataFolderLocation)
        {
            this.dataFolderLocation = dataFolderLocation;

            counterpick_info cp_info = new counterpick_info();
            matrix_info = cp_info.getCounterTable();
            this.readTeam();
            heroGenerateTypes hGT = new heroGenerateTypes();
            HT_table = hGT.getTypeTable();
        }

        public counter_pick_logic()
        {
            counterpick_info cp_info = new counterpick_info();
            matrix_info = cp_info.getCounterTable();
            heroGenerateTypes hGT = new heroGenerateTypes();
            HT_table = hGT.getTypeTable();
        }


        /*
         *build up the hero_ID_Client_Team Team
         */
        public void readTeam()
        {
            //count the hero name
            int count = 0;
            //count the hero team
            //int count1 = 0;
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
                if (mode == 1)
                {
                    if (ban == 1)
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
                    if (words[words.Length - 1].Contains("false"))
                    {
                        ban = 1;
                    }
                }
                else if (mode == 2)
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
                //int team = 0;
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
            }
        }

        /*
         *Output: the hero_ID_Client_Team table which is all pick and ban hero, 
         *          the second dimension first column is about hero_id,the second_column is about hero client id, the third_column is about team side, the fourth_column is about tic.
         *          team side(0: (ban from team 1), 1: (ban from team 2) , 2: (pick from team 1), 3: (pick from team 2)).
         */
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
            int table_count = 0;
            if (team_name == 2)
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

        /*
         * version_1.0.0 logic_counter 
         * Input: the hero_sequence which is heros picked by enemy team
         *        the ban_list contains the heros picked by own team and all baned heros by both team.
         * output: the best five hero we suggest to pick
         * improve place: consider the role of the heros in the own team, consider the difficulty of the heros.
         */
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
                foreach (KeyValuePair<double, int> pair in porper_pick)
                {
                    if (!ban_list.Contains(pair.Value))
                    {
                        five_picks.Add(pair.Key, pair.Value);
                    }
                    if (five_picks.Count == 5)
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

        /*
         * Call this function require to call suggestionTable() to update the table_suggestion table.
         * Output: int[,]: the first dimension is about how many checkmark or X mark will show in the selection part.
         * The second dimension: the first column is start_tic of this checkmark or x mark. the second column is end_tic of this checkmark or x mark.
         * the third column is whether is a checkmark or x mark and combine with hero_id.
         */
        public int[,] checkMark()
        {
            int[,] checkTable = new int[25, 3];
            int index = 0;
            for (int i = 0; i < 25; i++)
            {
                int banorpick = 1;
                int tic_1;
                int tic_2;
                int tic_3;
                if (table_suggestion [i,0] >0 && i < 23)
                {
                    tic_1 = table_suggestion[i, 0];
                    tic_2 = table_suggestion[i + 1, 0];
                    tic_3 = table_suggestion[i + 2, 0];
                }
                else
                {
                    tic_1 = table_suggestion[i, 0];
                    tic_2 = table_suggestion[i, 0];
                    tic_3 = table_suggestion[i, 0];
                }
                
                int shootIndex = 0;
                for (int j = 1; j < 6; j++)
                {
                    if (table_suggestion[i, j] == hero_ID_Client_Team[i + 1, 0])
                    {
                        if (hero_ID_Client_Team[i + 1, 2] <= 1)
                        {
                            banorpick = -1;
                        }
                        shootIndex = (j - 1) * banorpick;
                        checkTable[index, 0] = tic_2;
                        if ((tic_2 + (int)(tic_3 - tic_2) / 2) > 60)
                        {
                            checkTable[index, 1] = tic_2 + 60;
                        }
                        else
                        {
                            checkTable[index, 1] = tic_2 + (int)(tic_3 - tic_2) / 2;
                        }
                        checkTable[index, 2] = shootIndex;
                        index++;
                    }
                }
            }
            return checkTable;
        }






        /*
         * team_name is come from the selectionTable. So proper input is 2 and 3.
         * suggestionTable: second dim: first column : tic, second to six column is hero_id.
         */
        public int[,] suggestionTable_1(int team_name, int diff_level)
        {
            int table_count = 0;
            if (team_name == 2)
            {
                int[] pick_list = new int[5];
                int[] team_list = new int[5];
                int pick_count = 0;
                int team_count = 0;
                int[] ban_list = new int[12];
                int ban_count = 0;
                //     tip for hero_ID_Client_Team the second dimension first column is about hero_id,the second_column is about hero client id, the third_column is about team side, the fourth_column is about tic.
                //*team side(0: (ban from team 1), 1: (ban from team 2) , 2: (pick from team 1), 3: (pick from team 2)).
                for (int i = 0; i < hero_ID_Client_Team.Length / 4; i++)
                {
                    if (hero_ID_Client_Team[i, 1] != 0)
                    {
                        int tic = hero_ID_Client_Team[i, 3];
                        table_suggestion[table_count, 0] = tic;
                        if (hero_ID_Client_Team[i, 2] < 2)
                        {
                            ban_list[ban_count] = hero_ID_Client_Team[i, 0];
                            ban_count++;
                        }
                        else if (hero_ID_Client_Team[i, 2] == 2)
                        {
                            team_list[team_count] = hero_ID_Client_Team[i, 0];
                            team_count++;
                        }
                        else
                        {
                            pick_list[pick_count] = hero_ID_Client_Team[i, 0];
                            pick_count++;
                        }
                        int[] suggest_cur = logic_counter_1(pick_list, team_list, ban_list, diff_level);
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
            else
            {
                int[] pick_list = new int[5];
                int[] team_list = new int[5];
                int pick_count = 0;
                int team_count = 0;
                int[] ban_list = new int[12];
                int ban_count = 0;
                for (int i = 0; i < hero_ID_Client_Team.Length / 4; i++)
                {
                    if (hero_ID_Client_Team[i, 1] != 0)
                    {
                        int tic = hero_ID_Client_Team[i, 3];
                        table_suggestion[table_count, 0] = tic;
                        if (hero_ID_Client_Team[i, 2] < 2 )
                        {
                            ban_list[ban_count] = hero_ID_Client_Team[i, 0];
                            ban_count++;
                        }
                        else if ( hero_ID_Client_Team[i, 2] == 3)
                        {
                            team_list[team_count] = hero_ID_Client_Team[i, 0];
                            team_count++;
                        }
                        else
                        {
                            pick_list[pick_count] = hero_ID_Client_Team[i, 0];
                            pick_count++;
                        }
                        int[] suggest_cur = logic_counter_1(pick_list, team_list, ban_list, diff_level);
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



        /*
         * version_1.1.0 logic_counter 
         * Input: the hero_sequence which is heros picked by enemy team
         *        the ban_list contains the heros picked by own team and all baned heros by both team.
         * output: the best five hero we suggest to pick
         * improve place: consider the role of the heros in the own team, consider the difficulty of the heros.
         */
        public static int[] logic_counter_1(int[] hero_sequence_enemy, int[] hero_sequence_teammate, int[] ban_list,int diff_level)
        {
            Dictionary<double, int> five_picks = new Dictionary<double, int>();
            Dictionary<double, int> proper_pick = new Dictionary<double, int>();
            hero_difficulty h_diff = new hero_difficulty();
            heroGenerateTypes h_type = new heroGenerateTypes();
            int[,] h_type_table = h_type.getTypeTable();
            // At least a team needs at least two Disabler[1], 
            // better have one middle laner[10] and one offlaner[9],
            // at least two support[4] and at least two carry[0], 
            // no more than 4 support or no more 4 carry.
            // 0:[Carry], 1: [Disabler] 2: [Initiator], 3:[Jungler], 4:[Support], 5:[Durable] 
            // 6:[Nuker], 7: [Pusher], 8: [Escape], 9: [Offlane] 10: [Midlane]
            int[] roleList = new int[5];
            for(int i =0; i< hero_sequence_teammate.Length; i++)
            {
                if (h_type_table[hero_sequence_teammate[i], 0] == 1)
                {
                    roleList[0]++;
                }
                if (h_type_table[hero_sequence_teammate[i], 1] == 1)
                {
                    roleList[1]++;
                }
                if (h_type_table[hero_sequence_teammate[i], 4] == 1)
                {
                    roleList[2]++;
                }
                if (h_type_table[hero_sequence_teammate[i], 9] == 1)
                {
                    roleList[3]++;
                }
                if (h_type_table[hero_sequence_teammate[i], 10] == 1)
                {
                    roleList[4]++;
                }
            }

            proper_pick.Add(1, 91);
            proper_pick.Add(1.4, 46);
            proper_pick.Add(1.6, 87);
            proper_pick.Add(1.8, 105);
            proper_pick.Add(2, 13);
            proper_pick.Add(1.7, 15);
            proper_pick.Add(1.2, 20);
            int[] five_hero_array = new int[5];
            int length = hero_sequence_enemy.Length;
            int count = 1;
            int flag = 0;
            double lowest_fact = 100;
            double[] counterFact = new double[5];
            if (hero_sequence_enemy.Sum() == 0)
            {
                foreach (KeyValuePair<double, int> pair in proper_pick)
                {
                    if (!ban_list.Contains(pair.Value))
                    {
                        five_picks.Add(pair.Key, pair.Value);
                    }
                    if (five_picks.Count == 5)
                    {
                        break;
                    }
                }
            }
            else
            {
                while (count <= 115)
                {
                    if (hero_sequence_enemy.Contains(count) || ban_list.Contains(count) || hero_sequence_teammate.Contains(count))
                    {
                        count++;
                        continue;
                    }
                    double sum1 = 0;
                    for (int i = 0; i < length; i++)
                    {
                        sum1 = sum1 + matrix_info[hero_sequence_enemy[i], count]+ 0.2*role_factor(roleList,count)+0.2*(3- diff_level) *(14-h_diff.getFinalRating(count));
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
                            int removeKey = five_picks[lowest_fact];
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


        /*
         *  A factor will used on pick logic. the factor is based on role for a whole team. 
         *  The rule of this factor is 
         *  // At least a team needs at least two Disabler[1], 
            // better have one middle laner[10] and one offlaner[9],
            // at least two support[4] and at least two carry[0], 
            // no more than 4 support or no more 4 carry.
            // no more than 3 midlane.
            // 0:[Carry], 1: [Disabler] 2:[Support], 3: [Offlane] 4: [Midlane]
            // 0:[Carry], 1: [Disabler] 2: [Initiator], 3:[Jungler], 4:[Support], 5:[Durable] 
            // 6:[Nuker], 7: [Pusher], 8: [Escape], 9: [Offlane] 10: [Midlane]
         */
        private static int role_factor(int[] roleList, int hero_id)
        {
            int roleFactor = 0;

            int badAction = -10;
            int earnAction = 1;
            int greatAction = 50;

            // no more than 4 support or no more 4 carry.
            // no more than 3 midlane.
            if (roleList[2] >= 4 && HT_table[hero_id, 4] == 1)
            {
                roleFactor = roleFactor + badAction;
            }
            if (roleList[0] >= 4 && HT_table[hero_id, 0] == 1)
            {
                roleFactor = roleFactor + badAction;
            }
            if (roleList[4] >= 3 && HT_table[hero_id, 10] == 1)
            {
                roleFactor = roleFactor + badAction;
            }

            // at least two support[4] and at least two carry[0], 
            if (HT_table[hero_id, 4] == 1 && roleList[2] < 2)
            {
                roleFactor = roleFactor + greatAction;
            }
            if (HT_table[hero_id, 0] == 1 && roleList[0] < 2)
            {
                roleFactor = roleFactor + greatAction;
            }

            // better have one middle laner[10] and one offlaner[9],
            if (HT_table[hero_id, 9] == 1 && roleList[3] == 0)
            {
                roleFactor = roleFactor + greatAction;
            }
            if (HT_table[hero_id, 10] == 1 && roleList[4] == 0)
            {
                roleFactor = roleFactor + greatAction;
            }

            // At least a team needs at least two Disabler[1], 
            if (HT_table[hero_id, 1] == 1 && roleList[1] < 2)
            {
                roleFactor = roleFactor + greatAction;
            }

            if (HT_table[hero_id, 1] == 1)
            {
                roleFactor = roleFactor + earnAction;
            }
            if (HT_table[hero_id, 5] == 1)
            {
                roleFactor = roleFactor + earnAction;
            }
            if (HT_table[hero_id, 6] == 1)
            {
                roleFactor = roleFactor + earnAction;
            }
            if (HT_table[hero_id, 7] == 1)
            {
                roleFactor = roleFactor + earnAction;
            }
            if (HT_table[hero_id, 8] == 1)
            {
                roleFactor = roleFactor + earnAction;
            }



            return roleFactor;
        }


    }
}
