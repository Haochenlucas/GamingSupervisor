using replayParse;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace GamingSupervisor
{
    static class ParserHandler
    {
        private static Thread infoThread;
        private static Thread fullThread;
        private static bool infoParsingComplete;

        public static void StartInfoParsing()
        {
            infoThread = new Thread(() => ParseInfoReplayFiles());
            infoThread.Start();
        }

        public static void StartFullParsing(string fileName)
        {
            fullThread = new Thread(() => ParseFullReplayFile(fileName));
            fullThread.Start();
        }

        private static void ParseInfoReplayFiles()
        {
            string replayDirectory = Path.Combine(SteamAppsLocation.Get(), "replays");

            string[] fileEntries = Directory.GetFiles(replayDirectory);

            foreach (string file in fileEntries)
            {
                if (Path.GetExtension(file) != ".dem")
                    continue;

                ParseReplay(Path.Combine(replayDirectory, file), "info");
            }
        }

        private static void ParseFullReplayFile(string fileName)
        {
            string replayDirectory = Path.Combine(SteamAppsLocation.Get(), "replays");

            ParseReplay(Path.Combine(replayDirectory, fileName), "full");
        }

        private static void ParseReplay(string replayLocation, string arg)
        {
            string directoryPath = Path.Combine(Environment.CurrentDirectory,
                "../../Parser/",
                Path.GetFileNameWithoutExtension(replayLocation));
            string fileName = Path.GetFileName(replayLocation);

            if (Directory.Exists(directoryPath) && arg == "info")
                return;
            else if (File.Exists(Path.Combine(directoryPath, "hero.txt")))
                return;

            Directory.CreateDirectory(directoryPath);

            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "javaw";
            p.StartInfo.Arguments =
                "-jar "
                + '"' + Path.Combine(Environment.CurrentDirectory, "../../Parser/parser.jar") + '"'
                + " "
                + '"' + replayLocation.Replace(@"\", "/") + '"' // Replay .dem location
                + " "
                + '"' + directoryPath.Replace(@"\", "/") + '"' // Data dump location
                + " "
                + '"' + arg + '"'; // "info" or "full" parsing
            p.Start();

            if (arg == "info")
                p.PriorityClass = ProcessPriorityClass.Idle;
            else if (arg == "full")
                p.PriorityClass = ProcessPriorityClass.High;

            while (!p.HasExited)
            {
                Console.WriteLine(p.StandardOutput.ReadLine());
            }

            p.WaitForExit();
        }

        public static void WaitForFullParsing()
        {
            fullThread.Join();
        }

        public static void WaitForInfoParsing()
        {
            infoThread.Join();
        }

        public static List<string> GetHeroNameList(string directory)
        {
            List<string> heroNameList = new List<string>();
            heroID h_ID = new heroID();
            Dictionary<int, string> ID_table = h_ID.getHeroID(); // key is ID, value is hero_name;
            Dictionary<string, int> hero_table = h_ID.getIDfromLowercaseHeroname(); // key is hero_name, value is ID;
            string[] infoFile = File.ReadAllLines(directory + "info.txt");
            foreach (string line in infoFile)
            {
                if (line.Contains("hero_name"))
                {
                    String parsedHeroName = line.Split(new string[] { "hero_" }, StringSplitOptions.None).Last();
                    parsedHeroName = parsedHeroName.Split(new char[] { '\"' }).First();
                    String[] temp = parsedHeroName.Split(new char[] { '_' });
                    String[] upperCase = new String[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                    {
                        upperCase[i] = temp[i].First().ToString().ToUpper() + temp[i].Substring(1);
                    }
                    parsedHeroName = string.Join("", upperCase);
                    string name = parsedHeroName.ToLower();
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
                    if (name.Contains("Rattletrap"))
                    {
                        name = "Clockwerk";
                    }
                    int key = hero_table[name];

                    heroNameList.Add(ID_table[key]);
                }
            }

            return heroNameList;
        }
    }
}
