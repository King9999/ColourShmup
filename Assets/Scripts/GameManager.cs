using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is used to set up and manage the game screen. Main loop is here

public class GameManager : MonoBehaviour
{
    [Header("Player")]
    public GameObject playerPrefab;
    GameObject player;                                            //used to reference position on the screen

    [Header("Sounds")]
    [HideInInspector]
    public AudioSource audioSource;
    public AudioClip pickupSound;                                 //plays whenever player touches a powerup

    [Header("Labels")]
    public GameObject speedUpLabelPrefab;
    [HideInInspector]
    public List<GameObject> speedUpLabelList;                              //manages the labels

    const float SCREEN_BOUNDARY_X = 10;                           //used with WorldToViewPort to get the screen boundary. calculated by dividing screen width with PPU (100)
    const float SCREEN_BOUNDARY_Y = 7;                            //Screen height divided by PPU

    public static GameManager instance;

    #region Constants Accessor Methods

    public float ScreenBoundaryX() { return SCREEN_BOUNDARY_X; }
    public float ScreenBoundaryY() { return SCREEN_BOUNDARY_Y; }


    #endregion

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);    //Only want one instance of game manager
            return;
        }

        instance = this;
        
    }

    // Start is called before the first frame update
    void Start()
    {
        player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        audioSource = GetComponent<AudioSource>();
        speedUpLabelList = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        /********check player boundaries*********/
        CheckPlayerBoundaries();

        StartCoroutine(ManagePickupLabels());
    }

    IEnumerator ManagePickupLabels()
    {
        for (int i = 0; i < speedUpLabelList.Count; i++)
        {          
            SpriteRenderer sr = speedUpLabelList[i].GetComponent<SpriteRenderer>();
            //destroy label when alpha reaches 0.
            while (sr != null && sr.color.a > 0)
            {
                Debug.Log("Reducing alpha");
                //reduce alpha and move label upwards
                sr.color = new Color(1, 1, 1, sr.color.a - (0.02f * Time.deltaTime));
                speedUpLabelList[i].GetComponent<SpriteRenderer>().color = sr.color;
                speedUpLabelList[i].transform.position = new Vector3(speedUpLabelList[i].transform.position.x, 
                    speedUpLabelList[i].transform.position.y + (0.04f * Time.deltaTime), 0);

                yield return null;
            }
            
        }

        //Once we get here it's safe to clear list
        for (int i = 0; i < speedUpLabelList.Count; i++)
            Destroy(speedUpLabelList[i]);

        if (speedUpLabelList.Capacity > 0)
        {
            speedUpLabelList.Clear();
            speedUpLabelList.Capacity = 0;
        }
    }

    void CheckPlayerBoundaries()
    {
        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);   //converting screen pixels to units

        //left edge
        if (player.transform.position.x < screenPos.x * -SCREEN_BOUNDARY_X)
        {
            player.transform.position = new Vector3(screenPos.x * -SCREEN_BOUNDARY_X, player.transform.position.y, 0);
            Debug.Log("Hit the left boundary");
        }

        //right edge
        if (player.transform.position.x > screenPos.x * SCREEN_BOUNDARY_X)
        {
            player.transform.position = new Vector3(screenPos.x * SCREEN_BOUNDARY_X, player.transform.position.y, 0);
            Debug.Log("Hit the right boundary");
        }

        //top edge
        if (player.transform.position.y > screenPos.y * SCREEN_BOUNDARY_Y)
        {
            player.transform.position = new Vector3(player.transform.position.x, screenPos.y * SCREEN_BOUNDARY_Y, 0);
            Debug.Log("Hit the top boundary");
        }

        //bottom edge
        if (player.transform.position.y < screenPos.y * -SCREEN_BOUNDARY_Y)
        {
            player.transform.position = new Vector3(player.transform.position.x, screenPos.y * -SCREEN_BOUNDARY_Y, 0);
            Debug.Log("Hit the bottom boundary");
        }
    }

}
