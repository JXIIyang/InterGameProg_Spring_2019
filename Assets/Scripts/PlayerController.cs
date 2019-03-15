using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;


public class PlayerController : MonoBehaviour
{

    
    private Animator _animator;
    private Rigidbody2D rb;
    private SpriteRenderer _renderer;
    private AudioSource _audio;
    
    

    // Key mapping
    public KeyCode RightKeyCode;
    public KeyCode LeftKeyCode;
    public KeyCode JumpKeyCode;
    public KeyCode DashKeyCode;   
    public KeyCode HeartKeyCode;
    
    
   
    
    public int Movement;
    
    // public Input
    public float WalkSpeed;
    public float DashSpeed;
    public float JumpForce;
    public float DashTime;
    public ParticleSystem DashPar;
        
    private float _alpha;

    
    
    // public GameObject Target;
    public GameObject PlayerInverted;
   
        
    // dash
    public float _gravity;

    // audio
    public AudioClip Hint;
    private bool _meetNPC;   
    private Vector3 _progression;


    public static PlayerController Singleton;

    public enum PlayerState
    {
        Idle,
        Walk,
        Hurt,
        Dash,
        Jump,    
    }

    public PState State;
    public Dictionary<PlayerState,PState> States = new Dictionary<PlayerState, PState>();

    
    
    
    
    void Awake()
    {
        // Get Components that I need
        _animator = GetComponent<Animator>();
        _renderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        _audio = GetComponent<AudioSource>();
        _gravity = rb.gravityScale;
        
    }


    void Start()
    {
        PlayerController.Singleton = this;       
        _progression = transform.position;
               

        AddState(new PState(PlayerState.Idle));
        AddState(new WalkState());
        AddState(new HurtState());
        AddState(new JumpState());
        AddState(new DashState());
        SetState(PlayerState.Idle);
    }


    void Update()
    {
        if (State == null || State.InControl)
        {
            Inputs();
        }

        if (State != null)
        {
            State.Run(this);         
        }

        GameProgressionCheck();
        Debug.Log("Progression: " + _progression);
    }

    public void Inputs()
    {
        if (Input.GetKeyDown(LeftKeyCode) || Input.GetKey(LeftKeyCode) && !Input.GetKey(RightKeyCode))
        {
            _renderer.flipX = true;
            Movement = -1;

        }

        if (Input.GetKeyDown(RightKeyCode) || Input.GetKey(RightKeyCode) && !Input.GetKey(LeftKeyCode))
        {
            _renderer.flipX = false;
            Movement = 1;
        }

        if (!Input.GetKey(LeftKeyCode) && !Input.GetKey(RightKeyCode))
        {
            if (State.State != PlayerState.Dash)
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }

                SetState(PlayerState.Idle);
        }
        
        if ((Input.GetKey(LeftKeyCode) || Input.GetKey(RightKeyCode)) && State.State != PlayerState.Dash && State.State != PlayerState.Hurt)
        {
            SetState(PlayerState.Walk);
            
        }
        if (Input.GetKeyDown(DashKeyCode))
        {
            SetState(PlayerState.Dash);
        }


        if ((State.State == PlayerState.Walk || State.State == PlayerState.Idle) && Input.GetKeyDown(JumpKeyCode))
        {
            SetState(PlayerState.Jump);
        }


