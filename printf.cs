using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emulatore_Pdp8;
using UtilityStuff;

namespace PrintSpace
{
    class Printf
    {

        static public void printRam(i16[] ram)
        {
            for(int i = 0; i < ram.Length; i++)
            {
                string toBin = Convert.ToString(ram[i].getValue(), 2);
                Console.Write("register: " + Convert.ToString(i, 16) + " DEC: " + ram[i].getValue() + " BIN: " + Utility.valueToBin(ram[i].getValue(), RegType.bit16_reg));
                Console.Write("\n");
                //Console.WriteLine(toBin);
            }

            return;
        }

        static public void printRegisters(pdp8 vm)
        {
            Console.WriteLine("MBR: DEC " + vm.mbr.getValue() + " BIN " + Convert.ToString(vm.mbr.getValue(), 2));
            Console.WriteLine("A: DEC " + vm.a.getValue() + " BIN " + Convert.ToString(vm.a.getValue(), 2));
            Console.WriteLine("MAR: DEC " + vm.mar.getValue() + " BIN " + Convert.ToString(vm.mar.getValue(), 2));
            Console.WriteLine("PC: DEC " + vm.pc.getValue() + " BIN " + Convert.ToString(vm.pc.getValue(), 2));
            Console.WriteLine("S: " + (vm.s ? 1 : 0) + " F: " + (vm.f ? 1 : 0) + " R: " + (vm.r ? 1 : 0) + " I: " + (vm.i ? 1 : 0));
            Console.WriteLine("OPR: " + Utility.valueToBin(vm.opr, RegType.bit16_reg));
        }
    }
}
