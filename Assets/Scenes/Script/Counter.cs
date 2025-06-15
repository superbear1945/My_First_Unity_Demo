using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Counter : MonoBehaviour
{
    [SerializeField] int _price = 10;

    public enum CounterType // ö�ٶ��壬��Ϊ public �Ա� Inspector ����
    {
        Melee1,
        SingleGun,
        AutoGun,
        HP
    }
    [SerializeField] public CounterType _counterType; // ���� CounterType ���͵��ֶβ����л�

    void Start()
    {
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
