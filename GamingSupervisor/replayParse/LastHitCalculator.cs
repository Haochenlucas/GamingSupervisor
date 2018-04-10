using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace replayParse
{
    public class LastHitCalculator
    {
        public LastHitCalculator()
        {

        }

        private double ArmorCalculation(double armor)
        {
            return 1 - (.05 * armor / (1 + .05 * Math.Abs(armor)));
        }

        // use min baseAtk here to ensure we get the last hit
        public bool CanLastHit(double time, double baseAtk, double primaryAtr, double percentBonus, double flatBonus, double multi, double armor, double hpLeft)
        {
            int levelUp = Convert.ToInt32(Math.Floor(time / 450));
            double damage = ((baseAtk + primaryAtr) * (1 + percentBonus) + flatBonus) * multi * ArmorCalculation(armor);

            return damage > hpLeft;
        }

        public bool CanLastHit(double damage, double hpLeft)
        {
            return damage > hpLeft;
        }
    }
}
