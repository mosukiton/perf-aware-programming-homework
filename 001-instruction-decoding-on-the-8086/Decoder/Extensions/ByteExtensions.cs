using System;

namespace Homework001.Extensions;

public static class ByteExtensions
{
    public static string GetBits(this byte code)
    {
        return Convert.ToString(code, 2).PadLeft(8, '0').Insert(4, "_");
    }
}
