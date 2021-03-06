﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Timers;
using UnityEngine;
using Random = UnityEngine.Random;

public class NpcController : MonoBehaviour
{   
    private Animator _animator;
    private Rigidbody2D rb;
    private SpriteRenderer _renderer;
    private AudioSource _audio;



    [HideInInspector]
    public bool PlayerReply;
    [HideInInspector]
    public int HeartNum;

    public enum NPCState
    {
        Idle,
        Walk,
        Angry,
        Attack,
        Hurt,
        Hi,
        Leave,
        Sad
    }
    
    public NState State;
    public Dictionary<NPCState,NState> States = new Dictionary<NPCState, NState>();

    private bool _stateChange;    
    private int _movement;
    
    // public Input
    public float WalkSpeed;
    public float DashSpeed;

    
    

    public ParticleSystem DashPar;

    public static NpcController Singleton;

    
    
    
    
    // audio

    public AudioClip Hint;
    public AudioClip Bad;
    


    private bool _dashAudioPlayed;

    private float timer = 4f;
    
    
    void Awake()
    {
        // Get Components that I need
        _animator = GetComponent<Animator>();
        _renderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        _audio = GetComponent<AudioSource>();
        PlayerReply = false;
    }


    void Start()
    {
        if (NpcController.Singleton == null)
        {
            NpcController.Singleton = this;
        }
        else
        {
            Destroy(this);
        }
        AddState(new NState(NPCState.Idle));
        AddState(new WalkState());
        AddState(new AngryState());
        AddState(new HurtState());
        AddState(new AttackState());
        AddState(new HiState());
        AddState(new SadState());
        AddState(new LeaveState());
        rb.velocity = Vector3.zero;
        SetState(NPCState.Walk);
        WalkSpeed = 10f;
        DashSpeed = PlayerController.Singleton.DashSpeed;
        
        _renderer.flipX = true;
        _movement = -1;
        

    }


    void Update()  
    {
        if (GameManager.Singleton.EndGame) return;
        if (State == null)
        {
            SetState(NPCState.Walk);
        }else
        {
            State.Run(this);         
        }

        if ( PlayerController.Singleton.transform.position.x - transform.position.x > 10 || transform.position.x - PlayerController.Singleton.transform.position.x > 25 )
        {
            Destroy(gameObject);
        }      
        if (State.State != NPCState.Leave && State.State != NPCState.Sad  && State.State != NPCState.Attack && PlayerController.Singleton.transform.position.x > transform.position.x)
        {
            _renderer.flipX = false;
            _movement = 1;
            timer -= Time.deltaTime;
             if(timer <= 0 ){
                 SetState(NPCState.Leave, true);
                 timer = 4f;
             }
            
        }
        if (State.State != NPCState.Leave && State.State != NPCState.Sad && PlayerController.Singleton.transform.position.x < transform.position.x)
        {
            _renderer.flipX = true;
            _movement = -1; 
        }
        
        
    }
    
    
    
