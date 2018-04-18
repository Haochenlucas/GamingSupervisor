using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Yato.DirectXOverlay;

namespace GamingSupervisor
{
    class Overlay
    {
        private OverlayManager overlayManager = null;
        private OverlayWindow window = null;
        public Direct2DRenderer renderer;
        private IntPtr dotaProcessHandle;

        public Overlay()
        {
#if DEBUG
            if (SteamAppsLocation.Get() == "./../../debug")
            {
                dotaProcessHandle = Process.GetProcessesByName("notepad")[0].MainWindowHandle;
                overlayManager = new OverlayManager(dotaProcessHandle, out window, out renderer);
                renderer.SetupHintSlots();
                return;
            }
#endif
            while (Process.GetProcessesByName("dota2").Length == 0)
            {
                Console.WriteLine("Waiting for dota to start...");
                Thread.Sleep(500);
            }

            while (Process.GetProcessesByName("dota2")[0].MainWindowHandle == IntPtr.Zero)
            {
                Console.WriteLine("Waiting for dota window handle to appear...");
                Thread.Sleep(500);
            }

            dotaProcessHandle = Process.GetProcessesByName("dota2")[0].MainWindowHandle;
            overlayManager = new OverlayManager(dotaProcessHandle, out window, out renderer);
            renderer.SetupHintSlots();
            Console.WriteLine("Overlay running!");
        }

        public void UpdateWindowHandler()
        {
            IntPtr Dota2 = Process.GetProcessesByName("dota2")[0].MainWindowHandle;
            if (Dota2 == dotaProcessHandle)
            {
                return;
            }
            else
            {
                dotaProcessHandle = Process.GetProcessesByName("dota2")[0].MainWindowHandle;
                overlayManager = new OverlayManager(dotaProcessHandle, out window, out renderer);
                renderer.SetupHintSlots();
            }
        }

        public IntPtr GetOverlayHandle()
        {
            return dotaProcessHandle;
        }

        public void Clear()
        {
            renderer.clear();
        }

        public void CreepLowEnough()
        {
            renderer.LastHit();
        }

        public void CreepDied()
        {
            renderer.LastHitted();
        }

        public void AddRetreatMessage(string message, string img)
        {
            renderer.Retreat(message, img);
        }

        public void AddHeroesSuggestionMessage(string[] heroes, string[] imgs)
        {
            renderer.HeroSelectionHints(heroes, imgs);
        }
        
        public void AddItemSuggestionMessage(string message, string img)
        { 
            renderer.ItemSelectionHints(message, img);
        }

        public void AddHeroInfoMessage(string message, string img)
        {
            renderer.HeroInfoHints(message, img);
        }
        public void AddJungleStackingMessage(string message, string img, double _x, double _y)
        {
            renderer.JungleStacking(message, img, _x, _y);
        }
        public void ShowCloestEnemy(double x, double y)
        {
            renderer.SetClosetHeroPosition(x, y);
        }

        public void ToggleGraphForHeroHP(bool tog = true)
        {
            renderer.ToggleGraph(tog);
        }

        public void AddHPs(double[] newhps, double[] newmaxhps)
        {
            renderer.UpdateHeroHPGraph(newhps, newmaxhps);
        }

        public void AddHp(double newhp)
        {
            renderer.UpdateHeroHPQueue(newhp);
        }

        public void AddHeroGraphIcons(List<int> graphsIds)
        {
            renderer.UpdateHeroHpGraphIcons(graphsIds);
        }
        
        public void Intructions_setup(string content)
        {
            renderer.Intructions_setup(content);
        }

        public void ShowInstructionMessage(double positionX, double positionY, IntPtr visualCustomizeHandle)
        {
            renderer.Intructions_Draw(dotaProcessHandle, window, (float)positionX, (float)positionY, visualCustomizeHandle);
        }

        public void HideInitialInstructions()
        {
            renderer.HideInitialInstructions();
        }

        public void ShowHealthGraphs()
        {
            renderer.ToggleGraph(true);
        }

        public void HideHealthGraphs()
        {
            renderer.ToggleGraph(false);
        }

        public void ShowItemSuggestions()
        {
            renderer.ShowItemSuggestions();
        }

        public void HideItemSuggestions()
        {
            renderer.HideItemSuggestions();
        }

        public void ToggleHighlight(bool tog = true)
        {
            renderer.ToggleHightlight(tog);
        }

        public void UpdateHighlight(Dictionary<int, Tuple<string, List<Tuple<String, String, String>>>> ticks, float maxTick)
        {
            renderer.UpdateHighlightTime(ticks, maxTick);
        }

        public void ShowInGameOverlay(IntPtr visualCustomizeHandle,
            double highlightBarPositionX, double highlightBarPositionY,
            double healthGraphsPositionX, double healthGraphsPositionY,
            double itemPositionX, double itemPositionY,
            double highlightBarWidth)
        {
            renderer.Ingame_Draw(
                parentWindowHandle: dotaProcessHandle,
                overlay: window,
                doNotIgnoreHandle: visualCustomizeHandle,
                highlightBarPositionX: (float)highlightBarPositionX,
                highlightBarPositionY: (float)highlightBarPositionY,
                healthGraphsPositionX: (float)healthGraphsPositionX,
                healthGraphsPositionY: (float)healthGraphsPositionY,
                itemPositionX: (float)itemPositionX,
                itemPositionY: (float)itemPositionY,
                highlightBarWidth: (float)highlightBarWidth);
        }

        public void ShowDraftMessage(double positionX, double positionY, IntPtr visualCustomizeHandle)
        {
            renderer.HeroSelection_Draw(dotaProcessHandle, window, (float) positionX, (float) positionY, visualCustomizeHandle);
        }

        private void ClearMessage(Direct2DRenderer.hints hint)
        {
            renderer.DeleteMessage(hint);
        }

        public void ClearHeroSuggestion()
        {
            renderer.DeleteMessage(Direct2DRenderer.hints.hero_selection_1);
            renderer.DeleteMessage(Direct2DRenderer.hints.hero_selection_2);
            renderer.DeleteMessage(Direct2DRenderer.hints.hero_selection_3);
            renderer.DeleteMessage(Direct2DRenderer.hints.hero_selection_4);
            renderer.DeleteMessage(Direct2DRenderer.hints.hero_selection_5);
        }

        public void ClearItemSuggestion()
        {
            renderer.DeleteMessage(Direct2DRenderer.hints.items_selection);
        }

        public void ClearHeroInfo()
        {
            renderer.DeleteMessage(Direct2DRenderer.hints.heroinformation);
        }

        public void ClearRetreat()
        {
            renderer.DeleteMessage(Direct2DRenderer.hints.retreat);
        }
        public void ClearJungle()
        {
            renderer.DeleteMessage(Direct2DRenderer.hints.jungle);
        }

        public void XorCheck(int code)
        {
            renderer.HeroSelectionFeedBack(code);
        }
    }
}
