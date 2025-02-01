using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Homework001;

/// <summary>
/// Walks the assembly file bytes
/// </summary>
public class AssemblyWalker
{
    private readonly byte[] _buffer;
    private int _currentIndex;

    /// <summary>
    /// Creates an instance of the <see cref="AssemblyWalker"/>
    /// </summary>
    /// <param name="inputFilePath">The input file path of the assembly file.</param>
    public AssemblyWalker(string inputFilePath)
    {
        _buffer = File.ReadAllBytes(inputFilePath);
        _currentIndex = 0;
    }

    public byte GetNextByte()
    {
        byte nextByte = _buffer[_currentIndex];
        _currentIndex++;
        return nextByte;
    }

    public byte GetByteAt(int index)
    {
        if (index >= _buffer.Length)
        {
            throw new ArgumentOutOfRangeException();
        }

        return _buffer[index];
    }

    public bool TryGetNextInstruction(
        [NotNullWhen(true)] out byte? firstByte,
        [NotNullWhen(true)] out int? opcodeIndex)
    {
        if (_currentIndex >= _buffer.Length)
        {
            firstByte = null;
            opcodeIndex = null;
            return false;
        }

        opcodeIndex = _currentIndex;
        firstByte = GetNextByte();

        return true;
    }
}

