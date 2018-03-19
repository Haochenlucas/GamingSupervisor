using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

                    initialInstructions = new ContentControl
                    {
                        Width = visualCustomize.ScreenWidth / 32 * 20,
                        MinWidth = 1,
                        Height = visualCustomize.ScreenHeight / 32 * 8,
                        MinHeight = 1,
                    };

                    double initialInstructionsX = visualCustomize.ScreenWidth / 32 * 20 - Yato.DirectXOverlay.Direct2DRenderer.size_scale * visualCustomize.ScreenWidth * 3 + visualCustomize.ScreenWidth * 2 * Yato.DirectXOverlay.Direct2DRenderer.size_scale;
                    double initialInstructionsY = visualCustomize.ScreenHeight / 32 * 6 - visualCustomize.ScreenHeight * 4 * Yato.DirectXOverlay.Direct2DRenderer.size_scale;

                    visualCustomize.AddElement(initialInstructions, (int)initialInstructionsX, (int)initialInstructionsY);
                });
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
