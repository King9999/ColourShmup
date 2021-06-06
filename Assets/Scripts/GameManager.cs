using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

//This script is used to set up and manage the game screen. Main loop is here

public class GameManager : MonoBehaviour
{
    
    [Header("Player")]
    public GameObject playerPrefab;
    [HideInInspector]
    public GameObject player;                           //used to reference position on the screen
    public Vector3 playerPos;

    [Header("Powerup data")]
    public float energyPowerUpChance;                   //odds that an energy powerup is generated upon killing enemy
    public float speedPowerUpChance;                    //only applies when super bullet is active.

    [Header("Sounds")]  
    public AudioClip pickupSound;                                 //plays whenever player touches a powerup
    public AudioClip bulletSound;                                   //SFX for firing bullets
    public AudioClip colourChange;                                  //sound when player changes colour
    public AudioClip explodeSound;                      //used for when enemy is destroyed
    public AudioClip blockSound;                        //when player hits enemy of same colour
    public AudioClip absorbSound;
    public AudioClip playerHit;                         //plays when player hit by something but not destroyed
    public AudioClip rainbowShot;
    public AudioClip levelClear;
    public AudioClip altMusic;                          //plays when player reaches level 10.
    [HideInInspector]
    public AudioSource audioSource;                     //used to play sound effects
    public AudioSource musicSource;
    

    [Header("HUD & Game State")]
    public float rainbowGaugeMaxValue;
    public int enemyCount;
    public int targetCount;                                 //total # of enemies required to defeat to advance level.
    public int level;                                       //game difficulty rises after certain levels.
    public int playerLives;
    public bool isGameOver;
    public bool gamePaused;
    public float scrollSpeed;                                      //controls background movement.


    [Header("Prefabs")]
    public GameObject speedPowerupPrefab;
    public GameObject energyPowerupPrefab;
    public GameObject speedUpLabelPrefab;
    public GameObject energyLabelPrefab;
    public GameObject absorbLabelPrefab;
    public GameObject starPrefab;
    public GameObject enemyPrefab;
    public GameObject[] backgroundPrefab;                     //this should be an array. 2 of them must be instantiated for scrolling purposes
    GameObject[] background;

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
   

    //consts
    const float SFX_VOLUME = 0.2f;                                   //default sound volume so it doesn't overpower the music.
    const float INIT_MUSIC_VOLUME = 0.5f;
    const int STAR_COUNT = 80;
    const float SCREEN_BOUNDARY_X = 10;                           //used with WorldToViewPort to get the screen boundary. calculated by dividing screen width with PPU (100)
    const float SCREEN_BOUNDARY_Y = 7;                            //Screen height divided by PPU
    const int DEFAULT_ENEMY_TOTAL = 4;
    const int DEFAULT_TARGET = 10;                              //initial number of enemies to kill to advance level
    //const string STATE_EXPLOSION = "Explosion";

    //coroutine checks
    bool isRestartCoroutineRunning;
    bool isIntensifyCoroutineRunning;

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
        Random.InitState((int)System.DateTime.Now.Ticks);   //set a new seed. Unsure if Unity keeps the same seed until game is closed.

