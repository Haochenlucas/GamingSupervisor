using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace replayParse
{
    public class counterpick_info
    {
        public static double[,] counter_info_table = new double[116, 116];
        public counterpick_info()
        {
            heroID h_ID = new heroID();
            Dictionary<int, string> ID_table = h_ID.getHeroID(); // key is ID, value is hero_name;
            Dictionary<string, int> hero_table = h_ID.getIDHero(); // key is hero_name, value is ID;
            string s = Path.Combine(Environment.CurrentDirectory, @"..\..\Properties\c");
            string s_end = ".txt";
            int i = 1;
            string path = "";
            while (i < 116)
            {
                path = s + 1 + s_end;
                string[] lines = System.IO.File.ReadAllLines(path);
                string[] second_lines = lines;
                int counter = 0;
                foreach (string line in lines)
                {
                    if (counter == 0)
                    {
                        string[] words = line.Split('\t');
                        int length_this = words.Length;
                        string name = "";
                        int count_in = 0;
                        while (length_this > 1)
                        {
                            name = words[count_in];
                            count_in++;
                            length_this--;
                        }
                        string temp = words[count_in].Replace('%', ' ');
                        double d_temp = Convert.ToDouble(temp);
                        int index = -1;
                        hero_table.TryGetValue(name, out index);
                        if (index !=-1)
                            counter_info_table[i, index] = d_temp;
                        if (index == 0)
                        {
                            Console.WriteLine(name);
                        }
                        counter++;
                    }
                    else
                    {
                        counter = 0;
                    }
                }
                i++;
            }

            string path1 = Path.Combine(Environment.CurrentDirectory, @"..\..\Properties\countertable.txt");
            if (!File.Exists(path1))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path1))
                {
                    int row_number = 0;
                    while (row_number< 116)
                    {
                        double[] nthRow = GetRow(counter_info_table, row_number);
                        sw.WriteLine(convertString(nthRow));
                        row_number++;
                    }
                }
            }

        }
       

        public static T[] GetRow<T>(T[,] matrix, int row)
        {
            var columns = matrix.GetLength(1);
            var array = new T[columns];
            for (int i = 0; i < columns; ++i)
                array[i] = matrix[row, i];
            return array;
        }
        public static string convertString(double[] matrix)
        {
            string start = "";
            string add = " ";
            foreach(double d in matrix)
            {
                if (String.IsNullOrEmpty(start))
                    start = start + d;
                else
                    start = start + add + d;
            }
            return start;
        }

        public double[,] getCounterTable()
        {
            return counter_info_table;
        }
    }
}
