using System;
using System.Collections.Generic;

namespace GamingSupervisor
{
    public static class GUISelection
    {
        public enum Difficulty
        {
            novice,
            learning,
            experienced
        }

        public enum Customize
        {
            lastHit,
            heroSelection,
            itemHelper,
            laning,
            jungling,
            safeFarming
        }

        public enum GameType
        {
            replay,
            live
        }

        public static Difficulty difficulty;
        public static Dictionary<Customize, bool> customize;
        public static GameType gameType;
        public static string fileName;
        public static string heroName;
        public static string replayDataFolderLocation;

        static GUISelection()
        {
            customize = new Dictionary<Customize, bool>();
            foreach (Customize c in Enum.GetValues(typeof(Customize)))
            {
                customize.Add(c, false);
            }
        }        
    }
}
