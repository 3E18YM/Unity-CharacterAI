using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Random = UnityEngine.Random;

public static class ListExtensions
{
    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static List<T> Shuffle<T>(this List<T> ts)
    {
        var shuffledList = new List<T>(ts); // 建立一個新列表
        var count = shuffledList.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i)
        {
            var r = Random.Range(i, count);
            var tmp = shuffledList[i];
            shuffledList[i] = shuffledList[r];
            shuffledList[r] = tmp;
        }
        return shuffledList; // 返回打亂的新列表
    }

    public static List<T> RandomPick<T>(this List<T> source, int count)
    {
        if (count > source.Count) throw new ArgumentException("Count cannot be greater than the number of items in the list.");

        return source
            .Distinct() // 確保列表中元素唯一
            .OrderBy(_ => Random.value) // 隨機打亂列表
            .Take(count) // 取前 count 個
            .ToList(); // 返回作為新的 List
    }

    public static void MergeByProperty<T, TKey>(
        this List<T> listA,
        List<T> listB,
        Func<T, TKey> keySelector)
        where TKey : notnull
    {
        var dictA = listA.ToDictionary(keySelector, item => item);

        foreach (var itemB in listB)
        {
            var keyB = keySelector(itemB);
            if (dictA.ContainsKey(keyB))
            {
                // 如果找到，則取代
                var index = listA.FindIndex(a => keySelector(a).Equals(keyB));
                if (index != -1) listA[index] = itemB;
            }
            else
            {
                // 如果沒有找到，則添加
                listA.Add(itemB);
            }
        }
    }

    public static void ListsToDropdown(this TMP_Dropdown tMP_Dropdown, List<string> options)
    {
        // 清除現有選項
        tMP_Dropdown.ClearOptions();

        // 創建新的Dropdown選項列表
        var optionDataList = new List<TMP_Dropdown.OptionData>();
        foreach (var option in options) optionDataList.Add(new TMP_Dropdown.OptionData(option));

        // 添加選項到Dropdown
        tMP_Dropdown.AddOptions(optionDataList);
    }
}