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
        private GameStateIntegration gsi;
        private Process Dota2Process;

        public GamingSupervisorManager()
        {
        }

        public void Start()
        {
            StartDota();

            switch (GUISelection.gameType)
            {
                case GUISelection.GameType.live:
                    liveAnalyzer = new LiveAnalyzer();
                    break;
                case GUISelection.GameType.replay:
                    replayAnalyzer = new ReplayAnalyzer();
                    break;
            }

            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
            if (regKey != null)
            {
                string serverLog = regKey.GetValue("SteamPath") + @"\steamapps\common\dota 2 beta\game\dota\server_log.txt";
                var originalLastLine = File.ReadLines(serverLog).Last();
                while (originalLastLine != File.ReadLines(serverLog).Last())
                {
                    Thread.Sleep(1000);
                }
            }
            else
            {
                while (Process.GetProcessesByName("dota2").Length == 0)
                {
                    Thread.Sleep(500);
                }
            }

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
            Dota2Process = new Process();

            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
            if (regKey != null)
            {
                Dota2Process.StartInfo.FileName = regKey.GetValue("SteamPath") + "/Steam.exe";
            }
            else
            {
                Dota2Process.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%programfiles(x86)%\Steam\Steam.exe");
            }
            Dota2Process.StartInfo.Arguments = "-applaunch 570";
            try
            {
                Dota2Process.Start();
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
