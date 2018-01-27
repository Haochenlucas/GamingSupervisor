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
        public string difficulty;

        //private static string parsed_file = @"../../Parser/replay.txt";
        public replay_version01 parsed_replay;
        public int[,,] parsed_info;

        public Boolean hero_selection = false;
        public Boolean item_helper = false;
        public Boolean laning = false;
        public Boolean last_hit = false;
        public Boolean jungling = false;
        public Boolean safe_farming_area = false;

        public Boolean replay_selected = false;
        public bool isReplayRunning = false;

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

        public string filename;

        public GamingSupervisorManager()
        {
            tickTimer = new System.Timers.Timer(1000.0 / 30.0);
            tickTimer.Elapsed += new System.Timers.ElapsedEventHandler(tick_timer_Tick);
        }

        public void start()
        {
            startDota();

            while (Process.GetProcessesByName("dota2").Length == 0)
            {
                Thread.Sleep(500);
            }

            StartAnalyzing();
        }

        private void replay_button_Click(object sender, EventArgs e)
        {
            //openFileDialog1.Filter = "dem files (*.dem)|*.dem|bz2 files (*.bz2)|*.bz2";
            //openFileDialog1.Title = "Select a replay file";
            //DialogResult result = openFileDialog1.ShowDialog();

            if (string.IsNullOrEmpty(filename))
            {
                return;
            }

            ParserHandler parser = new ParserHandler(filename);
            Thread thread = new Thread(parser.ParseReplayFile);
            thread.Start();

            thread.Join();
            parsed_replay = new replay_version01();
            parsed_info = parsed_replay.getReplayInfo();
            
            //Path.Combine(Environment.CurrentDirectory, @"..\..\Parser\")
            string[] info = File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, @"..\..\..\Parser\info.txt"));
            foreach (string test in info)
            {
                if (test.Contains("hero_name"))
                {
                    String parsed = test.Split(new string[] { "hero_" }, StringSplitOptions.None).Last();
                    parsed = parsed.Split(new char[] { '\"' }).First();
                    String[] temp = parsed.Split(new char[] { '_' });
                    String[] to_upper = new String[temp.Length];
                    for (int i = 0; i < temp.Length; i++)
                    {
                        to_upper[i] = temp[i].First().ToString().ToUpper() + temp[i].Substring(1);
                    }
                    parsed = string.Join(" ", to_upper);
                    //hero_select_box.Items.Add(parsed);
                }
            }
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
                d2d.setupHintSlots();
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

                #region Hints call logic
                bool inHeroSelect = true;
                if (inHeroSelect)
                {
                    // Function call that produce suggestions as output
                    // Each hero suggestion seperated by new line
                    // An hero collection entity that can access image name with its ID
                    string message = "Hero A \n Hero B \n Hero C";
                    string img = "";
                    d2d.heroSelectionHints(message, img);
                }
                else
                {
                    d2d.clear();
                }
                #endregion

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
