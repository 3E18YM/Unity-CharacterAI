using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using UnityEngine;
using Random = UnityEngine.Random;
using Markdig;

public static class StringExtensions
{
    static StringExtensions()
    {
    }
    /// <summary>
    /// 取出句子中最後一個英文單字
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    public static string ExtractLastEnglishWord(this string text)
    {
        // 使用正則表達式直接查找最後一個英文單字
        var match = Regex.Match(text, @"[a-zA-Z]+(?=\s*[^a-zA-Z]*$)");
        return match.Success ? match.Value : "";
    }
    /// <summary>
    /// 取出每個單字
    /// </summary>
    /// <param name="sentence"></param>
    /// <returns></returns>
    public static List<string> SentenceWordToList(this string sentence)
    {
        List<string> words;
        if (sentence.IsChinese())
            // 如果是中文，逐字拆分
            words = SpitEveryText(sentence);
        else
            // 如果是英文，按原方式處理
            words = SplitEverySpace(sentence);

        return words;

    }
    public static List<string> SplitEverySpace(this string sentence)
    {
        return sentence.ToLower().Split(' ')
            .Select(word => Regex.Replace(word, "^[\\p{P}\\s]+|[\\p{P}\\s]+$", ""))
            .Where(word => !string.IsNullOrWhiteSpace(word))
            .ToList();
    }
    public static List<string> SpitEveryText(this string sentence)
    {
        return sentence.Where(c => !char.IsPunctuation(c) && !char.IsWhiteSpace(c))
            .Select(c => c.ToString())
            .ToList();
    }
    public static string UrlQueryParameter(this string url, string key)
    {
        try
        {
            var uri = new Uri(url);
            var query = uri.Query;
            var queryParams = HttpUtility.ParseQueryString(query);
            var parameter = queryParams[key];
            return parameter ?? url;
        }
        catch
        {

            return url;
        }


    }
    public static List<string> ExtractBracketsToList(this string input, char openBracket, char closeBracket, char delimiter = ',')
    {
        // 使用自訂的括號建立正則表達式
        var pattern = $@"\{openBracket}(.*?)\{closeBracket}";
        var regex = new Regex(pattern);
        var match = regex.Match(input);

        // 建立一個清單來存儲結果
        var result = new List<string>();

        if (match.Success)
        {
            // 取得括號中的內容
            var content = match.Groups[1].Value;

            // 用逗號分割並加入結果
            foreach (var item in content.Split(delimiter))
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                result.Add(item.Trim());
            }
        }

