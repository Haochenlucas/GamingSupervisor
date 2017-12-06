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
            OverlayWindow overlay;
            Direct2DRenderer d2d;

            if (Process.GetProcessesByName("dota2").Length > 0)
            {
                var dota_HWND = Process.GetProcessesByName("dota2")[0].MainWindowHandle;
                overlay_manager_example(dota_HWND, out overlay, out d2d);
            }

            var VS_HWND = Process.GetProcessesByName("devenv")[0].MainWindowHandle;
            overlay_manager_example(VS_HWND, out overlay, out d2d);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            while (true)
            {
                if (watch.ElapsedMilliseconds < 15)
                {
                    continue;
                }

                draw_overlay(VS_HWND, overlay, d2d, true);

                watch.Restart();
            }
        }

        static void overlay_manager_example(IntPtr parentWindowHandle, out OverlayWindow overlay, out Direct2DRenderer d2d)
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

            overlay = manager.Window;
            d2d = manager.Graphics;

            d2d.whiteSmoke = d2d.CreateBrush(0xF5, 0xF5, 0xF5, 100);

            d2d.blackBrush = d2d.CreateBrush(0, 0, 0, 255);
            d2d.redBrush = d2d.CreateBrush(255, 0, 0, 255);
            d2d.greenBrush = d2d.CreateBrush(0, 255, 0, 255);
            d2d.blueBrush = d2d.CreateBrush(0, 0, 255, 255);
            d2d.font = d2d.CreateFont("Consolas", 22);
        }

        static void draw_overlay(IntPtr parentWindowHandle, OverlayWindow overlay, Direct2DRenderer d2d, bool retreate)
        {
            IntPtr fg = GetForegroundWindow();
            if (fg == parentWindowHandle)
            {
                d2d.BeginScene();
                d2d.ClearScene();

                d2d.DrawTextWithBackground("FPS: " + d2d.FPS, 20, 40, d2d.font, d2d.redBrush, d2d.blackBrush);
                if (retreate)
                {
                    d2d.DrawTextWithBackground("Go back or DIE. The choice is simple ", 30, overlay.Height / 5 * 3, d2d.font, d2d.redBrush, d2d.blackBrush);
                    d2d.DrawCircle(overlay.Width / 2, overlay.Height / 2, 150, 2, d2d.redBrush);
                }

                d2d.DrawCrosshair(CrosshairStyle.Gap, Cursor.Position.X, Cursor.Position.Y, 25, 4, d2d.redBrush);

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
