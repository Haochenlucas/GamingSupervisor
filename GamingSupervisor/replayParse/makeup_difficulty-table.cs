using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace replayParse
{
    public class makeup_difficulty_talbe
    {
        public makeup_difficulty_talbe()
        {
            string s = Path.Combine(Environment.CurrentDirectory, @"..\..\..\replayParse\Properties\hero_difficulty_version_0.txt");
            string[] lines = System.IO.File.ReadAllLines(s);
            string[] second_lines = lines;
            int key = 0;
            foreach (string line in lines)
            {
                string second_string = string.Empty;
                string[] words = line.Split('\t');
                string hero_line = "";
                for (int i = 2; i < words.Length; i++)
                {
                    if (i == 2)
                        hero_line = words[2];
                    else
                        hero_line = hero_line + " " + words[i];
                }
                second_string = (key + 1) + " " + hero_line;
                second_lines[key] = second_string;
                key++;

            }
            string path = Path.Combine(Environment.CurrentDirectory, @"..\..\..\replayParse\Properties\hero_difficulty_version_1.txt");
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
    }
}
