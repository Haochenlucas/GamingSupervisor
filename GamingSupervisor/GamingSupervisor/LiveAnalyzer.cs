using System;

namespace GamingSupervisor
{
    class LiveAnalyzer : Analyzer
    {
        private GameStateIntegration gsi;

        public LiveAnalyzer() : base()
        {
        }

        public override void Start()
        {
            Console.WriteLine("Started live...");

            overlay = OverlaySingleton.Instance;
            gsi = GameStateIntegrationSingleton.Instance;

            gsi.StartListener();

            bool gameStarted = false;
            bool keepLooping = true;

            Console.WriteLine("Currently analyzing...");
            while (keepLooping)
            {
                switch (gsi.GameState)
                {
                    case null:
                    case "":
                    case "Undefined":
                        if (gameStarted)
                        {
                            keepLooping = false;
                        }
                        break;
                    case "DOTA_GAMERULES_STATE_HERO_SELECTION":
                        gameStarted = true;
                        break;
                    case "DOTA_GAMERULES_STATE_PRE_GAME":
                    case "DOTA_GAMERULES_STATE_GAME_IN_PROGRESS":
                        gameStarted = true;

                        overlay.ClearHeroSuggestion();
                        HandleGamePlay();
                        overlay.ShowIngameMessage();
                        break;
                    default:
                        gameStarted = true;
                        break;
                }

                if (!keepLooping)
                {
                    break;
                }
            }

            overlay.Clear();

            Console.WriteLine("Game stopped!");
        }

        private void HandleGamePlay()
        {
            double[] hpToSend = new double[5] { 0, 0, 0, 0, 0 };

            int healthPercent = gsi.HealthPercent;
            
            overlay.AddHPs(hpToSend);
            overlay.AddHp(hpToSend[0]);

            if (true)//healthPercent < 25)
            {
                overlay.AddRetreatMessage("Health " + gsi.Health, "");

            }
            else
            {
                //overlay.ClearMessage(7);
            }
        }
    }
}
