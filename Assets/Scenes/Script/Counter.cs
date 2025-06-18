using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Counter : MonoBehaviour
{
    [SerializeField] int _price = 10;

    public enum CounterType // 枚举定义，设为 public 以便 Inspector 访问
    {
        Melee1 = 3,
        SingleGun = 0,
        AutoGun = 1,
        HP = 2
    }
    [SerializeField] public CounterType _counterType; // 声明 CounterType 类型的字段并序列化
    [SerializeField]static int _purchaseHPCount = 0;

    void Start()
    {
        // 检查此展台是否已被购买
        if (CounterStatusManager.IsPurchased(_counterType))
        {
            gameObject.SetActive(false); // 如果已购买，则禁用此展台
            return; // 无需再执行后续逻辑
        }

        if (Player.Instance != null)
        {
            Player.Instance.OnShoppingEvent += ShopFunc;
        }
        else
        {
            Debug.LogError("Player instance not found for Counter event subscription.");
        }
    }

    void OnDisable()
    {
        if (Player.Instance != null)
        {
            Player.Instance.OnShoppingEvent -= ShopFunc;
        }
    }

    private void ShopFunc(GameObject shop)
    {

        if (shop != gameObject) return;
        // 在这里添加购物逻辑
        if (UIManager._instance._coin < _price)
        {
            Debug.Log("金币不足，无法购买。");
            return;
        }
        Debug.Log($"与商店 {shop.name} 进行购物。");
        UIManager._instance._coin -= _price;
        if (!gameObject.CompareTag("HpShop"))
        {
            // 在销毁之前标记为已购买
            CounterStatusManager.MarkAsPurchased(_counterType);
            Destroy(gameObject);
        }
        else
        {
            _purchaseHPCount++;
            if (_purchaseHPCount >= 9) //购买9个HP后，销毁HP商店
            {
                CounterStatusManager.MarkAsPurchased(_counterType);
                Destroy(gameObject);
            }
        }
    }
}
