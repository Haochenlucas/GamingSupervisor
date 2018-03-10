
namespace GamingSupervisor
{
    class OverlaySingleton
    {
        private static Overlay instance = null;
        private static readonly object instanceLock = new object();

        public static Overlay Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new Overlay();
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
