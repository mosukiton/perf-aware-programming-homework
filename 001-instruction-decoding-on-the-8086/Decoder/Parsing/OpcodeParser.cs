using System;
using Homework001.Extensions;
using Homework001.Instructions;

namespace Homework001.Parsing;

public static class OpcodeParser
{
    public static Opcode Parse(byte code)
    {
        bool bit7 = Convert.ToBoolean(code & 0b_1000_0000);

        Opcode opcode = bit7 ? Pattern_1xxx_xxxx(code) : Pattern_0xxx_xxxx(code);

        return opcode;
    }

    private static Opcode Pattern_1xxx_xxxx(byte code)
    {
        bool bit6 = Convert.ToBoolean(code & 0b_0100_0000);

        if (!bit6)
        {
            return Pattern_10xx_xxxx(code);
        }

        bool bit5 = Convert.ToBoolean(code & 0b_0010_0000);
        bool bit4 = Convert.ToBoolean(code & 0b_0001_0000);

        if (bit5 && !bit4)
        {
            return Pattern_1110_xxxx(code);
        }

        // 1 1 x x _ x x x x
        if (!bit5 &&                                    // 1 1 0 x _ x x x x
            !bit4 &&                                    // 1 1 0 0 _ x x x x
            !Convert.ToBoolean(code & 0b_0000_1000) &&  // 1 1 0 0 _ 0 x x x
            Convert.ToBoolean(code & 0b_0000_0100) &&   // 1 1 0 0 _ 0 1 x x
            Convert.ToBoolean(code & 0b_0000_0010))     // 1 1 0 0 _ 0 1 1 x
        {
            return Opcode.Mov_ImmediateToRegOrMem;
        }

        throw new NotSupportedException($"Opcode not supported: {code.GetBits()}");
    }

    private static Opcode Pattern_1110_xxxx(byte code)
    {
        bool bit3 = Convert.ToBoolean(code & 0b_0000_1000);
        bool bit2 = Convert.ToBoolean(code & 0b_0000_0100);
        bool bit1 = Convert.ToBoolean(code & 0b_0000_0010);
        bool bit0 = Convert.ToBoolean(code & 0b_0000_0001);

        if (bit3 || bit2)
        {
            throw new NotSupportedException($"Opcode not supported: {code.GetBits()}");
        }

        if (bit1)
        {
            return bit0 ? Opcode.JCXZ : Opcode.Loop;
        }

        return bit0 ? Opcode.LoopZ_LoopE : Opcode.LoopNZ_LoopNE;
    }

    private static Opcode Pattern_10xx_xxxx(byte code)
    {
        bool bit5 = Convert.ToBoolean(code & 0b_0010_0000);

        // 1 0 1 x _ x x x x
        if (bit5)
        {
            return Pattern_101x_xxxx(code);
        }

        // 1 0 0 x _ x x x x
        bool bit4 = Convert.ToBoolean(code & 0b_0001_0000);
        bool bit3 = Convert.ToBoolean(code & 0b_0000_1000);
        bool bit2 = Convert.ToBoolean(code & 0b_0000_0100);

        if (bit4)
        {
            // 1 0 0 1 _ x x x x
            throw new NotSupportedException($"Opcode not supported: {code.GetBits()}");
        }

        if (bit3 && // 1 0 0 0 _ 1 x x x
            !bit2)  // 1 0 0 0 _ 1 0 x x
        {
            // 1 0 0 0 _ 1 0 x x
            return Opcode.Mov_RegOrMemTo_FromReg;
        }

        if (!bit4 && // 1 0 0 0 _ x x x x 
            !bit3 && // 1 0 0 0 _ 0 x x x
            !bit2)   // 1 0 0 0 _ 0 0 x x
        {
            return Opcode.Add_Sub_Cmp_ImmediateToRegOrMem;
        }

        throw new NotSupportedException($"Opcode not supported: {code.GetBits()}");
    }

