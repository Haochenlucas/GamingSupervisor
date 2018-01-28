using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace replayParse
{
    class heroIDClient
    {
        public static Dictionary<int, string> hero_IDClientDictionary = new Dictionary<int, string>();
        public static Dictionary<string, int> ID_heroClientDictionary = new Dictionary<string, int>();
        public static string[] heroName = new string[116]; // make the index be the ID value, so the first string is empty
        public heroIDClient()
        {
            string s = Path.Combine(Environment.CurrentDirectory, @"..\..\Properties\heroIDtable1.txt");
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
                length_name = name.Length;
                string hero_name = "";
                for (int i = 1; i < length_name; i++)
                {
                    if (name[i] != "")
                    {
                        if (hero_name == "")
                        {
                            hero_name = name[i];
                        }
                        else
                        {
                            hero_name = hero_name + " " + name[i];
                        }
                    }   
                }
                key= Convert.ToInt32(name[0]);
                second_string = (key + 1) + "     " + hero_name;
                second_lines[key] = second_string;
                hero_IDClientDictionary.Add(key, hero_name);
                ID_heroClientDictionary.Add(hero_name, key);
                heroName[key] = hero_name;

            }
            string path = Path.Combine(Environment.CurrentDirectory, @"..\..\Properties\heroIDtable1.txt");
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

        public heroIDClient(string filePath)
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
                length_name = name.Length;
                string hero_name = "";
                for (int i = 1; i < length_name; i++)
                {
                    if (name[i] != "")
                    {
                        if (hero_name == "")
                        {
                            hero_name = name[i];
                        }
                        else
                        {
                            hero_name = hero_name + " " + name[i];
                        }
                    }
                }
                key = Convert.ToInt32(name[0]);
                second_string = (key + 1) + "     " + hero_name;
                second_lines[key] = second_string;
                hero_IDClientDictionary.Add(key, hero_name);
                ID_heroClientDictionary.Add(hero_name, key);
                heroName[key] = hero_name;
            }
            string path = Path.Combine(Environment.CurrentDirectory, @"..\..\Properties\heroIDtable1.txt");
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
            return hero_IDClientDictionary;
        }
        public Dictionary<string, int> getIDHero()
        {
            return ID_heroClientDictionary;
        }

        public string[] getHeroName()
        {
            return heroName;
        }
    }
}