        return result;
    }
    public static string DoubleToStringPercent(this double value)
    {

        var Percent = (int)((value - 1.0) * 100); // 轉換為增減的百分比
        var Str = Percent.ToString() + "%";
        return Str;

    }
    public static string ToRichText(this string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
        {
            return string.Empty;
        }

        // 使用 Markdig 將 Markdown 轉換為 HTML
        string richText = Markdown.ToHtml(markdown);
        richText = richText.Replace("<strong>", "<b>").Replace("</strong>", "</b>");
        richText = richText.Replace("<em>", "<i>").Replace("</em>", "</i>");
        //richText = richText.Replace("<del>", "<s>").Replace("</del>", "</s>");
        string pattern = @"<(?!\/?(img\b|b\b|i\b|recommendPrompt\b|br\b)[^>]*>)[^>]+>";
        richText = Regex.Replace(richText, pattern, string.Empty, RegexOptions.IgnoreCase);
        return richText;
    }
    public static string GetImageLink(this string markdown)
    {
        // 連結 [text](url) 轉換為 <link="url">text</link>
        markdown = Regex.Replace(markdown, @"\[(.*?)\]\((.*?)\)", "<link=\"$2\">$1</link>");
        return markdown;
    }
    public static string ToIntString<T>(this T enumValue) where T : Enum
    {
        // 將 enum 轉換成 int
        var intValue = Convert.ToInt32(enumValue);
        // 將 int 轉換成 string 並返回
        return intValue.ToString();
    }
    public static string BuildSha256(this string rawData)
    {
        // 創建一個 SHA256
        using (var sha256Hash = SHA256.Create())
        {
            // 計算哈希值 - 返回的 byte 陣列
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            // 轉換成字串
            var builder = new StringBuilder();
            for (var i = 0; i < bytes.Length; i++) builder.Append(bytes[i].ToString("x2"));
            return builder.ToString();
        }
    }
    public static bool IsChinese(this string text)
    {
        var containsChinese = text.Any(c =>
                (c >= 0x4E00 && c <= 0x9FFF) || // 中文漢字範圍
                (c >= 0x3100 && c <= 0x312F) // 注音符號範圍
        );

        return containsChinese;
    }

    public static bool IsBoPoMo(this string text)
    {
        var containsBoPoMo = text.Any(c =>
                (c >= 0x3100 && c <= 0x312F) || // 注音符號範圍
                c == 0x02C7 || // 音符ˇ
                c == 0x02CB || // 音符ˋ
                c == 0x02CA || // 音符ˊ
                c == 0x02D9 // 音符˙
        );

        return containsBoPoMo;
    }

    private static readonly Dictionary<SystemLanguage, string> LanguageCodeMap = new()
    {
        { SystemLanguage.English, "en-US" },
        { SystemLanguage.Chinese, "zh-CN" },
        { SystemLanguage.ChineseSimplified, "zh-CN" },
        { SystemLanguage.ChineseTraditional, "zh-TW" },
        { SystemLanguage.Vietnamese, "vi-VN" }


    };
    public static string ToLanguageCode(this SystemLanguage systemLanguage)
    {
        return LanguageCodeMap[systemLanguage];
    }
    public static string MoveCharacter(this string input, int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= input.Length || toIndex < 0 || toIndex > input.Length) throw new ArgumentOutOfRangeException("Out of Index");

        // 把字串轉換成 char array
        var charArray = input.ToCharArray();

        // 取出要移動的字符
        var charToMove = charArray[fromIndex];

        // 將字符移出原位置
        var removedCharString = input.Remove(fromIndex, 1);

        // 將字符插入到新的位置
        var result = removedCharString.Insert(toIndex, charToMove.ToString());

        return result;
    }
    /// <summary>
    /// 在指定索引處替換為新字串
    /// </summary>
    /// <param name="original">原始字串</param>
    /// <param name="position">要替換的索引位置</param>
    /// <param name="replacement">新的字串</param>
    /// <returns>替換後的字串</returns>
    public static string ReplaceCharAt(this string original, int position, string replacement)
    {
        if (original == null)
            throw new ArgumentNullException(nameof(original), "原始字串不能為空");
        if (replacement == null)
            throw new ArgumentNullException(nameof(replacement), "替換字串不能為空");
        if (position < 0 || position > original.Length)
            throw new ArgumentOutOfRangeException(nameof(position), "替換位置超出範圍");

        // 使用 Substring 分割和重組字串
        var result = original.Substring(0, position) + replacement;

        if (position < original.Length)
        {
            result += original.Substring(position + replacement.Length); // 加上後續部分
        }
        return result;

    }
    public static string RandomMaskWord(this string sentence, int optionCount, string ignoreWord, string xmlTags = "WordMask", string attribute = null)
    {
        var startTag = string.IsNullOrWhiteSpace(attribute) ? $"<{xmlTags}>" : $"<{xmlTags} {attribute}>";
        var endTag = $"</{xmlTags}>";

        var words = Regex.Matches(sentence, @"\w+|[^\w\s]+")
            .Cast<Match>()
            .Select(m => m.Value)
            .ToList();

        // 找到答案单词的索引位置
        var answerIndexes = words
            .Select((word, index) => new { word, index })
            .Where(x => string.Equals(x.word, ignoreWord, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.index)
            .ToHashSet();

        // 确保答案单词被遮罩
        foreach (var index in answerIndexes) words[index] = $"{startTag}{words[index]}{endTag}";

        // 计算还需要遮罩的单词数量
        var remainingMaskCount = optionCount - answerIndexes.Count;

        if (remainingMaskCount > 0)
        {
            // 可供遮罩的单词索引（排除已遮罩的）
            var availableIndexes = Enumerable.Range(0, words.Count)
                .Except(answerIndexes)
                .ToList();

            // 随机选择索引进行遮罩
            for (var i = 0; i < remainingMaskCount && availableIndexes.Count > 0; i++)
            {
                var randomIndex = Random.Range(0, availableIndexes.Count);
                var selectedWordIndex = availableIndexes[randomIndex];
                words[selectedWordIndex] = $"{startTag}{words[selectedWordIndex]}{endTag}";
                availableIndexes.RemoveAt(randomIndex);
            }
        }

        return string.Join(" ", words);
    }
    
}