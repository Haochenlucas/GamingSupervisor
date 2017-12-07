using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GamingSupervisor
{
    class ReplayStartAnnouncer
    {
        private static GameStateIntegration gameStateIntegration;

        public ReplayStartAnnouncer()
        {
        }

        //public void waitForReplayToStart()
        //{
        //    Console.WriteLine("Waiting for replay to start...");
        //    gameStateIntegration.StartListener();
        //    SpinWait.SpinUntil(() => gameStateIntegration.GameStarted == true);
        //    Console.WriteLine("Replay started!");
        //}

        public void waitForHeroSelectionToComplete()
        {
            Console.WriteLine("Waiting for hero selection to complete...");
            try
            {
                gameStateIntegration = new GameStateIntegration();
            }
            catch (Exception e)
            {
                // For some reason the GSI library throws an exception when the replay actually starts.
                // So that is our starting point.
            }
            Console.WriteLine("Hero selection started!");
        }
    }
}
