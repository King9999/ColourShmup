using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is used to set up and manage the game screen. Main loop is here

public class GameManager : MonoBehaviour
{
    
    [Header("Player")]
    public GameObject playerPrefab;
    [HideInInspector]
    public GameObject player;                           //used to reference position on the screen

    [Header("Powerup data")]
    public float energyPowerUpChance;                   //odds that an energy powerup is generated upon killing enemy

    [Header("Sounds")]  
    public AudioClip pickupSound;                                 //plays whenever player touches a powerup
    public AudioClip bulletSound;                                   //SFX for firing bullets
    public AudioClip colourChange;                                  //sound when player changes colour
    [HideInInspector]
    public AudioSource audioSource;
    

    [Header("HUD")]
    public float rainbowGaugeMaxValue;
    public int enemyCount;
    public int targetCount;                                 //total # of enemies required to advance level.
    public int level;                                       //game difficulty rises after certain levels.


    [Header("Prefabs")]
    public GameObject speedPowerupPrefab;
    public GameObject energyPowerupPrefab;
    public GameObject speedUpLabelPrefab;
    public GameObject energyLabelPrefab;
    public GameObject absorbLabelPrefab;
    public GameObject starPrefab;

    //lists
    [HideInInspector]
    public List<GameObject> speedUpLabelList;                     //These lists are used to manage label movement when player interacts with
    [HideInInspector]                                             //powerups or enemies
    public List<GameObject> energyLabelList;                             
    [HideInInspector]
    public List<GameObject> absorbLabelList;
    [HideInInspector]
    public List<GameObject> energyPowerupList;
    [HideInInspector]
    public List<GameObject> speedPowerupList;

    List<GameObject> starList;                             //used to manage the random stars on screen

    //consts
    const float SFX_VOLUME = 0.2f;                                   //default sound volume so it doesn't overpower the music.
    const int STAR_COUNT = 80;
    const float SCREEN_BOUNDARY_X = 10;                           //used with WorldToViewPort to get the screen boundary. calculated by dividing screen width with PPU (100)
    const float SCREEN_BOUNDARY_Y = 7;                            //Screen height divided by PPU
    const int DEFAULT_TARGET = 20;                              //initial number of enemies to kill to advance level

    public static GameManager instance;

    #region Constants Accessor Methods

    public float ScreenBoundaryX() { return SCREEN_BOUNDARY_X; }
    public float ScreenBoundaryY() { return SCREEN_BOUNDARY_Y; }

    public float SoundEffectVolume() { return SFX_VOLUME; }

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
        energyLabelList = new List<GameObject>();
        absorbLabelList = new List<GameObject>();
        

        //HUD set up
        HUD.instance.SetRainbowGaugeMaxValue(rainbowGaugeMaxValue);
        HUD.instance.levelText.text = "Level " + level;
        targetCount = DEFAULT_TARGET;
        HUD.instance.enemyCountText.text = "Enemies Destroyed: " + enemyCount + " / " + targetCount;

        //set up stars
        starList = new List<GameObject>();
        SetupStars(starList);
    }

    // Update is called once per frame
    void Update()
    {
        //check player boundaries
        CheckPlayerBoundaries();

        //manage labels
        StartCoroutine(ManagePickupLabels(speedUpLabelList));
        StartCoroutine(ManagePickupLabels(energyLabelList));
        StartCoroutine(ManagePickupLabels(absorbLabelList));

        //manage stars
        StartCoroutine(ManageStars());
    }

    IEnumerator ManagePickupLabels(List<GameObject> labelList)
    {
        for (int i = 0; i < labelList.Count; i++)
        {          
            SpriteRenderer sr = labelList[i].GetComponent<SpriteRenderer>();
            //destroy label when alpha reaches 0.
            while (sr != null && sr.color.a > 0)
            {
                //Debug.Log("Reducing alpha");
                //reduce alpha and move label upwards
                sr.color = new Color(1, 1, 1, sr.color.a - (0.02f * Time.deltaTime));
                labelList[i].GetComponent<SpriteRenderer>().color = sr.color;
                labelList[i].transform.position = new Vector3(labelList[i].transform.position.x,
                    labelList[i].transform.position.y + (0.04f * Time.deltaTime), 0);

                yield return null;
            }
            
        }

        //Once we get here it's safe to clear list
        for (int i = 0; i < labelList.Count; i++)
            Destroy(labelList[i]);

        if (labelList.Capacity > 0)
        {
            labelList.Clear();
            labelList.Capacity = 0;
        }
    }

    IEnumerator ManageStars()
    {
        //move all stars down. If they reach bottom of screen, move them back up to top of screen
        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);
        foreach (GameObject star in starList)
        {
            star.transform.position = new Vector3(star.transform.position.x, star.transform.position.y - (1 * Time.deltaTime), 1);

            if (star.transform.position.y < -(screenPos.y * SCREEN_BOUNDARY_Y) - 1)
            {
                //move star to top of screen outside of view
                star.transform.position = new Vector3(star.transform.position.x, (screenPos.y * SCREEN_BOUNDARY_Y) + 1, 1);
            }
            yield return null;
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

    void SetupStars(List<GameObject> stars)
    {
        //randomize star locations
        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);
        List<Vector3> starPosList = new List<Vector3>();    //used to prevent duplicate positions

        for (int i = 0; i < STAR_COUNT; i++)
        {

            Vector3 randomPos = new Vector3(Random.Range(-(screenPos.x * SCREEN_BOUNDARY_X), screenPos.x * SCREEN_BOUNDARY_X),
                                            Random.Range(-(screenPos.y * SCREEN_BOUNDARY_Y), (screenPos.y * SCREEN_BOUNDARY_Y) + 1), 1);

            //Debug.Log("New Star Pos: " + randomPos);

            //if random pos was already used, keep finding random number until we get an original.         
            while (starPosList.Contains(randomPos))
            {
                randomPos = randomPos + new Vector3(Random.Range(-(screenPos.x * SCREEN_BOUNDARY_X), screenPos.x * SCREEN_BOUNDARY_X),
                                    Random.Range(-(screenPos.y * SCREEN_BOUNDARY_Y), (screenPos.y * SCREEN_BOUNDARY_Y) + 1), 1);

                Debug.Log("Found duplicate, new Star Pos: " + randomPos);
            }
            

            starPosList.Add(randomPos);
            stars.Add(Instantiate(starPrefab, randomPos, Quaternion.identity));
        }
    }
}

