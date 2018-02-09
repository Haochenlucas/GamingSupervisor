﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
            renderer.SetupHintSlots();
            Console.WriteLine("Overlay running!");
        }

        public void Clear()
        {
            renderer.clear();
        }

        public void AddRetreatMessage(string message, string img)
        {
            renderer.Retreat(message, img);
        }

        public void AddHeroesSuggestionMessage(string[] heroes, string[] imgs)
        {
            renderer.HeroSelectionHints(heroes, imgs);
        }

        public void ToggleGraphForHeroHP(bool tog = true)
        {
            renderer.ToggleGraph(tog);
        }

        public void AddHPs(double[] newhps)
        {
            renderer.UpdateHeroHPGraph(newhps);
        }

        public void ShowMessage()
        {
            renderer.Draw(dotaProcessHandle, window);
        }

        public void ClearMessage(int MessageNum)
        {
            renderer.DeleteMessage(MessageNum);
        }
    }
}
