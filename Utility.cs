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
        static public ushort setBit(ushort value, int pos, bool bit) //this thing is bugged lmao
        {
            int bitValue = bit ? 1 : 0;
            ushort one = 1;
            one = (ushort)(one & (1 << pos));
            if ((value & (1 << pos)) != bitValue)
            {
                if(bitValue == 0)
                    value = (ushort)(value & (~one));

                else
                    value = (ushort)(value | (one));
            }
                //value = (ushort)(~(value & (1 << pos)));


            return value;
        }

        static public bool isBitSet(int value, int position)
            => (value & (1 << position)) != 0; //:)
        

        static public string valueToBin(int value, RegType type)
        {
            string toBin = "";

            for (int i = (int)type - 1; i >= 0; i--)
                toBin += Utility.isBitSet(value, i) ? 1 : 0;

            return toBin;
        }
    }
}
