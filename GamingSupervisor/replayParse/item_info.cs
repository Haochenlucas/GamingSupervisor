using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;


namespace replayParse
{
    public class item_info
    {
        private string[,] item_table_info = new string[157, 120]; // start at [3,1] with ID of first item.
        private int[,] item_KB = new int[116, 3];
        public item_info()
        {
            //Create COM Objects. Create a COM object for everything that is referenced
            Excel.Application xlApp = new Excel.Application();
            string s = Path.Combine(Environment.CurrentDirectory, "../../Properties/item_info.xlsx");
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(s);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;
            
            int rowCount = xlRange.Rows.Count;
            int colCount = xlRange.Columns.Count;

            //iterate over the rows and columns and print to the console as it appears in the file
            //excel is not zero based!!
            for (int i = 1; i <= 157; i++)
            {
                for (int j = 1; j <= colCount; j++)
                {
                    if (j == 1)
                    {
                        Console.Write("\r\n");
                    }
                    //write the value to the console
                    if (xlRange.Cells[i, j] != null && xlRange.Cells[i, j].Value2 != null)
                    {
                        item_table_info[i,j] = xlRange.Cells[i, j].Value2.ToString();
                    }
                }
            }


            //cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //rule of thumb for releasing com objects:
            //  never use two dots, all COM objects must be referenced and released individually
            //  ex: [somthing].[something].[something] is bad

            //release com objects to fully kill excel process from running in the background
            Marshal.ReleaseComObject(xlRange);
            Marshal.ReleaseComObject(xlWorksheet);

            //close and release
            xlWorkbook.Close();
            Marshal.ReleaseComObject(xlWorkbook);

            //quit and release
            xlApp.Quit();
            Marshal.ReleaseComObject(xlApp);

            s = Path.Combine(Environment.CurrentDirectory, "../../Properties/hero_item.txt");
            string[] lines = System.IO.File.ReadAllLines(s);

            for( int i = 0; i < lines.Length; i ++ )
            {
                if (lines.Contains("["))
                {
                    string[] three_items = lines[i].Split('[');
                    three_items[1].Remove('[');
                    three_items[1].Remove(']');
                    string[] three_item_cur = three_items[1].Split(' ');
                    item_KB[i + 1, 0] = int.Parse(three_item_cur[0]);
                    item_KB[i + 1, 1] = int.Parse(three_item_cur[1]);
                    item_KB[i + 1, 2] = int.Parse(three_item_cur[2]);
                }
            }
        }

        public Dictionary<int,int> item_suggestion(int money, string dataFolderLocation, string myHero)
        {
            Dictionary<int, int> item_ID = new Dictionary<int, int>();
            heroID hid = new heroID();
            Dictionary<int, string> ID_table = hid.getHeroID(); // key is ID, value is hero_name;
            Dictionary<string, int> hero_table = hid.getIDfromLowercaseHeroname(); // key is hero_name, value is ID;
            int firstTick;
            int lastTick;
            string timePath = dataFolderLocation + "time.txt";
            string combatPath = dataFolderLocation + "combat.txt";

            List<String> timeLines = new List<String>(System.IO.File.ReadAllLines(timePath));
            Int32.TryParse(timeLines.First().Split(' ')[0], out firstTick);
            Int32.TryParse(timeLines.Last().Split(' ')[0], out lastTick);
            List<String> combatLines = new List<String>(System.IO.File.ReadAllLines(combatPath));
            List<List<String>> teamfight = new List<List<string>>();
            Dictionary<int, List<Tuple<String, String, String>>> tickInfo = new Dictionary<int, List<Tuple<string, string, string>>>();
            int currInd = 0;
            TimeSpan prevTime = new TimeSpan();
            TimeSpan thirty = TimeSpan.FromSeconds(30);
            string heroPattern = "hero.*hero";

            foreach (var line in combatLines)
            {
                if (line.Contains("[KILL]"))
                {
                    if (Regex.IsMatch(line, heroPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        if (teamfight.Count < currInd + 1)
                            teamfight.Add(new List<String>());

                        List<String> contents = new List<String>(line.Split(new char[] { ' ' }));
                        if (prevTime == new TimeSpan())
                            prevTime = TimeSpan.FromSeconds(Double.Parse(contents[0]));

                        TimeSpan currTime = TimeSpan.FromSeconds(Double.Parse(contents[0]));

                        if (prevTime == currTime)
                        {
                            teamfight[currInd].Add(contents[0]);
                            //teamfight[currInd].Add(currTime.ToString(@"hh\:mm\:ss"));
                            teamfight[currInd].Add(contents[2] + " " + contents[3]);
                        }
                        else if (prevTime.Add(thirty) > currTime)
                        {
                            teamfight[currInd].Add(contents[2] + " " + contents[3]);
                        }
                        else
                        {
                            currInd++;
                            teamfight.Add(new List<String>());
                            prevTime = currTime;
                            teamfight[currInd].Add(contents[0]);
                            //teamfight[currInd].Add(currTime.ToString(@"hh\:mm\:ss"));
                            teamfight[currInd].Add(contents[2] + " " + contents[3]);
                        }

                    }
                }
            }
            foreach (var kills in teamfight)
            {
                tickInfo[(int)Double.Parse(kills[0])] = new List<Tuple<string, string, string>>();
                for (int i = 1; i < kills.Count; i++)
                {
                    string[] cont = kills[i].Split(new char[] { ' ' });
                    string killed = ID_table[hero_table[ConvertedHeroName.Get(cont[0])]];
                    string killer = ID_table[hero_table[ConvertedHeroName.Get(cont[1])]];

                    string color = "we";
                    if (killed == myHero)
                    {
                        item_ID.Add((int)Double.Parse(kills[0]),item_KB[hero_table[killed],0]);
                    }
                    else if (killer == myHero)
                    {
                        item_ID.Add((int)Double.Parse(kills[0]), item_KB[hero_table[killer], 2]);
                    }
                    tickInfo[(int)Double.Parse(kills[0])].Add(new Tuple<string, string, string>(killer, killed, color));
                }
            }
            return item_ID;
        }        
    }

}
