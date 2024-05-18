using System;
using System.IO;

namespace Homework001;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0) throw new ArgumentOutOfRangeException();

        Reader reader = new(args[0]);
        try
        {
            reader.ReadFile();

            if (args.Length == 2)
            {
                File.WriteAllLines(args[1], reader.Instructions);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            reader.Instructions.ForEach(x => Console.WriteLine(x));
        }
    }
}

