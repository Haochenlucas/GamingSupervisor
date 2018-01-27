using replayParse;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace GamingSupervisor
{
    class GamingSupervisorManager
    {
        private GUISelection selection;
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

        public GamingSupervisorManager(GUISelection selection)
        {
            this.selection = selection;

            tickTimer = new System.Timers.Timer(1000.0 / 30.0);
            tickTimer.Elapsed += new System.Timers.ElapsedEventHandler(tickCallback);
        }

        public void Start()
        {
            startDota();

            while (Process.GetProcessesByName("dota2").Length == 0)
            {
                Thread.Sleep(500);
            }

            StartAnalyzing();
        }

        private void startDota()
        {
            Console.WriteLine("Starting dota...");
            Process p = new Process();
            p.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%programfiles(x86)%\Steam\Steam.exe");
            p.StartInfo.Arguments = "-applaunch 570";
            try
            {
                p.Start();
                Console.WriteLine("Dota running!");
            }
            catch (Win32Exception ex)
            {
                Console.WriteLine("Starting dota failed! " + ex.Message);
            }
        }

        private void StartAnalyzing()
        {
            if (announcer == null)
            {
                announcer = new ReplayStartAnnouncer();
            }
            CurrentTick = announcer.GetStartTick();
            //announcer.waitForReplayToStart();
            announcer.waitForHeroSelectionToComplete();

            if (overlay == null)
            {
                overlay = new Overlay();
            }

            replay_version01 parsedReplay = new replay_version01();
            int[,,] parsed_info = parsedReplay.getReplayInfo();
            int heroId = parsedReplay.getHeros()[selection.heroName];

            announcer.waitForHeroShowcaseToComplete();
            tickTimer.Start();

            int lastGameTime = announcer.GetCurrentGameTime();
            int currentGameTime = 0;
            int lastTickSynced = CurrentTick;
            while (true)
            {
                if (announcer.GetCurrentGameState() == "Undefined")
                {
                    tickTimer.Stop();
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

                if (CurrentTick < 0)
                {
                    tickTimer.Stop();
                    break;
                }

                int health = parsed_info[CurrentTick - parsedReplay.getOffSet(), heroId, 0];

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

                Thread.Sleep(10);
            }

            overlay.Clear();

            Console.WriteLine("Replay stopped!");
        }

        private void tickCallback(object sender, EventArgs e)
        {
            CurrentTick++;
        }
    }
}
