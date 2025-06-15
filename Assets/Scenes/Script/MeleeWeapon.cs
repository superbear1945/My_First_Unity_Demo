using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
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
        _attackEnemyCount = 0; //重置攻击计数
    }

    void MeleeAttackStart()
    {
        _collider2D.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //只有用武器打到敌人才能伤害，其它东西打到或者打到的不是敌人都无效，并且在一次攻击中最多只能打到三个敌人
        if (collision.CompareTag("Enemy") && gameObject.CompareTag("Weapon") && _attackEnemyCount < 3)
        {
            Health enemy = collision.GetComponent<Health>();
            if (enemy._isHurt == true) return; //如果敌人已经受伤，则不再造成伤害
            enemy.CauseDamage(_damage, gameObject);
            _attackEnemyCount++;
        }
        
    }

    float GetAnimationClipLength(Animator targetAnimator, string clipName)//获取clipNmae片段动画播放一次的时长
    {
        if (targetAnimator == null || string.IsNullOrEmpty(clipName))
        {
            return 0f;
        }

        // 获取 Animator Controller
        RuntimeAnimatorController rac = targetAnimator.runtimeAnimatorController;
        if (rac == null)
        {
            return 0f;
        }

        // 遍历 Animator Controller 中的所有动画片段
        foreach (AnimationClip clip in rac.animationClips)
        {
            if (clip != null && clip.name == clipName)
            {
                return clip.length; // 返回动画片段的长度 (以秒为单位)
            }
        }
        return 0f;
    }
}
