using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using Random = UnityEngine.Random;

public class NpcController : MonoBehaviour
{   
    private Animator _animator;
    private Rigidbody2D rb;
    private SpriteRenderer _renderer;
    private AudioSource _audio;


    private bool _higherChance;

    [HideInInspector]
    public bool PlayerReply;

    public enum NPCState
    {
        Idle,
        Walk,
        Angry,
        Attack,
        Hurt,
        Hi,
        Wait,
        Leave
    }
    
    public NState State;
    public Dictionary<NPCState,NState> States = new Dictionary<NPCState, NState>();

//    [HideInInspector]
//    public NPCState State;

    private GameObject _player;


    private bool _saidhi;
    private bool _stateChange;
    

    
    private bool _movingLeft;
//    private int _direction; // 0: still; 1 left; 2 right;
    private bool _attacked;
    private bool _walking;
    
    
    private int _movement;
    
    // public Input
    public float WalkSpeed;
    public float DashSpeed;

//    public GameObject Target;
    
    
    
    // dash
    float timer = 0.0f;
    private bool _timerstart;
    private float _idleTimer = 0.0f; 
    private float _idletime; 
    private bool _leave; 

    public ParticleSystem DashPar;

    private Vector3 _fallPos;

    
    
    
    
    // audio

    public AudioClip Hint;
    public AudioClip Bad;
    


    private bool _dashAudioPlayed;
    
    
    void Awake()
    {
        // Get Components that I need
        _animator = GetComponent<Animator>();
        _renderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        _audio = GetComponent<AudioSource>();
        _player = GameObject.FindWithTag("Player");
    }


    void Start()
    {
        AddState(new NState(NPCState.Idle));
        AddState(new WalkState());
        AddState(new AngryState());
        AddState(new HurtState());
        AddState(new AttackState());
        AddState(new HiState());
        AddState(new LeaveState());
        State.State = NPCState.Walk;
        _renderer.flipX = true;
        _movement = -1;
        _idletime = Random.Range(1f, 4f);

    }


