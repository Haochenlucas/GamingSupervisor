﻿using Dota2GSI;
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

            if (!gameStateListener.Start())
            {
                throw new Exception("GameStateListener could not start. Try running this program as Administrator. Exiting.");
            }
            Console.WriteLine("Listening for game integration calls...");

            while (true)
            {
                Thread.Sleep(1000);
            }
        }

        private void OnNewGameState(GameState gs)
        {
            if (gs.Map.ClockTime != -1)
            {
                GameStarted = true;
            }

            //Console.Clear();
            //Console.WriteLine("Press ESC to quit");
            //Console.WriteLine("Current Dota version: " + gs.Provider.Version);
            //Console.WriteLine("Current time as displayed by the clock (in seconds): " + gs.Map.ClockTime);
            //Console.WriteLine("Your steam name: " + gs.Player.Name);
            //Console.WriteLine("hero ID: " + gs.Hero.ID);
            //Console.WriteLine("Health: " + gs.Hero.Health);
            //for (int i = 0; i < gs.Abilities.Count; i++)
            //{
            //    Console.WriteLine("Ability {0} = {1}", i, gs.Abilities[i].Name);
            //}
            //Console.WriteLine("First slot inventory: " + gs.Items.GetInventoryAt(0).Name);
            //Console.WriteLine("Second slot inventory: " + gs.Items.GetInventoryAt(1).Name);
            //Console.WriteLine("Third slot inventory: " + gs.Items.GetInventoryAt(2).Name);
            //Console.WriteLine("Fourth slot inventory: " + gs.Items.GetInventoryAt(3).Name);
            //Console.WriteLine("Fifth slot inventory: " + gs.Items.GetInventoryAt(4).Name);
            //Console.WriteLine("Sixth slot inventory: " + gs.Items.GetInventoryAt(5).Name);
            //
            //if (gs.Items.InventoryContains("item_blink"))
            //    Console.WriteLine("You have a blink dagger");
            //else
            //    Console.WriteLine("You DO NOT have a blink dagger");
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
                    "    \"uri\"           \"http://localhost:4000\"",
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
