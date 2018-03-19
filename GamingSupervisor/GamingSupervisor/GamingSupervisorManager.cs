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
            switch (GUISelection.gameType)
            {
                case GUISelection.GameType.live:
                    break;
                case GUISelection.GameType.replay:
                    ParserHandler.StartFullParsing(GUISelection.fileName + ".dem");
                    break;
            }

            bool isDotaAlreadyRunning = Process.GetProcessesByName("dota2").Length != 0;

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
                WaitForDotaToOpen();

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

        private void WaitForDotaToOpen()
        {
            string consoleLog = Path.Combine(SteamAppsLocation.Get(), "console.log");
            using (FileStream fileStream = File.Open(consoleLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                fileStream.Seek(0, SeekOrigin.End);
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    while (true)
                    {
                        Thread.Sleep(500);
                        if (streamReader.ReadToEnd().Contains("ChangeGameUIState: DOTA_GAME_UI_STATE_LOADING_SCREEN -> DOTA_GAME_UI_STATE_DASHBOARD"))
                            break;
                    }
                }
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
