﻿using System;
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
                    d2d.addMessage(0, "Run");
                    d2d.addMessage(1, "Dont Run");
                    d2d.addMessage(2, "Actually... Run");
                }
                if (Control.ModifierKeys == Keys.Alt)
                    d2d.deleteMessage(1);

                d2d.draw(VS_HWND, overlay, "Run");

                watch.Restart();
            }
        }
    }
}
