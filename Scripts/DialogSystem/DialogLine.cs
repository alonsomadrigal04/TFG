using Godot;
using System;
/// <summary>
/// Represents a single line of dialog in the dialog system. In the dialog CSV file, each line corresponds to an instance of this class.
/// </summary>
public class DialogLine
{
    public string Uid { get; set; }
    public string Type { get; set; }    
    public string Speaker { get; set; }
    public string Text { get; set;}
    public string Next { get; set;}
}