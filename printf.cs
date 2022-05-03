using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emulatore_Pdp8;
using UtilityStuff;
using Assembler;

namespace PrintSpace
{
    class Printf
    {
        static string[] ramBuffer;
        static List<string> logBuffer;

        public static void inizializeBuffers()
        {
            ramBuffer = new string[4096];
            logBuffer = new List<string>();
        }

        public static string[] getLogBuffer()
        {
            return logBuffer.ToArray();
        }

        public static string[] getRamBuffer()
        {
            return ramBuffer;
        }

        public static void printOnLog(string log)
        {
            logBuffer.Add(log);
        }


        static public void printRamOnBuffer(CompilerData compilationData)
        {
            i16[] ram = compilationData.machineCode;
            string[] ramToString = new string[ram.Length];
            for (int i = 0; i < ram.Length; i++)
            {
                string toBin = Convert.ToString(ram[i].getValue(), 2);
                ramBuffer[i] = ("Register:\t" + Convert.ToString(i, 16) + "\tDEC:\t" + ram[i].getValue() + "\tBIN:  " + Utility.valueToBin(ram[i].getValue(), RegType.bit16_reg) + "   INSTR:   " + ram[i].getInstruction());
            } 
        }

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
            Console.WriteLine("");
            Console.WriteLine("MBR: DEC " + vm.mbr.getValue() + " BIN " + Utility.valueToBin(vm.mbr.getValue(), RegType.bit16_reg));
            Console.WriteLine("A: DEC " + vm.a.getValue() + " BIN " + Utility.valueToBin(vm.a.getValue(), RegType.bit16_reg));
            Console.WriteLine("MAR: DEC " + vm.mar.getValue() + " BIN " + Utility.valueToBin(vm.mar.getValue(), RegType.bit12_reg));
            Console.WriteLine("PC: DEC " + vm.pc.getValue() + " BIN " + Utility.valueToBin(vm.pc.getValue(), RegType.bit12_reg));
            Console.WriteLine("S: " + (vm.s ? 1 : 0) + " F: " + (vm.f ? 1 : 0) + " R: " + (vm.r ? 1 : 0) + " I: " + (vm.i ? 1 : 0) + " E: " + (vm.e ? 1 : 0));
            Console.WriteLine("OPR: " + Utility.valueToBin(vm.opr, RegType.bit3_reg));
            Console.WriteLine("");
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
            string log = "Line " + line + ": " + logToPrint;
            Console.WriteLine(log);
            logBuffer.Add(log);
        }
    }
}
