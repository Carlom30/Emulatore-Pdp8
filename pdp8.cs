using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PrintSpace;
using UtilityStuff;
using Assembler;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;


namespace Emulatore_Pdp8
{
    enum RegType
    {
        bit16_reg = 16,
        bit12_reg = 12,
        bit3_reg = 3
    }

    enum addressing
    {
        direct = 0,
        indirect = 1
    }

    enum PseudoInstruction
    {
        ORG = -2,
        END = -3,
    }

    enum MRI //memory reference instruction
    {
        AND = 0b_000,
        ADD = 0b_001,
        LDA = 0b_010,
        STA = 0b_011,
        BUN = 0b_100,
        BSA = 0b_101, // the cursed instruction
        ISZ = 0b_110,

        NOT_INSTR = -1
    }

    enum RRI //register reference instruction
    {
        CLA = 0b_0111_1000_0000_0000, // azzera l'accumulatore
        CLE = 0b_0111_0100_0000_0000, // azzera il registro E
        CMA = 0b_0111_0010_0000_0000, // complementa ad uno l'accumulatore
        CME = 0b_0111_0001_0000_0000, // complementa E
        CIR = 0b_0111_0000_1000_0000, // circola a destra E-AC
        CIL = 0b_0111_0000_0100_0000, // circola a sinistra E-AC
        INC = 0b_0111_0000_0010_0000, // incrementa di 1 l'accumulatore
        SPA = 0b_0111_0000_0001_0000, // skippa la prossima istruzione se l'accumulatore è positivo (non uguale a 0)
        SNA = 0b_0111_0000_0000_1000, // skippa la prossima istruzione se l'accumulatore è negativo (non uguale a 0)
        SZA = 0b_0111_0000_0000_0100, // skippa la prossima istruzione se l'accumulatore è uguale a 0
        SZE = 0b_0111_0000_0000_0010, // skippa la prossima istruzione se E è uguale a 0
        HLT = 0b_0111_0000_0000_0001, // spenge la macchina (o termina il programma nel caso della vm)

        NOT_INSTR = -1
    }

    enum IOI // I/O instruction set
    {
        INP = 0b_1111_1000_0000_0000, 
        OUT = 0b_1111_0100_0000_0000,

        NOT_IOIISTR = -1
    }

    enum pseudoOPs
    {
        ORG,
        END,
        DEC,
        HEX
    }

    static class Program
    {
        public static string[] source;

        //singleton vm
        private static pdp8 vm;
        static int nSteps = -1; //se -1 allora non ci sono step da compiere
        public static Form1 form;

        public static int getnSteps()
        {
            return nSteps;
        }

        public static void setnSteps(int n)
        {
            nSteps = n;
        }


        public static pdp8 getVm()
        {
            return vm;
        }

        public static void inizializeVM()
        {
            vm = new pdp8();
        }

        public static void setVMParameters(i16[] ram, u12 pc)
        {
            vm.ram = ram;
            vm.pc.setValue(pc.getValue());
            vm.pointer.pointedAddress = pc;
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        
        public static string[] readSource(OpenFileDialog sourceFile)
        {

            try
            {
                source = File.ReadAllLines(sourceFile.FileName);
            }
            catch
            {
                Printf.printLogOnBuffer("no File Selected");
                return null;
            }

            if (source.Length == 0)
            {
                waitForClose("Source can't be empty");
                return null;
            }

            return source;
        }
        
        [STAThread]
        static void Main()
        {
            vm = new pdp8();
            
            Printf.inizializeBuffers();
            //vm.addressToMAR();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            form = new Form1();
            Application.Run(form);

            //vm.run(); //not so much to say
           
        }

        public static void waitForClose(string log)
        {
            Console.WriteLine("\n" + log);
            Console.ReadKey();
        }
    }

    struct i16 
    {
        private short value;
        private string instruction;
        private string label;

        public i16(short firstValue)
        {
            value = 0;
            instruction = "";
            label = "";
            setValue(firstValue);
            
        }

 
        public short getValue()
        {
            return value;
        }

        public void setValue(short newValue)
        {
            value = newValue;
        }

        public string getInstruction()
        {
            return instruction;
        }

        public void setInstruction(string instr)
        {
            instruction = instr;
        }

        public void setLabel(string lab)
        {
            label = lab;
        }

