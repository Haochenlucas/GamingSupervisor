using System;
using replayParse;
using System.Threading;

namespace GamingSupervisor
{
    class ReplayAnalyzer
    {
        private GUISelection selection;

        replay_version01 parsedReplay;
        int[,,] parsedData;
        int heroId;

        private ReplayStartAnnouncer announcer = null;
        private Overlay overlay = null;

        private System.Timers.Timer tickTimer;
        private readonly object tickLock = new object();
        private int currentTick;
        private int CurrentTick
        {
            get { lock (tickLock) { return currentTick; } }
            set { lock (tickLock) { currentTick = value; } }
        }

        public ReplayAnalyzer(GUISelection selection)
        {
            this.selection = selection;

            tickTimer = new System.Timers.Timer(1000.0 / 30.0);
            tickTimer.Elapsed += new System.Timers.ElapsedEventHandler(tickCallback);

            parsedReplay = new replay_version01();
            parsedData = parsedReplay.getReplayInfo();
            heroId = parsedReplay.getHeros()[selection.heroName];
        }

        public void Start()
        {
            if (announcer == null)
            {
                announcer = new ReplayStartAnnouncer();
            }

            CurrentTick = announcer.GetStartTick();
            announcer.waitForReplayToStart();
            tickTimer.Start();

            if (overlay == null)
            {
                overlay = new Overlay();
            }

            int lastGameTime = announcer.GetCurrentGameTime();
            int currentGameTime = 0;
            int lastTickSynced = CurrentTick;
            bool keepLooping = true;
            while (keepLooping)
            {
                if (CurrentTick < 0)
                {
                    tickTimer.Stop();
                    break;
                }

                switch (announcer.GetCurrentGameState())
                {
                    case "Undefined":
                        tickTimer.Stop();
                        keepLooping = false;
                        break;
                    case "DOTA_GAMERULES_STATE_HERO_SELECTION":
                        HandleHeroSelection();
                        break;
                    case "DOTA_GAMERULES_STATE_PRE_GAME":
                    case "DOTA_GAMERULES_STATE_GAME_IN_PROGRESS":
                        HandleGamePlay();
                        break;
                    default:
                        //Console.WriteLine(announcer.GetCurrentGameState());
                        break;
                }

                currentGameTime = announcer.GetCurrentGameTime();
                if (currentGameTime != lastGameTime)
                {
                    int gameTimeChange = currentGameTime - lastGameTime;
                    lastGameTime = currentGameTime;
                    lastTickSynced += 30 * gameTimeChange;
                    CurrentTick = lastTickSynced;
                }

                Thread.Sleep(10);
            }

            overlay.Clear();

            Console.WriteLine("Replay stopped!");
        }

        private void HandleHeroSelection()
        {
            //Console.WriteLine("Should be selecting");
        }

        private void HandleGamePlay()
        {
            int health = parsedData[CurrentTick - parsedReplay.getOffSet(), heroId, 0];
            //if (health < 470)
            if (true)
            {
                //overlay.ShowMessage("Health is low, retreat");
                overlay.ShowMessage("Health: " + health);
            }
            else
            {
                overlay.Clear();
            }
        }

        private void tickCallback(object sender, EventArgs e)
        {
            CurrentTick++;
        }
    }
}
