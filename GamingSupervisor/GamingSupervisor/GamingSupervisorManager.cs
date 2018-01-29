using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace GamingSupervisor
{
    class GamingSupervisorManager
    {
        private GUISelection selection;
        
        public GamingSupervisorManager(GUISelection selection)
        {
            this.selection = selection;            
        }

        public void Start()
        {
            StartDota();

            ReplayAnalyzer analyzer = new ReplayAnalyzer(selection);

            while (Process.GetProcessesByName("dota2").Length == 0)
            {
                Thread.Sleep(500);
            }
            
            analyzer.Start();
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
                p.StartInfo.FileName = Environment.ExpandEnvironmentVariables(@"%programfiles(x86)%\Steam\Steam.exe");
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
    }
}
