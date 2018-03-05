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
            d2d.Intructions_setup();
            d2d.HeroIntro_setup(38);
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
                    messages[0] = "Abaddon";
                    messages[1] = "Alchemist";
                    messages[2] = "Ancient Apparition";
                    messages[3] = "Anti-mage";
                    messages[4] = "Axe";
                    string[] imgName = new string[5];
                    imgName[0] = "1";
                    imgName[1] = "2";
                    imgName[2] = "3";
                    imgName[3] = "4";
                    imgName[4] = "6";
                    d2d.HeroSelectionHints(messages, imgName);
                    d2d.ItemSelectionHints(messages[0], imgName[0]);
                }
                if (Control.ModifierKeys == Keys.Alt)
                {
                    //d2d.Ingame_Draw(VS_HWND, overlay);
                    d2d.HeroInfo_Draw(VS_HWND, overlay);
                }
                else
                {
                    d2d.Intructions_Draw(VS_HWND, overlay);
                }

                watch.Restart();
            }
        }
    }
}
