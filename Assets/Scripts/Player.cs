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
    public float invulDuration;          //period of invulnerability after getting hit.
    const byte BULLET_LIMIT = 5;         //max number of bullets that can be generated in the game
    
    List<GameObject> playerBullets;   
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
        }

        currentBullet = 0;
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(ManageBullets());
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
            /*int i = GameManager.instance.currentBullet;     //added this to make code below more readable

            //fire weapon
            if (GameManager.instance.playerBulletClip[i] == true)
            {               
                GameManager.instance.playerBulletClip[i] = false;
                GameManager.instance.playerBullets[i].GetComponent<Bullet>().BulletFired = true;

                //bullet fired. move to next bullet
                GameManager.instance.currentBullet++;

                if (GameManager.instance.currentBullet >= GameManager.instance.BulletLimit())
                    GameManager.instance.currentBullet = 0;
            }*/
            

            //fire weapon
            if (playerBulletClip[currentBullet] == true)
            {
                playerBulletClip[currentBullet] = false;
                playerBullets[currentBullet].GetComponent<Bullet>().BulletFired = true;

                //bullet fired. move to next bullet
                currentBullet++;

                if (currentBullet >= BULLET_LIMIT)
                    currentBullet = 0;
            }
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
