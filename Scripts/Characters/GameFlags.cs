using System;
using System.Collections.Generic;

public class GameFlagsManager
{
    static HashSet<GameFlags> ActiveGameFlags;

    static GameFlagsManager()
    {
        ResetFlags();
    }

    /// <summary>
    /// Limpia todas las flags (estado inicial)
    /// </summary>
    public static void ResetFlags() => ActiveGameFlags = [];

    /// <summary>
    /// Añade una flag
    /// </summary>
    public static void AddFlag(GameFlags flag)
    {
        if (!Enum.IsDefined(typeof(GameFlags), flag))
        {
            throw new ArgumentException($"[Game Flags] This flag does not exist: {flag}");
        }

        ActiveGameFlags.Add(flag);
    }

    /// <summary>
    /// Add a flag by its name as string (e.g., "Chapter1Completed")
    /// </summary>
    public static void AddFlag(string flagName)
    {
        if (string.IsNullOrWhiteSpace(flagName))
        {
            throw new ArgumentException("[Game flags] a flag cannot be empty");
        }

        if (Enum.TryParse(flagName, ignoreCase: true, out GameFlags parsedFlag))
        {
            if (!Enum.IsDefined(typeof(GameFlags), parsedFlag))
            {
                throw new ArgumentException($"[Game Flags] This flag does not exist: {flagName}");
            }

            AddFlag(parsedFlag);
        }
        else
        {
            throw new ArgumentException($"[Game Flags] the flag {flagName} cannot be parsed");
        }
}

    /// <summary>
    /// Eliminate a flag
    /// </summary>
    public static void RemoveFlag(GameFlags flag) => ActiveGameFlags.Remove(flag);

    /// <summary>
    /// Check if a flag is active
    /// </summary>
    public static bool HasFlag(GameFlags flag) => ActiveGameFlags.Contains(flag);

    /// <summary>
    /// Add a flag if it is not active, or remove it if it is already active
    /// </summary>
    public static void ToggleFlag(GameFlags flag)
    {
        if (!ActiveGameFlags.Remove(flag))
            AddFlag(flag);
    }

    /// <summary>
    /// All the active flags in the actual game state
    /// </summary>
    public static IEnumerable<GameFlags> GetAllFlags() => ActiveGameFlags;
}

public enum GameFlags{
    Chapter1Completed,
    Chapter2Completed,
    Chaper3Completed
}