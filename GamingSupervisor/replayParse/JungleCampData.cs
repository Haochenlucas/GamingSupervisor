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
        // Bitmap image for DirectionImg
        // DrawBitmap(bmp, 0.2f, messages[i].img_x, messages[i].img_y - modifier, messages[i].img_width, messages[i].img_height);
        private Tuple<string, float, float, float, float>[] DirectionImg = new Tuple<string, float, float, float, float>[18];

        public JungleCampData()
        {
            // Order the camps by difficulties
            for(int i = 0; i < 18; i++)
            {
                switch (i)
                {
                    case 1:
                        Console.WriteLine("Case 1");
                        break;
                    case 2:
                        Console.WriteLine("Case 2");
                        break;
                    case 3:
                        Console.WriteLine("Case 2");
                        break;
                    case 4:
                        Console.WriteLine("Case 2");
                        break;
                    case 5:
                        Console.WriteLine("Case 2");
                        break;
                    case 6:
                        Console.WriteLine("Case 2");
                        break;
                    case 7:
                        Console.WriteLine("Case 2");
                        break;
                    case 8:
                        Console.WriteLine("Case 2");
                        break;
                    case 9:
                        Console.WriteLine("Case 2");
                        break;
                    case 10:
                        Console.WriteLine("Case 2");
                        break;
                    case 11:
                        Console.WriteLine("Case 2");
                        break;
                    case 12:
                        Console.WriteLine("Case 2");
                        break;
                    case 13:
                        Console.WriteLine("Case 2");
                        break;
                    case 14:
                        Console.WriteLine("Case 2");
                        break;
                    case 15:
                        Console.WriteLine("Case 2");
                        break;
                    case 16:
                        Console.WriteLine("Case 2");
                        break;
                    case 17:
                        Console.WriteLine("Case 2");
                        break;
                    case 18:
                        Console.WriteLine("Case 2");
                        break;
                    default:
                        Console.WriteLine("Default case");
                        break;
                }
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

        public Tuple<string, float, float, float, float> GetDirectionImg(int index)
        {
            return DirectionImg[index];
        }
    }
}
