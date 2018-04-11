using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace replayParse
{
    // This class include all jungle camps position, stacking timer and images representing the direction of wave pulling for each camp
    // Further more, It can include all neutral monsters' data
    class JungleCampData
    {
        // Hard coded position x,y
        private Tuple<double,double>[] jungleCampPos = new Tuple<double, double>[18];
        // Timer to pull the wave
        private int[] campMinMark = new int[18];

        public JungleCampData()
        {
            for(int i = 0; i < 18; i++)
            {

            }
        }

        public Tuple<double, double> GetCampPos(int index)
        {
            return jungleCampPos[index];
        }

        public int GetCampMinMark(int index)
        {
            return campMinMark[index];
        }

        public string GetDirectionImg(int index)
        {
            return "";
        }
    }
}
