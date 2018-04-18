using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace replayParse
{
    class heroAttribute
    {
        // table: 0,name, 1, primary attribute, 2 base strength,  3 strength growth, 4 total strength, 5 base agility, 6 agility growth, 7 total agility,
        // 8 base intelligence, 9 intelligence growth, 10 total intelligence, 11 total start attributes, 12 total attributes growth, 13 total attribute 25 level, 14 base movement speed, 
        // 15 starting armor, 16 starting attack damage(min), 17 starting attack damage(max), 18 attack range, 19 base attack time, 20 attack point, 21 attack baskswing, 
        // 22 vision during daytime, 23 vision during night, 24 turn rate, 25 collision size, 26 base health regeneration,  27 legs.
        double[,] hero_attribute = new double[117, 28];

        public heroAttribute()
        {
            heroID hero_id = new heroID();
            Dictionary<int, string> hero_id_name = hero_id.getHeroID();
            string s = Path.Combine(Environment.CurrentDirectory, "../../Properties/dota_hero_info_1.txt");
            string[] lines = System.IO.File.ReadAllLines(s);
            string[] second_lines = lines;
            int key = 0;
            foreach (string line in lines)
            {
                key++;
                string second_string = string.Empty;
                string[] words = line.Split('\t');
                hero_attribute[key, 0] = key;
                if (words[1] == "Strength")
                {
                    hero_attribute[key, 1] = 0;
                }
                else if (words[1] == "Agility")
                {
                    hero_attribute[key, 1] = 1;
                }
                else if (words[1] == "Intelligence")
                {
                    hero_attribute[key, 1] = 2;
                }
                for (int i = 2; i < words.Length; i++)
                {
                    double.TryParse(words[i], out hero_attribute[key, i]);
                }
            }
        }
        // table: 0,name, 1, primary attribute, 2 base strength,  3 strength growth, 4 total strength, 5 base agility, 6 agility growth, 7 total agility,
        // 8 base intelligence, 9 intelligence growth, 10 total intelligence, 11 total start attributes, 12 total attributes growth, 13 total attribute 25 level, 14 base movement speed, 
        // 15 starting armor, 16 starting attack damage(min), 17 starting attack damage(max), 18 attack range, 19 base attack time, 20 attack point, 21 attack baskswing, 
        // 22 vision during daytime, 23 vision during night, 24 turn rate, 25 collision size, 26 base health regeneration,  27 legs.

        /*
         * 
         * 
         */
        public Dictionary<string, double> heroCurrentAttributeAndDamage( int hero_id, int hero_level)
        {
            Dictionary<string, double> currentAttribute = new Dictionary<string, double>();
            double curStrength = hero_attribute[hero_id, 2] + hero_attribute[hero_id, 3] * (hero_level-1);
            currentAttribute.Add("Strength", curStrength);
            double curAgility = hero_attribute[hero_id, 5] + hero_attribute[hero_id, 6] * (hero_level - 1);
            currentAttribute.Add("Agility", curAgility);
            double curIntelligence = hero_attribute[hero_id, 8] + hero_attribute[hero_id, 9] * (hero_level - 1);
            currentAttribute.Add("Intelligence", curIntelligence);
            double maxDamage = hero_attribute[hero_id, 17];
            double minDamage = hero_attribute[hero_id, 16];
            if (hero_attribute[hero_id, 1] == 0)
            {
                minDamage = minDamage - hero_attribute[hero_id, 2] + curStrength;
                maxDamage = maxDamage - hero_attribute[hero_id, 2] + curStrength;
                currentAttribute.Add("Damage_Min", minDamage);
                currentAttribute.Add("Damage_Max", maxDamage);
            }
            else if (hero_attribute[hero_id, 1] == 1)
            {
                minDamage = minDamage - hero_attribute[hero_id, 5] + curAgility;
                maxDamage = maxDamage - hero_attribute[hero_id, 5] + curAgility;
                currentAttribute.Add("Damage_Min", minDamage);
                currentAttribute.Add("Damage_Max", maxDamage);
            }
            else if(hero_attribute[hero_id, 1] == 2)
            {
                minDamage = minDamage - hero_attribute[hero_id, 8] + curIntelligence;
                maxDamage = maxDamage - hero_attribute[hero_id, 8] + curIntelligence;
                currentAttribute.Add("Damage_Min", minDamage);
                currentAttribute.Add("Damage_Max", maxDamage);
            }
            return currentAttribute;
        }
    }
}
