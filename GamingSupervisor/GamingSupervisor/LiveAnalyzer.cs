using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace GamingSupervisor
{
    class LiveAnalyzer : Analyzer
    {
        private GameStateIntegration gsi;
        private string lastGameState;

        public LiveAnalyzer() : base()
        {
        }

        public override void Start()
        {
            Console.WriteLine("Started live...");

            overlay = OverlaySingleton.Instance;
            gsi = GameStateIntegrationSingleton.Instance;

            gsi.StartListener();

            overlay.Intructions_setup("Start a game");

            while (gsi.GameState == "Undefined" || gsi.GameState == null || gsi.GameState == "")
            {
                if (!IsDotaRunning())
                {
                    overlay.Clear();
                    Console.WriteLine("Dota ended");
                    return;
                }

                double positionX = 0;
                double positionY = 0;
                GetBoxPosition(initialInstructionsBox, out positionX, out positionY);

                overlay.ShowInstructionMessage(positionX, positionY, visualCustomizeHandle);

                Thread.Sleep(10);
            }

            bool gameStarted = false;
            bool keepLooping = true;

            Console.WriteLine("Currently analyzing...");
            while (keepLooping)
            {
                if (!IsDotaRunning())
                {
                    overlay.Clear();
                    Console.WriteLine("Dota ended");
                    return;
                }

                switch (gsi.GameState)
                {
                    case null:
                    case "":
                    case "Undefined":
                        if (gameStarted)
                        {
                            keepLooping = false;
                        }
                        if (lastGameState != "Undefined")
                        {
                            lastGameState = "Defined";
                            Application.Current.Dispatcher.Invoke(
                            () =>
                            {
                                initialInstructionsBox.Visibility = Visibility.Hidden;
                                heroSelectionBox.Visibility = Visibility.Hidden;
                                inGameMessagesBox.Visibility = Visibility.Hidden;
                                highlightBarBox.Visibility = Visibility.Hidden;
                                healthGraphsBox.Visibility = Visibility.Hidden;
                            });
                        }
                        break;
                    case "DOTA_GAMERULES_STATE_HERO_SELECTION":
                        gameStarted = true;
                        break;
                    case "DOTA_GAMERULES_STATE_WAIT_FOR_MAP_TO_LOAD":
                        gameStarted = true;
                        break;
                    case "DOTA_GAMERULES_STATE_PRE_GAME":
                    case "DOTA_GAMERULES_STATE_GAME_IN_PROGRESS":
                        gameStarted = true;

                        if (lastGameState != "DOTA_GAMERULES_STATE_GAME_IN_PROGRESS")
                        {
                            lastGameState = "DOTA_GAMERULES_STATE_GAME_IN_PROGRESS";

                            Application.Current.Dispatcher.Invoke(
                            () =>
                            {
                                initialInstructionsBox.Visibility = Visibility.Hidden;
                                heroSelectionBox.Visibility = Visibility.Hidden;
                                inGameMessagesBox.Visibility = Visibility.Hidden; // What is this?
                                highlightBarBox.Visibility = Visibility.Hidden;
                                healthGraphsBox.Visibility = Visibility.Hidden;
                            });
                        }

                        HandleGamePlay();
                        UpdateInGameOverlay();
                        break;
                    default:
                        gameStarted = true;
                        break;
                }

                if (!keepLooping)
                {
                    break;
                }

                Thread.Sleep(10);
            }

            overlay.Clear();
            Application.Current.Dispatcher.Invoke(() => { visualCustomize.CloseWindow(); });
            Console.WriteLine("Game stopped!");
        }

        private void UpdateInGameOverlay()
        {
            double messagesPositionX = 0;
            double messagesPositionY = 0;
            GetBoxPosition(inGameMessagesBox, out messagesPositionX, out messagesPositionY);

            double highlightBarPositionX = 0;
            double highlightBarPositionY = 0;
            GetBoxPosition(highlightBarBox, out highlightBarPositionX, out highlightBarPositionY);
            double highlightBarWidth = 0;
            GetBoxWidth(highlightBarBox, out highlightBarWidth);

            double healthGraphPositionX = 0;
            double healthGraphPositionY = 0;
            GetBoxPosition(healthGraphsBox, out healthGraphPositionX, out healthGraphPositionY);

            overlay.ShowInGameOverlay(visualCustomizeHandle,
                messagesPositionX, messagesPositionY,
                highlightBarPositionX, highlightBarPositionY,
                healthGraphPositionX, healthGraphPositionY,
                highlightBarWidth);
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
