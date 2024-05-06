using System;
using System.Collections.Generic;
using System.IO;

namespace Homework001;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0) throw new ArgumentOutOfRangeException();

        Reader reader = new(args[0]);
        List<string> result = reader.ReadFile();

        if (args.Length == 2)
        {
            File.WriteAllLines(args[1], result);
        }
        result.ForEach(x => Console.WriteLine(x));
    }
}

