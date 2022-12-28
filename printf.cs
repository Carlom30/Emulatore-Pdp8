using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emulatore_Pdp8;
using UtilityStuff;
using Assembler;
using System.Windows.Forms;

namespace PrintSpace
{
    //singleton per "puntare" al banco di memoria dove si intenderebbe posizionare un break point
    
    public sealed class TerminalBuffer : System.Threading.WaitHandle //tenatvo di implementazione mutex
    {
        public enum BufferType
        {
            ram = 1,
            log = 2,
            registers = 3
        };

        static string[] ramBuffer;
        static List<string> logBuffer;
        static string[] registersBuffer;

        static public bool dataAreBusy;
        static string[] getLogBuffer()
            => logBuffer.ToArray();

        static string[] getRamBuffer()
            => ramBuffer;

        static string[] getRegisterBuffer()
            => registersBuffer;

        public static string[] getAccessToData(BufferType type)
        {
                        
            if (type == BufferType.ram)
                return getRamBuffer();

            else if (type == BufferType.log)
                return getLogBuffer();

            else
                return getRegisterBuffer();

        }
    }
    class Printf
    {
        static string[] ramBuffer;
        static List<string> logBuffer;
        static string[] registersBuffer;

        public static void inizializeBuffers()
        {
            ramBuffer = new string[4096];
            logBuffer = new List<string>();
            registersBuffer = new string[10];
        }

        public static string[] getLogBuffer()
            => logBuffer.ToArray();

        public static string[] getRamBuffer()
            => ramBuffer;

        public static string[] getRegisterBuffer() 
            => registersBuffer;

        public static void printLogOnBuffer(string log)
        {
            logBuffer.Add(log);
        }
        public static void printRegisterOnBuffer(string[] registers)
        {
            registersBuffer = registers;
        }
        static public void printRamOnBuffer(i16[] ram, u12 programAddres)
        {
            string pointer = "";
            string[] ramToString = new string[ram.Length];
            int k = 0;
            for (int i = programAddres.getValue(); i < ram.Length; i++)
            {
                if(Program.getVm().pointer.checkIfMemoryPointed(new u12((ushort)i)))
                {
                    pointer = "> ";
                }

                else if(Program.getVm().breakPoints.Contains(new u12((ushort)i)))
                {
                    pointer = "* ";
                }

                string toBin = Convert.ToString(ram[i].getValue(), 2);
                ramBuffer[k] = (pointer + Convert.ToString(i, 16) + "\t" + ram[i].getValue() + "\t" + Utility.valueToBin(ram[i].getValue(), RegType.bit16_reg) + "\t" + ram[i].getLabel() + "\t" + ram[i].getInstruction());
                k++;
                pointer = "";
            } 
        }
        public static void printLog(TextBox LOG)
        {
            string[] logBuffer = Printf.getLogBuffer();
            for (int i = 0; i < logBuffer.Length; i++)
            {
                LOG.AppendText(logBuffer[i] + Environment.NewLine);
            }
        }

        static public void printRegisters(pdp8 vm)
        {

            string[] registers =
            {
                " MBR\t" + vm.mbr.getValue() + "\t" + Utility.valueToBin(vm.mbr.getValue(), RegType.bit16_reg),
                " A\t" + vm.a.getValue() + "\t" + Utility.valueToBin(vm.a.getValue(), RegType.bit16_reg),
                " MAR\t" + vm.mar.getValue() + "\t" + Utility.valueToBin(vm.mar.getValue(), RegType.bit12_reg),
                " PC\t" + vm.pc.getValue() + "\t" + Utility.valueToBin(vm.pc.getValue(), RegType.bit12_reg),
                " S\t" + (vm.s ? 1 : 0),
                " F\t" + (vm.f ? 1 : 0),
                " R\t" + (vm.r ? 1 : 0),
                " I\t" + (vm.i ? 1 : 0),
                " E\t" + (vm.e ? 1 : 0),
                " OPR\t" + Utility.valueToBin(vm.opr, RegType.bit3_reg)
            };

            printRegisterOnBuffer(registers);
        }

        //necessito di una funzione che faccia print dall'i-esimo elemento della ram, al j-esimo
        static public void printSpecRam(int start, int end, i16[] ram)
        {
            if(start < 0 || start > 4096 || end < 0 || end > 4096)
            {
                printLogOnBuffer("range of memory out of bounds");
                return;
            }

            for(int i = start; i <= end; i++)
            {
                Console.Write(" > register: " + Convert.ToString(i, 16) + " DEC: " + ram[i].getValue() + " BIN: " + Utility.valueToBin(ram[i].getValue(), RegType.bit16_reg));
                Console.Write("\n");
            }
            return;
        }

        public static void printCompileErr(int line, string logToPrint)
        {
            string log = "Line " + line + ": " + logToPrint;
            Console.WriteLine(log);
            printLogOnBuffer(log);
        }
    }
}
