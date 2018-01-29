using replayParse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamingSupervisor
{

    class demo{
        static void Main()
        {
            string path = @"X:\data_info\replay.txt";
            //replay_version01 r = new replay_version01();
            //Dictionary<string, int> h = r.getHeros();
            //int[,,] info = r.getReplayInfo();
            //heroID h_ID = new heroID();
            //Dictionary<int, string> ID_table = h_ID.getHeroID(); // key is ID, value is hero_name;
            //Dictionary<string, int> hero_table = h_ID.getIDHero(); // key is hero_name, value is ID;
            //string[] heroName = h_ID.getHeroName();
            //foreach (KeyValuePair<int, string> kvp in ID_table)
            //{
            //    Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            //}

            //foreach (KeyValuePair<string, int> kvp in hero_table)
            //{
            //    Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            //}
            //foreach (string s in heroName)
            //{
            //    Console.WriteLine(s);
            //}

            counter_pick_logic cp = new counter_pick_logic();
            cp.readTeam();
            int[,] table = cp.selectTable();
            int[] hero_pick = { 1, 2, 3, 7, 115 };
            int[] ban = { 10, 20, 30, 70, 114 };
            int[,] suggestiontable = cp.suggestionTable(2);
            Console.Read();
        }
    }
}