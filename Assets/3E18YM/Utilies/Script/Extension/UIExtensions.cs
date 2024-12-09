using System;
using System.Threading.Tasks;
using UnityEngine.UI;

public static class UIExtensions
{
    private static bool isMethodExecuting = false;

    public static void AddScrollToBottomListener(this ScrollRect scrollRect, float threshold, Func<Task> asyncMethod)
    {
        scrollRect.onValueChanged.AddListener(async position =>
        {
            if (position.y <= threshold && !isMethodExecuting)
            {
                isMethodExecuting = true; // 標記正在執行
                await asyncMethod(); // 異步執行方法
                isMethodExecuting = false; // 重置標誌
            }
        });
    }

    public static void AddScrollToBottomListener(this ScrollRect scrollRect, float threshold, Action Method)
    {
        scrollRect.onValueChanged.AddListener(position =>
        {
            if (position.y <= threshold && !isMethodExecuting)
            {
                isMethodExecuting = true; // 標記正在執行
                Method(); // 異步執行方法
                isMethodExecuting = false; // 重置標誌
            }
        });
    }
    
}