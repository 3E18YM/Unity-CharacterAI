using System;
using UnityEngine;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using Sirenix.Utilities;

public static class DataExtensions
{
    /// <summary>
    /// 取得現在時間，年, 月, 日, 時, 分, 秒
    /// </summary>
    /// <returns></returns>
    public static DateTime GetCurrentTime()
    {

        var currentTime = DateTime.Now; // 獲得當前時間
        var currentTimeRounded = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour, currentTime.Minute, currentTime.Second);

        Debug.Log("Current Time: " + currentTimeRounded.ToString());
        return currentTimeRounded;

    }

    /// <summary>
    /// 從開始時間，到結束時間，計算時間差
    /// </summary>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    public static TimeSpan CountDown(this DateTime startTime, DateTime endTime = default)
    {
        if (endTime == default) endTime = DateTime.Now;
        var span = endTime - startTime; // 计算时间差
        // 计算总分钟数和剩余秒数
        var totalMinutes = (int)span.TotalMinutes;
        var seconds = span.Seconds; // 只取秒的整数部分

        var roundedSpan = new TimeSpan(0, totalMinutes, seconds); // 构建新的 TimeSpan，忽略小时

        Debug.Log("Count Down: Minutes: " + totalMinutes + ", Seconds: " + seconds); // 输出到控制台
        return roundedSpan;
    }
}
