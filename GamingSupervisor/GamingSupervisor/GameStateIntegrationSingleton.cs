namespace GamingSupervisor
{
    class GameStateIntegrationSingleton
    {
        private static GameStateIntegration instance = null;
        private static readonly object instanceLock = new object();

        public static GameStateIntegration Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new GameStateIntegration();
                    }
                    return instance;
                }
            }
        }

        public static void Reset() // Needed in the event client exits
        {
            instance = null;
        }
    }
}
