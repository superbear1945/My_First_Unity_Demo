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
        
        if (shop != gameObject) return;
        // ��������ӹ����߼�
        if (UIManager._instance._coin < _price)
        {
            Debug.Log("��Ҳ��㣬�޷�����");
            return;
        }
        Debug.Log($"���̵� {shop.name} ���й��");
        UIManager._instance._coin -= _price;
        if(!gameObject.CompareTag("HpShop"))
            Destroy(gameObject);
    }
}
