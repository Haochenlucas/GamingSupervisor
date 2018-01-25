using System;
using System.Diagnostics;
using System.IO;

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

        public void ParseReplayFile()
        {
            Console.WriteLine("Starting parsing..." + fileName);
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "java";
            p.StartInfo.Arguments =
                "-jar "
                + Path.Combine(Environment.CurrentDirectory, @"..\..\..\Parser\parser.jar ")
                + "\"" + fileName.Replace(@"\", @"\\") + "\""
                + " "
                + Path.Combine(Environment.CurrentDirectory, @"..\..\..\Parser\"); // Data dump location
            p.Start();

            while (!p.HasExited)
            {
                Console.WriteLine(p.StandardOutput.ReadLine());
            }

            p.WaitForExit();
            Console.WriteLine("Finished parsing!");
        }
    }
}
