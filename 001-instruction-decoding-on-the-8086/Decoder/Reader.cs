using System;
using System.Collections.Generic;
using System.IO;
using Homework001.Instructions;
using Homework001.Parsing;

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

    public List<string> Instructions { get; private set; } = new();

    public void ReadFile()
    {
        Instructions.Add("bits 16");
        Span<byte> bufferAsSpan = _buffer;
        using (FileStream fileStream = File.Open(_inputFilePath, FileMode.Open))
        {
            while (fileStream.Read(bufferAsSpan) != 0)
            {
                byte firstByte = bufferAsSpan[0];
                Opcode opcode = OpcodeParser.Parse(firstByte);

                switch (opcode)
                {
                    case Opcode.Mov_RegOrMemTo_FromReg:
                    case Opcode.Add_RegOrMemWithRegToEither:
                    case Opcode.Sub_RegOrMemAndRegToEither:
                    case Opcode.Cmp_RegOrMemAndReg:
                        Instructions.Add(ParseRegisterOrMemoryTo_FromRegister(opcode, firstByte, fileStream));
                        break;
                    case Opcode.Mov_ImmediateToRegOrMem:
                        Instructions.Add(ParseImmediateToRegisterOrMemory(opcode, firstByte, fileStream));
                        break;
                    case Opcode.Mov_ImmediateToReg:
                        Instructions.Add(ParseImmediateToRegister(firstByte, fileStream));
                        break;
                    case Opcode.Mov_MemToAcc:
                        Instructions.Add(ParseMemoryToAccumulator(firstByte, fileStream));
                        break;
                    case Opcode.Mov_AccToMem:
                        Instructions.Add(ParseAccumulatorToMemory(firstByte, fileStream));
                        break;
                    case Opcode.Add_ImmediateToAcc:
                    case Opcode.Sub_ImmediateFromAcc:
                    case Opcode.Cmp_ImmediateWithAcc:
                        Instructions.Add(ParseImmediateToAccumulator(opcode, firstByte, fileStream));
                        break;
                    case Opcode.Add_Sub_Cmp_ImmediateToRegOrMem:
                        Instructions.Add(ParseImmediateToRegisterOrMemory(opcode, firstByte, fileStream));
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
                        Instructions.Add(ParseConditionalJump(opcode, fileStream));
                        break;
                }
            }
        }
    }

    private string ParseImmediateToAccumulator(Opcode opcode, byte firstByte, FileStream fileStream)
    {
        string instruction = opcode
            .ToString()
            .Split('_', StringSplitOptions.RemoveEmptyEntries)[0]
            .ToLowerInvariant();

        byte w = (byte)(firstByte & 0b_0000_0001);
        bool wide = Convert.ToBoolean(w);
        
        short immediate = 0;
        if (wide)
        {
            immediate = ByteParser.GetShortAsString(fileStream);
            return $"{instruction} ax, {immediate}";
        }

        immediate = ByteParser.GetSbyteAsString(fileStream);
        return $"{instruction} al, {immediate}";

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
        bool wide = Convert.ToBoolean(w);
        if (wide)
        {
            return $"mov [{ByteParser.GetShortAsString(fileStream)}], ax";
        }

        return $"mov [{ByteParser.GetSbyteAsString(fileStream)}], ax";
    }

    private string ParseMemoryToAccumulator(byte firstByte, FileStream fileStream)
    {
        byte w = (byte)(firstByte & 0b_0000_0001);
        bool wide = Convert.ToBoolean(w);
        if (wide)
        {
            short memoryAddress = ByteParser.GetShortAsString(fileStream);
            return $"mov ax, [{memoryAddress}]";
        }
        else
        {
            sbyte memoryAddress = ByteParser.GetSbyteAsString(fileStream);
            return $"mov ax, [{memoryAddress}]";
        }
    }

    private string ParseImmediateToRegister(byte firstByte, FileStream fileStream)
    {
        byte reg = (byte)(firstByte & 0b_0000_0111);
        byte w = (byte)((firstByte >> 3) & 0b_0000_0001);
        bool wide = Convert.ToBoolean(w);
        string regDecoded = ByteParser.DecodeRegister(reg, w);
        short immediate = 0;
        if (wide)
        {
            immediate = ByteParser.GetShortAsString(fileStream);
        }
        else
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
        bool isRegTheSourceField = Convert.ToBoolean(d);

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

        if (isRegTheSourceField)
        {
            return $"{instruction} {regDecoded}, {r_mDecoded}";
        }
        else
        {
            return $"{instruction} {r_mDecoded}, {regDecoded}";
        }

        throw new Exception("should not throw here.");
    }
}
