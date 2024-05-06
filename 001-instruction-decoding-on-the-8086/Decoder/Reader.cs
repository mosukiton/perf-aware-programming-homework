using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Homework001;

public class Reader 
{
    private readonly string inputFilePath;
    private readonly byte[] buffer;

    public Reader(string inputFilePath)
    {
        this.inputFilePath = inputFilePath;
        this.buffer = new byte[2];
    }

    public List<string> ReadFile()
    {
        List<string> instructions = new();
        instructions.Add("bits 16");
        Span<byte> bufferAsSpan = buffer;
        using (FileStream fileStream = File.Open(inputFilePath, FileMode.Open))
        {
            while (fileStream.Read(bufferAsSpan) != 0)
            {
                string instruction = ReadInstruction(bufferAsSpan);
                instructions.Add(instruction);
            }
        }

        return instructions;
    }

    private string ReadInstruction(ReadOnlySpan<byte> instructionAsBytes)
    {
        (byte opcode, byte d, byte w) = ReadFirstByte(instructionAsBytes[..1]);
        Debug.Assert(d < 2, $"D equals {Convert.ToString(d, 2)}. D can only be 1 bit.");
        Debug.Assert(w < 2, $"W equals {Convert.ToString(w, 2)}. W can only be 1 bit.");

        (byte mod, byte reg, byte r_m) = ReadSecondByte(instructionAsBytes[1..]);
        Debug.Assert(mod < 4, $"MOD equals {Convert.ToString(mod, 2)}. MOD can only be 2 bits.");
        Debug.Assert(reg < 8, $"REG equals {Convert.ToString(reg, 2)}. REG can only be 3 bits.");
        Debug.Assert(r_m < 8, $"R/M equals {Convert.ToString(r_m, 2)}. R/M can only be 3 bits.");

        Debug.Assert(opcode == 0b_0010_0010, "opcode is not a 'MOV' register/memory to/from register instruction.");
        Debug.Assert(mod == 0b11, "MOD is not in register mode");

        string regRegister = DecodeRegister(reg, w);
        string r_mRegister = DecodeRegister(r_m, w);

        if (d == 1)
        {
            // reg is source field
            return $"mov {regRegister}, {r_mRegister}";
        }
        else if (d == 0)
        {
            // reg is destination field
            return $"mov {r_mRegister}, {regRegister}";
        }

        throw new Exception("should not throw here.");
    }

    private string DecodeRegister(byte code, byte w)
    {
        if (w == 0)
        {
            return code switch
            {
                0b000 => "al",
                0b001 => "cl",
                0b010 => "dl",
                0b011 => "bl",
                0b100 => "ah",
                0b101 => "ch",
                0b110 => "dh",
                0b111 => "bh",
                _ => throw new Exception("invalid reg value")
            };
        }
        else if (w == 1)
        {
            return code switch
            {
                0b000 => "ax",
                0b001 => "cx",
                0b010 => "dx",
                0b011 => "bx",
                0b100 => "sp",
                0b101 => "bp",
                0b110 => "si",
                0b111 => "di",
                _ => throw new Exception("invalid reg value")
            };
        }
        throw new Exception("invalid W value.");
    }

    /// <summary>
    /// Reads the first byte of the instruction.
    /// </summary>
    /// <param name="firstByte">The first byte</param>
    /// <returns>The opcode, the dValue, the wValue</returns>
    private (byte, byte, byte) ReadFirstByte(ReadOnlySpan<byte> firstByte)
    {
        byte opcode = (byte)(firstByte[0] >> 2);
        byte dValue = (byte)(firstByte[0] & 0b_0000_0010);
        byte wValue = (byte)(firstByte[0] & 0b_0000_0001);
        Console.WriteLine(
            $"""
            opcode: {Convert.ToString(opcode, 2)},
            dValue:{Convert.ToString(dValue, 2)},
            wValue:{Convert.ToString(wValue, 2)}
            """);

        return (opcode, dValue, wValue);
    }

    /// <summary>
    /// Reads the second byte of the instruction.
    /// </summary>
    /// <param name="secondByte">The second byte</param>
    /// <returns>The mod, the reg, the r/m value.</returns>
    private (byte, byte, byte) ReadSecondByte(ReadOnlySpan<byte> secondByte)
    {
        byte mod = (byte)((secondByte[0] >> 6) & 0b_0000_0011);
        byte reg = (byte)((secondByte[0] >> 3) & 0b_0000_0111);
        byte r_m = (byte)(secondByte[0] & 0b_0000_0111);
        Console.WriteLine(
            $"""
            mod: {Convert.ToString(mod, 2)},
            reg:{Convert.ToString(reg, 2)}
            r_m:{Convert.ToString(r_m, 2)}
            """);

        return (mod, reg, r_m);
    }
}
