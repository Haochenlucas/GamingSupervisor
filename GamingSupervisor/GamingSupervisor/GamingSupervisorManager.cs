using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace GamingSupervisor
{
    class GamingSupervisorManager
    {
        private ReplayAnalyzer replayAnalyzer = null;
        private LiveAnalyzer liveAnalyzer = null;

        public GamingSupervisorManager()
        {
        }

        public void Start()
        {
            ParserHandler.StartFullParsing(GUISelection.fileName + ".dem");

            bool isDotaAlreadyRunning = Process.GetProcessesByName("dota2").Length != 0;
            string serverLog = Path.Combine(SteamAppsLocation.Get(), "server_log.txt");
            var originalLastLine = File.ReadLines(serverLog).Last();

            if (!isDotaAlreadyRunning)
                StartDota();

            switch (GUISelection.gameType)
            {
                case GUISelection.GameType.live:
                    liveAnalyzer = new LiveAnalyzer();
                    break;
                case GUISelection.GameType.replay:
                    ParserHandler.WaitForFullParsing();
                    replayAnalyzer = new ReplayAnalyzer();
                    break;
            }

            if (!isDotaAlreadyRunning)
                while (originalLastLine != File.ReadLines(serverLog).Last())
                    Thread.Sleep(1000);

            // window dosent create after the process created
            Thread.Sleep(5000);

            switch (GUISelection.gameType)
            {
                case GUISelection.GameType.live:
                    liveAnalyzer.Start();
                    break;
                case GUISelection.GameType.replay:
                    replayAnalyzer.Start();
                    break;
            }            
        }

        private void StartDota()
        {
            Console.WriteLine("Starting dota...");
            Process p = new Process();

            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
            if (regKey != null)
            {
                p.StartInfo.FileName = regKey.GetValue("SteamPath") + "/Steam.exe";
            }
            else
            {
                throw new Exception("Could not start DotA 2. Is Steam installed?");
            }
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

        private void HandleDotaExiting()
        {
            Console.WriteLine("Dota process ended");
            switch (GUISelection.gameType)
            {
                case GUISelection.GameType.live:
                    liveAnalyzer.Terminate = true;
                    break;
                case GUISelection.GameType.replay:
                    replayAnalyzer.Terminate = true;
                    break;
            }

            ResetSingletons();
        }

        private void ResetSingletons()
        {
            OverlaySingleton.Reset();
            GameStateIntegrationSingleton.Reset();
        }
    }
}
