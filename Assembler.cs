﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrintSpace;
using UtilityStuff;
using Emulatore_Pdp8;

namespace Assembler
{
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

            Console.WriteLine(Utility.valueToBin(value, RegType.bit16_reg));
            return reg;
        }

        public static i16 buildRRIop(RRI instruction)
        {
            i16 reg = new i16();
            reg.setValue((short)instruction);

            return reg;
        }
    }
}