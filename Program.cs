using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Emulatore_Pdp8 //già definito da un'altra parte??

{
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
        }
    }

    struct i16
    {
        short value;
    }

    struct u12
    {
        private ushort _value; //la cosina sotto l'ha detta endu

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
        i16[] ram; //insieme di tutti i registri (eccetto quelli sotto)
        i16 mbr; // memory buffer register
        i16 a; //accumulatore

        u12 mar; //memory address register
        u12 pc; //program counter

        bool e;

        //insieme dei flip-flop per tipo di indirizzamento
        bool s;
        bool f;
        bool r;

    }

    
}
