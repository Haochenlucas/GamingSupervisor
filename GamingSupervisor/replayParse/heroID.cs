using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace replayParse
{
    public class heroID
    {
        public static Dictionary<int, string> hero_IDDictionary = new Dictionary<int, string>();
        public static Dictionary<string, int> ID_heroDictionary = new Dictionary<string, int>();
        public static string[] heroName = new string[116]; // make the index be the ID value, so the first string is empty
        public heroID()
        {
            string s = Path.Combine(Environment.CurrentDirectory, @"..\..\..\replayParse\Properties\dota_hero_info_1.txt");
            string[] lines = System.IO.File.ReadAllLines(s);
            string[] second_lines = lines;
            int key = 0;
            int length_name = 0;
            foreach (string line in lines)
            {
                length_name = 0;
                string second_string = string.Empty;
                string[] words = line.Split('\t');
                string[] name = words[0].Split(' ');
                length_name = name.Length / 2;
                string hero_name = "";
                for (int i = 0; i < length_name; i++)
                {
                    if (i == 0)
                        hero_name = name[0];
                    else
                        hero_name = hero_name + " " + name[i];
                }
                second_string = (key + 1) + "     " + hero_name;
                second_lines[key] = second_string;
                key++;
                if (hero_IDDictionary.Count == 115)
                    break;
                hero_IDDictionary.Add(key, hero_name);
                ID_heroDictionary.Add(hero_name, key);
                heroName[key] = hero_name;

            }
            string path = Path.Combine(Environment.CurrentDirectory, @"..\..\..\replayParse\Properties\heroIDtable.txt");
            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    foreach (string line in second_lines)
                    {
                        sw.WriteLine(line);
                    }

                }
            }
        }

        public heroID(string filePath)
        {
            string[] lines = System.IO.File.ReadAllLines(filePath);
            string[] second_lines = lines;
            // Display the file contents by using a foreach loop.
            System.Console.WriteLine("Contents of WriteLines2.txt = ");
            int key = 0;
            int length_name = 0;
            foreach (string line in lines)
            {
                length_name = 0;
                string second_string = string.Empty;
                string[] words = line.Split('\t');
                string[] name = words[0].Split(' ');
                length_name = name.Length / 2;
                string hero_name = "";
                for (int i = 0; i < length_name; i++)
                {
                    if (i == 0)
                        hero_name = name[0];
                    else
                        hero_name = hero_name + " " + name[i];
                }
                second_string = (key + 1) + "     " + hero_name;
                second_lines[key] = second_string;
                key++;
                hero_IDDictionary.Add(key, words[0]);
                ID_heroDictionary.Add(words[0], key);
            }
            string path = Path.Combine(Environment.CurrentDirectory, @"..\..\Properties\heroIDtable.txt");
            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    foreach (string line in second_lines)
                    {
                        sw.WriteLine(line);
                    }

                }
            }
        }

        public Dictionary<int, string> getHeroID()
        {
            return hero_IDDictionary;
        }
        public Dictionary<string, int> getIDHero()
        {
            return ID_heroDictionary;
        }

        public string[] getHeroName()
        {
            return heroName;
        }
    }
}