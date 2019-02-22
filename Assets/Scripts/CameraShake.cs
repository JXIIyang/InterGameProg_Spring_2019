using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{

    public static CameraShake Singleton;

    public Transform objToFollow; // The Object we're following.

    public float lerpScale = 5f; // How much we scale our smooth lerp movement. 
    
    private GameObject _player;

    private Vector3 _offset;
    
    private Vector3 followPos= new Vector3 (0, 0, -10);

    
    

    void Start()
    {
        CameraShake.Singleton = this;
        
    }
    
    public IEnumerator Shake(float duration, float magnitude)
    {
        float timer = 0.0f;

        while (timer < duration)
        {
            float x = Random.Range(-1, 1) * magnitude;
            float y = Random.Range(-1, 1) * magnitude;

            transform.position = new Vector3(x, y, -10);

            timer += Time.deltaTime;

            yield return null;
            
        }

        transform.localPosition = new Vector3(0, 0, -10);

    }

    void FixedUpdate()
    {
        if (objToFollow == null)
        {
            _player = GameObject.FindWithTag("Player");
            objToFollow = _player.transform;
            if (objToFollow == null) return; // Don't try to follow if we don't have a target.
        }

//        float offset_y = 2.5f;
//        if (objToFollow.transform.position.x - followPos.x > 2.5f)     
//        {
//
//            offset_y = -2.5f;
//        }
//
//        if (objToFollow.transform.position.x - followPos.x < -2.5f)
//        {
//            offset_y = 2.5f;
//        }
        Vector3 targetPos = Vector3.Lerp(transform.position, objToFollow.transform.position,
            Time.fixedDeltaTime * lerpScale);
//        if (objToFollow.transform.position.y > 2.0f)
//        {
//            followPos = new Vector3 (followPos.x, targetPos.y, followPos.z);
//        }
        
        
 
        

            followPos = new Vector3 (targetPos.x, followPos.y, followPos.z);
        
            transform.position = followPos;

    }
}
