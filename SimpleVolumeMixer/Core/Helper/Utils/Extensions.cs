using System;
using System.Collections.Generic;

namespace SimpleVolumeMixer.Core.Helper.Utils;

public static class Extensions
{
    public static void IfPresent<T>(this T? that, Action<T> consumer)
    {
        if (that != null) consumer(that);
    }

    public static bool IsEmpty<T>(this ICollection<T> src)
    {
        return src.Count <= 0;
    }

    public static bool IsNotEmpty<T>(this ICollection<T> src)
    {
        return !src.IsEmpty();
    }
}