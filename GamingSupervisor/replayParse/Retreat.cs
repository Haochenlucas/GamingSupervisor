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
        private Process p;

        public Retreat()
        {
            p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.FileName = "retreat_predict.exe";

            p.Start();
        }

        public bool CreateInput(float myID, float myLvl, float myHP, double myMana,
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


            Predict(sb.ToString(), out string prediction);

            return prediction.Contains("1");
        }

        private void Predict(string input, out string prediction)
        {
            p.StandardInput.WriteLine(input);

           prediction = p.StandardOutput.ReadLine();
        }
    }

    
}
