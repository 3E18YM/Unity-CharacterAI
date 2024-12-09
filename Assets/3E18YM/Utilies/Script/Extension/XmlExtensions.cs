using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEngine;

public static class XmlExtensions
{
    public static List<XmlElementData> GetAllTags(this string text)
    {
        var xmlContent = $"<root>{text}</root>";
        var doc = XDocument.Parse(xmlContent);

        var result = new List<XmlElementData>();

        foreach (var node in doc.Root.Nodes())
            if (node is XElement element)
            {
                var elementData = new XmlElementData
                {
                    Tag = element.Name.LocalName,
                    Content = element.Nodes().OfType<XText>().FirstOrDefault()?.Value.Trim() ?? "",
                    XmlText = element.ToString()
                };

                foreach (var attribute in element.Attributes()) elementData.Attributes.Add(attribute.Name.LocalName, attribute.Value);
                result.Add(elementData);
            }
            else if (node is XText textNode)
            {
                var trimmedValue = textNode.Value.Trim();
                if (!string.IsNullOrEmpty(trimmedValue))
                    result.Add(new XmlElementData
                    {
                        Tag = "",
                        Content = trimmedValue,
                        XmlText = trimmedValue
                    });
            }

        return result;


    }

    private static void TraverseNodes(XElement element, List<XmlElementData> result)
    {
        // 添加当前元素及其属性
        var elementData = new XmlElementData
        {
            Tag = element.Name.LocalName,
            Content = element.Nodes().OfType<XText>().FirstOrDefault()?.Value.Trim() ?? ""
        };

        foreach (var attribute in element.Attributes()) elementData.Attributes.Add(attribute.Name.LocalName, attribute.Value);

        result.Add(elementData);

        // 递归处理子节点
        foreach (var node in element.Nodes())
            if (node is XElement childElement)
            {
                TraverseNodes(childElement, result);
            }
            else if (node is XText textNode)
            {
                var trimmedValue = textNode.Value.Trim();
                if (!string.IsNullOrEmpty(trimmedValue))
                    result.Add(new XmlElementData
                    {
                        Tag = "",
                        Content = trimmedValue
                    });
            }
    }

    public static List<KeyValuePair<string, string>> GetSpecifyTags(this string text, string tag)
    {
        var xmlContent = $"<root>{text}</root>";
        XDocument doc;

        doc = XDocument.Parse(xmlContent);

        var result = new List<KeyValuePair<string, string>>();
        // Traverse the document to get all elements and text nodes
        foreach (var node in doc.Root.Nodes())
            if (node is XElement element)
            {
                if (element.Name.LocalName == tag)
                    result.Add(new KeyValuePair<string, string>(element.Name.LocalName, element.Value));
                else
                    result.Add(new KeyValuePair<string, string>("", element.ToString()));
            }
            else if (node is XText textNode)
            {
                result.Add(new KeyValuePair<string, string>("", textNode.Value.Trim()));
            }

        return result;

    }

    public static List<string> GetTagValue(this string text, string tag)
    {
        var xmlContent = $"<root>{text}</root>";
        var doc = XDocument.Parse(xmlContent);
        var words = doc.Descendants(tag).Select(x => x.Value).ToList();
        return words;

    }

    public static List<KeyValuePair<string, double>> GetTagAttributesByWord(this XDocument doc, string tag, string attribute = null)
    {

        var _list = doc.Descendants(tag)
            .SelectMany(x => x.Value.Split(' ')
                .Select(word => new KeyValuePair<string, double>(
                    word,
                    double.TryParse(x.Attribute(attribute)?.Value, out var sValue) ? sValue : 1.0f)))
            .ToList() ?? null;
        return _list;

    }

    public static T GetAttribute<T>(this XmlElementData elementData, string field, T defaultValue = default)
    {
        if (elementData.Attributes.TryGetValue(field, out var attributeString))
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    return (T)converter.ConvertFromString(attributeString);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                return defaultValue;
            }
        }
        return defaultValue;
    }
    public static string WrapXmlTagFromIndex(this string text, int index, string tag)
    {
        if (string.IsNullOrEmpty(text) || index < 0 || index >= text.Length)
            return text;

        var startTag = $"<{tag}>";
        var endTag = $"</{tag}>";

        return IndexXmlTagHandler(text, index, startTag, endTag);

    }

    public static string RemoveXmlTagFromIndex(this string text, int index, string tag)
    {
        if (string.IsNullOrEmpty(text) || index < 0 || index >= text.Length)
            return text;

        var startTag = $"<{tag}>";
        var endTag = $"</{tag}>";
        return IndexXmlTagHandler(text, index, endTag, startTag);


    }

    public static string RemoveSiblingXmlTagFromIndex(this string text, int index, string tag)
    {

        // 首先，检查索引是否在任何遮罩区域内
        var maskRegex = new Regex($"<mask>(.*?)</mask>");
        var matches = maskRegex.Matches(text);

        var startTag = $"<{tag}>";
        var endTag = $"</{tag}>";

        foreach (Match match in matches)
            if (index >= match.Index && index < match.Index + match.Length)
            {
                // 移除遮罩标签
                text = text.Remove(match.Index + match.Length - endTag.Length, endTag.Length);
                text = text.Remove(match.Index, startTag.Length);
                return text;
            }
        return text;

    }

    public static string IndexXmlTagHandler(this string text, int index, string startTag, string endTag)
    {
        // 检查索引左侧或右侧是否有遮罩标签
        var leftIsEndTag = index >= endTag.Length && text.Substring(index - endTag.Length, endTag.Length) == endTag;
        var rightIsStartTag = index + startTag.Length < text.Length && text.Substring(index + 1, startTag.Length) == startTag;


        if (leftIsEndTag && rightIsStartTag)
        {
            text = text.Remove(index + 1, startTag.Length);
            text = text.Remove(index - endTag.Length, endTag.Length);
            return text;


        }
        else if (leftIsEndTag)
        {
            text = text.MoveCharacter(index, index - endTag.Length);
            return text;


        }
        else if (rightIsStartTag)
        {
            text = text.MoveCharacter(index, index + startTag.Length);
            return text;

        }
        else
        {
            text = index + 1 >= text.Length ? text + endTag : text.Insert(index + 1, endTag);
            text = text.Insert(index, startTag);
            return text;
        }


    }

}

public class XmlElementData
{
    public string Tag { get; set; }
    public string Content { get; set; }
    public Dictionary<string, string> Attributes { get; set; }

    public string XmlText;

    public XmlElementData(string xmlText = null, string tag = null, string content = null, Dictionary<string, string> attributes = null)
    {
        XmlText = xmlText;
        Tag = tag;
        Content = content;
        Attributes = attributes ?? new Dictionary<string, string>();
    }

    public XmlElementData(XmlElementData xmlElementData, string xmlText = null, string tag = null, string content = null, Dictionary<string, string> attributes = null)
    {
        XmlText = xmlText ?? xmlElementData.XmlText;
        Tag = tag ?? xmlElementData.Tag;
        Content = content ?? xmlElementData.Content;
        Attributes = attributes ?? xmlElementData.Attributes;

    }
}