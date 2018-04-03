using System;
using System.Collections.Generic;
using System.IO;

namespace replayParse
{
    public class HeroParser
    {
        private static int FirstTick { get; set; }
        private class TickEntries<T> : Dictionary<int, T>
        {
            private int cachedTick = 0;
            private T cachedValue = default(T);

            private int lastTickAccessed = -1;

            public new T this[int tick]
            {
                get
                {
                    // Iterate through every previous tick until finding one with a value
                    if (base.ContainsKey(tick))
                    {
                        cachedTick = tick;
                        cachedValue = base[tick];
                        lastTickAccessed = tick;
                        return base[tick];
                    }
                    else if (lastTickAccessed != -1 && tick >= lastTickAccessed)
                    {
                        for (int i = tick; i >= lastTickAccessed; i--)
                        {
                            if (base.ContainsKey(i))
                            {
                                cachedTick = i;
                                cachedValue = base[i];
                                lastTickAccessed = tick;
                                return base[i];
                            }
                        }
                        lastTickAccessed = tick;
                        return cachedValue;
                    }
                    else if (lastTickAccessed != -1 && tick < lastTickAccessed && tick >= cachedTick)
                        return cachedValue;
                    else
                    {
                        for (int i = tick; i >= FirstTick; i--)
                        {
                            if (base.ContainsKey(i))
                            {
                                cachedTick = i;
                                cachedValue = base[i];
                                lastTickAccessed = tick;
                                return base[i];
                            }
                        }
                        return default(T);
                    }
                }
                set => base[tick] = value;
            }
        }

        private const int NUMBER_OF_PLAYERS = 10;
        
        private static TickEntries<int>[] health = new TickEntries<int>[NUMBER_OF_PLAYERS];
        private static TickEntries<Tuple<double, double, double>>[] heroPosition = new TickEntries<Tuple<double, double, double>>[NUMBER_OF_PLAYERS];
        private static TickEntries<int>[] level = new TickEntries<int>[NUMBER_OF_PLAYERS];
        private static TickEntries<double>[] mana = new TickEntries<double>[NUMBER_OF_PLAYERS];
        private static TickEntries<double>[] strength = new TickEntries<double>[NUMBER_OF_PLAYERS];
        private static TickEntries<double>[] agility = new TickEntries<double>[NUMBER_OF_PLAYERS];
        private static TickEntries<double>[] intellect = new TickEntries<double>[NUMBER_OF_PLAYERS];
        private static TickEntries<double>[] maxHealth = new TickEntries<double>[NUMBER_OF_PLAYERS];
        private static TickEntries<double>[] manaRegen = new TickEntries<double>[NUMBER_OF_PLAYERS];
        private static TickEntries<double>[] healthRegen = new TickEntries<double>[NUMBER_OF_PLAYERS];
        private static TickEntries<double>[] movementSpeed = new TickEntries<double>[NUMBER_OF_PLAYERS];
        private static TickEntries<double>[] damageMin = new TickEntries<double>[NUMBER_OF_PLAYERS];
        private static TickEntries<double>[] damageMax = new TickEntries<double>[NUMBER_OF_PLAYERS];

        public HeroParser(string dataFolderLocation)
        {
            for (int i = 0; i < NUMBER_OF_PLAYERS; i++)
            {
                health[i] = new TickEntries<int>();
                heroPosition[i] = new TickEntries<Tuple<double, double, double>>();
                level[i] = new TickEntries<int>();
                mana[i] = new TickEntries<double>();
                strength[i] = new TickEntries<double>();
                agility[i] = new TickEntries<double>();
                intellect[i] = new TickEntries<double>();
                maxHealth[i] = new TickEntries<double>();
                manaRegen[i] = new TickEntries<double>();
                healthRegen[i] = new TickEntries<double>();
                movementSpeed[i] = new TickEntries<double>();
                damageMin[i] = new TickEntries<double>();
                damageMax[i] = new TickEntries<double>();
            }

            FirstTick = -1;
            foreach (string line in File.ReadLines(dataFolderLocation + "hero.txt"))
            {
                string[] words = line.Split(' ');
                int tick = Int32.Parse(words[0]);
                if (FirstTick == -1)
                {
                    FirstTick = tick;
                }

                int heroId = Int32.Parse(words[2]);

                switch (words[1])
                {
                    case "[HEALTH]":
                        health[heroId][tick] = Int32.Parse(words[3]);
                        break;

                    case "[POSITION]":
                        if (words.Length < 6)
                        {
                            throw new ArgumentOutOfRangeException("Lost position information");
                        }

                        heroPosition[heroId][tick] = Tuple.Create(Double.Parse(words[3]), Double.Parse(words[4]), Double.Parse(words[5]));
                        break;

                    case "[LEVEL]":
                        level[heroId][tick] = Int32.Parse(words[3]);
                        break;

                    case "[MANA]":
                        mana[heroId][tick] = Double.Parse(words[3]);
                        break;

                    case "[MANAREGEN]":
                        manaRegen[heroId][tick] = Double.Parse(words[3]);
                        break;

                    case "[STRENGTH]":
                        strength[heroId][tick] = Double.Parse(words[3]);
                        break;

                    case "[AGILITY]":
                        agility[heroId][tick] = Double.Parse(words[3]);
                        break;

                    case "[INTELLECT]":
                        intellect[heroId][tick] = Double.Parse(words[3]);
                        break;

                    case "[MAXHEALTH]":
                        maxHealth[heroId][tick] = Double.Parse(words[3]);
                        break;

                    case "[HEALTHREGEN]":
                        healthRegen[heroId][tick] = Double.Parse(words[3]);
                        break;

                    case "[MOVEMENTSPEED]":
                        movementSpeed[heroId][tick] = Double.Parse(words[3]);
                        break;

                    case "[DAMAGEMIN]":
                        damageMin[heroId][tick] = Double.Parse(words[3]);
                        break;

                    case "[DAMAGEMAX]":
                        damageMax[heroId][tick] = Double.Parse(words[3]);
                        break;

                    default:
                        //Console.WriteLine("Found unimplemented identifier: " + words[1]);
                        break;
                }
            }
        }

        public int getHealth(int tick, int heroID)
        {
            return health[heroID][tick];
        }

        public (double x, double y, double z) getHeroPosition(int tick, int heroID)
        {
            return (x: heroPosition[heroID][tick].Item1, y: heroPosition[heroID][tick].Item2, z: heroPosition[heroID][tick].Item3);
        }

        public int getLevel(int tick, int heroID)
        {
            return level[heroID][tick];
        }

        public double getMana(int tick, int heroID)
        {
            return mana[heroID][tick];
        }

        public double getStrength(int tick, int heroID)
        {
            return strength[heroID][tick];
        }

        public double getAgility(int tick, int heroID)
        {
            return agility[heroID][tick];
        }

        public double getIntellect(int tick, int heroID)
        {
            return intellect[heroID][tick];
        }

        public double getMaxHealth(int tick, int heroID)
        {
            return maxHealth[heroID][tick];
        }

        public double getManaRegen(int tick, int heroID)
        {
            return manaRegen[heroID][tick];
        }

        public double getHealthRegen(int tick, int heroID)
        {
            return healthRegen[heroID][tick];
        }

        public double getMovementSpeed(int tick, int heroID)
        {
            return movementSpeed[heroID][tick];
        }

        public double getDamageMin(int tick, int heroID)
        {
            return damageMin[heroID][tick];
        }

        public double getDamageMax(int tick, int heroID)
        {
            return damageMax[heroID][tick];
        }
    }
}