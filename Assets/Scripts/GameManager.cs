using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Experimental.UIElements;
using UnityEngine.SceneManagement;
using Image = UnityEngine.UI.Image;


public class GameManager : MonoBehaviour
{
    
    
    public static GameManager Singleton;
    public Canvas _canvas;
    public Image Overlay;
    private float _alpha;

    private List<GameObject> HeartUI = new List<GameObject>();

    public int HeartNum;

    public bool EndGame;


    public GameObject Player;

    public GameObject HeartPrefab;
    public GameObject BadHeartPrefab;
    public GameObject NPCPrefab;
    
    public Camera MainCamera;
    public AudioSource Audio;
    
    
    public GameObject HeartUIPrefab;
    
    
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Singleton = this;
        GameManager.Singleton = this;
        for (int i = 0; i < HeartNum; i++)
        {
            var heart = Instantiate(HeartUIPrefab, new Vector3(20 * i, 0, 0), Quaternion.identity);
            heart.transform.SetParent(_canvas.transform, false);
            heart.gameObject.name = i.ToString();
            HeartUI.Add(heart);
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (HeartNum != HeartUI.Count)
        {
            foreach (var heart in HeartUI)
            {
                Destroy(heart);
            }
            for (int i = 0; i < HeartNum; i++)
            {
                var heart = Instantiate(HeartUIPrefab, new Vector3(20 * i, 0, 0), Quaternion.identity);
                heart.transform.SetParent(_canvas.transform, false);
                heart.gameObject.name = i.ToString();
                HeartUI.Add(heart);
            }
        }
        if (HeartNum <= 0)
        {
//            HeartNum = 0;
//            foreach (var heart in HeartUI)
//            {
//                Destroy(heart);
//            }
            EndGame = true;
            _alpha += 0.2f * Time.deltaTime;
            Overlay.color = new Color(0, 0, 0, _alpha);
            Audio.volume = Mathf.Lerp(Audio.volume, 0, Time.deltaTime);
        }

        if (HeartNum >= 6)
        {
            MainCamera.backgroundColor =
                Color.Lerp(MainCamera.backgroundColor, new Color(.76f, 0.54f, 0.93f), Time.deltaTime);
        }
        else
        {
            MainCamera.backgroundColor =
                Color.Lerp(MainCamera.backgroundColor, new Color(.76f, 0.76f, 0.76f), Time.deltaTime);
        }
        
        
        
        

        if (_alpha >= 1)
        {
            SceneManager.LoadScene("End");
        }
        if (GameObject.FindWithTag("NPC") == null)
        {
            Instantiate(NPCPrefab, Player.transform.position + Vector3.right * Random.Range(11, 20),
                Quaternion.identity);
        }
        if (Input.GetKeyUp(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void AddHeart(int num)
    {
        if (num > 0 && !EndGame)
        {
//            for (int i = 0; i < num; i++)
//            {
//                var heart = Instantiate(HeartUIPrefab, new Vector3(20 * (HeartNum), 0, 0), Quaternion.identity);
//                heart.transform.SetParent(_canvas.transform, false);
//                heart.gameObject.name = (i + HeartNum).ToString();
//                HeartUI.Add(heart);
//                Debug.Log("add");
//            }
            HeartNum += num;
            
        }

       
    }
    
    
    public void Win()
    {
        _alpha += 0.2f * Time.deltaTime;
        Overlay.color = new Color(1, 1, 1, _alpha);
        Audio.volume = Mathf.Lerp(Audio.volume, 0, Time.deltaTime);
    }



}
