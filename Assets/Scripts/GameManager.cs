using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is used to set up and manage the game screen. Main loop is here

public class GameManager : MonoBehaviour
{
    [Header("Player")]
    public GameObject playerPrefab;
    public GameObject playerBulletPrefab;
    GameObject player;                                           //used to reference position on the screen

    //bullet variables
    const byte BULLET_LIMIT = 5;                                //max number of bullets that can be generated in the game
    [HideInInspector]
    public List<GameObject> playerBullets;
    [HideInInspector]
    public bool[] playerBulletClip;                             //controls how many bullets are fired. When true, bullet can be fired.
    [HideInInspector]
    public int currentBullet;                                   //checks which bullet is fired currently in playerBulletClip

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
        player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        //bullets set up
        playerBullets = new List<GameObject>();
        playerBulletClip = new bool[BULLET_LIMIT];

        for (int i = 0; i < BULLET_LIMIT; i++)
        {
            playerBulletClip[i] = true;
            playerBullets.Add(Instantiate(playerBulletPrefab, player.transform.position, Quaternion.identity));
        }

        currentBullet = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //manage bullets. TODO: Use Coroutine?
        StartCoroutine(ManageBullets());
        /*foreach (GameObject bullet in playerBullets)
        {
            int i = playerBullets.IndexOf(bullet);
            //if bullet is offscreen, bullet is returned to player position
            if (bullet.GetComponent<Bullet>().BulletFired && !bullet.GetComponent<SpriteRenderer>().isVisible)
            {
                Debug.Log("Bullet " + i + " is offscreen");

                playerBulletClip[i] = true;
                bullet.GetComponent<Bullet>().BulletFired = false;
                //bullet.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, 1);
            }

            //bullets always follow player when not fired
            if (!bullet.GetComponent<Bullet>().BulletFired && playerBulletClip[i] == true)
               bullet.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, 1);
        }*/
    }

    public byte BulletLimit()
    {
        return BULLET_LIMIT;
    }

    IEnumerator ManageBullets()
    {
        foreach (GameObject bullet in playerBullets)
        {
            int i = playerBullets.IndexOf(bullet);
            //if bullet is offscreen, bullet is returned to player position
            if (bullet.GetComponent<Bullet>().BulletFired && !bullet.GetComponent<SpriteRenderer>().isVisible)
            {
                Debug.Log("Bullet " + i + " is offscreen");

                playerBulletClip[i] = true;
                bullet.GetComponent<Bullet>().BulletFired = false;
                //bullet.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, 1);
            }

            //bullets always follow player when not fired
            if (!bullet.GetComponent<Bullet>().BulletFired && playerBulletClip[i] == true)
                bullet.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, 1);   //Z is 1 so that it's hidden behind player
        }
        yield return null;
    }
}
