using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AutoRangeWeapon : MonoBehaviour
{
    private Animator _animator;
    public float _attackDelay = 0;
    [SerializeField]GameObject _muzzle;
    [SerializeField]GameObject _bullet;
    Animator _atr;
    [SerializeField] AnimationClip _flippedFireAnim;
    [SerializeField] AnimationClip _orignalFireAnim;
    GameObject _playerGameObject; // Renamed for clarity as it holds the GameObject
    Coroutine _autoFireCoroutine;
    AudioSource _fireAudio;

    // Start is called before the first frame update
    void Start()
    {
        //Initialize property
        _animator = GetComponent<Animator>();
        _attackDelay = GetAnimationClipLength(_animator, "Anim_Gun1Fire");
        _atr = GetComponent<Animator>();
        if (Player.Instance != null)
        {
            _playerGameObject = Player.Instance.gameObject;
        }
        else
        {
            Debug.LogError("Player instance not found in AutoRangeWeapon.");
        }
        _fireAudio = GetComponent<AudioSource>();

    }

    public void AutoRangeAttack()
    {
        if(_fireAudio != null)
                _fireAudio.Play();
        Instantiate(_bullet, _muzzle.transform.position, Quaternion.identity);
    }

    public void StartAutoFire()
    {
        _animator.SetBool("Fire", true);
    }
    
    public void StopAutoFire()
    {
        _animator.SetBool("Fire", false);
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

    private IEnumerator AttackDelay()
    {
        yield return new WaitForSeconds(_attackDelay);
    }

    //����Ϊai�����Զ������߼�

};
