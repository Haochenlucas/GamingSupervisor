using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace replayParse
{
    public class ReplayHeroID
    {
        private Dictionary<string, int> heroNameToID = new Dictionary<string, int>();
        private Dictionary<int, string> heroIDToName = new Dictionary<int, string>();

        public ReplayHeroID(string dataFolderLocation)
        {
            string[] lines = File.ReadAllLines(dataFolderLocation + "heroId.txt");
            foreach (string line in lines)
            {
                string[] words = line.Split(' ');
                string[] substrings = Regex.Split(words[1], "Hero_");
                var r = new Regex(@"(?<=[A-Z])(?=[A-Z][a-z]) | (?<=[^A-Z])(?=[A-Z]) | (?<=[A-Za-z])(?=[^A-Za-z])",
                    RegexOptions.IgnorePatternWhitespace);
                string name = r.Replace(substrings[1], "");
                name = string.Join("", name.Split(new string[] { "_" }, StringSplitOptions.None));
                name = name.ToLower();
                if (name.Contains("never"))
                {
                    name = "shadowfiend";
                }
                if (name.Contains("obsidian"))
                {
                    name = "outworlddevourer";
                }
                if (name.Contains("wisp"))
                {
                    name = "io";
                }
                if (name.Contains("magnataur"))
                {
                    name = "magnus";
                }
                if (name.Contains("treant"))
                {
                    name = "treantprotector";
                }
                if (name.Contains("skele"))
                {
                    name = "wraithking";
                }
                int heroID = Int32.Parse(words[0]);
                heroNameToID[name] = heroID;
                heroIDToName[heroID] = name;
            }
        }

        public int getHeroID(string heroName)
        {
            heroName = heroName.Replace(" ", "");
            heroName = heroName.Replace("_", "");
            heroName = heroName.ToLower();

            return heroNameToID[heroName];
        }

        public string getHeroName(int heroID)
        {
            return heroIDToName[heroID];
        }
    }
}
