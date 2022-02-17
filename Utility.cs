using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emulatore_Pdp8;

namespace UtilityStuff
{
    class Utility
    {
        static public bool isBitSet(short value, int position)
        {
            return (value & (1 << position)) != 0;
        }
    }
}
