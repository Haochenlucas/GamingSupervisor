using System;
using System.IO;
using System.Threading;

namespace GamingSupervisor
{
    class ReplayStartAnnouncer
    {
        private GameStateIntegration gameStateIntegration = null;
        private bool listenerStarted = false;

        public ReplayStartAnnouncer()
        {
            gameStateIntegration = GameStateIntegrationSingleton.Instance;
        }

        public int GetStartTick()
        {
            string firstLine = "";
            foreach (string line in
                File.ReadLines(GUISelection.replayDataFolderLocation + "state.txt"))
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

        public bool isReplayStarted()
        {
            if (!listenerStarted)
            {
                gameStateIntegration.StartListener();
                listenerStarted = true;
            }
            return(
                gameStateIntegration.GameState != "Undefined" &&
                gameStateIntegration.GameState != null &&
                gameStateIntegration.GameState != "");
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