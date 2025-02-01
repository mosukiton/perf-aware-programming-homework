using Homework001.Parsing;
using System;
using System.IO;

namespace Homework001;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            throw new ArgumentOutOfRangeException();
        }

        AssemblyWalker walker = new(args[0]);
        InstructionsManager instructionsManager = new(walker);
        ByteParser byteParser = new(walker);
        InstructionParser reader = new(walker, byteParser, instructionsManager);
        try
        {
            reader.ReadFile();

            if (args.Length == 2)
            {
                File.WriteAllLines(args[1], instructionsManager.GetAllInstructions());
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            instructionsManager.DEBUG_GetAllInstructions().ForEach(x => Console.WriteLine(x));
        }
    }
}

