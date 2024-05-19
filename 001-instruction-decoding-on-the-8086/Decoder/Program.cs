using Homework001.Parsing;
using System;
using System.IO;
using System.Linq;

namespace Homework001;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0) throw new ArgumentOutOfRangeException();

        AssemblyWalker walker = new(args[0]);
        ByteParser byteParser = new(walker);
        InstructionParser reader = new(walker, byteParser);
        try
        {
            reader.ReadFile();

            if (args.Length == 2)
            {
                File.WriteAllLines(args[1], reader.Instructions.Values);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            reader.Instructions.Values.ToList().ForEach(x => Console.WriteLine(x));
        }
    }
}

