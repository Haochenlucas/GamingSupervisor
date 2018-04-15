using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Yato.DirectXOverlay;

namespace GamingSupervisor
{
    abstract class Analyzer
    {
        protected Overlay overlay;

        protected VisualCustomize visualCustomize;
        protected OverlayBox initialInstructionsBox;
        protected OverlayBox heroSelectionBox;
        protected OverlayBox highlightBarBox;
        protected OverlayBox healthGraphsBox;
        protected OverlayBox itemBox;
        protected IntPtr visualCustomizeHandle;

        public Analyzer()
        {
            Console.WriteLine("Analyzer1");
            Terminate = false;

            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    visualCustomize = new VisualCustomize();
                    visualCustomize.Show();

                    visualCustomizeHandle = new WindowInteropHelper(visualCustomize).Handle;

                    AddInitialInstructionsBox();
                    AddHeroSelectionBox();
                    AddHighlightBarBox();
                    AddHealthGraphsBox();
                    AddItemBox();
                });
        }

        protected bool IsDotaRunning()
        {
#if DEBUG
            if (SteamAppsLocation.Get() == "./../../debug")
                return true;
#endif
            return Process.GetProcessesByName("dota2").Length != 0;
        }

        protected void GetBoxPosition(OverlayBox box, out double positionX, out double positionY)
        {
            double posX = 0;
            double posY = 0;
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    posX = Canvas.GetLeft(box) / visualCustomize.ActualWidth* visualCustomize.ScreenWidth;
                    posY = Canvas.GetTop(box) / visualCustomize.ActualHeight* visualCustomize.ScreenHeight;
                });
            positionX = posX;
            positionY = posY;
        }
 
        protected void GetBoxWidth(OverlayBox box, out double width)
        {
            double w = 0;
            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    w = box.Width / visualCustomize.ActualWidth* visualCustomize.ScreenWidth;
                });
            width = w;
        }

        private void AddInitialInstructionsBox()
        {
            // Calculations taken directly from Direct2DRenderer.cs
            double box_left = visualCustomize.ActualWidth / 32 * 20 - Direct2DRenderer.size_scale * visualCustomize.ActualWidth / 32 * 3 + visualCustomize.ActualWidth / 32 * 2 * Direct2DRenderer.size_scale;
            double box_top = visualCustomize.ActualHeight / 32 * 6 - visualCustomize.ActualHeight / 32 * 4 * Direct2DRenderer.size_scale;
            double box_right = box_left + visualCustomize.ActualWidth / 32 * 12 * Direct2DRenderer.size_scale;
            double box_bottom = box_top + visualCustomize.ActualHeight / 32 * 12 * Direct2DRenderer.size_scale;

            initialInstructionsBox = new OverlayBox
            {
                Width = box_right - box_left,
                MinWidth = 1,
                Height = box_bottom - box_top,
                MinHeight = 1,
            };

            double initialInstructionsBoxX = box_left;
            double initialInstructionsBoxY = box_top;

            visualCustomize.AddBox(
                initialInstructionsBox,
                (int) Math.Round(initialInstructionsBoxX),
                (int) Math.Round(initialInstructionsBoxY));
            
            initialInstructionsBox.Visibility = Visibility.Visible;
        }

        private void AddHeroSelectionBox()
        {
            // Calculations taken directly from Direct2DRenderer.cs
            double box_left = (visualCustomize.ActualWidth / 32) * 24 - Direct2DRenderer.size_scale * (visualCustomize.ActualWidth / 32) * 3 - visualCustomize.ActualWidth / 32;
            double box_top = (visualCustomize.ActualHeight / 32) * 3 * 2 - (visualCustomize.ActualHeight / 32) * 4;
            double box_right = box_left + (visualCustomize.ActualWidth / 32) * 10;
            double box_bottom = box_top + (visualCustomize.ActualHeight / 32) * 24;

            heroSelectionBox = new OverlayBox
            {
                Width = box_right - box_left,
                MinWidth = 1,
                Height = box_bottom - box_top,
                MinHeight = 1,
            };

            double heroSelectionBoxX = box_left;
            double heroSelectionBoxY = box_top;

            visualCustomize.AddBox(
                heroSelectionBox,
                (int) Math.Round(heroSelectionBoxX),
                (int) Math.Round(heroSelectionBoxY));
            
            heroSelectionBox.Visibility = Visibility.Hidden;
        }

        private void AddHighlightBarBox()
        {
            // Calculations taken directly from Direct2DRenderer.cs
            double xInit = visualCustomize.ActualWidth / 4;
            double xEnd = 3 * visualCustomize.ActualWidth / 4;
            double positionY = 3 * visualCustomize.ActualHeight / 4;

            highlightBarBox = new OverlayBox
            {
                Width = xEnd - xInit,
                MinWidth = 1,
                Height = 2,
                MinHeight = 2,
                MaxHeight = 2,
                Name = "HighlightBarBox"
            };

            double highlightBarBoxX = xInit;
            double highlightBarBoxY = positionY;

            visualCustomize.AddBox(
                highlightBarBox,
                (int) Math.Round(highlightBarBoxX),
                (int) Math.Round(highlightBarBoxY));

            highlightBarBox.Visibility = Visibility.Hidden;
        }

        private void AddHealthGraphsBox()
        {
            // Calculations taken directly from Direct2DRenderer.cs
            double start_x = 0;
            double end_x = 250 / visualCustomize.ScreenWidth * visualCustomize.ActualWidth;
            double start_y = visualCustomize.ActualHeight / 2 + (-100 + 28) / visualCustomize.ScreenHeight * visualCustomize.ActualHeight;
            double end_y = visualCustomize.ActualHeight / 2 + (150 + 28) / visualCustomize.ScreenHeight * visualCustomize.ActualHeight;
            start_y -= end_y - start_y; // Just use the line graph height as the bar graph height

            healthGraphsBox = new OverlayBox
            {
                Width = end_x - start_x,
                MinWidth = 1,
                Height = end_y - start_y,
                MinHeight = 1,
            };

            double healthGraphsBoxX = start_x;
            double healthGraphsBoxY = start_y;

            visualCustomize.AddBox(
                healthGraphsBox,
                (int)Math.Round(healthGraphsBoxX),
                (int)Math.Round(healthGraphsBoxY));

            healthGraphsBox.Visibility = Visibility.Hidden;
        }

        private void AddItemBox()
        {
            // Calculations taken directly from Direct2DRenderer.cs
            double box_left = visualCustomize.ActualWidth / 32 * 4 - Direct2DRenderer.size_scale * visualCustomize.ActualWidth / 32 * 3 - visualCustomize.ActualWidth / 32 * 0.5f * Direct2DRenderer.size_scale;
            double box_top = visualCustomize.ActualHeight / 32 * 6 - visualCustomize.ActualHeight / 32 * 4 * Direct2DRenderer.size_scale;
            double box_right = box_left + visualCustomize.ActualWidth / 32 * 10 * Direct2DRenderer.size_scale;
            double box_bottom = box_top + visualCustomize.ActualHeight / 32 * 8 * Direct2DRenderer.size_scale;

            itemBox = new OverlayBox
            {
                Width = box_right - box_left,
                MinWidth = 1,
                Height = box_bottom - box_top,
                MinHeight = 1,
            };

            double itemBoxX = box_left;
            double itemBoxY = box_top;

            visualCustomize.AddBox(
                itemBox,
                (int)Math.Round(itemBoxX),
                (int)Math.Round(itemBoxY));

            itemBox.Visibility = Visibility.Hidden;
        }

        public abstract void Start();

        private static readonly object terminateLock = new object();
        private bool terminate;
        public bool Terminate
        {
            get { lock (terminateLock) { return terminate; } }
            set { lock (terminateLock) { terminate = value; } }
        }
    }
}
