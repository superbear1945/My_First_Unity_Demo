using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Counter : MonoBehaviour
{
    [SerializeField] int _price = 10;

    void Start()
    {
        Player.OnShopping += ShopFunc;
    }

    void OnDisable()
    {
        Player.OnShopping -= ShopFunc;
    }

    private void ShopFunc(GameObject shop)
    {
        Debug.Log($"与商店 {shop.name} 进行购物。");
        if (shop != gameObject) return;
        // 在这里添加购物逻辑
        if (UIManager._instance._coin < _price)
        {
            Debug.Log("金币不足，无法购买。");
            return;
        }
        UIManager._instance._coin -= _price;
    }
}
