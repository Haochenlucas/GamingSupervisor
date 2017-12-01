using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GamingSupervisor
{
    class Program
    {
        static private void ProcessLine(string line)
        {

        }

        static private void ReadDotaLog(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    while (true)
                    {
                        while (!sr.EndOfStream)
                        {
                            ProcessLine(sr.ReadLine());
                        }
                        while (sr.EndOfStream)
                        {
                            Thread.Sleep(10);
                        }
                        ProcessLine(sr.ReadLine());
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            ReadDotaLog("");
        }
    }
}