        if (_meetNPC)
        {
            if (Input.GetKeyDown(HeartKeyCode))
            {
                GameManager.Singleton.HeartNum --;
                _audio.PlayOneShot(Hint);
                Instantiate(GameManager.Singleton.HeartPrefab, transform.position + Vector3.up, Quaternion.identity);
                NpcController.Singleton.PlayerReply = true;
                _meetNPC = false;
            }
        }

        
        
    }
    
    
    public void AddState(PState s)
    {
        States.Add(s.State,s);
    }



    void GameProgressionCheck()
    {
        float tempAlpha1 = 0f;
        float tempAlpha2 = 0f;
        if (transform.position.x >= _progression.x)
        {
            _progression = transform.position;
        }
        
        if (!GameManager.Singleton.EndGame)
        {
            if ( Mathf.Abs(transform.position.x - PlayerInverted.transform.position.x) > 20)
            {
                tempAlpha1 = (Mathf.Abs(transform.position.x - PlayerInverted.transform.position.x) - 20) * 0.1f;
                GameManager.Singleton.Audio.volume = Mathf.Lerp(GameManager.Singleton.Audio.volume, 0, Time.deltaTime);
            }
            if (_progression.x - transform.position.x > 15)
            {
                tempAlpha2 = (_progression.x - transform.position.x - 15) * 0.1f;
                GameManager.Singleton.Audio.volume = Mathf.Lerp(GameManager.Singleton.Audio.volume, 0, Time.deltaTime);
            }
            else
            {
                _alpha = 0.0f;
            }
            
            if (_alpha >= 1.0f)
            {
                SceneManager.LoadScene("End");
            }

            _alpha = Mathf.Max(tempAlpha1, tempAlpha2);
            GameManager.Singleton.Overlay.color = new Color(0, 0, 0, _alpha);
        }
        else 
        {
            _alpha += 0.2f * Time.deltaTime;
            GameManager.Singleton.Overlay.color = new Color(1,1,1,_alpha);
            if (_alpha >= 1f)
            {
                SceneManager.LoadScene("Win");
            }
        }       

        
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("NPCTriger"))
        {
            _meetNPC = true;
        }

        if (other.CompareTag("Win"))
        {
            GameManager.Singleton.Win();
            
        }
        
        
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (State.State == PlayerState.Jump && collision.gameObject.CompareTag("Platform"))
        {
            SetState(PlayerState.Idle); 
        }

         if (State.State == PlayerState.Dash && collision.gameObject.CompareTag("Obstacle"))
        {
            Destroy(collision.gameObject);
        }
        
        if (State.State != PlayerState.Hurt && collision.gameObject.CompareTag("NPC") && collision.gameObject.GetComponent<NpcController>().State.State == NpcController.NPCState.Attack)
        {
            SetState(PlayerState.Hurt);
            

        }
     
        
    }

    
    public class PState
    {
        public PlayerState State;
        public bool InControl = true;
        public string TriggerName;
        
        
        public PState()
        { }
        
        public PState(PlayerState s)
        {
            State = s;
            TriggerName = "";
        }
        
        public PState(PlayerState s, string t)
        {
            State = s;
            TriggerName = t;
        }

        public virtual void OnStart(PlayerController c)
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
 
        public virtual void Run(PlayerController c)
        { }
 
        public virtual void OnEnd(PlayerController c)
        { }
    }
    
    public void SetState(PlayerState s)
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

    public class HurtState : PState
    {
        public Vector3 FallPos;
        public HurtState()
        {
            State = PlayerState.Hurt;
            TriggerName = "Fall";
            InControl = false;
        }

        public override void OnStart(PlayerController c)
        {
            base.OnStart(c);
            FallPos = c.transform.position;
            GameManager.Singleton.HeartNum --;
            GameManager.Singleton.HeartNum --;
        }
        
        public override void Run(PlayerController c)
        {
            if (Mathf.Abs(c.transform.position.x - FallPos.x) >= 10)
                //|| Mathf.Abs(c.rb.velocity.x) <= 0f
            {           
                c.SetState(PlayerState.Idle);     
            }
            else
            {
                c.rb.AddForce(Vector2.left * 100);
            }
        }

        public override void OnEnd(PlayerController c)
        {
            c.rb.velocity = new Vector2(0.0f, c.rb.velocity.y);
        }
    }
    
    
    public class WalkState : PState
    {
        public WalkState()
        {
            State = PlayerState.Walk;
            TriggerName = "Walk";
        }

        public override void Run(PlayerController c)
        {
            c.rb.velocity = new Vector2(Mathf.Lerp(Mathf.Abs(c.rb.velocity.x), c.WalkSpeed, Time.deltaTime * 4f) * c.Movement, c.rb.velocity.y);
        }
        
        
        public override void OnEnd(PlayerController c)
        {
            c.rb.velocity = new Vector2 (0, c.rb.velocity.y);
        }
        
    }
    
    public class JumpState : PState
    {
        public JumpState()
        {
            State = PlayerState.Jump;
            TriggerName = "Jump";
            InControl = false;
        }


        public override void Run(PlayerController c)
        {


            if (c.rb.velocity.y > 20)
            {
                c.rb.velocity = new Vector2(c.rb.velocity.x, 20f);
            }
            else
            {
                c.rb.AddForce(Vector2.up * c.JumpForce);
            }
         }

        public override void OnEnd(PlayerController c)
        {
            InControl = true;
        }
    }
    
    
    public class DashState : PState
    {
        public float DashTimer;
                
        public DashState()
        {
            State = PlayerState.Dash;
            TriggerName = "Dash";
            InControl = false;          
        }

        public override void OnStart(PlayerController c)
        {
            base.OnStart(c);
            DashTimer = c.DashTime;
            Debug.Log("PlayerDash");
        }
        
        
        public override void Run(PlayerController c)
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
                c.SetState(PlayerState.Idle);
            }
        }


        public override void OnEnd(PlayerController c)
        {
            DashTimer = c.DashTime;  
            c.rb.velocity = new Vector2 (0, c.rb.velocity.y);
            c.rb.gravityScale = c._gravity;
        }       
    }
    
}




