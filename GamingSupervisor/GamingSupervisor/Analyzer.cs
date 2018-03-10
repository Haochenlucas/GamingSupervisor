using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamingSupervisor
{
    abstract class Analyzer
    {
        protected Overlay overlay;

        public Analyzer()
        {
            Terminate = false;
        }

        public abstract void Start();

        private static readonly object terminateLock = new object();
        private bool terminate;
        public bool Terminate
        {
            get { lock (terminateLock) { return terminate; } }
            set { lock (terminateLock) { terminate = value; } }
        }
    }
}
