using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace replayParse
{
    public class Retreat
    {
        public static bool CreateInputFile(float myID, float myLvl, float myHP, double myMana,
        float enemyID, float enemyLvl, float enemyHP, double enemyMana)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(myID.ToString());
            sb.Append(" ");
            sb.Append(myLvl.ToString());
            sb.Append(" ");
            sb.Append(myHP.ToString());
            sb.Append(" ");
            sb.Append(myMana.ToString());
            sb.Append(" ");

            sb.Append(enemyID.ToString());
            sb.Append(" ");
            sb.Append(enemyLvl.ToString());
            sb.Append(" ");
            sb.Append(enemyHP.ToString());
            sb.Append(" ");
            sb.Append(enemyMana.ToString());

            using (StreamWriter file = new StreamWriter(Path.Combine(Environment.CurrentDirectory, "input.txt")))
            {
                file.WriteLine(sb.ToString());
            }

            Predict(out string prediction);

            return prediction.Contains("1");
        }

        private static void Predict(out string prediction)
        {
            Process p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "retreat_predict.exe";

            p.Start();

            prediction = p.StandardOutput.ReadLine();
        }
    }

    
}
