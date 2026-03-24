using Godot;
using System;
using System.Collections.Generic;
/// <summary>
/// Represents a dialog, which is a collection of dialog lines. The dialog is constructed from a CSV file, where each line corresponds to an instance of DialogLine. The Conversation property is a dictionary that maps UIDs to their corresponding DialogLine instances, while the orderedUids list maintains the order of the dialog lines as they appear in the CSV file. This structure allows for efficient retrieval of dialog lines by their UID while preserving the original sequence of the conversation.
/// </summary>
/// <param name="keyValuePairs">The dictionary of dialog lines, keyed by UID.</param>
/// <param name="orderedUids">The list of UIDs in the order they appear in the dialog.</param>
public class Dialog(Dictionary<string, DialogLine> keyValuePairs, List<string> orderedUids)
{
    public Dictionary<string, DialogLine> Conversation {get; private set;} = keyValuePairs;
    public List<string> OrderedUids {get; private set;} = orderedUids;
}