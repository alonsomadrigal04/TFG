using Game.Common.Modules;
using Godot;
using System;
using System.Collections.Generic;

public partial class TagParser
{

    public static List<TagToken> Parse(string text)
    {
        List<TagToken> tagTokenList = [];

        int i = 0;
        while (i < text.Length)
        {
            if (text[i] == '[')
            {
                int closingBracket = text.IndexOf(']', i);
                if (closingBracket != -1)
                {
                    string tag = text.Substring(i, closingBracket - i + 1);
                    string tagContent = tag.Trim('[', ']');
                    bool isClosingTag = tagContent.StartsWith('/');
                    string baseTagName = isClosingTag ? tagContent[1..] : tagContent.Split(' ')[0];

                    if (isClosingTag)
                    {
                        tagTokenList.Add(new CloseTagToken(baseTagName));
                    }
                    else
                    {
                        var parts = tagContent.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        var parameters = new Dictionary<string, string>();

                        for (int j = 1; j < parts.Length; j++)
                        {
                            var param = parts[j].Split('=', 2);
                            if (param.Length == 2)
                                parameters[param[0]] = param[1];
                        }

                        tagTokenList.Add(new OpenTagToken(baseTagName, parameters));
                    }

                    i = closingBracket + 1;
                    continue;
                }
                else
                {
                    string remainingText = text[i..];
                    tagTokenList.Add(new TextToken(remainingText));
                    break;
                }
            }
            else
            {
                int nextTag = text.IndexOf('[', i);
                string segment = nextTag != -1 ? text[i..nextTag] : text[i..];
                tagTokenList.Add(new TextToken(segment));
                i = nextTag == -1 ? text.Length : nextTag;
            }
        }

        return tagTokenList;
    }

}






/*
                if (baseTagName == "speed")
                {
                    if (!isClosingTag)
                    {
                        float newSpeed = currentSpeed;
                        var parts = tagContent.Split(' ');
                        foreach (var part in parts)
                        {
                            if (part.StartsWith("s=") &&
                                float.TryParse(part.AsSpan(2), NumberStyles.Float, CultureInfo.InvariantCulture, out float sValue))
                            {
                                newSpeed = sValue;
                            }
                        }

                        speedStack.Push(currentSpeed);
                        currentSpeed = newSpeed;
                    }
                    else if (speedStack.Count > 0)
                        currentSpeed = speedStack.Pop();

                    i = closingBracket + 1;
                    continue;
                }

                if (baseTagName == "w" || baseTagName == "s")
                {
                    string expandedTag;

                    if (!isClosingTag)
                    {
                        expandedTag = baseTagName == "w"
                            ? $"[wave {waveParams}]"
                            : $"[shake {shakeParams}]";
                    }
                    else
                    {
                        expandedTag = baseTagName == "w" ? "[/wave]" : "[/shake]";
                    }

                    cleanText += expandedTag;

                    if (isClosingTag)
                    {
                        if (tagStack.Count > 0)
                            tagStack.Pop();
                    }
                    else
                    {
                        tagStack.Push(expandedTag);
                    }

                    i = closingBracket + 1;
                    continue;
                }

                if (isClosingTag)
                {
                    if (tagStack.Count > 0)
                        tagStack.Pop();
                }
                else
                {
                    tagStack.Push(tagName);
                }

                cleanText += tag;
                i = closingBracket + 1;
                continue;*/