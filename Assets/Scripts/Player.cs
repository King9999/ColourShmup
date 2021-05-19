using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    
    [Header("Player Colours")]
    public Sprite playerRed;
    public Sprite playerBlue;
    public Sprite playerBlack;
    public Sprite playerWhite;
    public GameObject bulletPrefab;

    [Header("Player Properties")]
    private float vx, vy;                //velocity. Both values should be the same
    public float moveSpeed;
    public float bulletSpeed;           
    public float invulDuration;          //period of invulnerability after getting hit.
    public float shotCooldown;           //delay in seconds in between bullets being fired.
    float currentTime;                   //gets the current time. Used to check if player can fire again.  

    //constants
    const byte BULLET_LIMIT = 5;         //max number of bullets that can be generated in the game
    const float MAX_SPEED = 12;          //highest bullet speed
    const float INIT_COOLDOWN = 0.4f;    //need to have this since cooldown changes over time.

    [HideInInspector]
    public List<GameObject> playerBullets;   
    bool[] playerBulletClip;             //controls how many bullets are fired. When true, bullet can be fired.
    int currentBullet;


    byte currentColor;
    const byte RED = 0;
    const byte BLUE = 1;
    const byte WHITE = 2;
    const byte BLACK = 3;
    
    //game manager instance variables to make 

    // Start is called before the first frame update
    void Start()
    {

        //set up player colours
        currentColor = WHITE;

        //bullets set up
        playerBullets = new List<GameObject>();
        playerBulletClip = new bool[BULLET_LIMIT];

        for (int i = 0; i < BULLET_LIMIT; i++)
        {
            playerBulletClip[i] = true;
            playerBullets.Add(Instantiate(bulletPrefab, transform.position, Quaternion.identity));
            playerBullets[i].GetComponent<Bullet>().BulletSpeed = bulletSpeed;
        }

        currentBullet = 0;
        shotCooldown = INIT_COOLDOWN;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Shot cooldown " + shotCooldown);
        StartCoroutine(ManageBullets());

        //NOTE: This is currently the only way to enable "hold to shoot" with Unity's new input system.
        var kb = Keyboard.current;
        if (kb.spaceKey.isPressed)
        {
            //fire weapon           
            if (Time.time > currentTime + shotCooldown && playerBulletClip[currentBullet] == true)
            {
                currentTime = Time.time;                //need this to restart the cooldown
                playerBulletClip[currentBullet] = false;
                playerBullets[currentBullet].GetComponent<Bullet>().BulletFired = true;

                //bullet fired. move to next bullet
                currentBullet++;

                if (currentBullet >= BULLET_LIMIT)
                    currentBullet = 0;
            }
        }

    }


    public float MaxBulletSpeed()
    {
        return MAX_SPEED;
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
            }

            //bullets always follow player when not fired
            if (!bullet.GetComponent<Bullet>().BulletFired && playerBulletClip[i] == true)
                bullet.transform.position = new Vector3(transform.position.x, transform.position.y, 1);   //Z is 1 so that it's hidden behind player
        }
        yield return null;
    }

    private void FixedUpdate()
    {
        //update player movement
        transform.position = new Vector3(transform.position.x + (vx * Time.deltaTime),
            transform.position.y + (vy * Time.deltaTime), 0);
    }


    public void MoveUp(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            //move player up
            vy = moveSpeed;
        }
        else
        {
            vy = 0;
        }
    }

    public void MoveDown(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            //move player down
            vy = -moveSpeed;
        }
        else
        {
            vy = 0;
        }
    }

    public void MoveLeft(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            //move player left
            vx = -moveSpeed;
        }
        else
        {
            vx = 0;
        }
    }

    public void MoveRight(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            //move player right
            vx = moveSpeed;
        }
        else
        {
            vx = 0;
        }
    }

    public void Fire(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            //fire weapon           
            /*if (Time.time > currentTime + shotCooldown && playerBulletClip[currentBullet] == true)
            {
                currentTime = Time.time;                //need this to restart the cooldown
                playerBulletClip[currentBullet] = false;
                playerBullets[currentBullet].GetComponent<Bullet>().BulletFired = true;

                //bullet fired. move to next bullet
                currentBullet++;

                if (currentBullet >= BULLET_LIMIT)
                    currentBullet = 0;
            }*/
        }

    }

    /***Colour Change*****/
    public void TurnRed(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            GetComponent<SpriteRenderer>().sprite = playerRed;
            currentColor = RED;
            Debug.Log("Changing to red");
        }
    }

    public void TurnBlue(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {           
            GetComponent<SpriteRenderer>().sprite = playerBlue;
            currentColor = BLUE;
            Debug.Log("Changing to blue");
        }
    }

    public void TurnBlack(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            
            GetComponent<SpriteRenderer>().sprite = playerBlack;
            currentColor = BLACK;
            Debug.Log("Changing to black");
        }
    }

    public void TurnWhite(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
           
            GetComponent<SpriteRenderer>().sprite = playerWhite;
            currentColor = WHITE;
            Debug.Log("Changing to white");
        }
    }
}
