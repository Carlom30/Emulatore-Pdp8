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
        /*il problema principale con entrambe le funzioni e il type del registro. 
          non tanto perché uno è da 12 e uno da 16, ma più che altro perché quello da 12 è unsigned(e così deve essere)
          e quello da 16 è signed (as well) quindi per adesso faccio dei check, ma farò delle ricerche per capire se
          esiste una soluzione più utile e elegante :))
          inoltre devo ancora capire quanto è utile migliorare questo codice*/

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
