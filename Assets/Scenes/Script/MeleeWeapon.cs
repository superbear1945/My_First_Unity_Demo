using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MeleeWeapon : MonoBehaviour
{
    private Animator _animator;
    public float _attackDelay = 0.3f;
    public bool _attackBlock = false;
    public int _damage = 1;
    Collider2D _collider2D;
    private int _attackEnemyCount = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        //Initialize property
        _animator = GetComponent<Animator>();
        _collider2D = GetComponent<Collider2D>();
        _attackDelay = GetAnimationClipLength(_animator, "Anim_ShovelAttack");

        _collider2D.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MeleeAttack()
    {
        if (_attackBlock) return;
        _animator.SetTrigger("Attack");
        _attackBlock = true;
        StartCoroutine(AttackDelay());
    }

    private IEnumerator AttackDelay()
    {
        yield return new WaitForSeconds(_attackDelay);
        _attackBlock = false;
    }

    void MeleeAnimStart()
    {
        
    }

    void MeleeEnd()
    {
        _collider2D.enabled = false;
        _attackEnemyCount = 0; //���ù�������
    }

    void MeleeAttackStart()
    {
        _collider2D.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //ֻ���������򵽵��˲����˺������������򵽻��ߴ򵽵Ĳ��ǵ��˶���Ч��������һ�ι��������ֻ�ܴ���������
        if (collision.CompareTag("Enemy") && gameObject.CompareTag("Weapon") && _attackEnemyCount < 3)
        {
            Health enemy = collision.GetComponent<Health>();
            if (enemy._isHurt == true) return; //��������Ѿ����ˣ���������˺�
            enemy.CauseDamage(_damage, gameObject);
            _attackEnemyCount++;
        }
        
    }

    float GetAnimationClipLength(Animator targetAnimator, string clipName)//��ȡclipNmaeƬ�ζ�������һ�ε�ʱ��
    {
        if (targetAnimator == null || string.IsNullOrEmpty(clipName))
        {
            return 0f;
        }

        // ��ȡ Animator Controller
        RuntimeAnimatorController rac = targetAnimator.runtimeAnimatorController;
        if (rac == null)
        {
            return 0f;
        }

        // ���� Animator Controller �е����ж���Ƭ��
        foreach (AnimationClip clip in rac.animationClips)
        {
            if (clip != null && clip.name == clipName)
            {
                return clip.length; // ���ض���Ƭ�εĳ��� (����Ϊ��λ)
            }
        }
        return 0f;
    }
}
