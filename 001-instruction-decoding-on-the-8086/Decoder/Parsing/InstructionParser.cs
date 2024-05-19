using System;
using Homework001.Extensions;
using Homework001.Instructions;

namespace Homework001.Parsing;

public class InstructionParser 
{
    private readonly AssemblyWalker _walker;
    private readonly ByteParser _byteParser;
    private readonly InstructionsManager _instructionsManager;

    public InstructionParser(AssemblyWalker walker, ByteParser byteParser, InstructionsManager exporter)
    {
        _walker = walker;
        _byteParser = byteParser;
        _instructionsManager = exporter;
    }


    public void ReadFile()
    {
        while (_walker.TryGetNextInstruction(out byte? firstByte, out int? indexOfFirstByte) &&
                firstByte.HasValue && indexOfFirstByte.HasValue)
        {
            Opcode opcode = OpcodeParser.Parse(firstByte.Value);
            // Console.WriteLine($"value: {firstByte.Value.GetBits()}, index: {indexOfFirstByte.Value.ToString().PadLeft(3,'0')}, opcode: {opcode}");

            switch (opcode)
            {
                case Opcode.Mov_RegOrMemTo_FromReg:
                case Opcode.Add_RegOrMemWithRegToEither:
                case Opcode.Sub_RegOrMemAndRegToEither:
                case Opcode.Cmp_RegOrMemAndReg:
                    _instructionsManager.Add(indexOfFirstByte.Value, ParseRegisterOrMemoryTo_FromRegister(opcode, firstByte.Value));
                    break;
                case Opcode.Mov_ImmediateToRegOrMem:
                    _instructionsManager.Add(indexOfFirstByte.Value, ParseImmediateToRegisterOrMemory(opcode, firstByte.Value));
                    break;
                case Opcode.Mov_ImmediateToReg:
                    _instructionsManager.Add(indexOfFirstByte.Value, ParseImmediateToRegister(firstByte.Value));
                    break;
                case Opcode.Mov_MemToAcc:
                    _instructionsManager.Add(indexOfFirstByte.Value, ParseMemoryToAccumulator(firstByte.Value));
                    break;
                case Opcode.Mov_AccToMem:
                    _instructionsManager.Add(indexOfFirstByte.Value, ParseAccumulatorToMemory(firstByte.Value));
                    break;
                case Opcode.Add_ImmediateToAcc:
                case Opcode.Sub_ImmediateFromAcc:
                case Opcode.Cmp_ImmediateWithAcc:
                    _instructionsManager.Add(indexOfFirstByte.Value, ParseImmediateToAccumulator(opcode, firstByte.Value));
                    break;
                case Opcode.Add_Sub_Cmp_ImmediateToRegOrMem:
                    _instructionsManager.Add(indexOfFirstByte.Value, ParseImmediateToRegisterOrMemory(opcode, firstByte.Value));
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
                    _instructionsManager.Add(indexOfFirstByte.Value, ParseConditionalJump(opcode, indexOfFirstByte.Value));
                    break;
            }
        }
    }

