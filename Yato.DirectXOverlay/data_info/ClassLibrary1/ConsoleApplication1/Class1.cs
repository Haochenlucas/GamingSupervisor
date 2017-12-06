using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary1
{
    class Class1
    {
        static void Main()
        {
            replay_version01 r = new replay_version01();
            Dictionary<string, int> h = r.getHeros();
            int[,,] info = r.getReplayInfo();
            Console.ReadKey();
        }
    }
}
