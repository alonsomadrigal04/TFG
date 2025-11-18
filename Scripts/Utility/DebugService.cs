using System;
using System.Collections.Generic;
using System.Linq;

public static class DebugService
{
    private static readonly Dictionary<string, Func<string>> entries = [];

    public static void Register(string key, Func<string> getter)
    {
        entries[key] = getter;
    }

    public static IEnumerable<string> GetInfo()
    {
        foreach (var kv in entries)
            yield return $"{kv.Key}: {kv.Value()}";
    }
}
