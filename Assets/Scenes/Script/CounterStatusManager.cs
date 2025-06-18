using System.Collections.Generic;
using UnityEngine;


//���ڿ���Counter����״̬
public static class CounterStatusManager
{
    // ʹ�� Counter.CounterType ��Ϊ��
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

    // ��ѡ�����ϣ������Ϸ�Ự����������״̬�����ڲ��ԣ���������Ӵ˷���
    public static void ResetAllCounterStatus()
    {
        purchasedCounters.Clear();
        Debug.Log("All counter statuses have been reset.");
    }
}
