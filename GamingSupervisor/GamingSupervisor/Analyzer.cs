using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Yato.DirectXOverlay;

namespace GamingSupervisor
{
    abstract class Analyzer
    {
        protected Overlay overlay;

        protected VisualCustomize visualCustomize;
        protected ContentControl initialInstructions;

        public Analyzer()
        {
            Terminate = false;

            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    visualCustomize = new VisualCustomize();
                    visualCustomize.Show();

                    // Calculations taken directly from Direct2DRenderer.cs
                    // These should probably be changed
                    double box_left = visualCustomize.ScreenWidth / 32 * 20 - Direct2DRenderer.size_scale * visualCustomize.ScreenWidth / 32 * 3 + visualCustomize.ScreenWidth / 32 * 2 * Direct2DRenderer.size_scale;
                    double box_top = visualCustomize.ScreenHeight / 32 * 6 - visualCustomize.ScreenHeight / 32 * 4 * Direct2DRenderer.size_scale;
                    double box_right = box_left + visualCustomize.ScreenWidth / 32 * 12 * Direct2DRenderer.size_scale;
                    double box_bottom = box_top + visualCustomize.ScreenHeight / 32 * 12 * Direct2DRenderer.size_scale;

                    initialInstructions = new ContentControl
                    {
                        Width = box_right - box_left,
                        MinWidth = 1,
                        Height = box_bottom - box_top,
                        MinHeight = 1,
                    };

                    double initialInstructionsX = box_left;
                    double initialInstructionsY = box_top;

                    visualCustomize.AddElement(initialInstructions, (int)initialInstructionsX, (int)initialInstructionsY);
                });
        }

        protected bool IsDotaRunning()
        {
            return Process.GetProcessesByName("dota2").Length != 0;
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
