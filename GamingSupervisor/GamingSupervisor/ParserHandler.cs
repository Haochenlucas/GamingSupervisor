using ICSharpCode.SharpZipLib.BZip2;
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
            if (Path.GetExtension(fileName) == ".bz2")
            {
                DecompressFile();
            }

            Console.WriteLine("Starting parsing...");
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "java";
            p.StartInfo.Arguments =
                "-jar "
                + Path.Combine(Environment.CurrentDirectory, @"..\..\Parser\parser.jar ")
                + fileName.Replace(@"\", @"\\")
                + " "
                + Path.Combine(Environment.CurrentDirectory, @"..\..\Parser\"); // Data dump location
            p.Start();

            while (!p.HasExited)
            {
                Console.WriteLine(p.StandardOutput.ReadLine());
            }

            p.WaitForExit();
            Console.WriteLine("Finished parsing!");
        }

        private void DecompressFile()
        {
            Console.WriteLine("Starting decompressing...");
            FileInfo zipFile = new FileInfo(fileName);
            using (FileStream fileToDecompressAsStream = zipFile.OpenRead())
            {
                fileName = Path.Combine(Environment.CurrentDirectory, @"..\..\Parser\" + Path.GetFileNameWithoutExtension(fileName));
                using (FileStream decompressedStream = File.Create(fileName))
                {
                    try
                    {
                        BZip2.Decompress(fileToDecompressAsStream, decompressedStream, true);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            Console.WriteLine("Finished decompressing!");
        }
    }
}
