using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace replayParse
{
    public class counter_pick_logic
    {
        public static double[,] matrix_info = new double[116, 116];

        public counter_pick_logic()
        {
            counterpick_info cp_info = new counterpick_info();
            matrix_info = cp_info.getCounterTable();
        }

        public int[] logic_counter(int[] hero_sequence, int[] ban_list)
        {
            Dictionary<double, int> five_picks = new Dictionary<double, int>();
            int[] five_hero_array = new int[5];
            int length = hero_sequence.Length;
            int count = 1;
            int flag = 0;
            double lowest_fact = 100;
            double[] counterFact = new double[5];
            while (count <= 115)
            {
                if (hero_sequence.Contains(count) || ban_list.Contains(count))
                {
                    count++;
                    continue;
                }    
                double sum1 = 0;
                for (int i = 0; i < length; i++)
                {
                    sum1 = sum1 + matrix_info[hero_sequence[i], count];
                }
                if (flag < 5)
                {
                    five_picks.Add(sum1, count);
                    if (sum1 < lowest_fact)
                        lowest_fact = sum1;
                    flag++;
                }
                else if (flag >= 5)
                {
                    if (sum1 > lowest_fact)
                    {
                        five_picks.Remove(lowest_fact);
                        five_picks.Add(sum1, count);
                        lowest_fact = five_picks.Min(i =>i.Key);
                    }
                }
                count++;
            }
            var list = five_picks.Keys.ToList();
            list.Sort();

            // Loop through keys.
            int index = 4;
            foreach (var key in list)
            {
                five_hero_array[index] = five_picks[key];
                index--;
            }

            return five_hero_array;
        }
    }
}
