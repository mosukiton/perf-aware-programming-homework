using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                Opcode opcode = OpcodeParser.Parse(firstByte);
                string output = "";

                switch (opcode)
                {
                    case Opcode.Mov_RegOrMemTo_FromReg:
                    case Opcode.Add_RegOrMemWithRegToEither:
                    case Opcode.Sub_RegOrMemAndRegToEither:
                    case Opcode.Cmp_RegOrMemAndReg:
                        output = ParseRegisterOrMemoryTo_FromRegister(opcode, firstByte, fileStream);
                        Console.WriteLine(output);
                        instructions.Add(output);
                        break;
                    case Opcode.Mov_ImmediateToRegOrMem:
                        output = ParseImmediateToRegisterOrMemory(opcode, firstByte, fileStream);
                        Console.WriteLine(output);
                        instructions.Add(output);
                        break;
                    case Opcode.Mov_ImmediateToReg:
                        output = ParseImmediateToRegister(firstByte, fileStream);
                        Console.WriteLine(output);
                        instructions.Add(output);
                        break;
                    case Opcode.Mov_MemToAcc:
                        output = ParseMemoryToAccumulator(firstByte, fileStream);
                        Console.WriteLine(output);
                        instructions.Add(output);
                        break;
                    case Opcode.Mov_AccToMem:
                        output = ParseAccumulatorToMemory(firstByte, fileStream);
                        Console.WriteLine(output);
                        instructions.Add(output);
                        break;
                    case Opcode.Add_ImmediateToAcc:
                    case Opcode.Sub_ImmediateFromAcc:
                    case Opcode.Cmp_ImmediateWithAcc:
                        output = ParseImmediateToAccumulator(opcode, firstByte, fileStream);
                        Console.WriteLine(output);
                        instructions.Add(output);
                        break;
                    case Opcode.Add_Sub_Cmp_ImmediateToRegOrMem:
                        output = ParseImmediateToRegisterOrMemory(opcode, firstByte, fileStream);
                        Console.WriteLine(output);
                        instructions.Add(output);
                        break;
                    case Opcode.JE_JZ:
                    case Opcode.JL_JNGE:
                    case Opcode.JLE_JNG:
                    case Opcode.JB_JNAE:
                    case Opcode.JBE_JNA:
                    case Opcode.JP_JPE:
                    case Opcode.JO:
                    case Opcode.JS:
                    case Opcode.JNS:
                    case Opcode.JNE_JNZ:
                    case Opcode.JNL_JGE:
                    case Opcode.JNLE_JG:
                    case Opcode.JNB_JAE:
                    case Opcode.JNBE_JA:
                    case Opcode.JNP_JPO:
                    case Opcode.JNO:
                        output = ParseConditionalJump(opcode, fileStream);
                        Console.WriteLine(output);
                        break;
                }
            }
        }

        return instructions;
    }

    private string ParseImmediateToAccumulator(Opcode opcode, byte firstByte, FileStream fileStream)
    {
        string instruction = opcode
            .ToString()
            .Split('_', StringSplitOptions.RemoveEmptyEntries)[0]
            .ToLowerInvariant();
        byte w = (byte)(firstByte & 0b_0000_0001);
        short immediate = 0;
        if (w == 1)
        {
            immediate = ByteParser.GetShortAsString(fileStream);
        }
        else if (w == 0)
        {
            immediate = ByteParser.GetSbyteAsString(fileStream);
        }

        return $"{instruction} ax, {immediate}";
    }

    private string ParseConditionalJump(Opcode opcode, FileStream fileStream)
    {
        string conditionalJumpInstruction = opcode
            .ToString()
            .Split('_', StringSplitOptions.RemoveEmptyEntries)[0]
            .ToLowerInvariant();

        sbyte instructionPointerIncrementValue = ByteParser.GetSbyteAsString(fileStream);
        string output =  $"{conditionalJumpInstruction} {instructionPointerIncrementValue}";
        Console.WriteLine(output);
        return output;
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

    private string ParseImmediateToRegisterOrMemory(Opcode opcode, byte firstByte, FileStream fileStream)
    {
        string instruction = "";
        byte w = (byte)(firstByte & 0b_0000_0001);
        byte s = (byte)((firstByte >> 1) & 0b_0000_0001);

        bool wide = Convert.ToBoolean(w);
        bool signed = Convert.ToBoolean(s);

        fileStream.Read(_buffer.AsSpan<byte>());

        if (opcode == Opcode.Mov_ImmediateToRegOrMem &&
            (_buffer[0] >> 3) == 0b_000)
        {
            instruction = "mov";
        }
        else if (opcode == Opcode.Add_Sub_Cmp_ImmediateToRegOrMem)
        {
            byte code = (byte)((_buffer[0] >> 3) & 0b_0000_0111);
            ImmediateToRegisterOrMemorySubCode subcode = (ImmediateToRegisterOrMemorySubCode)code;

            if (!signed && wide)
            {
                instruction = $"{subcode} word";
            }
            else if (!wide)
            {
                instruction = $"{subcode} byte";
            }
            else
            {
                instruction = $"{subcode}";
            }
        }

        byte mod = (byte)((_buffer[0] >> 6) & 0b_0000_0011);
        byte r_m = (byte)(_buffer[0] & 0b_0000_0111);

        string r_mDecoded = ByteParser.DecodeR_M(
            mod: mod,
            r_m: r_m,
            w: w,
            fileStream: fileStream);

        string immediate = "";
        if (opcode == Opcode.Mov_ImmediateToRegOrMem)
        {
            if (wide)
            {
                immediate = $"word {ByteParser.GetShortAsString(fileStream)}";
            }

            immediate = $"byte {ByteParser.GetSbyteAsString(fileStream)}";
        }
        else if (opcode == Opcode.Add_Sub_Cmp_ImmediateToRegOrMem)
        {
            // Console.WriteLine($"signed: {signed}, wide: {wide}");
            if (signed)
            {
                // if (wide)
                // {
                //     immediate = $"{ByteParser.GetShortAsString(fileStream)}";
                // }

                immediate = $"{ByteParser.GetSbyteAsString(fileStream)}";
            }
            else
            {
                if (wide)
                {
                    immediate = $"{ByteParser.GetUShortAsString(fileStream)}";
                }
                
                immediate = $"{ByteParser.GetByteAsString(fileStream)}";
            }
        }
        return $"{instruction} {r_mDecoded}, {immediate}";
    }

    private string ParseRegisterOrMemoryTo_FromRegister(Opcode opcode, byte firstByte, FileStream fileStream)
    {
        string instruction = opcode
            .ToString()
            .Split('_', StringSplitOptions.RemoveEmptyEntries)[0]
            .ToLowerInvariant();

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
            return $"{instruction} {regDecoded}, {r_mDecoded}";
        }
        else if (d == 0)
        {
            // reg is destination field
            return $"{instruction} {r_mDecoded}, {regDecoded}";
        }

        throw new Exception("should not throw here.");
    }
}
