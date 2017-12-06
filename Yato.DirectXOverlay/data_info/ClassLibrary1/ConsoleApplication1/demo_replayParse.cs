using replayParse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace demo_replayParse
{
    class demo_replayParse
    {
        static void Main()
        {
            string path = @"X:\data_info\replay.txt";
            replay_version01 r = new replay_version01(path);
            Dictionary<string, int> h = r.getHeros();
            int[,,] info = r.getReplayInfo();
            Console.ReadKey();
        }
    }
}