        public string getLabel()
            => label;
    }

    struct u12 //registro unsigned poiché riferito a soli indirizzi (che quindi non saranno mai negativi)
    {
        private ushort _value;

        public u12(ushort firstValue)
        {
            _value = 0;
            setValue(firstValue);
        }

        public ushort getValue()
        {
            return (ushort)(_value & ((ushort)0b_0000_1111_1111_1111));
        }

        public void setValue(ushort newValue)
        {
            if(newValue >= (1 << 12))
            {
                Console.WriteLine("overflow!\n");
                return; //crash probabilmente ma tanto non accadrà mai (almeno credo)
            }

            _value = newValue;
        }

        public void increment()
        {
            _value++;
        }

    }

    class Pointer
    {
        char printedPointer;
        //i16 pointedMemory;
        public u12 pointedAddress;
        int BPlimit = 10;
        pdp8 vm;

        public Pointer(pdp8 newvm)
        {
            printedPointer = '>';
            //pointedMemory = Program.getVm().ram[0x100]; //il pointer punta sempre al primo i16, poi cambia con la compilazione
            pointedAddress = new u12(0x0);
            this.vm = newvm;
        }

        public bool checkIfMemoryPointed(u12 address)
        {
            if(pointedAddress.getValue() == address.getValue())
            {
                return true;
            }

            return false;
        }

        public void changePointedMemory(bool isPositive)
        {
            int one = isPositive ? 1 : -1;
            pointedAddress.setValue((ushort)(pointedAddress.getValue() + one));
        }

        public void registerBreakPoint()
        {
            if(vm.breakPoints.Count >= BPlimit)
            {
                goto end;
            }

            //the one piece is real

            if(vm.breakPoints.Contains(pointedAddress))
            {
                vm.breakPoints.Remove(pointedAddress);
                goto end;
            }

            vm.breakPoints.Add(pointedAddress);
            
            end: //just committing war crimes
                return;

        }
    }

    class pdp8
    {
        public Pointer pointer;
        public List<u12> breakPoints; //breaking bad
        public bool BPstop = false;

        //definizione contenuto della macchina e funzioni istruzioni
        public i16[] ram; //insieme di tutti i registri (eccetto quelli sotto)
        public i16 mbr; // memory buffer register
        public i16 a; //accumulatore

        public u12 mar; //memory address register
        public u12 pc; //program counter


        public byte opr; //3 bit per il codice operazione

        public bool i; //indirizzamento indiretto
        public bool e; //registro di riporto per l'accumulatore

        //insieme dei flip-flop per tipo di indirizzamento
        public bool s; //questo registro indica se la macchina è accesa o spenta :)
        public bool f;
        public bool r;

        public bool isRunning; 

        public pdp8() //ovviamente non prende parametri XD
        {
            this.breakPoints = new List<u12>();
            this.ram = new i16[4096];
            this.mbr = new i16();
            this.a = new i16();

            this.mar = new u12(0);
            this.pc = new u12(0);

            this.opr = 0;

            this.e = false;

            this.s = true;
            this.f = false;
            this.r = false;

            this.isRunning = false;
            this.pointer = new Pointer(this);

            return;
        }

        public void addressToMAR()
        {
            /*devo trasferire i primi 12 bit dell'MBR nel'MAR, essendo un'azione che accade in molte funzioni,
              ne faccio una funzione specifica*/
            ushort newMAR = 0;
            for(int i = 0; i < (int)RegType.bit12_reg; i++)
            {
                newMAR = Utility.setBit(newMAR, i, Utility.isBitSet(mbr.getValue(), i));
            }
            mar.setValue(newMAR);
            //newMAR = (ushort)(mbr.getValue() & 0b_0000_1111_1111_1111);
            //mar.setValue(newMAR);
            //works
        }

        public u12 getAddressFromMBR()
        {
            u12 ad = new u12(0);
            for (int i = 0; i < (int)RegType.bit12_reg; i++)
            {
                ad.setValue(Utility.setBit(ad.getValue(), i, Utility.isBitSet(mbr.getValue(), i)));
            }

            return ad;
        }

