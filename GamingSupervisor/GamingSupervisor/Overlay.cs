using System;
using System.Diagnostics;
using Yato.DirectXOverlay;

namespace GamingSupervisor
{
    class Overlay
    {
        private OverlayManager overlayManager = null;
        private OverlayWindow window = null;
        private Direct2DRenderer renderer = null;
        private IntPtr dotaProcessHandle;

        public Overlay()
        {
            dotaProcessHandle = Process.GetProcessesByName("dota2")[0].MainWindowHandle;
            overlayManager = new OverlayManager(dotaProcessHandle, out window, out renderer);
        }

        public void Clear()
        {
            renderer.clear();
        }

        public void ShowMessage(string message)
        {
            renderer.retreat(dotaProcessHandle, window, message);
        }
    }
}
