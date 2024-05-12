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
                byte firstByte = bufferAsSpan[0];
                Opcode opcode = GetOpcode(firstByte);
                string output = "";

                switch (opcode)
                {
                    case Opcode.Mov_RegisterOrMemoryTo_FromRegister:
                        output = ParseRegisterOrMemoryTo_FromRegister(firstByte, fileStream);
                        instructions.Add(output);
                        Console.WriteLine(output);
                        break;
                    case Opcode.Mov_ImmediateToRegisterOrMemory:
                        output = ParseImmediateToRegisterOrMemory(firstByte, fileStream);
                        instructions.Add(output);
                        Console.WriteLine(output);
                        break;
                    case Opcode.Mov_ImmediateToRegister:
                        output = ParseImmediateToRegister(firstByte, fileStream);
                        instructions.Add(output);
                        Console.WriteLine(output);
                        break;
                    case Opcode.Mov_MemoryToAccumulator:
                        output = ParseMemoryToAccumulator(firstByte, fileStream);
                        instructions.Add(output);
                        Console.WriteLine(output);
                        break;
                    case Opcode.Mov_AccumulatorToMemory:
                        output = ParseAccumulatorToMemory(firstByte, fileStream);
                        instructions.Add(output);
                        Console.WriteLine(output);
                        break;
                    default:
                        throw new InvalidOperationException("invalid opcode.");
                }
            }
        }

        return instructions;
    }

    private string ParseAccumulatorToMemory(byte firstByte, FileStream fileStream)
    {
        byte w = (byte)(firstByte & 0b_0000_0001);
        if (w == 1)
        {
            short memoryAddress = ByteParser.GetShortAsString(fileStream);
            return $"mov [{memoryAddress}], ax";
        }
        else if (w == 0)
        {
            sbyte memoryAddress = ByteParser.GetSbyteAsString(fileStream);
            return $"mov [{memoryAddress}], ax";
        }
        throw new Exception("should not throw here.");
    }

    private string ParseMemoryToAccumulator(byte firstByte, FileStream fileStream)
    {
        byte w = (byte)(firstByte & 0b_0000_0001);
        if (w == 1)
        {
            short memoryAddress = ByteParser.GetShortAsString(fileStream);
            return $"mov ax, [{memoryAddress}]";
        }
        else if (w == 0)
        {
            sbyte memoryAddress = ByteParser.GetSbyteAsString(fileStream);
            return $"mov ax, [{memoryAddress}]";
        }
        throw new Exception("should not throw here.");
    }

    private string ParseImmediateToRegister(byte firstByte, FileStream fileStream)
    {
        byte reg = (byte)(firstByte & 0b_0000_0111);
        byte w = (byte)((firstByte >> 3) & 0b_0000_0001);
        string regDecoded = ByteParser.DecodeRegister(reg, w);
        short immediate = 0;
        if (w == 1)
        {
            immediate = ByteParser.GetShortAsString(fileStream);
        }
        else if (w == 0)
        {
            immediate = ByteParser.GetSbyteAsString(fileStream);
        }

        return $"mov {regDecoded}, {immediate}";
    }

    private string ParseImmediateToRegisterOrMemory(byte firstByte, FileStream fileStream)
    {
        byte w = (byte)(firstByte & 0b_0000_0001);
        fileStream.Read(_buffer.AsSpan<byte>());
        byte mod = (byte)((_buffer[0] >> 6) & 0b_0000_0011);
        byte r_m = (byte)(_buffer[0] & 0b_0000_0111);

        string r_mDecoded = ByteParser.DecodeR_M(
            mod: mod,
            r_m: r_m,
            w: w,
            fileStream: fileStream);
        string immediate = "";
        if (w == 1)
        {
            immediate = $"word {ByteParser.GetShortAsString(fileStream)}";
        }
        else if (w == 0)
        {
            immediate = $"byte {ByteParser.GetSbyteAsString(fileStream)}";
        }
        return $"mov {r_mDecoded}, {immediate}";
    }

    private string ParseRegisterOrMemoryTo_FromRegister(byte firstByte, FileStream fileStream)
    {
        byte d = (byte)((firstByte >> 1) & 0b_0000_0001);
        byte w = (byte)(firstByte & 0b_0000_0001);
        fileStream.Read(_buffer.AsSpan<byte>());

        byte mod = (byte)((_buffer[0] >> 6) & 0b_0000_0011);
        byte reg = (byte)((_buffer[0] >> 3) & 0b_0000_0111);
        byte r_m = (byte)(_buffer[0] & 0b_0000_0111);
        string regDecoded = ByteParser.DecodeRegister(reg, w);
        string r_mDecoded = ByteParser.DecodeR_M(
            mod: mod,
            r_m: r_m,
            w: w,
            fileStream: fileStream);

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

    private Opcode GetOpcode(byte first)
    {
        if (first >> 2 == 0b_0010_0010)
        {
            return Opcode.Mov_RegisterOrMemoryTo_FromRegister;
        }

        if (first >> 1 == 0b_0110_0011)
        {
            return Opcode.Mov_ImmediateToRegisterOrMemory;
        }

        if (first >> 4 == 0b_0000_1011)
        {
            return Opcode.Mov_ImmediateToRegister;
        }

        if (first >> 1 == 0b_0101_0000)
        {
            return Opcode.Mov_MemoryToAccumulator;
        }

        if (first >> 1 == 0b_0101_0001)
        {
            return Opcode.Mov_AccumulatorToMemory;
        }

        throw new InvalidOperationException($"unrecognisable opcode: {Convert.ToString(first, 2)}");
    }
}
