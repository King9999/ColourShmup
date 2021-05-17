using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is used to set up and manage the game screen. Main loop is here

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;

    //bullet variables
    public List<GameObject> playerBullets;
    const byte BULLET_LIMIT = 5;                                //max number of bullets that can be generated in the game
    public bool[] playerBulletClip;                             //controls how many bullets are fired. When true, bullet can be fired.
    public int currentBullet;                                          //checks which bullet is fired currently in playerBulletClip

    public static GameManager instance;

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
        GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        //bullets set up
        playerBullets = new List<GameObject>();
        playerBulletClip = new bool[BULLET_LIMIT];

        for (int i = 0; i < BULLET_LIMIT; i++)
            playerBulletClip[i] = true;

        currentBullet = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //manage bullets

    }

    public byte BulletLimit()
    {
        return BULLET_LIMIT;
    }
}