        public void fetch()
        {
            mar.setValue(pc.getValue());
            mbr.setValue(ram[mar.getValue()].getValue());
            pc.increment();

            opr = 0;
            opr = (byte)Utility.setBit(opr, 0, Utility.isBitSet(mbr.getValue(), 12));
            opr = (byte)Utility.setBit(opr, 1, Utility.isBitSet(mbr.getValue(), 13));
            opr = (byte)Utility.setBit(opr, 2, Utility.isBitSet(mbr.getValue(), 14));
            //setto opr con i bit 12,13,14 di mbr 

            i = Utility.isBitSet(mbr.getValue(), 15); //l'ultima posizione di 16 bit è la quindicesima (15, 1111);

            if (i && opr != 0b_111)
                r = true; //passa ciclo di indirizzamento indiretto

            else
                f = true; // passa ciclo di execute

            return;
        }

        public void indirectAddressing()
        {
            addressToMAR();                               // MAR <- MBR(AD)
            mbr.setValue(ram[mar.getValue()].getValue()); // MBR <- M
 
            //passaggio ad execute
            f = true;
            r = false;
            return;
        }

        public void execute()
        {
            /*NB: è importante che il pdp8 riconosca le operazioni basandosi SOLAMENTE su opr, o comunque sui bit, 
              in quando è possibile, tramite manipolazione dei bit, modificare, a run time, le operazioni effettuate 
              dalla macchina (cosa ad oggi estremamente proibita XD)*/
            if (opr != 0b_111)
            {
                Printf.printLogOnBuffer("found MRI ops");
                Printf.printLogOnBuffer("execute ");
                switch (opr)
                {
                    case (byte)MRI.AND:
                        Printf.printLogOnBuffer(MRI.AND.ToString());
                        AND();
                        break;

                    case (byte)MRI.ADD:
                        Printf.printLogOnBuffer(MRI.ADD.ToString());
                        ADD();
                        break;

                    case (byte)MRI.LDA:
                        Printf.printLogOnBuffer(MRI.LDA.ToString());
                        LDA();
                        break;

                    case (byte)MRI.STA:
                        Printf.printLogOnBuffer(MRI.STA.ToString());
                        STA();
                        break;

                    case (byte)MRI.BUN:
                        Printf.printLogOnBuffer(MRI.BUN.ToString());
                        BUN();
                        break;

                    case (byte)MRI.BSA:
                        Printf.printLogOnBuffer(MRI.BSA.ToString());
                        BSA();
                        break;

                    case (byte)MRI.ISZ:
                        Printf.printLogOnBuffer(MRI.ISZ.ToString());
                        ISZ();
                        break;

                    default:
                        Printf.printLogOnBuffer("is this a label?");
                        f = false;
                        r = false;
                        break;
                }
            }

            if(opr == 0b_111 && !i)
            {
                Printf.printLogOnBuffer("found RRI ops");
                Printf.printLogOnBuffer("execute ");
                switch (ram[mar.getValue()].getValue())
                {
                    case (short)RRI.CLA:
                        Printf.printLogOnBuffer(RRI.CLA.ToString());
                        CLA();
                        break;

                    case (short)RRI.CLE:
                        Printf.printLogOnBuffer(RRI.CLE.ToString());
                        CLE();
                        break;

                    case (short)RRI.CMA:
                        Printf.printLogOnBuffer(RRI.CMA.ToString());
                        CMA();
                        break;

                    case (short)RRI.CME:
                        Printf.printLogOnBuffer(RRI.CME.ToString());
                        CME();
                        break;

                    case (short)RRI.CIR:
                        Printf.printLogOnBuffer(RRI.CIR.ToString());
                        CIR();
                        break;

                    case (short)RRI.CIL:
                        Printf.printLogOnBuffer(RRI.CIL.ToString());
                        CIL();
                        break;

                    case (short)RRI.INC:
                        Printf.printLogOnBuffer(RRI.INC.ToString());
                        INC();
                        break;

                    case (short)RRI.SPA:
                        Printf.printLogOnBuffer(RRI.SPA.ToString());
                        SPA();
                        break;

                    case (short)RRI.SNA:
                        Printf.printLogOnBuffer(RRI.SNA.ToString());
                        SNA();
                        break;

                    case (short)RRI.SZA:
                        Printf.printLogOnBuffer(RRI.SZA.ToString());
                        SZA();
                        break;

                    case (short)RRI.SZE:
                        Printf.printLogOnBuffer(RRI.SZE.ToString());
                        SZE();
                        break;

                    case (short)RRI.HLT:
                        Printf.printLogOnBuffer(RRI.HLT.ToString());
                        HLT();
                        break;

                    default:
                        Printf.printLogOnBuffer("is this a label?");
                        f = false;
                        r = false;
                        break;
                }

            }

            if(opr == 0b_111 && i)
            {
                Printf.printLogOnBuffer("found I/O ops");
                Printf.printLogOnBuffer("execute ");
                switch (ram[mar.getValue()].getValue())
                {
                    case (unchecked((short)IOI.INP)):
                        Printf.printLogOnBuffer(IOI.INP.ToString());
                        INP();
                        break;

                    case (unchecked((short)IOI.OUT)):
                        Printf.printLogOnBuffer(IOI.OUT.ToString());
                        OUT();
                        break;

                    default:
                        Printf.printLogOnBuffer("is this a label?");
                        f = false;
                        r = false;
                        break;
                }
            }
            Printf.printRegisters(this);
            //altra roba ovviamente, just testing
            return;
        }

