using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace replayParse
{
    public class heroGenerateTypes
    {
        // 0:[Carry], 1: [Disabler] 2: [Initiator], 3:[Jungler], 4:[Support], 5:[Durable] 
        // 6:[Nuker], 7: [Pusher], 8: [Escape], 9: [Offlane] 10: [Midlane]
        private int[,] hero_generate_type_table = new int[116, 11];
        private Dictionary<string, string> type_explain = new Dictionary<string, string>();
        /*
         * 
         */
        public heroGenerateTypes()
        {
            string s = Path.Combine(Environment.CurrentDirectory, @"..\..\..\replayParse\Properties\herotype.txt");
            string[] lines = System.IO.File.ReadAllLines(s);
            string[] second_lines = lines;
            int index = 0;
            heroID h_ID = new heroID();
            string[] heroesName = h_ID.getHeroName();
            Dictionary<int, string> ID_table = h_ID.getHeroID(); // key is ID, value is hero_name;
            Dictionary<string, int> hero_table = h_ID.getIDHero(); // key is hero_name, value is ID;
            foreach (string line in lines)
            {
                if (line.Contains("["))
                {
                    string[] words = line.Split('\t', '"', '\\');
                    string role_name = words[0].Replace("[", "");
                    role_name = role_name.Replace("]", "");
                    string explain = words[1].Replace("\"", "");
                    type_explain[role_name] = explain;
                    index = type_explain.Count - 1;
                }
                else
                {
                    string second_string = string.Empty;
                    string[] words = line.Split('*');
                    for (int i = 0; i < words.Length; i++)
                    {

                        var r = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z]) | (?<=[^A-Z])(?=[A-Z]) | (?<=[A-Za-z])(?=[^A-Za-z])",
                                            RegexOptions.IgnorePatternWhitespace);

                        string curName = r.Replace(words[i], " ");
                        string[] nameList = curName.Split(' ');
                        curName = "";
                        for (int j = nameList.Length - 1; j >= 0; j--)
                        {
                            if (curName.Length > 0)
                            {
                                if (nameList[j].Length > 0)
                                {
                                    curName = nameList[j] + " " + curName;
                                }
                            }
                            else
                            {
                                curName = nameList[j];
                            }

                            if (hero_table.ContainsKey(curName))
                            {
                                hero_generate_type_table[hero_table[curName], index] = 1;
                                break;
                            }
                        }
                    }
                }
            }
        }


        /*
         * return the table of the types for all heroes.
         * The information in table: 0:[Carry], 1: [Disabler] 2: [Initiator], 3:[Jungler], 4:[Support], 5:[Durable]  6:[Nuker], 7: [Pusher], 8: [Escape], 9: [Offlane] 10: [Midlane]
         */
        public int[,] getTypeTable()
        {
            return hero_generate_type_table;
        }

        /*
         * return a dictionary that contains all types of role and explainations.
         */
        public Dictionary<string, string> getTypeDic()
        {
            return type_explain;
        }

    }
}
