using System;

namespace Homework001.Parsing;

public class ByteParser
{
    private readonly AssemblyWalker _walker;

    public ByteParser(AssemblyWalker walker)
    {
        _walker = walker;
    }

    public string UshortDisplacementMemoryMode(byte r_m)
    {
        return DisplacementMemoryModeImplementation(r_m, GetShort());
    }

    public string ByteDisplacementMemoryMode(byte r_m)
    {
        return DisplacementMemoryModeImplementation(r_m, (short)GetSbyte());
    }

    public static string DisplacementMemoryModeImplementation(byte r_m, short displacementValue)
    {
        bool hasDisplacement = displacementValue != 0;
        int absoluteDisplacementValue = Math.Abs(displacementValue);
        string sign = displacementValue < 0 ? "-" : "+";

        return r_m switch
        {
            0b000 => $"[bx + si{(hasDisplacement ? $" {sign} {absoluteDisplacementValue}" : "")}]",
            0b001 => $"[bx + di{(hasDisplacement ? $" {sign} {absoluteDisplacementValue}" : "")}]",
            0b010 => $"[bp + si{(hasDisplacement ? $" {sign} {absoluteDisplacementValue}" : "")}]",
            0b011 => $"[bp + di{(hasDisplacement ? $" {sign} {absoluteDisplacementValue}" : "")}]",
            0b100 => $"[si{(hasDisplacement ? $" {sign} {absoluteDisplacementValue}" : "")}]",
            0b101 => $"[di{(hasDisplacement ? $" {sign} {absoluteDisplacementValue}" : "")}]",
            0b110 => $"[bp{(hasDisplacement ? $" {sign} {absoluteDisplacementValue}" : "")}]",
            0b111 => $"[bx{(hasDisplacement ? $" {sign} {absoluteDisplacementValue}" : "")}]",
            _ => throw new InvalidOperationException("invalid reg value")
        };
    }

    public string MostlyNoDisplacementMemoryMode(byte r_m)
    {
        return r_m switch
        {
            0b000 => "[bx + si]",
            0b001 => "[bx + di]",
            0b010 => "[bp + si]",
            0b011 => "[bp + di]",
            0b100 => "si",
            0b101 => "di",
            0b110 => $"[{GetShort()}]",
            0b111 => "[bx]",
            _ => throw new Exception("invalid reg value")
        };
    }

    public sbyte GetSbyte()
    {
        byte nextByte = _walker.GetNextByte();
        sbyte value = unchecked((sbyte)nextByte);
        return value;
    }
    
    public byte GetByte()
    {
        return _walker.GetNextByte();
    }
    
    public short GetShort()
    {
        short orderedBits;

        byte lowerBits = _walker.GetNextByte();
        byte upperBits = _walker.GetNextByte();

        orderedBits = (short)(upperBits << 8);
        orderedBits = (short)((ushort)orderedBits | (lowerBits));

        return orderedBits;
    }

    public ushort GetUShort()
    {
        ushort orderedBits;
        
        byte lowerBits = _walker.GetNextByte();
        byte upperBits = _walker.GetNextByte();

        orderedBits = (ushort)(upperBits << 8);
        orderedBits = (ushort)(orderedBits | (lowerBits));

        return orderedBits;
    }

    public static string DecodeRegister(byte reg, byte w)
    {
        if (w == 0)
        {
            return reg switch
            {
                0b000 => "al",
                0b001 => "cl",
                0b010 => "dl",
                0b011 => "bl",
                0b100 => "ah",
                0b101 => "ch",
                0b110 => "dh",
                0b111 => "bh",
                _ => throw new InvalidOperationException("invalid reg value")
            };
        }
        else if (w == 1)
        {
            return reg switch
            {
                0b000 => "ax",
                0b001 => "cx",
                0b010 => "dx",
                0b011 => "bx",
                0b100 => "sp",
                0b101 => "bp",
                0b110 => "si",
                0b111 => "di",
                _ => throw new InvalidOperationException("invalid reg value")
            };
        }
        throw new InvalidOperationException("invalid W value.");
    }

    public string DecodeR_M(byte mod, byte r_m, byte w)
    {
        return mod switch {
            0b00 => MostlyNoDisplacementMemoryMode(r_m),
            0b01 => ByteDisplacementMemoryMode(r_m),
            0b10 => UshortDisplacementMemoryMode(r_m),
            0b11 => DecodeRegister(r_m, w),
            _ => throw new InvalidOperationException("unexepected w value")
        };
    }
}