    void Update()
    
   
    {
        if (GameManager.Singleton.EndGame) return;

        if ( _player.transform.position.x - transform.position.x > 10)
        {
            Destroy(gameObject);
        }
        if ((State.State == NPCState.Wait || State.State == NPCState.Hi) && PlayerReply)
        {
            var chance = Random.value;
            
            if (chance < 0.25f )
            {
                GameManager.Singleton.AddHeart(1);
                _audio.PlayOneShot(Hint);
                Instantiate(GameManager.Singleton.HeartPrefab, transform.position + Vector3.up, Quaternion.identity);
                PlayerReply = false;
            }
            else if (chance > 0.25f && chance < 0.59f)
            {
                GameManager.Singleton.AddHeart(2);
                _audio.PlayOneShot(Hint);
                Instantiate(GameManager.Singleton.HeartPrefab, transform.position + Vector3.up, Quaternion.identity);
                Instantiate(GameManager.Singleton.HeartPrefab, transform.position + Vector3.up * 1.5f, Quaternion.identity);
                PlayerReply = false;
            }
            else if (chance > 0.61f)
            {
                GameManager.Singleton.HeartNum --;
                _audio.PlayOneShot(Bad);
                Instantiate(GameManager.Singleton.BadHeartPrefab, transform.position + Vector3.up + Vector3.left * 0.5f, Quaternion.identity);
                _higherChance = true;
                PlayerReply = false;
            }
            

            _leave = true;
        }
        

        if (_leave)
        {
            _idleTimer += Time.deltaTime;
            if (_idleTimer > _idletime)
            {
                State.State = NPCState.Leave;    
            }
        }
        
        if (State.State != NPCState.Leave && _player.transform.position.x > transform.position.x)
        {
            _renderer.flipX = false;
            _movement = 1;
            _leave = true;
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

    // Update is called once per frame
    void FixedUpdate()
    {
        if (State.State == NPCState.Leave)
        {
            Physics2D.IgnoreCollision(_player.GetComponent<Collider2D>(), GetComponent<Collider2D>());
            _renderer.flipX = true;
            _movement = -1;
            _animator.SetBool("Walk", true);
            rb.velocity =
                new Vector2(Mathf.Lerp(Mathf.Abs(rb.velocity.x), WalkSpeed * 2, Time.deltaTime * 4f) * _movement,
                    rb.velocity.y);
            return;
        }

        if (State.State == NPCState.Walk)
        {
            _animator.SetBool("Walk", true);
            rb.velocity = new Vector2(Mathf.Lerp(Mathf.Abs(rb.velocity.x), WalkSpeed, Time.deltaTime * 4f) *_movement, rb.velocity.y);
        }
        
            
            
        if (State.State == NPCState.Attack)
        {
            _animator.SetBool("Walk", false);
            _animator.SetBool("Dash", true);
            if (_timerstart || transform.position.x >= _player.transform.position.x)
            {
                timer += Time.deltaTime;
            }

            if (timer < 0.5f)
                {
                    rb.velocity =
                        new Vector2(Mathf.Lerp(Mathf.Abs(rb.velocity.x), DashSpeed, Time.deltaTime * 3f) * -1,
                            rb.velocity.y);
                    DashPar.Play();
                }
                else
                {
                    SetState(NPCState.Leave);
                    timer = 0.0f;
                    _animator.SetBool("Dash", false);
                    DashPar.Stop();
                    rb.velocity = new Vector2(Mathf.Lerp(Mathf.Abs(rb.velocity.x), WalkSpeed * 2, Time.deltaTime * 4f) * -1,
                        rb.velocity.y);
                }

        }
            

 
        if (_attacked)
        {
            Fall();
        }
        
    }


    void LateUpdate()
    {
        if (!_attacked || (Mathf.Abs(rb.velocity.x) > 0.0f)) return;
        _attacked = false;
        _animator.SetBool("Fall", false);
    }




    void OnCollisionEnter2D(Collision2D collision)
    {

        if (!_stateChange && collision.gameObject.CompareTag("Player"))
        {
            if (State.State == NPCState.Attack)
            {
                _timerstart = true;
                _stateChange = true;
            }
            
            else if ( !_stateChange && PlayerController.Singleton.State.State == PlayerController.PlayerState.Dash){
                _attacked = true;
                _animator.SetBool("Fall", true);
                _fallPos = transform.position;
                _stateChange = true;
            }

            else if ( !_stateChange && State.State != NPCState.Angry && PlayerController.Singleton.State.State != PlayerController.PlayerState.Dash)
            {
                SetState(NPCState.Angry);
                _animator.SetBool("Angry", true);
                _stateChange = true;
            }
            
            else if (!_stateChange && State.State == NPCState.Angry && PlayerController.Singleton.State.State != PlayerController.PlayerState.Dash)
            {
                SetState(NPCState.Attack);
                _stateChange = true;
            }
            
        }
        
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        _stateChange = !_stateChange;
    }


    
    

    void Fall()
    {
        if (Mathf.Abs(transform.position.x - _fallPos.x) >= 10)
        {
            rb.velocity = new Vector2(0.0f, rb.velocity.y);
            _attacked = false;
            _animator.SetBool("Fall", false);
            SetState(NPCState.Attack);;
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

        public virtual void OnStart(NpcController c)
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
            Debug.Log(State);
        }
 
        public virtual void Run(NpcController c)
        { }
 
        public virtual void OnEnd(NpcController c)
        { }
    }
    
    public void SetState(NPCState s)
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
        State.OnStart(this);
    }
    
    
    
     public class HurtState : NState
    {
        public Vector3 FallPos;
        public HurtState()
        {
            State = NPCState.Hurt;
            TriggerName = "Fall";
        }

        public override void OnStart(NpcController c)
        {
            base.OnStart(c);
            FallPos = c.transform.position;
            GameManager.Singleton.HeartNum --;
            GameManager.Singleton.HeartNum --;
        }
        
        public override void Run(NpcController c)
        {
            if (Mathf.Abs(c.transform.position.x - FallPos.x) >= 10)
                //|| Mathf.Abs(c.rb.velocity.x) <= 0f
            {           
                c.SetState(NPCState.Idle);     
            }
            else
            {
                c.rb.AddForce(Vector2.left * 100);
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
            c.rb.velocity = new Vector2(Mathf.Lerp(Mathf.Abs(c.rb.velocity.x), c.WalkSpeed, Time.deltaTime * 4f) * c.Movement, c.rb.velocity.y);
        }
        
        
        public override void OnEnd(NpcController c)
        {
            c.rb.velocity = new Vector2 (0, c.rb.velocity.y);
        }
        
    }
    
    public class HiState : NState
    {
        public HiState()
        {
            State = NPCState.Hi;
            TriggerName = "";
        }
        
        public override void OnStart(NpcController c)
        {
            base.OnStart(c);
                var chance = Random.value;                      
                if (!_higherChance && chance > 0.8f && chance < 0.9f || _higherChance && chance > 0.7f && chance > 0.9f)
                {
                    GameManager.Singleton.AddHeart(1);                   
                    c._audio.PlayOneShot(c.Hint);
                    Instantiate(GameManager.Singleton.HeartPrefab, c.transform.position + Vector3.up, Quaternion.identity);
                }
                        
                else if (!_higherChance && chance > 0.7f && chance < 0.8f || _higherChance && chance > 0.55f && chance > 0.7f)
                {
                    GameManager.Singleton.AddHeart(2);
                    c._audio.PlayOneShot(c.Hint);
                    Instantiate(GameManager.Singleton.HeartPrefab, c.transform.position + Vector3.up, Quaternion.identity);
                    Instantiate(GameManager.Singleton.HeartPrefab, c.transform.position + Vector3.up * 1.5f, Quaternion.identity);
                }
                else if (chance > 0.9f)
                {
                    GameManager.Singleton.HeartNum --;
                    c._audio.PlayOneShot(c.Bad);
                    Instantiate(GameManager.Singleton.BadHeartPrefab, c.transform.position + Vector3.up, Quaternion.identity);
                }
                {
                    c.SetState(NPCState.Wait);
                }
        }


        public override void Run(NpcController c)
        {

         }

        public override void OnEnd(NpcController c)
        {

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

        public override void OnStart(NpcController c)
        {
            base.OnStart(c);
            DashTimer = PlayerController.Singleton.DashTime;
        }
        
        
        public override void Run(NpcController c)
        {
            if (DashTimer > 0)
            {
                DashTimer -= Time.deltaTime;
                c.rb.gravityScale = 0.0f;
                c.rb.velocity = new Vector2(Mathf.Lerp(Mathf.Abs(c.rb.velocity.x), c.DashSpeed, Time.deltaTime * 3f) * c.Movement, c.rb.velocity.y);
                c.DashPar.Play();
            }
            else
            {
                c.SetState(NPCState.Idle);
            }
        }


        public override void OnEnd(NpcController c)
        {
            DashTimer = c.DashTime;  
            c.rb.velocity = new Vector2 (0, c.rb.velocity.y);
            c.rb.gravityScale = c._gravity;
        }       
    }
        
}
