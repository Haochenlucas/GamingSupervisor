using replayParse;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Yato.DirectXOverlay;

namespace GamingSupervisor
{
    class GamingSupervisorManager
    {
        //private static string parsed_file = @"../../Parser/replay.txt";
        public replay_version01 parsed_replay;
        public int[,,] parsed_info;
        
        public string hero_selected = "";
        public int hero_id;

        private readonly object tickLock = new object();
        private int currentTick;
        private int CurrentTick
        {
            get
            {
                lock (tickLock)
                {
                    return currentTick;
                }
            }
            set
            {
                lock (tickLock)
                {
                    currentTick = value;
                }
            }
        }

        private System.Timers.Timer tickTimer;

        private ReplayStartAnnouncer announcer = null;
        private OverlayManager overlayManager = null;
        private OverlayWindow window = null;
        private Direct2DRenderer d2d = null;
        private IntPtr dota_HWND;

        public GamingSupervisorManager()
        {
            tickTimer = new System.Timers.Timer(1000.0 / 30.0);
            tickTimer.Elapsed += new System.Timers.ElapsedEventHandler(tick_timer_Tick);
        }

        public void Start()
        {
            parsed_replay = new replay_version01();
            parsed_info = parsed_replay.getReplayInfo();

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

            if (overlayManager == null)
            {
                dota_HWND = Process.GetProcessesByName("dota2")[0].MainWindowHandle;
                overlayManager = new OverlayManager(dota_HWND, out window, out d2d);
            }

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

                int health = parsed_info[CurrentTick - parsed_replay.getOffSet(), hero_id, 0];


                //if (health < 470)
                if (true)
                {
                    //d2d.retreat(dota_HWND, window, "Health is low, retreat");
                    d2d.retreat(dota_HWND, window, "Health: " + health);
                }
                else
                {
                    d2d.clear();
                }

                Thread.Sleep(10);
            }

            d2d.clear();

            Console.WriteLine("Replay stopped!");
        }

        private void tick_timer_Tick(object sender, EventArgs e)
        {
            CurrentTick++;
        }
    }
}
