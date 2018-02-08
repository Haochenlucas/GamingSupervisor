using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamingSupervisor
{
    class ReplayTick
    {
        private Dictionary<int, int> gameTimeToTick;

        public ReplayTick(string dataFolderLocation)
        {
            gameTimeToTick = new Dictionary<int, int>();
            
            foreach (string line in File.ReadLines(dataFolderLocation + "time.txt").Reverse())
            {
                string[] entries = line.Split(' ');
                gameTimeToTick[(int)Convert.ToDouble(entries[2])] = Convert.ToInt32(entries[0]);
            }
        }

        public int this[int i]
        {
            get => gameTimeToTick[i];
        }
    }
}
