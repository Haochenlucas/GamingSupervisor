using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GamingSupervisor
{
    class DotaConsoleParser
    {
        private FileStream consoleFileStream;

        private string consoleLogPath;

        private readonly object heroCountLock = new object();
        private int heroCount;
        public int HeroCount
        {
            get { lock (heroCountLock) { return heroCount; } }
            private set { lock (heroCountLock) { heroCount = value; } }
        }

        private readonly object heroesSelectedLock = new object();
        private string[] heroesSelected;
        public string[] HeroesSelected
        {
            get { lock (heroesSelectedLock) { return heroesSelected; } }
            private set { lock (heroesSelectedLock) { heroesSelected = value; } }
        }

        public DotaConsoleParser()
        {
            consoleLogPath = Path.Combine(SteamAppsLocation.Get(), "console.log");
            consoleFileStream = File.Open(consoleLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            heroesSelected = new string[] { "null", "null", "null", "null", "null", "null", "null", "null", "null", "null" };

            heroCount = 0;
        }

        public void ReportHeroSelection()
        {
            new Thread(WatchHeroSelection).Start();
        }

        private void WatchHeroSelection()
        {
            consoleFileStream.Seek(0, SeekOrigin.End);
            using (StreamReader streamReader = new StreamReader(consoleFileStream))
            {
                string readSoFar = "";
                while (HeroCount < 10)
                {
                    Thread.Sleep(500);
                    readSoFar += streamReader.ReadToEnd();

                    var matches = Regex.Matches(readSoFar, @"PR:SetSelectedHero (?<ID>\d):\[I:0:0\] npc_dota_hero_(?<Name>.*?)\(");
                    if (matches.Count == 0)
                        continue;

                    foreach (Match match in matches)
                    {
                        int playerID = Int32.Parse(match.Groups["ID"].Value);
                        string heroName = match.Groups["Name"].Value;

                        if (HeroesSelected[playerID] == "null")
                            HeroCount++;
                        HeroesSelected[playerID] = heroName;
                    }

                    readSoFar = readSoFar.Substring(readSoFar.LastIndexOf("SetSelectedHero"));
                }
            }
        }
    }
}
