using Game.Common.Modules;
using Godot;
using System;
using System.Collections.Generic;

public class TagParser
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