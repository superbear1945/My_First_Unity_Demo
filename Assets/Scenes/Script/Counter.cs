using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Counter : MonoBehaviour
{
    [SerializeField] int _price = 10;

    public enum CounterType // ö�ٶ��壬��Ϊ public �Ա� Inspector ����
    {
        Melee1 = 3,
        SingleGun = 0,
        AutoGun = 1,
        HP = 2
    }
    [SerializeField] public CounterType _counterType; // ���� CounterType ���͵��ֶβ����л�
    [SerializeField]static int _purchaseHPCount = 0;

    void Start()
    {
        // ����չ̨�Ƿ��ѱ�����
        if (CounterStatusManager.IsPurchased(_counterType))
        {
            gameObject.SetActive(false); // ����ѹ�������ô�չ̨
            return; // ������ִ�к����߼�
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
        // ��������ӹ����߼�
        if (UIManager._instance._coin < _price)
        {
            Debug.Log("��Ҳ��㣬�޷�����");
            return;
        }
        Debug.Log($"���̵� {shop.name} ���й��");
        UIManager._instance._coin -= _price;
        if (!gameObject.CompareTag("HpShop"))
        {
            // ������֮ǰ���Ϊ�ѹ���
            CounterStatusManager.MarkAsPurchased(_counterType);
            Destroy(gameObject);
        }
        else
        {
            _purchaseHPCount++;
            if (_purchaseHPCount >= 9) //����9��HP������HP�̵�
            {
                CounterStatusManager.MarkAsPurchased(_counterType);
                Destroy(gameObject);
            }
        }
    }
}
