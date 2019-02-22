using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInverted : MonoBehaviour
{

    private Animator _animator;
    private Rigidbody2D rb;
    private SpriteRenderer _renderer;
    private AudioSource _audio;
    
    // public GameObject Target;
    public GameObject Player;
   
        
    private float _gravity;

    // audio
    public AudioClip Hint;
    public AudioClip Bad;

    public ParticleSystem DashPar;


    
    public PState State;
    public Dictionary<PlayerController.PlayerState,PState> States = new Dictionary<PlayerController.PlayerState, PState>();


    public static PlayerInverted Singleton;
    public KeyCode RejuvenateKeyCode;
    

    private float _RjTimer;
    private bool RJ;




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
        PlayerInverted.Singleton = this;
        _RjTimer = PlayerController.Singleton.DashTime;
        AddState(new PState(PlayerController.PlayerState.Idle));
        AddState(new WalkState());
        AddState(new HurtState());
        AddState(new JumpState());
        AddState(new DashState());

    }


    void Update()
    {
        SetState(PlayerController.Singleton.State.State);
        if (State == null || State.InControl)
        {
            PlayerController.Singleton.Inputs();
        }

        if (State != null)
        {
            State.Run(this);         
        }
        
        if (Input.GetKeyDown(PlayerController.Singleton.LeftKeyCode) || Input.GetKey(PlayerController.Singleton.LeftKeyCode) && !Input.GetKey(PlayerController.Singleton.RightKeyCode))
        {
            _renderer.flipX = true;

        }

        if (Input.GetKeyDown(PlayerController.Singleton.RightKeyCode) || Input.GetKey(PlayerController.Singleton.RightKeyCode) && !Input.GetKey(PlayerController.Singleton.LeftKeyCode))
        {
            _renderer.flipX = false;
        }

    }  
    
    public void AddState(PState s)
    {
        States.Add(s.State,s);
    }


    // Update is called once per frame
    void FixedUpdate()
   
    {
            if (Input.GetKeyDown(RejuvenateKeyCode) && GameManager.Singleton.HeartNum > 0)
            {
                RJ = true;
                GameManager.Singleton.HeartNum --;
                Debug.Log("LoseA");
                _audio.PlayOneShot(Bad);
            }


        // RJ

        if (RJ && _RjTimer >0)
        {
            _RjTimer -= Time.deltaTime;
            _animator.SetBool("Dash", true);
            rb.velocity =
                new Vector2(Mathf.Lerp(Mathf.Abs(rb.velocity.x), PlayerController.Singleton.DashSpeed, Time.deltaTime * 4f) * PlayerController.Singleton.Movement,
                    rb.velocity.y);
        }
        else if (RJ && _RjTimer <= 0)
        {
            RJ = false;
            _RjTimer = PlayerController.Singleton.DashTime;
            _animator.SetBool("Dash", false);     
            rb.velocity = new Vector2 (0, rb.velocity.y);
        }
        
    }


    void OnCollisionEnter2D(Collision2D collision)
    {

        if (State.State == PlayerController.PlayerState.Dash)
        {
            if (collision.gameObject.CompareTag("Obstacle"))
            {
                Destroy(collision.gameObject);
            }
            
        }
        
    }    void OnCollisionStay2D(Collision2D collision)
    {

        if (State.State == PlayerController.PlayerState.Dash)
        {
            if (collision.gameObject.CompareTag("Obstacle"))
            {
                Destroy(collision.gameObject);
            }
            
        }
        
    }

    public class PState
    {
        public PlayerController.PlayerState State;
        public bool InControl = true;
        public string TriggerName;
        
        
        public PState()
        { }
        
        public PState(PlayerController.PlayerState s)
        {
            State = s;
            TriggerName = "";
        }
        
        public PState(PlayerController.PlayerState s, string t)
        {
            State = s;
            TriggerName = t;
        }

        public virtual void OnStart(PlayerInverted i)
        {
            foreach(AnimatorControllerParameter parameter in i._animator.parameters) 
                {
                    if (parameter.name == TriggerName)
                    {
                        i._animator.SetBool(TriggerName, true);
                    }
                    else
                    {
                        i._animator.SetBool(parameter.name, false);
                    }
                }          
        }
 
        public virtual void Run(PlayerInverted i)
        { }
 
        public virtual void OnEnd(PlayerInverted i)
        { }
    }
    
    public void SetState(PlayerController.PlayerState s)
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
            State = PlayerController.PlayerState.Hurt;
            TriggerName = "Fall";
            InControl = false;
        }

        public override void OnStart(PlayerInverted i)
        {
            base.OnStart(i);
            FallPos = i.transform.position;
        }
        
        public override void Run(PlayerInverted i)
        {
            if (Mathf.Abs(i.transform.position.x - FallPos.x) >= 10 || Mathf.Abs(i.rb.velocity.x) <= 0f)
            {           
                i.SetState(PlayerController.PlayerState.Idle);     
            }
            else
            {
                i.rb.AddForce(Vector2.left * 100);
            }
        }

        public override void OnEnd(PlayerInverted i)
        {
            i.rb.velocity = new Vector2(0.0f, i.rb.velocity.y);
        }
    }
    
    
    public class WalkState : PState
    {
        public WalkState()
        {
            State = PlayerController.PlayerState.Walk;
            TriggerName = "Walk";
        }

        public override void Run(PlayerInverted i)
        {
            i.rb.velocity = new Vector2(Mathf.Lerp(Mathf.Abs(i.rb.velocity.x), PlayerController.Singleton.WalkSpeed, Time.deltaTime * 4f) * PlayerController.Singleton.Movement, i.rb.velocity.y);
        }
        
        public override void OnEnd(PlayerInverted i)
        {
            i.rb.velocity = new Vector2 (0, i.rb.velocity.y);
        }
    }
    
    public class JumpState : PState
    {
        public JumpState()
        {
            State = PlayerController.PlayerState.Jump;
            TriggerName = "Jump";
        }


        public override void Run(PlayerInverted i)
        {


            if (i.rb.velocity.y > 20)
            {
                i.rb.velocity = new Vector2(i.rb.velocity.x, 20f);
            }
            else
            {
                i.rb.AddForce(Vector2.down * PlayerController.Singleton.JumpForce);
            }
    }
    }
    
    
    public class DashState : PState
    {
        public float DashTimer;
                
        public DashState()
        {
            State = PlayerController.PlayerState.Dash;
            TriggerName = "Dash";
            InControl = false;          
        }

        public override void OnStart(PlayerInverted i)
        {
            base.OnStart(i);
            DashTimer = PlayerController.Singleton.DashTime;
        }
        
        
        public override void Run(PlayerInverted i)
        {
            if (DashTimer > 0)
            {
                DashTimer -= Time.deltaTime;
                i.rb.gravityScale = 0.0f;
                i.rb.velocity = new Vector2(Mathf.Lerp(Mathf.Abs(i.rb.velocity.x), PlayerController.Singleton.DashSpeed, Time.deltaTime * 3f) * PlayerController.Singleton.Movement, i.rb.velocity.y);
                i.DashPar.Play();
            }
        }


        public override void OnEnd(PlayerInverted i)
        {
            DashTimer = PlayerController.Singleton.DashTime;  
            i.rb.velocity = new Vector2 (0, i.rb.velocity.y);
            i.rb.gravityScale = i._gravity;
        }       
    }

}
