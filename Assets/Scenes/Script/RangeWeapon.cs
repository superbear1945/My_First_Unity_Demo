using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RangeWeapon : MonoBehaviour
{
    private Animator _animator;
    [SerializeField]GameObject _muzzle;
    [SerializeField]GameObject _bullet;
    Animator _atr;
    [SerializeField] AnimationClip _flippedFireAnim;
    [SerializeField] AnimationClip _orignalFireAnim;
    GameObject _player;
    AudioSource _fireAudio;

    // Start is called before the first frame update
    void Start()
    {
        //Initialize property
        _animator = GetComponent<Animator>();
        _atr = GetComponent<Animator>();
        _player = GameObject.FindWithTag("Player");
        _fireAudio = GetComponent<AudioSource>();

        //other logic
    }

    public void SemiRangeAttack()
    {
        if (_fireAudio != null)
            _fireAudio.Play();
        Instantiate(_bullet, _muzzle.transform.position, Quaternion.identity);
    }
};