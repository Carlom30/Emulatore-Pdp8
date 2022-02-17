using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using PrintSpace;
using UtilityStuff;


namespace Emulatore_Pdp8 //già definito da un'altra parte??
{
    enum RegType
    {
        bit16_reg = 16,
        bit12_reg = 12
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

            //u12 a = new u12((ushort)(Math.Pow(2, 13))); //test per overflow (worked)

            pdp8 vm = new pdp8();

            for (int i = 0; i < vm.ram.Length; i++)
                vm.ram[i].setValue(0);

            if(Utility.isBitSet(vm.ram[11].getValue(), 0))
            {
                Console.WriteLine("ciclo di indirizzamento indiretto\n");
                vm.r = true;
            }

            Printf.printRam(vm.ram);
            Printf.printRegisters(vm);
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
                return; //crash probabilmente
            }

            _value = newValue;
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

        public bool e; //registro di riporto per l'accumulatore

        //insieme dei flip-flop per tipo di indirizzamento
        public bool s;
        public bool f;
        public bool r;

        public pdp8() //ovviamente non prende parametri XD
        {
            this.ram = new i16[4096];
            this.mbr = new i16();
            this.a = new i16();

            this.mar = new u12(0);
            this.pc = new u12(0);

            e = false;

            s = false;
            f = false;
            r = false;
            return;
        }


    }



}
