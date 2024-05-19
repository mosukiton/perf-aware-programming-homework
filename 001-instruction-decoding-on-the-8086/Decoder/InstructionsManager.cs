using System.Collections.Generic;
using System.Linq;

namespace Homework001;

public class InstructionsManager
{
    private readonly Dictionary<double, string> _instructions;
    private readonly AssemblyWalker _assemblyWalker;

    private int labelId;
    public InstructionsManager(AssemblyWalker assemblyWalker)
    {
        _instructions = new Dictionary<double, string>();
        _assemblyWalker = assemblyWalker;
        _instructions.Add(-1, "bits 16");
        labelId = 0;
    }

    public void Add(double instructionByteIndex, string disassembledInstruction)
        => _instructions.Add(instructionByteIndex, disassembledInstruction);

    public List<string> GetAllInstructions()
    {
        List<string> instructions = new();
        List<double> orderedKeys = _instructions.Keys.Order().ToList();

        foreach (double key in orderedKeys)
        {
            // DEBUG
            // instructions.Add($"{key} : {_instructions[key]}");
            instructions.Add(_instructions[key]);
        }

        return instructions;
    }

    public string LabelliseConditionalJumpVector(sbyte vector)
    {
        double latestInstructionIndex = _instructions.Keys
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
        if (_instructions.TryGetValue(labelIndex, out string? label) &&
            label[^1] == ':')
        {
            return label;
        }

        label = $"label_{labelId++}"; 
        Add(labelIndex, $"{label}:");
        
        return label;
    }
}

