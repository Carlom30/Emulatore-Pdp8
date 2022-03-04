using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrintSpace;
using UtilityStuff;
using Emulatore_Pdp8;
using System.IO;
using System.Runtime.InteropServices;


//siccome i commenti sul pdp8 possono essere aprti ma mai chiusi, allora se trovo un "/" vado a capo boh non lo so aiuto
namespace Assembler
{
    class Compiler
    {
        private static ushort LC = 0;
        private static Dictionary<string, u12> labelTabel = new Dictionary<string, u12>();
        private static MRI[] MRIops = new MRI[]
        {
            MRI.AND,
            MRI.ADD,
            MRI.LDA,
            MRI.STA,
            MRI.BUN,
            MRI.BSA,
            MRI.ISZ
        };

        private static RRI[] RRIops = new RRI[]
        {
            RRI.CLA,
            RRI.CLE,
            RRI.CMA,
            RRI.CME,
            RRI.CIR,
            RRI.CIL,
            RRI.INC,
            RRI.SPA,
            RRI.SNA,
            RRI.SZA,
            RRI.SZE,
            RRI.HLT
        };

        private static IOI[] IOIops = new IOI[]
        {
            IOI.INP,
            IOI.OUT
        };

        public static void Compile(string[] sourceCode)
        {
            int lineCounter = 1;
            string[] source = sourceCode;

            removeSpaces(source);

            //prima passata
            string label = "";
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i].Length == 0)
                    continue;

                for (int j = 0; j < source[i].Length; j++)
                {
                    char ch = source[i][j];
                    if (ch != ',')
                    {
                        label += ch;
                    }

                    else if (ch == ',')
                    {
                        short isOp = checkIfOp(label);
                        if(isOp != -1)
                        {
                            Console.WriteLine("Line " + lineCounter + ": you can't use an instruction or pseudo-instruction as a Label");
                            return;
                        }

                        string isValid = "";
                        isValid += source[i][j + 1].ToString() + source[i][j + 2].ToString() + source[i][j + 3].ToString();
                        Console.WriteLine(isValid);
                        

                        if (isValid == "DEC" || isValid == "HEX" || (checkIfOp(isValid) != -1 && checkIfOp(isValid) != (short)PseudoInstruction.END && checkIfOp(isValid) != (short)PseudoInstruction.ORG))
                        {
                            Console.WriteLine("Label!");
                        }

                        else
                        {
                            Console.WriteLine("Line " + lineCounter + ": you can only use <DEC>, <HEX> or an istruction for a label value");
                            return;
                        }

                        //lookForLabel(source);
                    }
                    LC++;
                }
                label = "";
                lineCounter++;
            }
        }


        private static short checkIfOp(string label) //verifico se una certa label è una istruzione/pseudo-istruzione
        {
            short ret = -1; //nb nessuna istruzione ha -1 come opr (infatti -1 è 0b_1111_1111_1111_1111)

            //check for MRI
            for(int i = 0; i < MRIops.Length; i++)
            {
                if(label == MRIops[i].ToString())
                {
                    ret = (short)MRIops[i];
                    return ret;
                }
            }

            for(int i = 0; i < RRIops.Length; i++)
            {
                if (label == RRIops[i].ToString())
                {
                    ret = (short)RRIops[i];
                    return ret;
                }
            }

            for (int i = 0; i < IOIops.Length; i++)
            {
                if (label == IOIops[i].ToString())
                {
                    ret = (short)IOIops[i];
                    return ret;
                }
            }

            if(label == PseudoInstruction.ORG.ToString() || label == PseudoInstruction.END.ToString())
            {
                if (label == PseudoInstruction.ORG.ToString())
                    ret = (short)PseudoInstruction.ORG;

                else
                    ret = (short)PseudoInstruction.END;

                return ret;
            }

            return ret; 
        }
 
        private static void removeSpaces(string[] source) //public static momentaneamente
        {
            for (int i = 0; i < source.Length; i++)
            {
                //if (source[i].Length == 0)
                    //continue;
                source[i] = source[i].Replace(" ", "");
                source[i] = source[i].Replace("\t", "");
                source[i] = source[i].Replace("\r", "");
                Console.WriteLine(source[i]);
            }
        }
    }


    class instructionAssembler
    {
        public static i16 buildMRIop(MRI instruction, u12 address, addressing I = addressing.direct)
        {
            i16 reg = new i16();
            short value = 0;

            //inserisco l'opr nei bit 12, 13 e 14 di value
            short OPR = (short)instruction;
            value = (short)(Utility.setBit((ushort)value, 12, Utility.isBitSet(OPR, 0)));
            value = (short)(Utility.setBit((ushort)value, 13, Utility.isBitSet(OPR, 1)));
            value = (short)(Utility.setBit((ushort)value, 14, Utility.isBitSet(OPR, 2)));

            //setto l'indirizzamento indiretto
            value = (short)(Utility.setBit((ushort)value, 15, I == addressing.indirect));

            //inserisco l'address
            value = (short)(value | (short)(address.getValue()));

            reg.setValue(value);

            return reg;
        }

        public static i16 buildRRIop(RRI instruction)
        {
            i16 reg = new i16();
            reg.setValue((short)instruction);

            return reg;
        }

        public static i16 buildIOIop(IOI instruction)
        {
            i16 reg = new i16();
            reg.setValue((short)instruction);

            return reg;
        }
    }
}
