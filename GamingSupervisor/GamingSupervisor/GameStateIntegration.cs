using Dota2GSI;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace GamingSupervisor
{
    class GameStateIntegration
    {
        private static GameStateListener gameStateListener;
        public bool GameStarted { get; set; }

        private readonly object gameStateLock = new object();
        private readonly object gameTimeLock = new object();

        private string gameState;
        public string GameState
        {
            get
            {
                lock (gameStateLock)
                {
                    return gameState;
                }
            }
            set
            {
                lock (gameStateLock)
                {
                    gameState = value;
                }
            }
        }

        public int gameTime;
        public int GameTime
        {
            get
            {
                lock (gameTimeLock)
                {
                    return gameTime;
                }
            }
            set
            {
                lock (gameTimeLock)
                {
                    gameTime = value;
                }
            }
        }

        public GameStateIntegration()
        {
            CreateGameStateIntegrationFile();

            while (true)
            {
                Process[] processName = Process.GetProcessesByName("Dota2");
                if (processName.Length != 0)
                {
                    break;
                }
                Thread.Sleep(1000);
            }

            gameStateListener = new GameStateListener(3000);
            gameStateListener.NewGameState += OnNewGameState;
        }

        public void StartListener()
        {
            if (!gameStateListener.Start())
            {
                throw new Exception("GameStateListener could not start. Try running this program as Administrator. Exiting.");
            }
            Console.WriteLine("Listening for game integration calls...");
        }

        private void OnNewGameState(GameState gs)
        {
            GameState = gs.Map.GameState.ToString();
            GameTime = gs.Map.GameTime;
        }

        private static void CreateGameStateIntegrationFile()
        {
            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");

            if (regKey != null)
            {
                string gsifolder = regKey.GetValue("SteamPath") +
                                   @"\steamapps\common\dota 2 beta\game\dota\cfg\gamestate_integration";
                Directory.CreateDirectory(gsifolder);
                string gsifile = gsifolder + @"\gamestate_integration_testGSI.cfg";
                if (File.Exists(gsifile))
                {
                    return;
                }

                string[] contentofgsifile =
                {
                    "\"Dota 2 Integration Configuration\"",
                    "{",
                    "    \"uri\"           \"http://localhost:3000\"",
                    "    \"timeout\"       \"5.0\"",
                    "    \"buffer\"        \"0.1\"",
                    "    \"throttle\"      \"0.1\"",
                    "    \"heartbeat\"     \"30.0\"",
                    "    \"data\"",
                    "    {",
                    "        \"provider\"      \"1\"",
                    "        \"map\"           \"1\"",
                    "        \"player\"        \"1\"",
                    "        \"hero\"          \"1\"",
                    "        \"abilities\"     \"1\"",
                    "        \"items\"         \"1\"",
                    "    }",
                    "}",

                };

                File.WriteAllLines(gsifile, contentofgsifile);
            }
            else
            {
                Console.WriteLine("Registry key for steam not found, cannot create Gamestate Integration file");
            }
        }
    }
}
