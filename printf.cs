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
            Console.WriteLine("MBR: DEC " + vm.mbr.getValue() + " BIN " + Utility.valueToBin(vm.mbr.getValue(), RegType.bit16_reg));
            Console.WriteLine("A: DEC " + vm.a.getValue() + " BIN " + Utility.valueToBin(vm.a.getValue(), RegType.bit16_reg));
            Console.WriteLine("MAR: DEC " + vm.mar.getValue() + " BIN " + Utility.valueToBin(vm.mar.getValue(), RegType.bit12_reg));
            Console.WriteLine("PC: DEC " + vm.pc.getValue() + " BIN " + Utility.valueToBin(vm.pc.getValue(), RegType.bit12_reg));
            Console.WriteLine("S: " + (vm.s ? 1 : 0) + " F: " + (vm.f ? 1 : 0) + " R: " + (vm.r ? 1 : 0) + " I: " + (vm.i ? 1 : 0) + " E: " + (vm.e ? 1 : 0));
            Console.WriteLine("OPR: " + Utility.valueToBin(vm.opr, RegType.bit3_reg));
        }

        //necessito di una funzione che faccia print dall'i-esimo elemento della ram, al j-esimo
        static public void printSpecRam(int start, int end, i16[] ram)
        {
            for(int i = start; i <= end; i++)
            {
                Console.Write("register: " + Convert.ToString(i, 16) + " DEC: " + ram[i].getValue() + " BIN: " + Utility.valueToBin(ram[i].getValue(), RegType.bit16_reg));
                Console.Write("\n");
            }
            return;
        }

        public static void printCompileErr(int line, string logToPrint)
        {
            Console.WriteLine("Line " + line + ": " + logToPrint);
        }
    }
}