        public void AND() // AND con AC
        {
            Printf.printLogOnBuffer("MAR <- MBR(AD)");
            addressToMAR();

            Printf.printLogOnBuffer("MBR <- M");
            mbr.setValue(ram[mar.getValue()].getValue());

            Printf.printLogOnBuffer("AC <- AC & MBR");
            a.setValue((short)(a.getValue() & mbr.getValue()));

            f = false;
            return;
        }

        public void ADD() // ADD con AC
        {
            Printf.printLogOnBuffer("MAR <- MBR(AD)");
            addressToMAR();

            Printf.printLogOnBuffer("MBR <- M");
            mbr.setValue(ram[mar.getValue()].getValue());

            Printf.printLogOnBuffer("EAC <- AC + MBR");
            short c = (short)(a.getValue() + mbr.getValue());
            e = ((a.getValue() ^ mbr.getValue()) >= 0) & ((a.getValue() ^ c) < 0); //tested (works)

            /*Cosa accade sopra: 
              il registro "E" è il riporto della somma tra ac e mbr, nel caso ci sia overflow
              il registro "E" deve essere settato ad 1, quindi verifico che ci sia overflow, prima di fare la somma.
              Se lo xor bit a bit tra i due addendi è positivo e lo xor bit a bit tra il primo addendo e la somma tra 
              i due addendi è minore di zero, allora c'è overflow, quindi l'and bit a bit tra 1 & 1 setta "E" a 1
              altrimenti a 0 (in quel caso non si ha overflow)

              link: https://stackoverflow.com/questions/32021741/integer-overflow-detection-c-sharp-for-add */

            a.setValue((short)(a.getValue() + mbr.getValue()));

            f = false;
            return;
        }

        public void LDA() // carica in AC
        {
            Printf.printLogOnBuffer("MAR <- MBR(AD)");
            addressToMAR();

            Printf.printLogOnBuffer("MBR <- M, AC <- 0");
            mbr.setValue(ram[mar.getValue()].getValue());
            a.setValue(0);

            Printf.printLogOnBuffer("AC <- AC + MBR");
            a.setValue((short)(a.getValue() + mbr.getValue()));

            f = false;
            return;
        }

        public void STA() // memorizza AC
        {
            Printf.printLogOnBuffer("MAR <- MBR(AD)");
            addressToMAR();

            Printf.printLogOnBuffer("MBR <- AC");
            mbr.setValue(a.getValue());

            Printf.printLogOnBuffer("M <- MBR");
            ram[mar.getValue()].setValue(mbr.getValue());

            f = false;
            return;
        }

        public void BUN() // salto incondizionato
        {
            Printf.printLogOnBuffer("PC <- MBR(AD)");
            pc.setValue(getAddressFromMBR().getValue());

            f = false;
            return;
        }

