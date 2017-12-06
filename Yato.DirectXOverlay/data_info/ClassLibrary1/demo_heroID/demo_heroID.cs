using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using replayParse;
using System.Threading.Tasks;

namespace demo_heroID
{
    class demo_heroID
    {
        static void Main(string[] args)
        {
            heroID h = new heroID();
            Dictionary<int, string> ID_table = h.getHeroID(); // key is ID, value is hero_name;
            Dictionary<string, int> hero_table = h.getIDHero(); // key is hero_name, value is ID;
            string[] heroName = h.getHeroName();
            //foreach (KeyValuePair<int, string> kvp in ID_table)
            //{
            //    Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            //}

            //foreach (KeyValuePair<string, int> kvp in hero_table)
            //{
            //    Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            //}
            foreach (string s in heroName)
            {
                Console.WriteLine(s);
            }
            Console.Read();
        }
    }
}
