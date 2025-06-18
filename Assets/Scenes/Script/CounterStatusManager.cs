using System.Collections.Generic;
using UnityEngine;


//用于控制Counter购买状态
public static class CounterStatusManager
{
    // 使用 Counter.CounterType 作为键
    private static Dictionary<Counter.CounterType, bool> purchasedCounters = new Dictionary<Counter.CounterType, bool>();

    public static void MarkAsPurchased(Counter.CounterType counterType)
    {
        if (!purchasedCounters.ContainsKey(counterType))
        {
            purchasedCounters.Add(counterType, true);
        }
        else
        {
            purchasedCounters[counterType] = true;
        }
    }

    public static bool IsPurchased(Counter.CounterType counterType)
    {
        return purchasedCounters.ContainsKey(counterType) && purchasedCounters[counterType];
    }

    // 可选：如果希望在游戏会话结束后重置状态（用于测试），可以添加此方法
    public static void ResetAllCounterStatus()
    {
        purchasedCounters.Clear();
        Debug.Log("All counter statuses have been reset.");
    }
}
