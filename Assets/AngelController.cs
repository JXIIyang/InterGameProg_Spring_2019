using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngelController : MonoBehaviour
{
    private SpriteRenderer _renderer;
    private Collider2D _col;
    private AudioSource _audio;
    
    

    private bool _fade;
    private bool _fly;
    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _col = GetComponent<Collider2D>();
        _audio = GetComponent<AudioSource>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!_fly)
        {
            transform.Translate(0f, Mathf.Sin(2.5f*Time.time)/35,0);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, NpcController.Singleton.transform.position, 0.03f);
        }
        if (!_fade) return;        
        _renderer.color = new Color(1, 1, 1, Mathf.Lerp(_renderer.color.a, 0, 0.03f)); 
        if (_renderer.color.a < 0.01f) Destroy(this);
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == PlayerInverted.Singleton.gameObject ||
            other.gameObject == PlayerController.Singleton.gameObject)
        {
            GameManager.Singleton.AddHeart(1);
            _fade = true;
            _col.enabled = false;
            _audio.Play();
        }
        
        if (other.CompareTag("NPCTrigger") && transform.position.x - PlayerController.Singleton.transform.position.x < 6f)
        {
            Debug.Log("NPC");
            if (Random.value < 0.7f) return;
            _fade = true;
            _fly = true;
            _col.enabled = false;
            _audio.Play();
        }
    }
}
