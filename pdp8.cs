using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PrintSpace;
using UtilityStuff;


namespace Emulatore_Pdp8
{
    enum RegType
    {
        bit16_reg = 16,
        bit12_reg = 12,
        bit3_reg = 3
    }

    enum MRI //memory reference instruction
    {
        AND = 0b_000,
        ADD = 0b_001,
        LDA = 0b_010,
        STA = 0b_011,
        BUN = 0b_100,
        BSA = 0b_101,
        ISZ = 0b_110
    }

    enum RRI //register reference instruction
    {
        CLA = 0b_0111_1000_0000_0000,
        CLE = 0b_0111_0100_0000_0000,
        CMA = 0b_0111_0010_0000_0000, 
        CME = 0b_0111_0001_0000_0000, 
        CIR = 0b_0111_0000_1000_0000, 
        CIL = 0b_0111_0000_0100_0000, 
        INC = 0b_0111_0000_0010_0000, 
        SPA = 0b_0111_0000_0001_0000, 
        SNA = 0b_0111_0000_0000_1000, 
        SZA = 0b_0111_0000_0000_0100, 
        SZE = 0b_0111_0000_0000_0010, 
        HLT = 0b_0111_0000_0000_0001
    }

    enum IOI // I/O instruction set
    {
        INP = 0b_1111_1000_0000_0000, 
        OUT = 0b_1111_0100_0000_0000
    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            /*Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false); //things di form che per ora non mi servono
            Application.Run(new Form1());*/
            Console.WriteLine("hello world!\n");


            pdp8 vm = new pdp8();

            /*for (int i = 0; i < vm.ram.Length; i++)
                vm.ram[i].setValue(10);*/

            //di seguito i test fatti con la macchina virtuale
      
            vm.ram[0].setValue(unchecked((short)0b_0101_0000_0000_0100)); 
            vm.ram[1].setValue(unchecked((short)0b_0111_0000_0000_0001)); //HLT
            vm.ram[4].setValue(unchecked((short)0b_0000_0000_0000_0000));
            vm.ram[5].setValue(unchecked((short)0b_0000_0000_0000_0000));
            vm.ram[6].setValue(unchecked((short)0b_1100_0000_0000_0100)); 
            //vm.ram[7].setValue(unchecked((short)0b_0111_0000_0000_0001)); //HLT (works)
            //vm.a.setValue(unchecked((short)0b_1000_0000_0000_0000)); //setting scripted dell'accumulatore per testing


            vm.run(); //not so much to say
            //vm.addressToMAR();

            Printf.printRegisters(vm);
            Printf.printSpecRam(0, 6, vm.ram);

            //Printf.printRam(vm.ram);
        }
    }

    struct i16 //tutta la roba sotto non dovrebbe servire, ma la faccio in caso... beh, servisse :D
    {
        private short value;

        public i16(short firstValue)
        {
            value = 0;
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
                switch (opr)
                {
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
                switch (ram[mar.getValue()].getValue())
                {
                    case (short)RRI.HLT:
                        Console.WriteLine("HLT");
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
                switch(ram[mar.getValue()].getValue())
                {
                    default:
                        Console.WriteLine("is this a label?");
                        f = false;
                        r = false;
                        break;
                }
            }
            //altra roba ovviamente, just testing
            return;
        }

        public void ADD()
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

        public void LDA()
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

        public void STA()
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

        public void BUN()
        {
            Console.WriteLine("PC <- MBR(AD)");
            pc.setValue(getAddressFromMBR().getValue());

            f = false;
            return;
        }

        public void BSA()
        {
            //salto con memorizzazione indirizzo di ritorno
            Console.WriteLine("MAR <- MBR(AD), MBR(AD) <- PC, PC <- MBR(AD)");
            addressToMAR();

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

        public void HLT()
        {
            Console.WriteLine("S <- 0");
            s = false;
        }

        public void run()
        {
            //ovviamente qui va un while
            while (s)
            {
                //Console.WriteLine(pc.getValue());

                /*sussiste un problema da risolvere, anche fetch dovrebbe avere un controllo (if (!f && !r))
                  che però, se l'istruzione precedente non era un istruzione, ma una label o comunque un valore
                  allora i due flip flop non verranno mai settati a false e il ciclo continuerà all'infinito senza mai
                  cambiare istruzione, questo perché i flip-flop vengono cambiati solo nei cicli di I e execute, che
                  siccome non vengono mai chiamati, i flip-flop non cambieranno mai. per ora su execute controllo
                  semplicemente se la cella non corrisponde ad alcune istruzioni, se questo è il caso, allora setto
                  f == false e r == false. questo risolve il problema*/

                Console.WriteLine("Fetch");
                if(!f && !r)
                    fetch();

                if (!f && r)
                {
                    Console.WriteLine("indirect Addressing");
                    indirectAddressing();
                }

                if (f && !r)
                {
                    Console.WriteLine("execute");
                    execute();
                }
            }
        }
    }
}
