using System.Collections.Generic;
using System.Linq;

namespace Homework001;

/// <summary>
/// Manages the decoded instructions
/// </summary>
public class InstructionsManager
{
    private readonly Dictionary<double, string> instructionsByByteIndex;
    private readonly AssemblyWalker assemblyWalker;

    private int labelId;

    /// <summary>
    /// Creates an instance of the <see cref="InstructionsManager"/>
    /// </summary>
    /// <param name="assemblyWalker">The assembly walker</param>
    public InstructionsManager(AssemblyWalker assemblyWalker)
    {
        instructionsByByteIndex = new Dictionary<double, string>();
        this.assemblyWalker = assemblyWalker;
        instructionsByByteIndex.Add(-1, "bits 16");
        labelId = 0;
    }

    /// <summary>
    /// Adds a decoded instruction.
    /// </summary>
    /// <param name="instructionByteIndex">The byte index of the instruction in the assembly file.</param>
    /// <param name="disassembledInstruction">The disassembled instruction.</param>
    public void Add(double instructionByteIndex, string disassembledInstruction)
        => instructionsByByteIndex.Add(instructionByteIndex, disassembledInstruction);

    /// <summary>
    /// Gets all instructions.
    /// </summary>
    public List<string> GetAllInstructions()
    {
        List<string> instructions = new();
        List<double> orderedKeys = instructionsByByteIndex.Keys.Order().ToList();

        foreach (double key in orderedKeys)
        {
            instructions.Add(instructionsByByteIndex[key]);
        }

        return instructions;
    }

    public List<string> DEBUG_GetAllInstructions()
    {
        List<string> instructions = new();
        List<double> orderedKeys = instructionsByByteIndex.Keys.Order().ToList();

        instructions.Add("line number : byte index : instruction");
        int lineNumber = 0;
        foreach (double key in orderedKeys)
        {
            instructions.Add($"{(++lineNumber).ToString().PadLeft(3, ' ')} : {key.ToString().PadLeft(5, ' ')} : {instructionsByByteIndex[key]}");
        }

        return instructions;
    }

    public string LabelliseConditionalJumpVector(sbyte vector)
    {
        double latestInstructionIndex = instructionsByByteIndex.Keys
            .Where(x => (x % 1) == 0) // check for whole number, to make sure that latest instruction isnt a label
            .OrderDescending()
            .First();
        double labelIndex = 0;

        // add 2 because we parsed 2 bytes to get this instruction, but haven't added it to the dictionary yet.
        labelIndex = latestInstructionIndex + vector + 2 + 0.5;

        return GetOrCreateLabel(labelIndex);
    }

    private string GetOrCreateLabel(double labelIndex)
    {
        if (instructionsByByteIndex.TryGetValue(labelIndex, out string? label) &&
            label[^1] == ':')
        {
            return label[..^1]; // don't return colon
        }

        label = $"label_{labelId++}"; 
        Add(labelIndex, $"{label}:");
        
        return label;
    }
}

