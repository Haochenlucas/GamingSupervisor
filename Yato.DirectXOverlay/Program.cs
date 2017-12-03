﻿using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Yato.DirectXOverlay
{
    class Program
    {
        static void Main(string[] args)
        {
            bool runExample = false;

            if(runExample) example();

            overlay_manager_example(Process.GetCurrentProcess().MainWindowHandle);
        }

        static void example()
        {
            var overlay = new OverlayWindow(0, 0, 800, 600);

            var rendererOptions = new Direct2DRendererOptions()
            {
                AntiAliasing = true,
                Hwnd = overlay.WindowHandle,
                MeasureFps = true,
                VSync = false
            };

            var d2d = new Direct2DRenderer(rendererOptions);

            var whiteSmoke = d2d.CreateBrush(0xF5, 0xF5, 0xF5, 100);

            var blackBrush = d2d.CreateBrush(0, 0, 0, 255);
            var redBrush = d2d.CreateBrush(255, 0, 0, 255);
            var greenBrush = d2d.CreateBrush(0, 255, 0, 255);
            var blueBrush = d2d.CreateBrush(0, 0, 255, 255);

            var font = d2d.CreateFont("Consolas", 22);

            while(true)
            {
                d2d.BeginScene();
                d2d.ClearScene(whiteSmoke);

                d2d.DrawTextWithBackground("FPS: " + d2d.FPS, 20, 40, font, greenBrush, blackBrush);

                d2d.BorderedRectangle(300, 40, 100, 200, 4, redBrush, blackBrush);
                d2d.DrawHorizontalBar(100, 290, 40, 2, 200, 4, greenBrush, blackBrush);
                d2d.DrawHorizontalBar(100, 280, 40, 2, 200, 4, blueBrush, blackBrush);

                d2d.DrawCrosshair(CrosshairStyle.Gap, 400, 300, 25, 4, redBrush);

                d2d.EndScene();
            }
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

            while (true)
            {
                d2d.BeginScene();
                d2d.ClearScene(whiteSmoke);

                d2d.DrawTextWithBackground("FPS: " + d2d.FPS, 20, 40, font, greenBrush, blackBrush);

                d2d.BorderedRectangle(300, 40, 100, 200, 4, redBrush, blackBrush);
                d2d.DrawHorizontalBar(100, 290, 40, 2, 200, 4, greenBrush, blackBrush);
                d2d.DrawHorizontalBar(100, 280, 40, 2, 200, 4, blueBrush, blackBrush);

                d2d.DrawCrosshair(CrosshairStyle.Gap, Cursor.Position.X, Cursor.Position.Y, 25, 4, redBrush);

                d2d.EndScene();
            }
        }
    }
}
