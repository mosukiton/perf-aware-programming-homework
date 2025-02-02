using System.Runtime.InteropServices;
using System;

namespace Homework001.Instructions;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Opcode
{
    public readonly byte Bits;
    public readonly OperationCode Code;

    public Opcode(byte bits)
    {
        Bits = bits;
        Code = ParseCode();
    }

    private byte Top4 => (byte)(Bits >> 4);
    private byte Bottom4 => (byte)(Bits & 0b_1111);
    private byte Skip1Bottom3 => (byte)((Bits >> 1) & 0b_0111);
    private byte Skip2Bottom2 => (byte)((Bits >> 2) & 0b_0011);

    private OperationCode ParseCode()
    {
        if (Top4 == 0b_1000)
        {
            if (Skip2Bottom2 == 0b_10)
            {
                return OperationCode.Mov_RegOrMemTo_FromReg;
            }
            else if (Skip2Bottom2 == 0)
            {
                return OperationCode.Add_Sub_Cmp_ImmediateToRegOrMem;
            }
        }

        if (Top4 == 0b1100 && Skip1Bottom3 == 0b011)
        {
            return OperationCode.Mov_ImmediateToRegOrMem;
        }

        if (Top4 == 0b1011)
        {
            return OperationCode.Mov_ImmediateToReg;
        }

        if (Top4 == 0b1010 && Skip1Bottom3 == 0)
        {
            return OperationCode.Mov_MemToAcc;
        }

        if (Top4 == 0b1010)
        {
            if (Skip1Bottom3 == 0)
            {
                return OperationCode.Mov_MemToAcc;
            }
            else if (Skip1Bottom3 == 1)
            {
                return OperationCode.Mov_AccToMem;
            }
        }

        if (Top4 == 0)
        {
            if (Skip2Bottom2 == 0)
            {
                return OperationCode.Add_RegOrMemWithRegToEither;
            }
            else if (Skip1Bottom3 == 0b010)
            {
                return OperationCode.Add_ImmediateToAcc;
            }
        }

        if (Top4 == 0b0010)
        {
            if (Skip2Bottom2 == 0b10)
            {
                return OperationCode.Sub_RegOrMemAndRegToEither;
            }
            else if (Skip1Bottom3 == 0b110)
            {
                return OperationCode.Sub_ImmediateFromAcc;
            }
        }

        if (Top4 == 0b0011)
        {
            if (Skip2Bottom2 == 0b10)
            {
                return OperationCode.Cmp_RegOrMemAndReg;

            }
            else if (Skip1Bottom3 == 0b110)
            {
                return OperationCode.Cmp_ImmediateWithAcc;
            }
        }

        if (Top4 == 0b0111)
        {
            return (OperationCode)Bits;
        }

        if (Top4 == 0b1110)
        {
            return (OperationCode)Bits;
        }

        throw new NotSupportedException();
    }

    private OperationCode CalculateJump()
    {
        return (OperationCode)Bits;
    }

    private OperationCode CalculateLoop()
    {
        return (OperationCode)Bits;
    }
}
