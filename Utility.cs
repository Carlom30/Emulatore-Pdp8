using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emulatore_Pdp8;
using Assembler;
using System.Windows.Forms;

namespace UtilityStuff
{
    class Utility
    {
        static public ushort setBit(ushort value, int pos, bool bit) //(fixed)
        {
            int bitValue = bit ? 1 : 0;
            //one = (ushort)(one & (1 << pos));
            /*il bug era la variabile "one", che alla fine dell'operazione sopra risultava essere 
              0b_0000_0000_0000, quindi inutile per qualunque ops and bit a bit.*/
            if ((value & (1 << pos)) != bitValue) //prima cosa controllo che il bit che voglio modificare non sia già
                                                  //come voglio farlo diventare XD
            {
                if(bitValue == 0)
                    value = (ushort)(value & (~(1 << pos))); 
                    /*se il bit che devo settare è 0, allora shifto 1 pos volte e lo complemento, facendo poi
                      l'and bit a bit tutti i bit di value restano invariati, eccetto il bit pos, che è settato a 0*/

                else
                    value = (ushort)(value | (1 << pos));
                    /*come prima ma in questo caso farò l'or bit a bit con lo shift di 1 non complementato, così 
                      facendo, cambio solo il bit pos e lo setto a 1*/
            }

            return value; //astruso ma funziona sempre :DD
        }

        static public bool isBitSet(int value, int position)
            => (value & (1 << position)) != 0; //:)
        

        static public string valueToBin(int value, RegType type)
        {
            string toBin = "";

            for (int i = (int)type - 1; i >= 0; i--)
                toBin += Utility.isBitSet(value, i) ? 1 : 0;

            return toBin;
        }

        public static LineCode[] noEmptyTkSource(LineCode[] tkSource)
        {
            int trueLength = 0;
            List<LineCode> noEmptyTK = new List<LineCode>();
            for(int i = 0; i < tkSource.Length; i++)
            {
                if(tkSource[i].type != PatternType.PT_EMPTYLINE)
                {
                    noEmptyTK.Add(tkSource[i]);
                }
            }

            return noEmptyTK.ToArray();
        }


        //utility di form

        //this function go on newline by definition
        public static void updateTextBox(TextBox textBox, string text)
        {
            textBox.Text = "";
            textBox.AppendText(text + Environment.NewLine);
        }
    }
}

//unchecked((short)0b_0000_0000_0000_0000)
