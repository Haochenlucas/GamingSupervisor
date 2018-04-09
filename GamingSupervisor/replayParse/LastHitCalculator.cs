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

        public bool CanLastHit(double baseAtk, double primaryAtr, double armor, double hpLeft)
        {
            double damage = (baseAtk + primaryAtr) * ArmorCalculation(armor);

            return damage > hpLeft;
        }

        public bool CanLastHit(double damage, double hpLeft)
        {
            return damage > hpLeft;
        }

        public string GetPrimaryAttribute(int heroID)
        {
            return primaryAttributes[heroID];
        }

        private Dictionary<int, string> primaryAttributes = new Dictionary<int, string>
        {
            {1  , "Strength"    },
            { 2  , "Strength"    },
            { 3  , "Intelligence"},
            { 4  , "Agility"     },
            { 5  , "Agility"     },
            { 6  , "Strength"    },
            { 7  , "Intelligence"},
            { 8  , "Intelligence"},
            { 9  , "Strength"    },
            { 10 , "Agility"     },
            { 11 , "Agility"     },
            { 12 , "Strength"    },
            { 13 , "Strength"    },
            { 14 , "Agility"     },
            { 15 , "Strength"    },
            {16 , "Strength"    },
            {17 , "Intelligence"},
            {18 , "Agility"     },
            {19 , "Strength"    },
            {20 , "Intelligence"},
            {21 , "Intelligence"},
            {22 , "Intelligence"},
            {23 , "Intelligence"},
            {24 , "Intelligence"},
            {25 , "Intelligence"},
            {26 , "Strength"    },
            {27 , "Strength"    },
            {28 , "Agility"     },
            {29 , "Strength"    },
            {30 , "Strength"    },
            {31 , "Strength"    },
            {32 , "Agility"     },
            {33 , "Intelligence"},
            {34 , "Intelligence"},
            {35 , "Agility"     },
            {36 , "Agility"     },
            {37 , "Strength"    },
            {38 , "Intelligence"},
            {39 , "Strength"    },
            {40 , "Intelligence"},
            {41 , "Agility"     },
            {42 , "Intelligence"},
            {43 , "Strength"    },
            {44 , "Strength"    },
            {45 , "Intelligence"},
            {46 , "Intelligence"},
            {47 , "Strength"    },
            {48 , "Intelligence"},
            {49 , "Intelligence"},
            {50 , "Agility"     },
            {51 , "Agility"     },
            {52 , "Strength"    },
            {53 , "Strength"    },
            {54 , "Agility"     },
            {55 , "Agility"     },
            {56 , "Agility"     },
            {57 , "Agility"     },
            {58 , "Agility"     },
            {59 , "Agility"     },
            {60 , "Intelligence"},
            {61 , "Intelligence"},
            {62 , "Strength"    },
            {63 , "Agility"     },
            {64 , "Intelligence"},
            {65 , "Strength"    },
            {66 , "Intelligence"},
            {67 , "Intelligence"},
            {68 , "Agility"     },
            {69 , "Agility"     },
            {70 , "Agility"     },
            {71 , "Strength"    },
            {72 , "Intelligence"},
            {73 , "Strength"    },
            {74 , "Intelligence"},
            {75 , "Intelligence"},
            {76 , "Agility"     },
            {77 , "Agility"     },
            {78 , "Intelligence"},
            {79 , "Strength"    },
            {80 , "Intelligence"},
            {81 , "Agility"     },
            {82 , "Intelligence"},
            {83 , "Intelligence"},
            {84 , "Intelligence"},
            {85 , "Strength"    },
            {86 , "Agility"     },
            {87 , "Agility"     },
            {88 , "Agility"     },
            {89 , "Strength"    },
            {90 , "Intelligence"},
            {91 , "Strength"    },
            {92 , "Intelligence"},
            {93 , "Agility"     },
            {94 , "Agility"     },
            {95 , "Strength"    },
            {96 , "Strength"    },
            {97 , "Intelligence"},
            {98 , "Strength"    },
            {99 , "Strength"    },
            {100, "Agility"     },
            {101, "Strength"    },
            {102, "Strength"    },
            {103, "Strength"    },
            {104, "Agility"     },
            {105, "Agility"     },
            {106, "Agility"     },
            {107, "Agility"     },
            {108, "Intelligence"},
            {109, "Intelligence"},
            {110, "Agility"     },
            {111, "Intelligence"},
            {112, "Intelligence"},
            {113, "Intelligence"},
            {114, "Strength"    },
            {115, "Intelligence"},
        };
    }
}
