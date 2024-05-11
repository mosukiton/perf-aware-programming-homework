using System;
using System.IO;

namespace Homework001;

public static class ByteParser
{
    public static string UshortDisplacementMemoryMode(byte r_m, FileStream fileStream)
    {
        return DisplacementMemoryModeImplementation(r_m, GetUshortIntegerAsString(fileStream));
    }

    public static string ByteDisplacementMemoryMode(byte r_m, FileStream fileStream)
    {
        return DisplacementMemoryModeImplementation(r_m, (short)GetByteIntegerAsString(fileStream));
    }

    public static string DisplacementMemoryModeImplementation(byte r_m, short displacementValue)
    {
        return r_m switch
        {
            0b000 => displacementValue == 0 ? $"[bx + si]" : $"[bx + si + {displacementValue}]",
            0b001 => displacementValue == 0 ? $"[bx + di]" : $"[bx + di + {displacementValue}]",
            0b010 => displacementValue == 0 ? $"[bp + si]" : $"[bp + si + {displacementValue}]",
            0b011 => displacementValue == 0 ? $"[bp + di]" : $"[bp + di + {displacementValue}]",
            0b100 => displacementValue == 0 ? $"[si]" : $"[si + {displacementValue}]",
            0b101 => displacementValue == 0 ? $"[di]" : $"[di + {displacementValue}]",
            0b110 => displacementValue == 0 ? $"[bp]" : $"[bp + {displacementValue}]",
            0b111 => displacementValue == 0 ? $"[bx]" : $"[bx + {displacementValue}]",
            _ => throw new InvalidOperationException("invalid reg value")
        };
    }

    public static string MostlyNoDisplacementMemoryMode(byte r_m, FileStream fileStream)
    {
        return r_m switch
        {
            0b000 => "[bx + si]",
            0b001 => "[bx + di]",
            0b010 => "[bp + si]",
            0b011 => "[bp + di]",
            0b100 => "si",
            0b101 => "di",
            0b110 => $"{GetUshortIntegerAsString(fileStream)}",
            0b111 => "bx",
            _ => throw new Exception("invalid reg value")
        };
    }

    public static sbyte GetByteIntegerAsString(FileStream fileStream)
    {
        byte[] buffer = new byte[1];
        fileStream.Read(buffer.AsSpan<byte>());
        sbyte value = unchecked((sbyte)buffer[0]);
        return value;
    }
    
    public static short GetUshortIntegerAsString(FileStream fileStream)
    {
        short orderedBits;
        byte[] buffer = new byte[2];
        int  bytesRead = fileStream.Read(buffer.AsSpan<byte>());
        orderedBits = (short)(buffer[1] << 8);
        orderedBits = (short)((ushort)orderedBits | (buffer[0]));
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

    public static string DecodeR_M(byte mod, byte r_m, byte w, FileStream fileStream)
    {
        return mod switch {
            0b00 => MostlyNoDisplacementMemoryMode(r_m, fileStream),
            0b01 => ByteDisplacementMemoryMode(r_m, fileStream),
            0b10 => UshortDisplacementMemoryMode(r_m, fileStream),
            0b11 => DecodeRegister(r_m, w),
            _ => throw new InvalidOperationException("unexepected w value")
        };
    }
}
