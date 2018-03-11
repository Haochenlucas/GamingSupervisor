using Dota2GSI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace GamingSupervisor
{
    class GameStateIntegration
    {
        private static GameStateListener gameStateListener = null;
        public bool GameStarted { get; set; }

        private readonly object gameStateLock = new object();
        private string gameState;
        public string GameState
        {
            get { lock (gameStateLock) { return gameState; } }
            set { lock (gameStateLock) { gameState = value; } }
        }

        private readonly object gameTimeLock = new object();
        private int gameTime;
        public int GameTime
        {
            get { lock (gameTimeLock) { return gameTime; } }
            set { lock (gameTimeLock) { gameTime = value; } }
        }

        private readonly object goldLock = new object();
        private int gold;
        public int Gold
        {
            get { lock (goldLock) { return gold; } }
            set { lock (goldLock) { gold = value; } }
        }

        private readonly object nameLock = new object();
        private string name;
        public string Name
        {
            get { lock (nameLock) { return name; } }
            set { lock (nameLock) { name = value; } }
        }

        private readonly object levelLock = new object();
        private int level;
        public int Level
        {
            get { lock (levelLock) { return level; } }
            set { lock (levelLock) { level = value; } }
        }

        private readonly object healthLock = new object();
        private int health;
        public int Health
        {
            get { lock (healthLock) { return health; } }
            set { lock (healthLock) { health = value; } }
        }

        private readonly object maxHealthLock = new object();
        private int maxHealth;
        public int MaxHealth
        {
            get { lock (maxHealthLock) { return maxHealth; } }
            set { lock (maxHealthLock) { maxHealth = value; } }
        }

        private readonly object healthPercentLock = new object();
        private int healthPercent;
        public int HealthPercent
        {
            get { lock (healthPercentLock) { return healthPercent; } }
            set { lock (healthPercentLock) { healthPercent = value; } }
        }

        private readonly object manaLock = new object();
        private int mana;
        public int Mana
        {
            get { lock (manaLock) { return mana; } }
            set { lock (manaLock) { mana = value; } }
        }

        private readonly object itemLock = new object();
        private List<string> items;
        public List<string> Items
        {
            get { lock (itemLock) { return items; } }
            set { lock (itemLock) { items = value; } }
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

            if (gameStateListener == null)
            {
                gameStateListener = new GameStateListener(3000);
                gameStateListener.NewGameState += OnNewGameState;
            }
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
            Gold = gs.Player.Gold;
            Name = gs.Hero.Name;
            Level = gs.Hero.Level;
            Health = gs.Hero.Health;
            MaxHealth = gs.Hero.Health;
            HealthPercent = gs.Hero.HealthPercent;
            Mana = gs.Hero.Mana;

            List<string> itemList = new List<string>();
            foreach (var item in gs.Items.Inventory)
            {
                itemList.Add(item.Name);
            }
            Items = itemList;
        }

        private static void CreateGameStateIntegrationFile()
        {
            string gsifolder = Path.Combine(SteamAppsLocation.Get(), "cfg/gamestate_integration");
            Directory.CreateDirectory(gsifolder);
            string gsifile = Path.Combine(gsifolder, "gamestate_integration_testGSI.cfg");
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
    }
}