        public void BSA() // salto con memorizzazione dell'indirizzo di ritorno
        {
            Printf.printLogOnBuffer("MAR <- MBR(AD), MBR(AD) <- PC, PC <- MBR(AD)");
            addressToMAR();

            //update: Il professor Navarra mi ha informato che la bsa ha come effetto collaterale quello di trasferire oltre al pc anche l'opr
            short tmp = (short)(mbr.getValue() & 0b_1111_0000_0000_0000);
            tmp = (short)(tmp | (short)pc.getValue());

            /*
             supponiamo che mbr sia 0b_1001_0000_0000_0001 e pc 0000_0000_1010
             vogliamo ora quindi cambiare la sua parte address, ovvero i sui 12 bit meno significativi, inserendo al loro posto il pc, invariando però i 4 più significativi.
             salvando mbr su tmp e facendo and bit a bit con 0b_1111_0000_0000_0000 si ottiene:

             1001_0000_0000_0001 &
             1111_0000_0000_0000
            ---------------------
             1001_0000_0000_0000

             ovvero una variabile con i 4 bit più significativi uguali a mbr e la parte address di 12 bit uguale a 0.
             a questo punto, siccome pc è di 12 bit, ma salvato in un ushort, quindi 16 bit, (vedi definizione di i12) sicuramente i 4 bit più significativi di pc sono 0000
             per definizione.
             quindi facendo or bit a bit tra temp e pc si ottiene 

             1001_0000_0000_0000 |
             0000_0000_0000_1010
            ---------------------
             1001_0000_0000_1010

             che è esattamente mbr con gli stessi 4 bit del principio ma con address diverso. (che è appunto quello che cerchiamo di fare).
            */


            pc.setValue(getAddressFromMBR().getValue());
            mbr.setValue(tmp);

            Printf.printLogOnBuffer("M <- MBR");
            ram[mar.getValue()].setValue(mbr.getValue());

            Printf.printLogOnBuffer("PC <- PC + 1");
            pc.increment();


            f = false;
            return;
        }

        public void ISZ() //Incrementa e salta se zero
        {
            Printf.printLogOnBuffer("MAR <- MBR(AD)");
            addressToMAR();

            Printf.printLogOnBuffer("MBR <- M");
            mbr.setValue(ram[mar.getValue()].getValue());

            Printf.printLogOnBuffer("MBR <- MBR + 1");
            mbr.setValue((short)(mbr.getValue() + 1));

            Printf.printLogOnBuffer("M <- MBR");
            ram[mar.getValue()].setValue(mbr.getValue());

            Printf.printLogOnBuffer("if (MBR = 0) then (PC <- PC + 1)");
            if (mbr.getValue() == 0)
            {
                Printf.printLogOnBuffer("MBR is 0, skipping next instruction");
                pc.setValue((ushort)(pc.getValue() + 1)); //salta la prossima istruzione se 0
            }

            f = false;
            return;
        }

        //da qui cominciano le RRI (register reference instructions)

        public void CLA()
        {
            Printf.printLogOnBuffer("AC <- 0");
            a.setValue(0b_0000_0000_0000_0000);

            f = false;
            return; //XD
        }

        public void CLE()
        {
            Printf.printLogOnBuffer("E <- 0");
            e = false;

            f = false;
            return;
        }

        public void CMA() //complementa AC (complemento a 1)
        {
            Printf.printLogOnBuffer("AC <- AC'");
            a.setValue((short)~a.getValue());
            f = false;
            return;
        }

        public void CME() //comeplementa E
        {
            Printf.printLogOnBuffer("E <- E'");
            e = !e;

            f = false;
            return;
        }

        public void CIR() //circulate right E-AC
        {
            Printf.printLogOnBuffer("cir E-AC");
            bool tmp = Utility.isBitSet(a.getValue(), 0); //salvo il primo bit di ac su tmp
            short value = a.getValue(); //salvo ac in una variabile per comodità
            value = (short)(value >> 1); //shifto a destra value
            value = (short)(Utility.setBit((ushort)value, 15, e)); //a questo punto per "shiftare circolarmente" anche il registro E l'ultimo bit di value divanta uguale a E
            
            e = tmp; //infine per "shiftare circolarmente" anche E, questo diventa uguale tmp, ovvero il primo bit di ac prima dello shift
            a.setValue(value); //a ovviamente viene cambiato

            f = false;
            return;
        }

        public void CIL() //circulate left E-AC
        {
            Printf.printLogOnBuffer("cil E-AC");
            //la spiegazione è come quella di CIR, ma inversa
            bool tmp = Utility.isBitSet(a.getValue(), 15);
            short value = a.getValue();
            value = (short)(value << 1);
            value = (short)(Utility.setBit((ushort)value, 0, e));

            e = tmp;
            a.setValue(value);

            f = false;
            return;

        }

