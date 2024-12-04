using System;
using UnityEngine;

public static class MathExtensions
{
    // 扩展方法，用于重新映射一个数值的范围
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    public static Vector2 FindClosestFactors(this int num)
    {
        // 從最接近平方根的數開始尋找因數
        var sqrt = (int)Math.Sqrt(num);
        var l = num < 5 ? 0 : 1;
        for (var i = sqrt; i > l; i--)
            if (num % i == 0)
                return new Vector2(num / i, i); // 如果找到因數，直接返回
        // 無法找到精確的因數，返回最接近的數
        return FindClosestFactors(num + 1);

    }

    public static double IsNaN(this double value)
    {
        return double.IsNaN(value) ? 0 : value;
    }
}