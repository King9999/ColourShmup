using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    //Player player;
    // public GameObject playerBlue;
    //GameObject player;
    //public GameObject playerPrefab;
    public Sprite playerRed;
    public Sprite playerBlue;
    public Sprite playerBlack;
    public Sprite playerWhite;
    public GameObject bulletPrefab;

    byte currentColor;
    const byte RED = 0;
    const byte BLUE = 1;
    const byte WHITE = 2;
    const byte BLACK = 3;
    
    //game manager instance variables to make 

    // Start is called before the first frame update
    void Start()
    {
        //player = FindObjectOfType<Player>();
        //player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        //set up player colours
        currentColor = WHITE;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MoveUp(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            //move player up
        }
    }

    public void MoveDown(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            //move player down
        }
    }

    public void MoveLeft(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            //move player left
        }
    }

    public void MoveRight(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            //move player right
        }
    }

    public void Fire(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            int i = GameManager.instance.currentBullet;     //added this to make code below more readable

            //fire weapon
            if (GameManager.instance.playerBulletClip[i] == true)
            {               
                GameManager.instance.playerBulletClip[i] = false;
                GameManager.instance.playerBullets[i].GetComponent<Bullet>().BulletFired = true;

                //up to 5 bullets are tracked.
                /*if (GameManager.instance.playerBullets.Count < GameManager.instance.BulletLimit())
                {
                    GameObject bullet = Instantiate(bulletPrefab, gameObject.transform.position, Quaternion.identity);
                    GameManager.instance.playerBullets.Add(bullet);                  
                }*/

                //bullet fired. move to next bullet
                GameManager.instance.currentBullet++;

                if (GameManager.instance.currentBullet >= GameManager.instance.BulletLimit())
                    GameManager.instance.currentBullet = 0;
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
