using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClassLibrary1
{


    public class replay_version01
    { 
        public static int[,,] replayinfo = new int[200000, 10, 3];
        private int[,] prev_stat = new int[10, 3];
        public static Dictionary< string, int> heros = new Dictionary<string, int>();
        public replay_version01() {
            
            string[] lines = System.IO.File.ReadAllLines(@"C:\Users\dominate\Documents\replay.txt");
            int tic = 0;
            int value = 0;
            foreach (string line in lines)
            {
                string[] words = line.Split(' ');
                int time = Int32.Parse(words[0]);
                int mode = 0;
                int heroID = 0;
                if (words[1].Contains('P'))
                {
                    mode = 1;
                }
                string[] substrings = Regex.Split(words[2], "Hero_");
                if (!heros.Keys.Contains(substrings[1]))
                {
                    heros.Add(substrings[1], value);
                    heroID = heros[substrings[1]];
                    value++;
                }
                else
                {
                    heroID = heros[substrings[1]];
                }
                if (mode == 1)
                {
                    replayinfo[time, heroID, 1] = Int32.Parse(words[3]);
                    replayinfo[time, heroID, 2] = Int32.Parse(words[4]);
                    prev_stat[heroID, 1] = Int32.Parse(words[3]);
                    prev_stat[heroID, 2] = Int32.Parse(words[4]);
                }
                else
                {
                    replayinfo[time, heroID, 0] = Int32.Parse(words[3]);
                    prev_stat[heroID, 0] = Int32.Parse(words[3]);
                }

                if (tic == 0)
                    tic = time;

                while (time > tic)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (prev_stat[i, j] != 0)
                            {
                                replayinfo[tic, i, j] = prev_stat[i, j];
                            }
                        }
                    }
                    tic++;
                }
                tic = time;
            }
        }

        public Dictionary<string, int> getHeros()
        {
            return heros;
        }

        public int[,,] getReplayInfo()
        {
            return replayinfo;
        }

        
    }
}
