using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            Console.WriteLine("Started");
            Thread.Sleep(1000 * 60);

            gsi = GameStateIntegrationSingleton.Instance;
            overlay = OverlaySingleton.Instance;

            bool gameStarted = false;
            bool keepLooping = true;

            Console.WriteLine("Currently analyzing...");
            while (keepLooping)
            {
                Console.WriteLine(gsi.GameState + " game state");
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
                        //HandleHeroSelection();
                        //ShowDraftHints();
                        break;
                    case "DOTA_GAMERULES_STATE_PRE_GAME":
                    case "DOTA_GAMERULES_STATE_GAME_IN_PROGRESS":
                        gameStarted = true;
                        for (int i = 0; i < 5; i++)
                        {
                            overlay.ClearMessage(i);
                        }
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

            if (healthPercent < 25)
            {
                overlay.AddRetreatMessage("Health " + gsi.Health, "");

            }
            else
            {
                overlay.ClearMessage(7);
            }
        }
    }
}
