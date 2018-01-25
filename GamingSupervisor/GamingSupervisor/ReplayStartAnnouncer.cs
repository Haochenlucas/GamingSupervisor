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
        private static bool listenerStarted = false;

        public ReplayStartAnnouncer()
        {
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

        public int GetCurrentGameTime()
        {
            return gameStateIntegration.GameTime;
        }

        public string GetCurrentGameState()
        {
            return gameStateIntegration.GameState;
        }

        public void waitForReplayToStart()
        {
            Console.WriteLine("Waiting for replay to start...");
            gameStateIntegration.StartListener();
            if (!listenerStarted)
            {
                gameStateIntegration.StartListener();
                listenerStarted = true;
            }
            SpinWait.SpinUntil(() => gameStateIntegration.GameState == "DOTA_GAMERULES_STATE_HERO_SELECTION");
            Console.WriteLine("Replay started!");
        }

        public void waitForHeroSelectionToComplete()
        {
            Console.WriteLine("Waiting for hero selection to complete...");
            if (!listenerStarted)
            {
                gameStateIntegration.StartListener();
                listenerStarted = true;
            }
            SpinWait.SpinUntil(() => gameStateIntegration.GameState == "DOTA_GAMERULES_STATE_TEAM_SHOWCASE");
            Console.WriteLine("Hero showcase started!");
        }

        public void waitForHeroShowcaseToComplete()
        {
            Console.WriteLine("Waiting for hero showcase to complete...");
            if (!listenerStarted)
            {
                gameStateIntegration.StartListener();
                listenerStarted = true;
            }
            SpinWait.SpinUntil(() => gameStateIntegration.GameState == "DOTA_GAMERULES_STATE_PRE_GAME");
            Console.WriteLine("Game started!");
        }
    }
}
