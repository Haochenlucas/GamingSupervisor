using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GamingSupervisor
{
    class ReplayStartAnnouncer
    {
        private static GameStateIntegration gameStateIntegration;
        private static Semaphore semaphore;

        public ReplayStartAnnouncer()
        {
            semaphore = new Semaphore(0, 1);
            gameStateIntegration = new GameStateIntegration();
        }

        public int GetStartTick()
        {
            string firstLine = "";
            foreach (string line in File.ReadLines(@"../../Parser/replay.txt"))
            {
                firstLine = line;
                break;
            }
            string[] words = firstLine.Split(' ');

            return Convert.ToInt32(words[0]);
        }

        public void waitForReplayToStart()
        {
            Console.WriteLine("Waiting for replay to start...");
            gameStateIntegration.StartListener();
            SpinWait.SpinUntil(() => gameStateIntegration.GameState == "DOTA_GAMERULES_STATE_HERO_SELECTION");
            Console.WriteLine("Replay started!");
        }

        public void waitForHeroSelectionToComplete()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(HandleLibraryException);

            Console.WriteLine("Waiting for hero selection to complete...");
            semaphore.WaitOne();
            Console.WriteLine("Hero selection started!");
        }

        static private void HandleLibraryException(object sender, UnhandledExceptionEventArgs args)
        {
            semaphore.Release();
            Exception e = (Exception)args.ExceptionObject;
            Console.WriteLine("HandleLibraryException caught : " + e.Message);
        }
    }
}
