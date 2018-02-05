using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GamingSupervisor
{
    class ParserHandler
    {
        public ParserHandler()
        {
        }

        public List<string> ParseReplayFile()
        {
            if (!Directory.Exists(GUISelection.replayDataFolderLocation))
            {
                Directory.CreateDirectory(GUISelection.replayDataFolderLocation);

                Console.WriteLine("Starting parsing..." + GUISelection.fileName);
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = "javaw";
                p.StartInfo.Arguments =
                    "-jar "
                    + Path.Combine(Environment.CurrentDirectory, @"..\..\Parser\parser.jar ")
                    + "\"" + GUISelection.fileName.Replace(@"\", @"\\") + "\""
                    + " "
                    + GUISelection.replayDataFolderLocation; // Data dump location
                p.Start();

                while (!p.HasExited)
                {
                    Console.WriteLine(p.StandardOutput.ReadLine());
                }

                p.WaitForExit();
                Console.WriteLine("Finished parsing!");
            }

            List<string> heroNameList = new List<string>();

            string[] infoFile = File.ReadAllLines(GUISelection.replayDataFolderLocation + "info.txt");
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
                    parsedHeroName = string.Join(" ", upperCase);

                    heroNameList.Add(parsedHeroName);
                }
            }

            return heroNameList;
        }
    }
}
