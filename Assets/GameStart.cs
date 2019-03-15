using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStart : MonoBehaviour
{
    public GameObject Main0;
    public GameObject Main1;
    public Image Overlay;
    public float timer;
    private float _timer;
    private int index = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        _timer = 2f;
    }

    // Update is called once per frame
    void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer < 0)
        {
            if (index < 2)
            {
                _timer = timer;
                index++;
            }
            else
            {
                Overlay.color = new Color(1,1,1,Mathf.Lerp(Overlay.color.a, 1, 0.03f));
               
            }
        }

        if (Overlay.color.a > 0.99f)
        {
            SceneManager.LoadScene("Game");
        }
        

        switch (index)
        {
            case 1 :
                Main0.SetActive(false);
                return;
            case 2:
                Main1.SetActive(false);
                return;
        }
    }
}
