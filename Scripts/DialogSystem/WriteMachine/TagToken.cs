using Godot;
using System;
using System.Collections.Generic;

#nullable enable

public abstract class TagToken(string content)
{
    public string Content { get; } = content;
}

public class TextToken(string content) : TagToken(content)
{
}

public class OpenTagToken(string content, Dictionary<string, string>? parameters = null) : TagToken(content)
{
    public Dictionary<string, string> Parameters { get; } = parameters ?? [];
}

public class CloseTagToken(string content) : TagToken(content)
{
}
