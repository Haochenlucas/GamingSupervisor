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
    public struct Player
    {
        public int Level { get; set; }
        public string HeroName { get; set; }
        public string PlayerName { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public int LastHits { get; set; }
        public int Denies { get; set; }
        public int Gold { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public int Mana { get; set; }
        public int MaxMana { get; set; }
    }

    class DotaConsoleParser
    {
        private FileStream consoleFileStream;
        private string consoleLogPath;

        private Thread heroSelectionThread;
        private Thread heroDataThread;

        private bool stopHeroSelection;
        private bool stopHeroData;


        public DotaConsoleParser()
        {
            consoleLogPath = Path.Combine(SteamAppsLocation.Get(), "console.log");
            consoleFileStream = File.Open(consoleLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            heroesSelected = new string[] { "null", "null", "null", "null", "null", "null", "null", "null", "null", "null" };
            heroes = new Player[10];
            for (int i = 0; i < 10; i++)
            {
                heroes[i].Level = 0;
                heroes[i].HeroName = "";
                heroes[i].PlayerName = "";
                heroes[i].Kills = 0;
                heroes[i].Deaths = 0;
                heroes[i].Assists = 0;
                heroes[i].LastHits = 0;
                heroes[i].Denies = 0;
                heroes[i].Gold = 0;
                heroes[i].Health = 0;
                heroes[i].MaxHealth = 0;
                heroes[i].Mana = 0;
                heroes[i].MaxMana = 0;
            }

            heroCount = 0;

            stopHeroSelection = false;
            stopHeroData = false;
        }

        public void StartHeroDataParsing()
        {
            heroDataThread = new Thread(ReportHeroData);
            heroDataThread.Start();
        }

        public void StopHeroDataParsing()
        {
            stopHeroData = true;
            heroDataThread.Join();
            stopHeroData = false;
        }

        public void ReportHeroData()
        {
            consoleFileStream = File.Open(consoleLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            consoleFileStream.Seek(0, SeekOrigin.End);
            using (StreamReader streamReader = new StreamReader(consoleFileStream))
            {
                string readSoFar = "";
                while (true || !stopHeroData)
                {
                    Thread.Sleep(250);
                    readSoFar += streamReader.ReadToEnd();

                    if (!readSoFar.Contains("Lv Name         Player        K/ D/ A/ LH/ DN/ Gold Health    Mana"))
                        continue;

                    var matches = Regex.Matches(readSoFar,
                        @"(?<Level>\d+)\s+" +
                        @"(?<Hero>.{1,12}?)\s+" +
                        @"(?<Player>.{1,13}?)\s*" +
                        @"(?<Kills>\d+)\/\s*" +
                        @"(?<Deaths>\d+)\/\s*" +
                        @"(?<Assists>\d+)\/\s*" +
                        @"(?<LastHits>\d+)\/\s*" +
                        @"(?<Denies>\d+)\/\s*" +
                        @"(?<Gold>\d+)\s+" +
                        @"(?<Health>\d+)\/\s*" +
                        @"(?<MaxHealth>\d+)\s+" +
                        @"(?<Mana>\d+)\/\s*" +
                        @"(?<MaxMana>\d+)(\n|\r|\r\n)");

                    if (matches.Count < 10)
                        continue;

                    int count = 0;
                    foreach (Match match in matches)
                    {
                        heroes[count].Level = Int32.Parse(match.Groups["Level"].Value);
                        heroes[count].HeroName = match.Groups["Hero"].Value;
                        heroes[count].PlayerName = match.Groups["Player"].Value;
                        heroes[count].Kills = Int32.Parse(match.Groups["Kills"].Value);
                        heroes[count].Deaths = Int32.Parse(match.Groups["Deaths"].Value);
                        heroes[count].Assists = Int32.Parse(match.Groups["Assists"].Value);
                        heroes[count].LastHits = Int32.Parse(match.Groups["LastHits"].Value);
                        heroes[count].Denies = Int32.Parse(match.Groups["Denies"].Value);
                        heroes[count].Gold = Int32.Parse(match.Groups["Gold"].Value);
                        heroes[count].Health = Int32.Parse(match.Groups["Health"].Value);
                        heroes[count].MaxHealth = Int32.Parse(match.Groups["MaxHealth"].Value);
                        heroes[count].Mana = Int32.Parse(match.Groups["Mana"].Value);
                        heroes[count].MaxMana = Int32.Parse(match.Groups["MaxMana"].Value);

                        count++;

                        if (count >= 10)
                            break;
                    }

                    readSoFar = "";
                }

            }
        }

        public void StartHeroSelectionParsing()
        {
            heroSelectionThread = new Thread(ReportHeroSelection);
            heroSelectionThread.Start();
        }

        public void StopHeroSelectionParsing()
        {
            stopHeroSelection = true;
            if (heroSelectionThread != null)
                heroSelectionThread.Join();
            stopHeroSelection = false;
        }

        private void ReportHeroSelection()
        {
            consoleFileStream.Seek(0, SeekOrigin.End);
            using (StreamReader streamReader = new StreamReader(consoleFileStream))
            {
                string readSoFar = "";
                while (HeroCount < 10 && !stopHeroSelection)
                {
                    Thread.Sleep(500);
                    try
                    {
                        readSoFar += streamReader.ReadToEnd();
                    }
                    catch { }

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

        private readonly object heroesLock = new object();
        private Player[] heroes;
        public Player[] Heroes
        {
            get { lock (heroesSelectedLock) { return heroes; } }
            private set { lock (heroesSelectedLock) { heroes = value; } }
        }
    }
}