    private string ParseImmediateToAccumulator(Opcode opcode, byte firstByte)
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
            immediate = _byteParser.GetShort();
            return $"{instruction} ax, {immediate}";
        }

        immediate = _byteParser.GetSbyte();
        return $"{instruction} al, {immediate}";

    }

    private string ParseConditionalJump(Opcode opcode, int instructioIndex)
    {
        string conditionalJumpInstruction = opcode
            .ToString()
            .Split('_', StringSplitOptions.RemoveEmptyEntries)[0]
            .ToLowerInvariant();

        sbyte jumpVector = _byteParser.GetSbyte();
        string label = _instructionsManager.LabelliseConditionalJumpVector(jumpVector);
        string instruction =  $"{conditionalJumpInstruction} {label} ;{jumpVector}";
        return instruction;
    }

    private string ParseAccumulatorToMemory(byte firstByte)
    {
        byte w = (byte)(firstByte & 0b_0000_0001);
        bool wide = Convert.ToBoolean(w);
        if (wide)
        {
            return $"mov [{_byteParser.GetShort()}], ax";
        }

        return $"mov [{_byteParser.GetSbyte()}], ax";
    }

    private string ParseMemoryToAccumulator(byte firstByte)
    {
        byte w = (byte)(firstByte & 0b_0000_0001);
        bool wide = Convert.ToBoolean(w);
        if (wide)
        {
            short memoryAddress = _byteParser.GetShort();
            return $"mov ax, [{memoryAddress}]";
        }
        else
        {
            sbyte memoryAddress = _byteParser.GetSbyte();
            return $"mov ax, [{memoryAddress}]";
        }
    }

    private string ParseImmediateToRegister(byte firstByte)
    {
        byte reg = (byte)(firstByte & 0b_0000_0111);
        byte w = (byte)((firstByte >> 3) & 0b_0000_0001);
        bool wide = Convert.ToBoolean(w);
        string regDecoded = ByteParser.DecodeRegister(reg, w);
        short immediate = 0;
        if (wide)
        {
            immediate = _byteParser.GetShort();
        }
        else
        {
            immediate = _byteParser.GetSbyte();
        }

        return $"mov {regDecoded}, {immediate}";
    }

    private string ParseImmediateToRegisterOrMemory(Opcode opcode, byte firstByte)
    {
        string instruction = "";
        byte w = (byte)(firstByte & 0b_0000_0001);
        byte s = (byte)((firstByte >> 1) & 0b_0000_0001);

        bool wide = Convert.ToBoolean(w);
        bool signed = Convert.ToBoolean(s);

        byte secondByte = _walker.GetNextByte();

        if (opcode == Opcode.Mov_ImmediateToRegOrMem &&
            (secondByte >> 3) == 0b_000)
        {
            instruction = "mov";
        }
        else if (opcode == Opcode.Add_Sub_Cmp_ImmediateToRegOrMem)
        {
            byte code = (byte)((secondByte >> 3) & 0b_0000_0111);
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

        byte mod = (byte)((secondByte >> 6) & 0b_0000_0011);
        byte r_m = (byte)(secondByte & 0b_0000_0111);

        string r_mDecoded = _byteParser.DecodeR_M(
            mod: mod,
            r_m: r_m,
            w: w);

        string immediate = "";
        if (opcode == Opcode.Mov_ImmediateToRegOrMem)
        {
            if (wide)
            {
                immediate = $"word {_byteParser.GetShort()}";
            }

            immediate = $"byte {_byteParser.GetSbyte()}";
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

                immediate = $"{_byteParser.GetSbyte()}";
            }
            else
            {
                if (wide)
                {
                    immediate = $"{_byteParser.GetUShort()}";
                }
                
                immediate = $"{_byteParser.GetByte()}";
            }
        }
        return $"{instruction} {r_mDecoded}, {immediate}";
    }

    private string ParseRegisterOrMemoryTo_FromRegister(Opcode opcode, byte firstByte)
    {
        string instruction = opcode
            .ToString()
            .Split('_', StringSplitOptions.RemoveEmptyEntries)[0]
            .ToLowerInvariant();

        byte d = (byte)((firstByte >> 1) & 0b_0000_0001);
        bool isRegTheSourceField = Convert.ToBoolean(d);

        byte w = (byte)(firstByte & 0b_0000_0001);

        byte nextByte = _walker.GetNextByte();

        byte mod = (byte)((nextByte >> 6) & 0b_0000_0011);
        byte reg = (byte)((nextByte >> 3) & 0b_0000_0111);
        byte r_m = (byte)(nextByte & 0b_0000_0111);
        string regDecoded = ByteParser.DecodeRegister(reg, w);
        string r_mDecoded = _byteParser.DecodeR_M(
            mod: mod,
            r_m: r_m,
            w: w);

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
