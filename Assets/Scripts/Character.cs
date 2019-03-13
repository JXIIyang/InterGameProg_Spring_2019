using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    
    public Animator _animator;
    public Rigidbody2D rb;
    public SpriteRenderer _renderer;
    public AudioSource _audio;
    public float _gravity;
    
    // Start is called before the first frame update

    
    public enum PlayerState
    {
        Idle,
        Walk,
        Hurt,
        Dash,
        Jump,    
    }
    
    public void Awake()
    {
        GetComponents();
    }
    
    
    
    void Start()
    {
//        OnStart();

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public virtual void GetComponents()
    {
        _animator = GetComponent<Animator>();
        _renderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        _audio = GetComponent<AudioSource>();
        _gravity = rb.gravityScale;
    }
//
//    public virtual void OnStart()
//    {
//        AddState(new PState(PlayerState.Idle));
//        AddState(new WalkState());
//        AddState(new HurtState());
//        AddState(new JumpState());
//        AddState(new DashState());
//        SetState(PlayerState.Idle);
//    }
    
    
    
}
