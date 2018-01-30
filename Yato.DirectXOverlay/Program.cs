using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace Yato.DirectXOverlay
{
    class Program
    {
        static void Main(string[] args)
        {
            // Declare window and overlay object
            OverlayWindow overlay;
            Direct2DRenderer d2d;
            OverlayManager manager;

            // Pass window handle of Dota2 into Initialization function
            if (Process.GetProcessesByName("dota2").Length > 0)
            {
                var dota_HWND = Process.GetProcessesByName("dota2")[0].MainWindowHandle;
                manager = new OverlayManager(dota_HWND, out overlay, out d2d);
            }

            // For test use only. Show overlay on Visual Studio
            var VS_HWND = Process.GetProcessesByName("devenv")[0].MainWindowHandle;
            manager = new OverlayManager(VS_HWND,out overlay,out d2d);

            //Thread.Sleep(2000);
            // Control FPS
            Stopwatch watch = new Stopwatch();
            d2d.SetupHintSlots();
            watch.Start();
            while (true)
            {
                if (watch.ElapsedMilliseconds < 15)
                {
                    continue;
                }

                // Low health
                if (true)
                {
                    string[] messages = new string[5];
                    messages[0] = "Trump";
                    messages[1] = "Abaddon";
                    messages[2] = "Alchemist";
                    messages[3] = "Ancient Apparition";
                    messages[4] = "Anti-mage";
                    string[] imgName = new string[5];
                    imgName[0] = "trump";
                    imgName[1] = "1";
                    imgName[2] = "2";
                    imgName[3] = "3";
                    imgName[4] = "4";
                    d2d.HeroSelectionHints(messages, imgName);
                    //d2d.Retreat("Run", "");
                }
                if (Control.ModifierKeys == Keys.Alt)
                    d2d.DeleteMessage(0);

                d2d.Draw(VS_HWND, overlay);

                watch.Restart();
            }
        }
    }
}
