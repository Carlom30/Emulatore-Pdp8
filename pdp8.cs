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

        public static void setVMParameters(i16[] ram, u12 pc)
        {
            vm.ram = ram;
            vm.pc.setValue(pc.getValue());
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
                Printf.printOnLog("no File Selected");
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
            //vm.addressToMAR();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false); //things di form che per ora non mi servono
            Application.Run(new Form1());

            vm.run(); //not so much to say
            
            Console.WriteLine("End of programm, printing Registers...");
            Printf.printRegisters(vm);
            //Printf.printRam(vm.ram);
            Printf.printSpecRam(0x0, 0x30F, vm.ram);
            Console.WriteLine("Press any key to close the application");
            Console.ReadKey(); //serve per non far chiudere la console quando il programma finisce perché boh windows non dovrebbe esistere immagino e il suo terminale è spazzatura?
                               //https://www.youtube.com/watch?v=hxM8QmyZXtg&t=11s per un po di funny sul terminale di windows
            //Printf.printRam(vm.ram);
        }

        public static void waitForClose(string log)
        {
            Console.WriteLine("\n" + log);
            Console.ReadKey();
        }
    }

    struct i16 //tutta la roba sotto non dovrebbe servire, ma la faccio in caso... beh, servisse :D
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

    class pdp8
    {
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

        public pdp8() //ovviamente non prende parametri XD
        {
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
                Console.WriteLine("found MRI ops");
                Console.Write("execute ");
                switch (opr)
                {
                    case (byte)MRI.AND:
                        Console.WriteLine(MRI.AND);
                        AND();
                        break;

                    case (byte)MRI.ADD:
                        Console.WriteLine(MRI.ADD);
                        ADD();
                        break;

                    case (byte)MRI.LDA:
                        Console.WriteLine(MRI.LDA);
                        LDA();
                        break;

                    case (byte)MRI.STA:
                        Console.WriteLine(MRI.STA);
                        STA();
                        break;

                    case (byte)MRI.BUN:
                        Console.WriteLine(MRI.BUN);
                        BUN();
                        break;

                    case (byte)MRI.BSA:
                        Console.WriteLine(MRI.BSA);
                        BSA();
                        break;

                    case (byte)MRI.ISZ:
                        Console.WriteLine(MRI.ISZ);
                        ISZ();
                        break;

                    default:
                        Console.WriteLine("is this a label?");
                        f = false;
                        r = false;
                        break;
                }
            }

            if(opr == 0b_111 && !i)
            {
                Console.WriteLine("found RRI ops");
                Console.Write("execute ");
                switch (ram[mar.getValue()].getValue())
                {
                    case (short)RRI.CLA:
                        Console.WriteLine(RRI.CLA);
                        CLA();
                        break;

                    case (short)RRI.CLE:
                        Console.WriteLine(RRI.CLE);
                        CLE();
                        break;

                    case (short)RRI.CMA:
                        Console.WriteLine(RRI.CMA);
                        CMA();
                        break;

                    case (short)RRI.CME:
                        Console.WriteLine(RRI.CME);
                        CME();
                        break;

                    case (short)RRI.CIR:
                        Console.WriteLine(RRI.CIR);
                        CIR();
                        break;

                    case (short)RRI.CIL:
                        Console.WriteLine(RRI.CIL);
                        CIL();
                        break;

                    case (short)RRI.INC:
                        Console.WriteLine(RRI.INC);
                        INC();
                        break;

                    case (short)RRI.SPA:
                        Console.WriteLine(RRI.SPA);
                        SPA();
                        break;

                    case (short)RRI.SNA:
                        Console.WriteLine(RRI.SNA);
                        SNA();
                        break;

                    case (short)RRI.SZA:
                        Console.WriteLine(RRI.SZA);
                        SZA();
                        break;

                    case (short)RRI.SZE:
                        Console.WriteLine(RRI.SZE);
                        SZE();
                        break;

                    case (short)RRI.HLT:
                        Console.WriteLine(RRI.HLT);
                        HLT();
                        break;

                    default:
                        Console.WriteLine("is this a label?");
                        f = false;
                        r = false;
                        break;
                }

            }

            if(opr == 0b_111 && i)
            {
                Console.WriteLine("found I/O ops");
                Console.Write("execute ");
                switch (ram[mar.getValue()].getValue())
                {
                    case (unchecked((short)IOI.INP)):
                        Console.WriteLine(IOI.INP);
                        INP();
                        break;

                    case (unchecked((short)IOI.OUT)):
                        Console.WriteLine(IOI.OUT);
                        OUT();
                        break;

                    default:
                        Console.WriteLine("is this a label?");
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
            Console.WriteLine("MAR <- MBR(AD)");
            addressToMAR();

            Console.WriteLine("MBR <- M");
            mbr.setValue(ram[mar.getValue()].getValue());

            Console.WriteLine("AC <- AC & MBR");
            a.setValue((short)(a.getValue() & mbr.getValue()));

            f = false;
            return;
        }

        public void ADD() // ADD con AC
        {
            Console.WriteLine("MAR <- MBR(AD)");
            addressToMAR();

            Console.WriteLine("MBR <- M");
            mbr.setValue(ram[mar.getValue()].getValue());

            Console.WriteLine("EAC <- AC + MBR");
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
            Console.WriteLine("MAR <- MBR(AD)");
            addressToMAR();

            Console.WriteLine("MBR <- M, AC <- 0");
            mbr.setValue(ram[mar.getValue()].getValue());
            a.setValue(0);

            Console.WriteLine("AC <- AC + MBR");
            a.setValue((short)(a.getValue() + mbr.getValue()));

            f = false;
            return;
        }

        public void STA() // memorizza AC
        {
            Console.WriteLine("MAR <- MBR(AD)");
            addressToMAR();

            Console.WriteLine("MBR <- AC");
            mbr.setValue(a.getValue());

            Console.WriteLine("M <- MBR");
            ram[mar.getValue()].setValue(mbr.getValue());

            f = false;
            return;
        }

        public void BUN() // salto incondizionato
        {
            Console.WriteLine("PC <- MBR(AD)");
            pc.setValue(getAddressFromMBR().getValue());

            f = false;
            return;
        }

        public void BSA() // salto con memorizzazione dell'indirizzo di ritorno
        {
            Console.WriteLine("MAR <- MBR(AD), MBR(AD) <- PC, PC <- MBR(AD)");
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

            Console.WriteLine("M <- MBR");
            ram[mar.getValue()].setValue(mbr.getValue());

            Console.WriteLine("PC <- PC + 1");
            pc.increment();


            f = false;
            return;
        }

        public void ISZ() //Incrementa e salta se zero
        {
            Console.WriteLine("MAR <- MBR(AD)");
            addressToMAR();

            Console.WriteLine("MBR <- M");
            mbr.setValue(ram[mar.getValue()].getValue());

            Console.WriteLine("MBR <- MBR + 1");
            mbr.setValue((short)(mbr.getValue() + 1));

            Console.WriteLine("M <- MBR");
            ram[mar.getValue()].setValue(mbr.getValue());

            Console.WriteLine("if (MBR = 0) then (PC <- PC + 1)");
            if (mbr.getValue() == 0)
            {
                Console.WriteLine("MBR is 0, skipping next instruction");
                pc.setValue((ushort)(pc.getValue() + 1)); //salta la prossima istruzione se 0
            }

            f = false;
            return;
        }

        //da qui cominciano le RRI (register reference instructions)

        public void CLA()
        {
            Console.WriteLine("AC <- 0");
            a.setValue(0b_0000_0000_0000_0000);

            f = false;
            return; //XD
        }

        public void CLE()
        {
            Console.WriteLine("E <- 0");
            e = false;

            f = false;
            return;
        }

        public void CMA() //complementa AC (complemento a 1)
        {
            Console.WriteLine("AC <- AC'");
            a.setValue((short)~a.getValue());
            f = false;
            return;
        }

        public void CME() //comeplementa E
        {
            Console.WriteLine("E <- E'");
            e = !e;

            f = false;
            return;
        }

        public void CIR() //circulate right E-AC
        {
            Console.WriteLine("cir E-AC");
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
            Console.WriteLine("cil E-AC");
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
            Console.WriteLine("E-AC <- AC + 1");
            short c = (short)(a.getValue() + 1);
            //vedi ADD per spiegazione di e
            e = ((a.getValue() ^ (short)1) >= 0) & ((a.getValue() ^ c) < 0);
            a.setValue(c);

            f = false;
            return;
        }

        public void SPA() //skip on positive AC
        {
            Console.WriteLine("IF AC(1) = 0 THEN PC <- PC + 1");
            if (a.getValue() > 0)
            {
                Console.WriteLine("Accumulator is positive, Skipping next instruction");
                pc.increment();
            }

            f = false;
            return;
        }

        public void SNA() //skip on negatie AC
        {
            Console.WriteLine("IF AC(1) = 1 THEN PC <- PC + 1");
            if (a.getValue() < 0)
            {
                Console.WriteLine("Accumulator is negative, Skipping next instruction");
                pc.increment();
            }

            f = false;
            return;
        }

        public void SZA() //skip on AC = 0
        {
            Console.WriteLine("IF AC = 0 THEN PC <- PC + 1");
            if (a.getValue() == 0)
            {
                Console.WriteLine("Accumulator is zero, Skipping next instruction");
                pc.increment();
            }

            f = false;
            return;
        }

        public void SZE() //skip on E = 0
        {
            Console.WriteLine("IF E = 0 THEN PC <- PC + 1");
            if (e == false)
            {
                Console.WriteLine("E register is zero, Skipping next instruction");
                pc.increment();
            }

            f = false;
            return;
        }

        public void HLT()
        {
            Console.WriteLine("S <- 0");
            s = false;
        }

        //qui le due istruzioni di IO viste nel corso
        /*nb: queste due operazioni sono solo temporanee e dovranno essere modificate durante la realizzazione dell'interfaccia grafica*/
        public void INP()
        {
            Console.WriteLine("AC <- ASCII(keyboard)");
            ConsoleKeyInfo inp = Console.ReadKey();
            a.setValue((short)inp.Key);

            f = false;
            return;
        }

        public void OUT()
        {
            Console.WriteLine("Terminal <- AC");
            Console.WriteLine("Output: " + Convert.ToChar(a.getValue()));

            f = false;
            return;
        }

        public void run()
        {
            while (s) //fino a quando la macchina è accesa.
            {
                bool stepBystep = true; //mi servirà per la parte grafica
                /*if(stepBystep)
                {
                    Console.WriteLine("press any key for the next instruction");
                    Console.ReadKey();
                }*/
                //Console.WriteLine(pc.getValue());

                if(!f && !r)
                {
                    Console.WriteLine("Fetch");
                    fetch();
                }

                if (!f && r)
                {
                    Console.WriteLine("indirect Addressing");
                    indirectAddressing();
                }

                if (f && !r)
                {
                    execute();
                    Console.WriteLine("");
                }
            }
        }
    }
}
