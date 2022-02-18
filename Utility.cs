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
        /*il problema principali con entrambe le funzioni e il type del registro. 
          non tanto perché uno è da 12 e uno da 16, ma più che altro perché quello da 12 è unsigned(e così deve essere)
          e quello da 16 è signed (as well) quindi per adesso faccio dei check, ma farò delle ricerche per capire se
          esiste una soluzione più utile e elegante :))
          inoltre devo ancora capire quanto è utile migliorare questo codice*/
        static public bool isBitSet(short sValue, ushort uValue, int position, RegType type)
        {
            if(type == RegType.bit16_reg)
                return (sValue & (1 << position)) != 0;
            return (uValue & (1 << position)) != 0;
        }

        static public string valueToBin(short sValue, ushort uValue, RegType type)
        {
            string toBin = "";
            if (type == RegType.bit16_reg)
            {
                for (int i = (int)type - 1; i >= 0; i--)
                {
                    toBin += Utility.isBitSet(sValue, 0, i, RegType.bit16_reg) ? 1 : 0;
                    //Console.Write(Utility.isBitSet(value, i) ? 1 : 0);
                }
            }

            if (type == RegType.bit12_reg)
            {
                for (int i = (int)type - 1; i >= 0; i--)
                {
                    toBin += Utility.isBitSet(0, uValue, i, RegType.bit12_reg) ? 1 : 0;
                    //Console.Write(Utility.isBitSet(value, i) ? 1 : 0);
                }
            }
            return toBin;
        }
    }
}
