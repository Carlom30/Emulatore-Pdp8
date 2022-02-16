using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emulatore_Pdp8;

namespace print
{
    enum Base
    {
        bin,
        dec,
        hex
    };


    class printf
    {
        static public void printRam(Base basePrint, i16[] ram)
        {
            for(int i = 0; i < ram.Length; i++)
            {
                if(basePrint == Base.dec)
                {
                    Console.WriteLine(ram[i].getValue());
                }

                else if(basePrint == Base.bin)
                {
                    string toBin = Convert.ToString(ram[i].getValue(), 2);
                    Console.WriteLine(toBin);
                }
            }

            return;
        }


    }
}
