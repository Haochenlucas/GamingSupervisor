using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace replayParse
{
    public class itemConvert
    {
        public itemConvert()
        {
            Excel.Application xlApp = new Excel.Application();
            string s = Path.Combine(Environment.CurrentDirectory, "Properties/item_info.xlsx");
            Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(s);
            Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
            Excel.Range xlRange = xlWorksheet.UsedRange;

            int rowCount = 157;//xlRange.Rows.Count;
            int colCount = 118;// xlRange.Columns.Count;

            //iterate over the rows and columns and print to the console as it appears in the file
            //excel is not zero based!!
            string[] secondLine = new string[158];
            for (int i = 1; i <= rowCount; i++)
            {
                
                for (int j = 1; j <= colCount; j++)
                {
                    if (xlRange.Cells[i, j] != null && xlRange.Cells[i, j].Value2 != null)
                    {
                        if (secondLine[i] != null)
                        {
                            secondLine[i] = secondLine[i] + "* " + xlRange.Cells[i, j].Value2.ToString();
                        }
                        else
                        {
                            secondLine[i] = xlRange.Cells[i, j].Value2.ToString();
                        }
                        
                    }
                }
            }


            string path = Path.Combine(Environment.CurrentDirectory, "Properties/item_info.txt");
            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path))
                {
                    foreach (string line in secondLine)
                    {
                        sw.WriteLine(line);
                    }

                }
            }

        }


    }
}
