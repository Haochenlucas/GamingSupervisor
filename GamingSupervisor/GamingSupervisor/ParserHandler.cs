using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GamingSupervisor
{
    class ParserHandler
    {
        private string fileName;

        public ParserHandler(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException("File name cannot be null");
            }
            this.fileName = fileName;
        }

        public List<string> ParseReplayFile()
        {
            Console.WriteLine("Starting parsing..." + fileName);
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "javaw";
            p.StartInfo.Arguments =
                "-jar "
                + Path.Combine(Environment.CurrentDirectory, @"..\..\Parser\parser.jar ")
                + "\"" + fileName.Replace(@"\", @"\\") + "\""
                + " "
                + Path.Combine(Environment.CurrentDirectory, @"..\..\Parser\"); // Data dump location
            p.Start();

            while (!p.HasExited)
            {
                Console.WriteLine(p.StandardOutput.ReadLine());
            }

            p.WaitForExit();
            Console.WriteLine("Finished parsing!");

            List<string> heroNameList = new List<string>();

            string[] infoFile = File.ReadAllLines(System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\Parser\info.txt"));
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