        //set up player at bottom of screen
        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);   //converting screen pixels to units
        player = Instantiate(playerPrefab, new Vector3(0, screenPos.y * -SCREEN_BOUNDARY_Y, 0), Quaternion.identity);

        //background setup
        background = new GameObject[2];
        int backgroundNum = Random.Range(0, backgroundPrefab.Length);   //max is exclusive
        //Debug.Log("Background num is " + backgroundNum);
        background[0] = Instantiate(backgroundPrefab[backgroundNum], new Vector3(0, 0, 10), Quaternion.identity);
        background[1] = Instantiate(backgroundPrefab[backgroundNum], new Vector3(0, backgroundPrefab[backgroundNum].GetComponent<SpriteRenderer>().bounds.extents.y * 2, 10), Quaternion.identity);
        scrollSpeed = 1;

        //audio check
        audioSource = GetComponent<AudioSource>();
        HUD.instance.muteIcon.enabled = (HUD_Menu.instance.muted == true) ? true : false;
        audioSource.enabled = (HUD_Menu.instance.muted == false) ? true : false;
        musicSource.enabled = (HUD_Menu.instance.muted == false) ? true : false;

        speedUpLabelList = new List<GameObject>();
        energyLabelList = new List<GameObject>();
        absorbLabelList = new List<GameObject>();

        //level data
        //enemyTotal = DEFAULT_ENEMY_TOTAL;

        //HUD set up
        HUD.instance.SetRainbowGaugeMaxValue(rainbowGaugeMaxValue);
        //HUD.instance.AdjustRainbowGauge(rainbowGaugeMaxValue);
        HUD.instance.levelText.text = "Level " + level;
        targetCount = DEFAULT_TARGET;
        //enemyCount = 19;
        HUD.instance.enemyCountText.text = "Enemies Destroyed: " + enemyCount + " / " + targetCount;
        HUD.instance.livesCountText.text = "x " + playerLives;

        isGameOver = false;
        gamePaused = false;
        isRestartCoroutineRunning = false;
        isIntensifyCoroutineRunning = false;

        //get ready!
        HUD.instance.ShowGetReadyText();
    }

    // Update is called once per frame
    void Update()
    {
        if (gamePaused)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
        //update background
        UpdateBackground();

        //move enemies
        EnemyManager.instance.MoveAllEnemies();

        if (!isGameOver)
        {
            playerPos = player.transform.position;

            //check player boundaries
            CheckPlayerBoundaries();

            //manage labels
            StartCoroutine(ManagePickupLabels(speedUpLabelList));
            StartCoroutine(ManagePickupLabels(energyLabelList));
            StartCoroutine(ManagePickupLabels(absorbLabelList));

            //Update HUD
            HUD.instance.levelText.text = "Level " + level;
            HUD.instance.enemyCountText.text = "Enemies Destroyed: " + enemyCount + " / " + targetCount;
            HUD.instance.livesCountText.text = "x " + playerLives;

            //advance level?
            if (enemyCount >= targetCount)
                AdvanceLevel();

        }
        else
        {
            //game is over
            musicSource.Stop();
            Invoke("CallGameOver", 2f);
        }
    }

  
    void UpdateBackground()
    {
        //float scrollSpeed = 1;
        float yOffset = 0.5f;       //used to eliminate gap between backgrounds
        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);

        for (int i = 0; i < background.Length; i++)
        {
            background[i].transform.position = new Vector3(background[i].transform.position.x, background[i].transform.position.y - scrollSpeed * Time.deltaTime, 10);

            if (background[i].transform.position.y + background[i].GetComponent<SpriteRenderer>().bounds.extents.y + yOffset < screenPos.y * -SCREEN_BOUNDARY_Y)
            {
                //move background to top of screen, and on top of the other background.
                background[i].transform.position = new Vector3(background[i].transform.position.x,
                    background[i].GetComponent<SpriteRenderer>().bounds.extents.y * 2, 10);
            }
        }
    }

    #region Coroutines
    IEnumerator ManagePickupLabels(List<GameObject> labelList)
    {
        for (int i = 0; i < labelList.Count; i++)
        {          
            SpriteRenderer sr = labelList[i].GetComponent<SpriteRenderer>();
            //destroy label when alpha reaches 0.
            while (sr != null && sr.color.a > 0)
            {
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

    IEnumerator RestartGame()
    {
        isRestartCoroutineRunning = true;
        HUD.instance.anim.SetTrigger("Start");

        yield return new WaitForSeconds(1f);

        HUD.instance.gameoverImage.enabled = false; //this is done since HUD object is not destroyed.
        HUD.instance.anim.SetTrigger("End");
        SceneManager.LoadScene("Game");
    }

    IEnumerator IntensifyGame()
    {
        isIntensifyCoroutineRunning = true; //stays true since we don't need to run this coroutine again.

        float maxSpeed = 6f;
        //fade out music
        while (musicSource.volume > 0)
        {
            musicSource.volume -= 0.02f;
            yield return new WaitForSeconds(0.1f);
        }

        musicSource.Stop();
       
        //gradually increase background scroll speed
        while (scrollSpeed < maxSpeed)
        {
            scrollSpeed += 0.5f;
            yield return new WaitForSeconds(0.1f);
        }

        scrollSpeed = maxSpeed; //in case it's off by a little.

        //once scroll speed is maxed, play new music.
        musicSource.clip = altMusic;
        musicSource.volume = INIT_MUSIC_VOLUME;
        musicSource.Play();
    }

    #endregion

    void CallGameOver()
    {
        //Debug.Log("Game OVER");

        //show game over message, and allow player to restart.
        //should re-load same scene.
        HUD.instance.gameoverImage.enabled = true;

        //get input to restart game
        var kb = Keyboard.current;
        var pad = Gamepad.current;

        if (kb.spaceKey.isPressed || (pad != null && pad.rightTrigger.isPressed))
        {
            if (!isRestartCoroutineRunning)
                StartCoroutine(RestartGame());
        }
    }

    void CheckPlayerBoundaries()
    {
        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);   //converting screen pixels to units

        //left edge
        if (player.transform.position.x < screenPos.x * -SCREEN_BOUNDARY_X)
        {
            player.transform.position = new Vector3(screenPos.x * -SCREEN_BOUNDARY_X, player.transform.position.y, 0);
            //Debug.Log("Hit the left boundary");
        }

        //right edge
        if (player.transform.position.x > screenPos.x * SCREEN_BOUNDARY_X)
        {
            player.transform.position = new Vector3(screenPos.x * SCREEN_BOUNDARY_X, player.transform.position.y, 0);
            //Debug.Log("Hit the right boundary");
        }

        //top edge
        if (player.transform.position.y > screenPos.y * SCREEN_BOUNDARY_Y)
        {
            player.transform.position = new Vector3(player.transform.position.x, screenPos.y * SCREEN_BOUNDARY_Y, 0);
            //Debug.Log("Hit the top boundary");
        }

        //bottom edge
        if (player.transform.position.y < screenPos.y * -SCREEN_BOUNDARY_Y)
        {
            player.transform.position = new Vector3(player.transform.position.x, screenPos.y * -SCREEN_BOUNDARY_Y, 0);
            //Debug.Log("Hit the bottom boundary");
        }
    }

    /*void SetupStars(List<GameObject> stars)
    {
        //randomize star locations
        Vector3 screenPos = Camera.main.WorldToViewportPoint(transform.position);
        List<Vector3> starPosList = new List<Vector3>();    //used to prevent duplicate positions

        for (int i = 0; i < STAR_COUNT; i++)
        {

            Vector3 randomPos = new Vector3(Random.Range(-(screenPos.x * SCREEN_BOUNDARY_X), screenPos.x * SCREEN_BOUNDARY_X),
                                            Random.Range(-(screenPos.y * SCREEN_BOUNDARY_Y), (screenPos.y * SCREEN_BOUNDARY_Y) + 1), 1);

            //Debug.Log("New Star Pos: " + randomPos);

            float offset = 0.1f;
            //if random pos was already used, or if a star is too close to another, keep finding random number until we get an original.         
            while (starPosList.Contains(randomPos) || starPosList.Contains(new Vector3(Random.Range(randomPos.x - offset, randomPos.x + offset), 
                                                                                       Random.Range(randomPos.y - offset, randomPos.y + offset), 1)))
            {
                randomPos = randomPos + new Vector3(Random.Range(-(screenPos.x * SCREEN_BOUNDARY_X), screenPos.x * SCREEN_BOUNDARY_X),
                                    Random.Range(-(screenPos.y * SCREEN_BOUNDARY_Y), (screenPos.y * SCREEN_BOUNDARY_Y) + 1), 1);

                Debug.Log("Found duplicate star, new Star Pos: " + randomPos);
            }
            

            starPosList.Add(randomPos);
            stars.Add(Instantiate(starPrefab, randomPos, Quaternion.identity));
        }
    }*/

    /*Increases difficulty of next level. Player gains 1 life after every level*/
    void AdvanceLevel()
    {
        audioSource.PlayOneShot(levelClear, 1f);
        level++;
        EnemyManager.instance.AdvanceLevel();     
        enemyCount = 0;
        targetCount += 2;
        
        //gain a life every 3 levels.
        if (level % 3 == 0)
            playerLives++;

        //if level 10 is reached, play new music
        if (level >= 10 && !isIntensifyCoroutineRunning)
            StartCoroutine(IntensifyGame());

        //get ready
        HUD.instance.ShowGetReadyText();

    }
}

