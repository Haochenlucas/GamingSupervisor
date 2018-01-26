using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamingSupervisor
{
    public class GUISelection
    {
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
        
        public Dictionary<Customize, bool> customize;
        public GameType gameType;
        public string fileName;
        public string heroName;

        public GUISelection()
        {
            customize = new Dictionary<Customize, bool>();
            foreach (Customize c in Enum.GetValues(typeof(Customize)))
            {
                customize.Add(c, false);
            }
        }        
    }
}
