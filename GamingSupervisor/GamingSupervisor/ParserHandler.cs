﻿using replayParse;
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
                    + "\"" + Path.Combine(Environment.CurrentDirectory, @"..\..\Parser\parser.jar").Replace(@"\", @"\\") + "\""
                    + " "
                    + "\"" + GUISelection.fileName.Replace(@"\", @"\\") + "\""
                    + " "
                    + "\"" + GUISelection.replayDataFolderLocation.Replace(@"\", @"\\") + "\""; // Data dump location
                p.Start();

                while (!p.HasExited)
                {
                    Console.WriteLine(p.StandardOutput.ReadLine());
                }

                p.WaitForExit();
                Console.WriteLine("Finished parsing!");
            }

            List<string> heroNameList = new List<string>();
            heroID h_ID = new heroID();
            Dictionary<int, string> ID_table = h_ID.getHeroID(); // key is ID, value is hero_name;
            Dictionary<string, int> hero_table = h_ID.getIDfromLowercaseHeroname(); // key is hero_name, value is ID;
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
