using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Yato.DirectXOverlay
{
    class Program
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();

        static void Main(string[] args)
        {
            var dota_HWND = Process.GetProcessesByName("dota2")[0].MainWindowHandle;
            overlay_manager_example(dota_HWND);
            //overlay_manager_example(Process.GetCurrentProcess().MainWindowHandle);
        }

        static void overlay_manager_example(IntPtr parentWindowHandle)
        {
            Console.SetWindowSize(Console.LargestWindowWidth / 2, Console.LargestWindowHeight / 2);

            var rendererOptions = new Direct2DRendererOptions()
            {
                AntiAliasing = true,
                Hwnd = IntPtr.Zero,
                MeasureFps = true,
                VSync = false
            };

            OverlayManager manager = new OverlayManager(parentWindowHandle, rendererOptions);

            var overlay = manager.Window;
            var d2d = manager.Graphics;

            var whiteSmoke = d2d.CreateBrush(0xF5, 0xF5, 0xF5, 100);

            var blackBrush = d2d.CreateBrush(0, 0, 0, 255);
            var redBrush = d2d.CreateBrush(255, 0, 0, 255);
            var greenBrush = d2d.CreateBrush(0, 255, 0, 255);
            var blueBrush = d2d.CreateBrush(0, 0, 255, 255);

            var font = d2d.CreateFont("Consolas", 22);

            Stopwatch watch = new Stopwatch();
            watch.Start();


            while (true)
            {
                //Process currentProcess = Process.GetCurrentProcess();
                //IntPtr hWnd = currentProcess.MainWindowHandle;

                if (watch.ElapsedMilliseconds < 15)
                {
                    continue;
                }


                watch.Restart();
                IntPtr fg = GetForegroundWindow();
                if (fg == parentWindowHandle)
                {
                    d2d.BeginScene();
                    d2d.ClearScene();

                    d2d.DrawTextWithBackground("FPS: " + d2d.FPS, 20, 40, font, greenBrush, blackBrush);
                    d2d.DrawTextWithBackground("Go back or DIE. The choice is simple ", 30, Screen.PrimaryScreen.Bounds.Height / 5 * 3, font, greenBrush, blackBrush);

                    d2d.DrawCircle(overlay.Width / 2, overlay.Height / 2, 150, 2, redBrush);

                    d2d.DrawCrosshair(CrosshairStyle.Gap, Cursor.Position.X, Cursor.Position.Y, 25, 4, redBrush);

                    d2d.EndScene();
                }
                else
                {
                    d2d.BeginScene();
                    d2d.ClearScene();
                    d2d.EndScene();
                }
            }
        }
    }
}
