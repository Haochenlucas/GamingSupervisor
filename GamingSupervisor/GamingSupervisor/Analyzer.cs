﻿using System;
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
        protected ContentControl initialInstructions;
        protected IntPtr visualCustomizeHandle;

        public Analyzer()
        {
            Terminate = false;

            Application.Current.Dispatcher.Invoke(
                () =>
                {
                    visualCustomize = new VisualCustomize();
                    visualCustomize.Show();

                    visualCustomizeHandle = new WindowInteropHelper(visualCustomize).Handle;

                    // Calculations taken directly from Direct2DRenderer.cs
                    // These should probably be changed
                    double box_left = visualCustomize.Width / 32 * 20 - Direct2DRenderer.size_scale * visualCustomize.Width / 32 * 3 + visualCustomize.Width / 32 * 2 * Direct2DRenderer.size_scale;
                    double box_top = visualCustomize.Height / 32 * 6 - visualCustomize.Height / 32 * 4 * Direct2DRenderer.size_scale;
                    double box_right = box_left + visualCustomize.Width / 32 * 12 * Direct2DRenderer.size_scale;
                    double box_bottom = box_top + visualCustomize.Height / 32 * 12 * Direct2DRenderer.size_scale;

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
