using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace replayParse
{
    public class hero_intro
    {
        public static Dictionary<int, string> hero_IntroDictionary = new Dictionary<int, string>();
        public static Dictionary<string, int> Intro_heroDictionary = new Dictionary<string, int>();

        public hero_intro()
        {
            string s = Path.Combine(Environment.CurrentDirectory, "../../Properties/hero_intro.txt");
            string[] lines = System.IO.File.ReadAllLines(s);
            string[] second_lines = lines;
            int length_name = 0;
            foreach (string line in lines)
            {
                length_name = 0;
                string second_string = string.Empty;
                string[] words = line.Split('*');
                length_name = words.Length -1;
                string hero_intro = "";
                if (length_name == 1)
                {
                    hero_intro = hero_intro + words[1];
                }
                else
                {
                    for (int i = 1; i < length_name; i++)
                    {
                        if (i == 1)
                            hero_intro = words[1];
                        else
                        {
                            string[] tempLeft = words[i - 1].Split(' ');
                            string[] tempRight = words[i].Split(' ');
                            int index = 0;
                            int boundary = 4;
                            int direction = 0;
                            if (tempRight.Length<5)
                            {
                                boundary = tempRight.Length-1;
                            }
                            for (int m = boundary; m >= 0; m--)
                            {
                                if (tempLeft[tempLeft.Length-1].Contains(tempRight[m]))
                                {
                                    index = m;
                                    direction = 0;
                                }
                                if (tempRight[m].Contains(tempLeft[tempLeft.Length - 1]))
                                {
                                    index = m;
                                    direction = 1;
                                }
                            }
                            if (direction == 1 )
                            {
                                string appendLeft = tempRight[index + 1];
                                for (int j = index + 1; j < tempRight.Length; j++)
                                {
                                    appendLeft = appendLeft + " " + tempRight[j];
                                }
                                hero_intro = hero_intro + " " + appendLeft;
                            }
                            else
                            {
                                string[] herowholeLeft = hero_intro.Split(' ');
                                hero_intro = herowholeLeft[0];
                                for (int j = 1; j < herowholeLeft.Length-index; j++)
                                {
                                    hero_intro = hero_intro + " " + herowholeLeft[j];
                                }
                                hero_intro = hero_intro + " " + words[i];
                            }
                            
                        }
                    }
                }  
                if (hero_IntroDictionary.Count == 115)
                    break;
                int key = int.Parse(words[0]);
                hero_IntroDictionary.Add(key, hero_intro);
                Intro_heroDictionary.Add(hero_intro, key);
            }
        }

        public Dictionary<int, string> getHeroIntro()
        {
            return hero_IntroDictionary;
        }
        public Dictionary<string, int> getIntroHero()
        {
            return Intro_heroDictionary;
        }
    }
}
