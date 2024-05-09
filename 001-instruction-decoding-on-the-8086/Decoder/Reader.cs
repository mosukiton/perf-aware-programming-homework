using System;
using System.Collections.Generic;
using System.IO;
using Homework001.Instructions;

namespace Homework001;

public class Reader 
{
    private readonly string _inputFilePath;
    private readonly byte[] _buffer;

    public Reader(string inputFilePath)
    {
        _inputFilePath = inputFilePath;
        _buffer = new byte[1];
    }

    public List<string> ReadFile()
    {
        List<string> instructions = new();
        instructions.Add("bits 16");
        Span<byte> bufferAsSpan = _buffer;
        using (FileStream fileStream = File.Open(_inputFilePath, FileMode.Open))
        {
            while (fileStream.Read(bufferAsSpan) != 0)
            {
                MovOpcode opcode = GetOpcode(bufferAsSpan[0]);

                switch (opcode)
                {
                    case MovOpcode.RegisterOrMemoryTo_FromRegister:
                        instructions.Add(ParseRegisterOrMemoryTo_FromRegister(bufferAsSpan[0], fileStream));
                        break;
                    case MovOpcode.ImmediateToRegister:
                    case MovOpcode.ImmediateToRegisterOrMemory:
                    case MovOpcode.MemoryToAccumulator:
                    case MovOpcode.AccumulatorToMemory:
                    case MovOpcode.RegisterOrMemoryToSegmentRegister:
                    case MovOpcode.SegmentRegisterToRegisterOrMemory:
                        throw new NotImplementedException("opcodes not implemented yet.");
                    default:
                        throw new InvalidOperationException("invalid opcode.");
                }
            }
        }

        return instructions;
    }

    private string ParseRegisterOrMemoryTo_FromRegister(byte firstByte, FileStream fileStream)
    {
        byte d = (byte)(firstByte & 0b_0000_0010);
        byte w = (byte)(firstByte & 0b_0000_0001);
        fileStream.Read(_buffer.AsSpan<byte>());

        byte mod = (byte)((_buffer[0] >> 6) & 0b_0000_0011);
        byte reg = (byte)((_buffer[0] >> 3) & 0b_0000_0111);
        byte r_m = (byte)(_buffer[0] & 0b_0000_0111);
        string regDecoded = ByteParser.DecodeRegister(reg, w);
        string r_mDecoded = mod switch {
            0b00 => ByteParser.MostlyNoDisplacementMemoryMode(r_m, fileStream),
            0b01 => ByteParser.ByteDisplacementMemoryMode(r_m, fileStream),
            0b10 => ByteParser.UshortDisplacementMemoryMode(r_m, fileStream),
            0b11 => ByteParser.DecodeRegister(r_m, w),
            _ => throw new InvalidOperationException("unexepected w value")
        };

        if (d == 1)
        {
            // reg is source field
            return $"mov {regDecoded}, {r_mDecoded}";
        }
        else if (d == 0)
        {
            // reg is destination field
            return $"mov {r_mDecoded}, {regDecoded}";
        }

        throw new Exception("should not throw here.");
    }

    private MovOpcode GetOpcode(byte first)
    {
        if (first >> 2 == 0b_0010_0010)
        {
            return MovOpcode.RegisterOrMemoryTo_FromRegister;
        }

        if (first >> 1 == 0b_0110_0011)
        {
            return MovOpcode.ImmediateToRegisterOrMemory;
        }

        if (first >> 4 == 0b_0000_1011)
        {
            return MovOpcode.ImmediateToRegister;
        }

        if (first >> 1 == 0b_0101_0000)
        {
            return MovOpcode.MemoryToAccumulator;
        }

        if (first >> 1 == 0b_0101_0001)
        {
            return MovOpcode.AccumulatorToMemory;
        }

        if (first == 0b_1000_1110)
        {
            return MovOpcode.RegisterOrMemoryToSegmentRegister;
        }

        if (first >> 1 == 0b_1000_1100)
        {
            return MovOpcode.SegmentRegisterToRegisterOrMemory;
        }

        throw new InvalidOperationException($"unrecognisable opcode: {Convert.ToString(first, 2)}");
    }
}