        public void INC()
        {
            Printf.printLogOnBuffer("E-AC <- AC + 1");
            short c = (short)(a.getValue() + 1);
            //vedi ADD per spiegazione di e
            e = ((a.getValue() ^ (short)1) >= 0) & ((a.getValue() ^ c) < 0);
            a.setValue(c);

            f = false;
            return;
        }

        public void SPA() //skip on positive AC
        {
            Printf.printLogOnBuffer("IF AC(1) = 0 THEN PC <- PC + 1");
            if (a.getValue() > 0)
            {
                Printf.printLogOnBuffer("Accumulator is positive, Skipping next instruction");
                pc.increment();
            }

            f = false;
            return;
        }

        public void SNA() //skip on negatie AC
        {
            Printf.printLogOnBuffer("IF AC(1) = 1 THEN PC <- PC + 1");
            if (a.getValue() < 0)
            {
                Printf.printLogOnBuffer("Accumulator is negative, Skipping next instruction");
                pc.increment();
            }

            f = false;
            return;
        }

        public void SZA() //skip on AC = 0
        {
            Printf.printLogOnBuffer("IF AC = 0 THEN PC <- PC + 1");
            if (a.getValue() == 0)
            {
                Printf.printLogOnBuffer("Accumulator is zero, Skipping next instruction");
                pc.increment();
            }

            f = false;
            return;
        }

        public void SZE() //skip on E = 0
        {
            Printf.printLogOnBuffer("IF E = 0 THEN PC <- PC + 1");
            if (e == false)
            {
                Printf.printLogOnBuffer("E register is zero, Skipping next instruction");
                pc.increment();
            }

            f = false;
            return;
        }

        public void HLT()
        {
            Printf.printLogOnBuffer("S <- 0");
            s = false;
        }

        //qui le due istruzioni di IO viste nel corso
        /*nb: queste due operazioni sono solo temporanee e dovranno essere modificate durante la realizzazione dell'interfaccia grafica*/
        public void INP()
        {
            Printf.printLogOnBuffer("AC <- ASCII(keyboard)");
            ConsoleKeyInfo inp = Console.ReadKey();
            a.setValue((short)inp.Key);

            f = false;
            return;
        }

        public void OUT()
        {
            Printf.printLogOnBuffer("Terminal <- AC");
            Printf.printLogOnBuffer("Output: " + Convert.ToChar(a.getValue()));

            f = false;
            return;
        }

        public void run()
        {
            //ok probabilmente run funziona solamente quando l'utente vuole eseguire il programma NON step by step
            //per quello farò un'altra funzione

            if (s == false)
                s = true;

            int sbsCounter = -1; 

            if(Program.form.stepByStepIsEnabeld())
            {
                sbsCounter = 0;
            }

            while (s) //fino a quando la macchina è accesa.
            {
                Program.getVm().isRunning = true;

                if(this.breakPoints.Contains(pc) && !BPstop)
                {
                    //this.pc.increment();
                    BPstop = true;
                    break;
                }

                if(!f && !r)
                {
                    Printf.printLogOnBuffer("Fetch"); 
                    fetch();
                    Printf.printRegisters(Program.getVm());
                    Program.form.safePrintStuff();
                }

                if (!f && r)
                {
                    Printf.printLogOnBuffer("indirect Addressing");
                    indirectAddressing();
                    Printf.printRegisters(Program.getVm());
                    Program.form.safePrintStuff();
                }

                if (f && !r)
                {
                    execute();
                    Printf.printLogOnBuffer("");
                    Printf.printRegisters(Program.getVm());
                    Program.form.safePrintStuff();
                }

                BPstop = false;

                //print run info into buffer
                
                Printf.printRamOnBuffer(Program.getVm().ram, Compiler.getCompilerData().programAddress);
                Printf.printRegisters(Program.getVm());
                //TerminalBuffer.dataAreBusy = false;
                Program.form.safePrintStuff();
                //Thread.Sleep(1000);
                //Program.getVm().isRunning = false;
                //Printf.printLog(LOG);
                //Program.inizializeVM();

                if(sbsCounter != -1)
                {
                    if(sbsCounter == Program.form.nStepsValue - 1)
                    {
                        break;
                    }
                    sbsCounter++;
                }

            }
            sbsCounter = -1;
        }
    }
}
