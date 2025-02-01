using System;
using Homework001.Instructions;

namespace Homework001.Parsing;

public class InstructionParser 
{
    private readonly AssemblyWalker walker;
    private readonly ByteParser byteParser;
    private readonly InstructionsManager instructionsManager;

    public InstructionParser(
        AssemblyWalker walker,
        ByteParser byteParser,
        InstructionsManager instructionsManager)
    {
        this.walker = walker;
        this.byteParser = byteParser;
        this.instructionsManager = instructionsManager;
    }


    public void ReadFile()
    {
        while (walker.TryGetNextInstruction(out byte? firstByte, out int? indexOfFirstByte) &&
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
                    instructionsManager.Add(indexOfFirstByte.Value, ParseRegisterOrMemoryTo_FromRegister(opcode, firstByte.Value));
                    break;
                case Opcode.Mov_ImmediateToRegOrMem:
                    instructionsManager.Add(indexOfFirstByte.Value, ParseMovImmediateToRegisterOrMemory(opcode, firstByte.Value));
                    break;
                case Opcode.Mov_ImmediateToReg:
                    instructionsManager.Add(indexOfFirstByte.Value, ParseImmediateToRegister(firstByte.Value));
                    break;
                case Opcode.Mov_MemToAcc:
                    instructionsManager.Add(indexOfFirstByte.Value, ParseMemoryToAccumulator(firstByte.Value));
                    break;
                case Opcode.Mov_AccToMem:
                    instructionsManager.Add(indexOfFirstByte.Value, ParseAccumulatorToMemory(firstByte.Value));
                    break;
                case Opcode.Add_ImmediateToAcc:
                case Opcode.Sub_ImmediateFromAcc:
                case Opcode.Cmp_ImmediateWithAcc:
                    instructionsManager.Add(indexOfFirstByte.Value, ParseImmediateToAccumulator(opcode, firstByte.Value));
                    break;
                case Opcode.Add_Sub_Cmp_ImmediateToRegOrMem:
                    instructionsManager.Add(indexOfFirstByte.Value, ParseArithmeticImmediateToRegisterOrMemory(opcode, firstByte.Value));
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
                case Opcode.LoopZ_LoopE:
                case Opcode.LoopNZ_LoopNE:
                case Opcode.Loop:
                case Opcode.JCXZ:
                    instructionsManager.Add(indexOfFirstByte.Value, ParseConditionalJump(opcode, indexOfFirstByte.Value));
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
            immediate = byteParser.GetShort();
            return $"{instruction} ax, {immediate}";
        }

        immediate = byteParser.GetSbyte();
        return $"{instruction} al, {immediate}";

    }

    private string ParseConditionalJump(Opcode opcode, int instructioIndex)
    {
        string conditionalJumpInstruction = opcode
            .ToString()
            .Split('_', StringSplitOptions.RemoveEmptyEntries)[0]
            .ToLowerInvariant();

        sbyte jumpVector = byteParser.GetSbyte();
        string label = instructionsManager.LabelliseConditionalJumpVector(jumpVector);
        string instruction =  $"{conditionalJumpInstruction} {label} ; {jumpVector}";
        return instruction;
    }

    private string ParseAccumulatorToMemory(byte firstByte)
    {
        byte w = (byte)(firstByte & 0b_0000_0001);
        bool wide = Convert.ToBoolean(w);
        if (wide)
        {
            return $"mov [{byteParser.GetShort()}], ax";
        }

        return $"mov [{byteParser.GetSbyte()}], ax";
    }

    private string ParseMemoryToAccumulator(byte firstByte)
    {
        byte w = (byte)(firstByte & 0b_0000_0001);
        bool wide = Convert.ToBoolean(w);
        if (wide)
        {
            short memoryAddress = byteParser.GetShort();
            return $"mov ax, [{memoryAddress}]";
        }
        else
        {
            sbyte memoryAddress = byteParser.GetSbyte();
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
            immediate = byteParser.GetShort();
        }
        else
        {
            immediate = byteParser.GetSbyte();
        }

        return $"mov {regDecoded}, {immediate}";
    }

    private string ParseMovImmediateToRegisterOrMemory(Opcode opcode, byte firstByte)
    {
        byte w = (byte)(firstByte & 0b_0000_0001);
        byte secondByte = walker.GetNextByte();
        byte mod = (byte)((secondByte >> 6) & 0b_0000_0011);
        byte r_m = (byte)(secondByte & 0b_0000_0111);

        string r_mDecoded = byteParser.DecodeR_M(
            mod: mod,
            r_m: r_m,
            w: w);

        string immediate = "";

        bool wide = Convert.ToBoolean(w);
        if (wide)
        {
            immediate = $"word {byteParser.GetShort()}";
        }
        else 
        {
            immediate = $"byte {byteParser.GetSbyte()}";
        }
        return $"mov {r_mDecoded}, {immediate}";
    }

    private string ParseArithmeticImmediateToRegisterOrMemory(Opcode opcode, byte firstByte)
    {
        byte w = (byte)(firstByte & 0b_0000_0001);
        byte s = (byte)((firstByte >> 1) & 0b_0000_0001);

        bool wide = Convert.ToBoolean(w);
        bool signed = Convert.ToBoolean(s);

        byte secondByte = walker.GetNextByte();
        byte code = (byte)((secondByte >> 3) & 0b_0000_0111);
        ImmediateToRegisterOrMemorySubCode subcode = (ImmediateToRegisterOrMemorySubCode)code;

        byte mod = (byte)((secondByte >> 6) & 0b_0000_0011);
        byte r_m = (byte)(secondByte & 0b_0000_0111);

        string r_mDecoded = byteParser.DecodeR_M(
            mod: mod,
            r_m: r_m,
            w: w);

        string size = "";
        string immediate = "";

        if (signed)
        {
            immediate = $"{byteParser.GetSbyte()}";
            if (wide && mod != 0b11)
            {
                size = " word";
            }
        }
        else
        {
            if (wide)
            {
                size = " word";
                immediate = $"{byteParser.GetUShort()}";
            }
            else 
            {
                size = " byte";
                immediate = $"{byteParser.GetByte()}";
            }
        }
        
        string compiledInstruction =  $"{subcode}{size} {r_mDecoded}, {immediate}";

        // Console.WriteLine();
        // Console.WriteLine($"first: {firstByte.GetBits()}, s: {s}, w: {w}, mod: {mod}, size: {size}");
        // Console.WriteLine(compiledInstruction);

        return compiledInstruction;
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

        byte nextByte = walker.GetNextByte();

        byte mod = (byte)((nextByte >> 6) & 0b_0000_0011);
        byte reg = (byte)((nextByte >> 3) & 0b_0000_0111);
        byte r_m = (byte)(nextByte & 0b_0000_0111);
        string regDecoded = ByteParser.DecodeRegister(reg, w);
        string r_mDecoded = byteParser.DecodeR_M(
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
