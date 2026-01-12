using Godot;
using System;
using System.Collections.Generic;

public class CommandToken(string subject, string verb, string[] args)
{
    public string Verb {get; set;} = verb;
    public string Subject {get; set;} = subject;
    public IReadOnlyList<string> Arguments { get; init; } = args;
}