    public void AddState(NState s)
    {
        States.Add(s.State,s);
    }
    
    

    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("PlayerTrigger"))
        {
            if (State.State == NPCState.Idle || State.State == NPCState.Walk)
            {
                    SetState(NPCState.Hi);
            }
            
        }
    }
        




    void OnCollisionEnter2D(Collision2D collision)
    {

        
        if (collision.gameObject == PlayerController.Singleton.gameObject)
        {
            if ( PlayerController.Singleton.State.State == PlayerController.PlayerState.Dash){
                SetState(NPCState.Hurt);
                return;
            }

            if (State.State != NPCState.Leave && State.State != NPCState.Angry && State.State != NPCState.Attack && PlayerController.Singleton.State.State != PlayerController.PlayerState.Dash)
            {
                if (Random.value > 0.5f)
                {
                    SetState(NPCState.Angry);
                }
                else
                {
                    SetState(NPCState.Sad);
                }

                return;
            }
            
            if (State.State == NPCState.Angry && PlayerController.Singleton.State.State != PlayerController.PlayerState.Dash)
            {
                SetState(NPCState.Attack);
                return;
            }
            
        }
        
    }
    
    
    
    public class NState
    {
        public NPCState State;
        public string TriggerName;
        
        
        public NState()
        { }
        
        public NState(NPCState s)
        {
            State = s;
            TriggerName = "";
        }
        
        public NState(NPCState s, string t)
        {
            State = s;
            TriggerName = t;
        }

        public virtual void OnStart(NpcController c, bool instant = false)
        {
            foreach(AnimatorControllerParameter parameter in c._animator.parameters) 
            {
                if (parameter.name == TriggerName)
                {
                    c._animator.SetBool(TriggerName, true);
                }
                else
                {
                    c._animator.SetBool(parameter.name, false);
                }
            } 
        }
 
        public virtual void Run(NpcController c)
        { }
 
        public virtual void OnEnd(NpcController c)
        { }
        
    }
    
    public void SetState(NPCState s, bool instant = false)
    {
        if (State != null && State.State == s)
            return;
        if (!States.ContainsKey(s))
        {
            Debug.Log("ATTEMPTED TO USE INVALID STATE: " + s);
            return;
        }
        if (State != null)
            State.OnEnd(this);
        State = States[s];
        State.OnStart(this, instant);

    }
    
    
    
     public class HurtState : NState
    {
        public Vector3 FallPos;
        public HurtState()
        {
            State = NPCState.Hurt;
            TriggerName = "Fall";
        }

        public override void OnStart(NpcController c, bool instant = false)
        {
            base.OnStart(c);
            FallPos = c.transform.position;
        }
        
        public override void Run(NpcController c)
        {
            if (Mathf.Abs(c.transform.position.x - FallPos.x) >= 10)
                //|| Mathf.Abs(c.rb.velocity.x) <= 0f
            {           
                c.SetState(NPCState.Attack);     
            }
            else
            {
                c.rb.AddForce(Vector2.right * 100);
            }
        }

        public override void OnEnd(NpcController c)
        {
            c.rb.velocity = new Vector2(0.0f, c.rb.velocity.y);
        }
    }
    
    
    public class WalkState : NState
    {
        public WalkState()
        {
            State = NPCState.Walk;
            TriggerName = "Walk";
        }

        public override void Run(NpcController c)
        {
            c.rb.velocity = new Vector2(Mathf.Lerp(Mathf.Abs(c.rb.velocity.x), c.WalkSpeed, Time.deltaTime * 4f) * c._movement, c.rb.velocity.y);
        }
        
        
        public override void OnEnd(NpcController c)
        {
            c.rb.velocity = new Vector2 (0, c.rb.velocity.y);
        }
        
    }
    
    
    public class LeaveState : NState
    {
        public float WaitTime;
        public LeaveState()
        {
            State = NPCState.Leave;
            TriggerName = "";
            WaitTime = Random.Range(1.0f, 4.0f);
            
        }

        public override void OnStart(NpcController c, bool instant = false)
        {
            base.OnStart(c, instant);
            if (instant) WaitTime = 0.0f;
        }

        public override void Run(NpcController c)
        {
            WaitTime -= Time.deltaTime;
            if (WaitTime < 0)
            {
                Physics2D.IgnoreCollision(PlayerController.Singleton.GetComponent<Collider2D>(),
                    c.GetComponent<Collider2D>());
                c._animator.SetBool("Walk", true);
                c._renderer.flipX = true;
                c.rb.velocity = new Vector2(
                    Mathf.Lerp(Mathf.Abs(c.rb.velocity.x), c.WalkSpeed * 1.5f, Time.deltaTime * 4f) * -1,
                    c.rb.velocity.y);
            }
        }
    }
    
    
    
    public class SadState : NState
    {
        public float WaitTime;
        public SadState()
        {
            State = NPCState.Sad;
            TriggerName = "Sad";
            WaitTime = Random.Range(1.0f, 4.0f);
            
        }

        public override void Run(NpcController c)
        {
            WaitTime -= Time.deltaTime;
            if (WaitTime < 0)
            {
                c._renderer.flipX = false;
                Physics2D.IgnoreCollision(PlayerController.Singleton.GetComponent<Collider2D>(),
                    c.GetComponent<Collider2D>());
                c._animator.SetBool("Cry", true);
                c.rb.velocity = new Vector2(
                    Mathf.Lerp(Mathf.Abs(c.rb.velocity.x), c.WalkSpeed * 1.5f, Time.deltaTime * 4f),
                    c.rb.velocity.y);
            }
        }
    }
    
    
    
    public class AngryState : NState
    {
        public AngryState()
        {
            State = NPCState.Angry;
            TriggerName = "Angry";
        }
    }


    public class HiState : NState
    {
        public float waitTimer = Random.Range(4f, 5f);
        public HiState()
        {
            State = NPCState.Hi;
            TriggerName = "";
        }

        public override void OnStart(NpcController c, bool instant = false)
        {
            base.OnStart(c);
            var chance = Random.value;
            
            if (chance < 0.2f)
            {
                GameManager.Singleton.AddHeart(2);
                c.HeartNum += 2;
                c._audio.PlayOneShot(c.Hint);
                Instantiate(GameManager.Singleton.HeartPrefab, c.transform.position + Vector3.up,
                    Quaternion.identity);
                Instantiate(GameManager.Singleton.HeartPrefab, c.transform.position + Vector3.up * 1.5f,
                    Quaternion.identity);
            }

            else if (chance > 0.2f && chance < 0.35f)
            {
                GameManager.Singleton.HeartNum--;
                c.HeartNum += 1;
                c._audio.PlayOneShot(c.Bad);
                Instantiate(GameManager.Singleton.BadHeartPrefab, c.transform.position + Vector3.up,
                    Quaternion.identity);
            }

            else if (chance > 0.5f)
            {
                GameManager.Singleton.AddHeart(1);
                c.HeartNum += 1;
                c._audio.PlayOneShot(c.Hint);
                Instantiate(GameManager.Singleton.HeartPrefab, c.transform.position + Vector3.up,
                    Quaternion.identity);
            }
        }


        public override void Run(NpcController c)
        {
            if (c.PlayerReply)
            {
                var chance = Random.value;
                if (chance > 0.6f)
                {
                    GameManager.Singleton.AddHeart(1);
                    c._audio.PlayOneShot(c.Hint);
                    Instantiate(GameManager.Singleton.HeartPrefab, c.transform.position + Vector3.up * (1 + 0.5f * c.HeartNum),
                        Quaternion.identity);
                }

                else if (chance > 0.3f && chance < 0.5f)
                {
                    GameManager.Singleton.AddHeart(2);
                    c._audio.PlayOneShot(c.Hint);
                    Instantiate(GameManager.Singleton.HeartPrefab, c.transform.position + Vector3.up * (1 + 0.5f * c.HeartNum),
                        Quaternion.identity);
                    Instantiate(GameManager.Singleton.HeartPrefab, c.transform.position + Vector3.up * (1.5f + 0.5f * c.HeartNum),
                        Quaternion.identity);
                }
                else if (chance < 0.2f)
                {
                    GameManager.Singleton.HeartNum--;
                    c._audio.PlayOneShot(c.Bad);
                    Instantiate(GameManager.Singleton.BadHeartPrefab, c.transform.position + Vector3.up * (1 + 0.5f * c.HeartNum),
                        Quaternion.identity);
                }

                c.SetState(NPCState.Leave);
            }
/*            else
            {
                waitTimer -= Time.deltaTime;
                if (waitTimer < 0)
                {
                    c.SetState(NPCState.Leave);
                }
            }*/
            

        }
    }


    public class AttackState : NState
    {
        public float DashTimer;
                
        public AttackState()
        {
            State = NPCState.Attack;
            TriggerName = "Dash";         
        }

        public override void OnStart(NpcController c, bool instant = false)
        {
            base.OnStart(c);
            DashTimer = PlayerController.Singleton.DashTime * 2;
        }
        
        
        public override void Run(NpcController c)
        {
            if (DashTimer > 0)
            {
                DashTimer -= Time.deltaTime;
                c.rb.gravityScale = 0.0f;
                c.rb.velocity = new Vector2(Mathf.Lerp(Mathf.Abs(c.rb.velocity.x), c.DashSpeed, Time.deltaTime * 3f) * c._movement, c.rb.velocity.y);
                c.DashPar.Play();
            }
            else
            {
                c.SetState(NPCState.Leave, true);
            }
        }


        public override void OnEnd(NpcController c)
        {
            c.rb.velocity = new Vector2 (0, c.rb.velocity.y);
            c.rb.gravityScale = PlayerController.Singleton._gravity;
        }       
    }
        
}
