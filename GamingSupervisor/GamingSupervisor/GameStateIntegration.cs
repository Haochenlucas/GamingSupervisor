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
        private void OnNewGameState(GameState gs)
        {
            SteamID = gs.Player.SteamID;
            GameTime = gs.Map.GameTime;
            IsDayTime = gs.Map.IsDaytime;
            IsNightstalkerNight = gs.Map.IsNightstalker_Night;
            WinTeam = gs.Map.Win_team.ToString();
            WardPurchaseCooldown = gs.Map.Ward_Purchase_Cooldown;
            GameState = gs.Map.GameState.ToString();
            Activity = gs.Player.Activity.ToString();
            Kills = gs.Player.Kills;
            Deaths = gs.Player.Deaths;
            Assists = gs.Player.Assists;
            LastHits = gs.Player.LastHits;
            Denies = gs.Player.Denies;
            KillStreak = gs.Player.KillStreak;
            Team = gs.Player.Team.ToString();
            Gold = gs.Player.Gold;
            GoldReliable = gs.Player.GoldReliable;
            GoldUnreliable = gs.Player.GoldUnreliable;
            GoldPerMinute = gs.Player.GoldPerMinute;
            ExperiencePerMinute = gs.Player.ExperiencePerMinute;
            Name = gs.Hero.Name;
            Level = gs.Hero.Level;
            IsAlive = gs.Hero.IsAlive;
            SecondsToRespawn = gs.Hero.SecondsToRespawn;
            BuybackCost = gs.Hero.BuybackCost;
            BuybackCooldown = gs.Hero.BuybackCooldown;
            Health = gs.Hero.Health;
            MaxHealth = gs.Hero.MaxHealth;
            HealthPercent = gs.Hero.HealthPercent;
            Mana = gs.Hero.Mana;
            MaxMana = gs.Hero.MaxMana;
            ManaPercent = gs.Hero.ManaPercent;
            IsSilenced = gs.Hero.IsSilenced;
            IsStunned = gs.Hero.IsStunned;
            IsDisarmed = gs.Hero.IsDisarmed;
            IsMagicImmune = gs.Hero.IsMagicImmune;
            IsHexed = gs.Hero.IsHexed;
            IsMuted = gs.Hero.IsMuted;
            IsBreak = gs.Hero.IsBreak;
            HasDebuff = gs.Hero.HasDebuff;
            List<Ability> abilityList = new List<Ability>();
            foreach (var ability in gs.Abilities)
            {
                Ability a = new Ability();
                a.Name = ability.Name;
                a.Level = ability.Level;
                a.CanCast = ability.CanCast;
                a.IsPassive = ability.IsPassive;
                a.IsActive = ability.IsActive;
                a.Cooldown = ability.Cooldown;
                a.IsUltimate = ability.IsUltimate;
                abilityList.Add(a);
            }
            Abilities = abilityList;
            List<string> itemList = new List<string>();
            foreach (var item in gs.Items.Inventory)
            {
                itemList.Add(item.Name);
            }
            Items = itemList;
        }

        public struct Ability
        {
            public string Name { get; set; }
            public int Level { get; set; }
            public bool CanCast { get; set; }
            public bool IsPassive { get; set; }
            public bool IsActive { get; set; }
            public int Cooldown { get; set; }
            public bool IsUltimate { get; set; }
        }

        private static GameStateListener gameStateListener = null;
        private static bool gsiStarted = false;

        public GameStateIntegration()
        {
#if DEBUG
            if (SteamAppsLocation.Get() == "./../../debug")
                return;
#endif
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
            if (gsiStarted)
                return;

            if (!gameStateListener.Start())
            {
                throw new Exception("GameStateListener could not start. Try running this program as Administrator. Exiting.");
            }

            gsiStarted = true;
            Console.WriteLine("Listening for game integration calls...");
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

        private readonly object steamIDLock = new object();
        private string steamID;
        public string SteamID
        {
            get { lock (steamIDLock) { return steamID; } }
            private set { lock (steamIDLock) { steamID = value; } }
        }

        private readonly object gameTimeLock = new object();
        public int gameTime;
        public int GameTime
        {
            get { lock (gameTimeLock) { return gameTime; } }
            private set { lock (gameTimeLock) { gameTime = value; } }
        }

        private readonly object isDayTimeLock = new object();
        private bool isDayTime;
        public bool IsDayTime
        {
            get { lock (isDayTimeLock) { return isDayTime; } }
            private set { lock (isDayTimeLock) { isDayTime = value; } }
        }

        private readonly object isNightstalkerNightLock = new object();
        private bool isNightstalkerNight;
        public bool IsNightstalkerNight
        {
            get { lock (isNightstalkerNightLock) { return isNightstalkerNight; } }
            private set { lock (isNightstalkerNightLock) { isNightstalkerNight = value; } }
        }

        private readonly object winTeamLock = new object();
        private string winTeam;
        public string WinTeam
        {
            get { lock (winTeamLock) { return winTeam; } }
            private set { lock (winTeamLock) { winTeam = value; } }
        }

        private readonly object wardPurchaseCooldownLock = new object();
        private int wardPurchaseCooldown;
        public int WardPurchaseCooldown
        {
            get { lock (wardPurchaseCooldownLock) { return wardPurchaseCooldown; } }
            private set { lock (wardPurchaseCooldownLock) { wardPurchaseCooldown = value; } }
        }

        private readonly object gameStateLock = new object();
        private string gameState;
        public string GameState
        {
            get { lock (gameStateLock) { return gameState; } }
            private set { lock (gameStateLock) { gameState = value; } }
        }

        private readonly object activityLock = new object();
        private string activity;
        public string Activity
        {
            get { lock (activityLock) { return activity; } }
            private set { lock (activityLock) { activity = value; } }
        }

        private readonly object killsLock = new object();
        private int kills;
        public int Kills
        {
            get { lock (killsLock) { return kills; } }
            private set { lock (killsLock) { kills = value; } }
        }

        private readonly object deathsLock = new object();
        private int deaths;
        public int Deaths
        {
            get { lock (deathsLock) { return deaths; } }
            private set { lock (deathsLock) { deaths = value; } }
        }

        private readonly object assistsLock = new object();
        private int assists;
        public int Assists
        {
            get { lock (assistsLock) { return assists; } }
            private set { lock (assistsLock) { assists = value; } }
        }

        private readonly object lastHitsLock = new object();
        private int lastHits;
        public int LastHits
        {
            get { lock (lastHitsLock) { return lastHits; } }
            private set { lock (lastHitsLock) { lastHits = value; } }
        }

        private readonly object deniesLock = new object();
        private int denies;
        public int Denies
        {
            get { lock (deniesLock) { return denies; } }
            private set { lock (deniesLock) { denies = value; } }
        }

        private readonly object killStreakLock = new object();
        private int killStreak;
        public int KillStreak
        {
            get { lock (killStreakLock) { return killStreak; } }
            private set { lock (killStreakLock) { killStreak = value; } }
        }

        private readonly object teamLock = new object();
        private string team;
        public string Team
        {
            get { lock (teamLock) { return team; } }
            private set { lock (teamLock) { team = value; } }
        }

        private readonly object goldLock = new object();
        private int gold;
        public int Gold
        {
            get { lock (goldLock) { return gold; } }
            private set { lock (goldLock) { gold = value; } }
        }

        private readonly object goldReliableLock = new object();
        private int goldReliable;
        public int GoldReliable
        {
            get { lock (goldReliableLock) { return goldReliable; } }
            private set { lock (goldReliableLock) { goldReliable = value; } }
        }

        private readonly object goldUnreliableLock = new object();
        private int goldUnreliable;
        public int GoldUnreliable
        {
            get { lock (goldUnreliableLock) { return goldUnreliable; } }
            private set { lock (goldUnreliableLock) { goldUnreliable = value; } }
        }

        private readonly object goldPerMinuteLock = new object();
        private int goldPerMinute;
        public int GoldPerMinute
        {
            get { lock (goldPerMinuteLock) { return goldPerMinute; } }
            private set { lock (goldPerMinuteLock) { goldPerMinute = value; } }
        }

        private readonly object experiencePerMinuteLock = new object();
        private int experiencePerMinute;
        public int ExperiencePerMinute
        {
            get { lock (experiencePerMinuteLock) { return experiencePerMinute; } }
            private set { lock (experiencePerMinuteLock) { experiencePerMinute = value; } }
        }

        private readonly object nameLock = new object();
        private string name;
        public string Name
        {
            get { lock (nameLock) { return name; } }
            private set { lock (nameLock) { name = value; } }
        }

        private readonly object levelLock = new object();
        private int level;
        public int Level
        {
            get { lock (levelLock) { return level; } }
            private set { lock (levelLock) { level = value; } }
        }

        private readonly object isAliveLock = new object();
        private bool isAlive;
        public bool IsAlive
        {
            get { lock (isAliveLock) { return isAlive; } }
            private set { lock (isAliveLock) { isAlive = value; } }
        }

        private readonly object secondsToRespawnLock = new object();
        private int secondsToRespawn;
        public int SecondsToRespawn
        {
            get { lock (secondsToRespawnLock) { return secondsToRespawn; } }
            private set { lock (secondsToRespawnLock) { secondsToRespawn = value; } }
        }

        private readonly object buybackCostLock = new object();
        private int buybackCost;
        public int BuybackCost
        {
            get { lock (buybackCostLock) { return buybackCost; } }
            private set { lock (buybackCostLock) { buybackCost = value; } }
        }

        private readonly object buybackCooldownLock = new object();
        private int buybackCooldown;
        public int BuybackCooldown
        {
            get { lock (buybackCooldownLock) { return buybackCooldown; } }
            private set { lock (buybackCooldownLock) { buybackCooldown = value; } }
        }

        private readonly object healthLock = new object();
        private int health;
        public int Health
        {
            get { lock (healthLock) { return health; } }
            private set { lock (healthLock) { health = value; } }
        }

        private readonly object maxHealthLock = new object();
        private int maxHealth;
        public int MaxHealth
        {
            get { lock (maxHealthLock) { return maxHealth; } }
            private set { lock (maxHealthLock) { maxHealth = value; } }
        }

        private readonly object healthPercentLock = new object();
        private int healthPercent;
        public int HealthPercent
        {
            get { lock (healthPercentLock) { return healthPercent; } }
            private set { lock (healthPercentLock) { healthPercent = value; } }
        }

        private readonly object manaLock = new object();
        private int mana;
        public int Mana
        {
            get { lock (manaLock) { return mana; } }
            private set { lock (manaLock) { mana = value; } }
        }

        private readonly object maxManaLock = new object();
        private int maxMana;
        public int MaxMana
        {
            get { lock (maxManaLock) { return maxMana; } }
            private set { lock (maxManaLock) { maxMana = value; } }
        }

        private readonly object manaPercentLock = new object();
        private int manaPercent;
        public int ManaPercent
        {
            get { lock (manaPercentLock) { return manaPercent; } }
            private set { lock (manaPercentLock) { manaPercent = value; } }
        }

        private readonly object isSilencedLock = new object();
        private bool isSilenced;
        public bool IsSilenced
        {
            get { lock (isSilencedLock) { return isSilenced; } }
            private set { lock (isSilencedLock) { isSilenced = value; } }
        }

        private readonly object isStunnedLock = new object();
        private bool isStunned;
        public bool IsStunned
        {
            get { lock (isStunnedLock) { return isStunned; } }
            private set { lock (isStunnedLock) { isStunned = value; } }
        }

        private readonly object isDisarmedLock = new object();
        private bool isDisarmed;
        public bool IsDisarmed
        {
            get { lock (isDisarmedLock) { return isDisarmed; } }
            private set { lock (isDisarmedLock) { isDisarmed = value; } }
        }

        private readonly object isMagicImmuneLock = new object();
        private bool isMagicImmune;
        public bool IsMagicImmune
        {
            get { lock (isMagicImmuneLock) { return isMagicImmune; } }
            private set { lock (isMagicImmuneLock) { isMagicImmune = value; } }
        }

        private readonly object isHexedLock = new object();
        private bool isHexed;
        public bool IsHexed
        {
            get { lock (isHexedLock) { return isHexed; } }
            private set { lock (isHexedLock) { isHexed = value; } }
        }

        private readonly object isMutedLock = new object();
        private bool isMuted;
        public bool IsMuted
        {
            get { lock (isMutedLock) { return isMuted; } }
            private set { lock (isMutedLock) { isMuted = value; } }
        }

        private readonly object isBreakLock = new object();
        private bool isBreak;
        public bool IsBreak
        {
            get { lock (isBreakLock) { return isBreak; } }
            private set { lock (isBreakLock) { isBreak = value; } }
        }

        private readonly object hasDebuffLock = new object();
        private bool hasDebuff;
        public bool HasDebuff
        {
            get { lock (hasDebuffLock) { return hasDebuff; } }
            private set { lock (hasDebuffLock) { hasDebuff = value; } }
        }

        private readonly object abilitiesLock = new object();
        private List<Ability> abilities;
        public List<Ability> Abilities
        {
            get { lock (abilitiesLock) { return abilities; } }
            private set { lock (abilitiesLock) { abilities = value; } }
        }

        private readonly object itemLock = new object();
        private List<string> items;
        public List<string> Items
        {
            get { lock (itemLock) { return items; } }
            private set { lock (itemLock) { items = value; } }
        }
    }
}