    private static Opcode Pattern_101x_xxxx(byte code)
    {
        bool bit4 = Convert.ToBoolean(code & 0b_0001_0000);
        if (bit4)
        {
            // 1 0 1 1 _ x x x x
            return Opcode.Mov_ImmediateToReg;
        }

        // 1 0 1 0 _ x x x x
        if (!Convert.ToBoolean(code & 0b_0000_1000) && // 1 0 1 0 _ 0 x x x
            !Convert.ToBoolean(code & 0b_0000_0100))   // 1 0 1 0 _ 0 0 x x
        {
            bool bit1 = Convert.ToBoolean(code & 0b_0000_0010);
            return bit1 ? Opcode.Mov_AccToMem : Opcode.Mov_MemToAcc;
        }

        throw new NotSupportedException($"Opcode not supported: {code.GetBits()}");
    }

    private static Opcode Pattern_0xxx_xxxx(byte code)
    {
        bool bit6 = Convert.ToBoolean(code & 0b_0100_0000);

        return bit6 ? Pattern_01xx_xxxx(code) : Pattern_00xx_xxxx(code);

    }

    private static Opcode Pattern_01xx_xxxx(byte code)
    {
        if (Convert.ToBoolean(code & 0b_0010_0000) && // 0 1 1 x _ x x x x
            Convert.ToBoolean(code & 0b_0001_0000)) // 0 1 1 1 _ x x x x
        {
            return Pattern_0111_xxxx(code);
        }
        throw new NotSupportedException($"Opcode not supported: {code.GetBits()}");
    }

    private static Opcode Pattern_00xx_xxxx(byte code)
    {
        bool bit5 = Convert.ToBoolean(code & 0b_0010_0000);
        return bit5 ? Pattern_001x_xxxx(code) : Pattern_000x_xxxx(code);
    }

    private static Opcode Pattern_001x_xxxx(byte code)
    {
        bool bit4 = Convert.ToBoolean(code & 0b_0001_0000);
        bool bit3 = Convert.ToBoolean(code & 0b_0000_1000);
        bool bit2 = Convert.ToBoolean(code & 0b_0000_0100);
        bool bit1 = Convert.ToBoolean(code & 0b_0000_0010);

        if (bit4) // 0 0 1 1 _ x x x x
        {
            if (bit3) // 0 0 1 1 _ 1 x x x
            {
                if (!bit2) // 0 0 1 1 _ 1 0 x x
                {
                    return Opcode.Cmp_RegOrMemAndReg;
                }

                if (!bit1) // 0 0 1 1 _ 1 1 0 x
                {
                    return Opcode.Cmp_ImmediateWithAcc;
                }
            }
            throw new NotSupportedException($"Opcode not supported: {code.GetBits()}");
        }

        // 0 0 1 0 _ x x x x
        if (!bit3) // 0 0 1 0 _ 0 x x x
        {
            throw new NotSupportedException($"Opcode not supported: {code.GetBits()}");
        }

        // 0 0 1 0 _ 1 x x x
        if (!bit2) // 0 0 1 0 _ 1 0 x x
        {
            return Opcode.Sub_RegOrMemAndRegToEither;
        }

        // 0 0 1 0 _ 1 1 x x
        if (!bit1) // 0 0 1 0 _ 1 1 0 x
        {
            return Opcode.Sub_ImmediateFromAcc;
        }

        throw new NotSupportedException($"Opcode not supported: {code.GetBits()}");
    }

    private static Opcode Pattern_000x_xxxx(byte code)
    {
        bool bit4 = Convert.ToBoolean(code & 0b_0001_0000);
        bool bit3 = Convert.ToBoolean(code & 0b_0000_1000);
        bool bit2 = Convert.ToBoolean(code & 0b_0000_0100);
        bool bit1 = Convert.ToBoolean(code & 0b_0000_0010);

        if (bit4 | bit3)
        {
            throw new NotSupportedException($"Opcode not supported: {code.GetBits()}");
        }

        // 0 0 0 0 _ 0 x x x

        if (!bit2) // 0 0 0 0 _ 0 0 x x
        {
            return Opcode.Add_RegOrMemWithRegToEither;
        }

        if (!bit1)
        {
            return Opcode.Add_ImmediateToAcc;
        }
        
        throw new NotSupportedException($"Opcode not supported: {code.GetBits()}");
    }

    private static Opcode Pattern_0111_xxxx(byte code)
    {
        if (Enum.IsDefined<Opcode>((Opcode)code))
        {
            return (Opcode)code;
        }

        throw new NotSupportedException($"Opcode not supported: {code.GetBits()}");
    }
}
