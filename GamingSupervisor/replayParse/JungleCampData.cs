using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace replayParse
{
    // This class include all jungle camps position, stacking timer and images representing the direction of wave pulling for each camp
    // Further more, It can include all neutral monsters' data
    public class JungleCampData
    {
        // Hard coded position x,y
        private Tuple<double,double>[] jungleCampPos = new Tuple<double, double>[18];
        // Timer to pull the wave
        private int[] campMinMark = new int[18];
        // Bitmap image for DirectionImg
        // DrawBitmap(bmp, 0.2f, messages[i].img_x, messages[i].img_y - modifier, messages[i].img_width, messages[i].img_height);
        private Tuple<string, float, float, float, float>[] DirectionImg = new Tuple<string, float, float, float, float>[18];

        private string[] pullMethod = new string[18];

        public JungleCampData()
        {
            // Order the camps by difficulties
            for(int i = 1; i < 19; i++)
            {
                /* position: x,y        camp diffculity     timer
                 * 
                1:  19361.625,11711.03125     S             00:54
                2:  13715.0625,21044.125      S             00:55
                3:  12666.59375,17087.375     M             00:55
                4:  14574.1875,12179.6875     M             00:56
                5:  16756.4375,11678.78125    M             00:55
                6:  18824.9375,16441.4375     M             00:53
                7:  17643.625,19610           M             00:53
                8:  14362.34375,20633.09375   M             00:55
                9:  11445.28125,15938.59375   H             00:56
                10: 15970.03125,13066.40625   H             00:54
                11: 20957.84375,11969.15625   H             00:54
                12: 20774.59375,17135.125     H             00:55
                13: 16215.125,19781.46875     H             00:54
                14: 12158.1875,19853.21875    H             00:55
                15: 13275.84375, 16273.71875  A             00:53
                16: 16495.28125,14516.40625   A             00:54
                17: 20385.5,15999.28125       A             00:54
                18: 15574.5,18544.53125       A             00:53
                */

                switch (i)
                {
                    case 1:
                        jungleCampPos[i-1] = new Tuple<double, double>(19361.625, 11711.03125);
                        campMinMark[i-1] = 54;
                        pullMethod[i - 1] = "Lure the creeps Down";
                        break;
                    case 2:
                        jungleCampPos[i-1] = new Tuple<double, double>(13715.0625, 21044.125);
                        campMinMark[i-1] = 55;
                        pullMethod[i - 1] = "Lure the creeps Up";
                        break;
                    case 3:
                        jungleCampPos[i-1] = new Tuple<double, double>(12666.59375,17087.375);
                        campMinMark[i-1] = 55;
                        pullMethod[i - 1] = "Lure the creeps Left then Up";
                        break;
                    case 4:
                        jungleCampPos[i-1] = new Tuple<double, double>(14574.1875, 12179.6875);
                        campMinMark[i-1] = 56;
                        pullMethod[i - 1] = "Lure the creeps Up";
                        break;
                    case 5:
                        jungleCampPos[i-1] = new Tuple<double, double>(16756.4375, 11678.78125);
                        campMinMark[i-1] = 55;
                        pullMethod[i - 1] = "Lure the creeps Up";
                        break;
                    case 6:
                        jungleCampPos[i-1] = new Tuple<double, double>(18824.9375, 16441.4375);
                        campMinMark[i-1] = 53;
                        pullMethod[i - 1] = "Lure the creeps Right";
                        break;
                    case 7:
                        jungleCampPos[i-1] = new Tuple<double, double>(17643.625, 19610);
                        campMinMark[i-1] = 53;
                        pullMethod[i - 1] = "Lure the creeps Up then Right";
                        break;
                    case 8:
                        jungleCampPos[i-1] = new Tuple<double, double>(14362.34375, 20633.09375);
                        campMinMark[i-1] = 55;
                        pullMethod[i - 1] = "Lure the creeps Up";
                        break;
                    case 9:
                        jungleCampPos[i-1] = new Tuple<double, double>(11445.28125, 15938.59375);
                        campMinMark[i-1] = 56;
                        pullMethod[i - 1] = "Lure the creeps Up";
                        break;
                    case 10:
                        jungleCampPos[i-1] = new Tuple<double, double>(15970.03125, 13066.40625);
                        campMinMark[i-1] = 54;
                        pullMethod[i - 1] = "Lure the creeps Up then Right";
                        break;
                    case 11:
                        jungleCampPos[i-1] = new Tuple<double, double>(20957.84375, 11969.15625);
                        campMinMark[i-1] = 54;
                        pullMethod[i - 1] = "Lure the creeps Left";
                        break;
                    case 12:
                        jungleCampPos[i-1] = new Tuple<double, double>(20774.59375, 17135.125);
                        campMinMark[i-1] = 55;
                        pullMethod[i - 1] = "Lure the creeps Left then Down";
                        break;
                    case 13:
                        jungleCampPos[i-1] = new Tuple<double, double>(16215.125, 19781.46875);
                        campMinMark[i-1] = 54;
                        pullMethod[i - 1] = "Lure the creeps Up then Right";
                        break;
                    case 14:
                        jungleCampPos[i-1] = new Tuple<double, double>(12158.1875, 19853.21875);
                        campMinMark[i-1] = 55;
                        pullMethod[i - 1] = "Lure the creeps Right";
                        break;
                    case 15:
                        jungleCampPos[i-1] = new Tuple<double, double>(13275.84375, 16273.71875);
                        campMinMark[i-1] = 53;
                        pullMethod[i - 1] = "Lure the creeps Down";
                        break;
                    case 16:
                        jungleCampPos[i-1] = new Tuple<double, double>(16495.28125, 14516.40625);
                        campMinMark[i-1] = 54;
                        pullMethod[i - 1] = "Lure the creeps Down";
                        break;
                    case 17:
                        jungleCampPos[i-1] = new Tuple<double, double>(20385.5, 15999.28125);
                        campMinMark[i-1] = 54;
                        pullMethod[i - 1] = "Lure the creeps Left";
                        break;
                    case 18:
                        jungleCampPos[i-1] = new Tuple<double, double>(15574.5, 18544.53125);
                        campMinMark[i-1] = 53;
                        pullMethod[i - 1] = "Lure the creeps Right";
                        break;
                    default:
                        break;
                }
            }
        }

        public Tuple<double, double> GetCampPos(int index)
        {
            return jungleCampPos[index - 1];
        }

        public int GetCampSecMark(int index)
        {
            return campMinMark[index - 1];
        }

        public Tuple<string, float, float, float, float> GetDirectionImg(int index)
        {
            return DirectionImg[index - 1];
        }

        // Index should be between 1 to 18
        // S: Small; M: Medium; H: Hard; A: Ancients
        public string GetCampDifficulity(int index)
        {
            switch (index)
            {
                case 1:
                case 2:
                    return "S";
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                    return "M";
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                    return "H";
                case 15:
                case 16:
                case 17:
                case 18:
                    return "A";
                default:
                    throw new Exception("Index not between 1 and 18.");
            }
        }
        public string GetDirection(int index)
        {
            return pullMethod[index - 1];
        }
    }
}
