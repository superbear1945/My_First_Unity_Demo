using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PopUpText : MonoBehaviour
{
    [SerializeField] Animator _animator;
    public void Create(int damage, GameObject resource)
    {
        PopUpText pop = Instantiate(this, resource.transform.position, Quaternion.identity, resource.transform);
        pop.GetComponent<TextMeshPro>().text = damage.ToString();
        if (resource.CompareTag("Player"))
        {
            pop.GetComponent<TextMeshPro>().color = Color.red;
        }
    }

    void Start()
    {
        StartCoroutine(Destroy());
    }

    IEnumerator Destroy()
    {
        float time = _animator.runtimeAnimatorController.animationClips[0].length;
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
