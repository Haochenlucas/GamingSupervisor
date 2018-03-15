using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Python.Runtime;

namespace replayParse
{
    public class Retreat
    {
        // data format: fh = friendly hero, eh = enemy hero
        // data   [ fh_id, fh_hp, fh_mana, fh_lvl, fh_attack_damage, fh_ms , eh_id, eh_hp, eh_mana, eh_lvl, eh_attack_damage, eh_ms ] 
        // target [ -1 if fh dies in 5 seconds, 0 if neither, 1 if eh dies in 5 seconds ]
        
        public Retreat(List<int> currTickStat)
        {
            using (Py.GIL())
            {
                // TODO: http://scikit-learn.org/stable/modules/model_persistence.html
                dynamic sknn = Py.Import("sklearn.neighbors");
                dynamic nn = sknn.KNeighborsClassifier(3);

            }
        }
       

}
}
