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

    Color[] playerColors = new Color[4];
    const int RED = 0;
    const int BLUE = 1;
    const int WHITE = 2;
    const int BLACK = 3;
    


    // Start is called before the first frame update
    void Start()
    {
        //player = FindObjectOfType<Player>();
        //player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        //set up player colours
        playerColors[RED] = new Color(0.9f, 0, 0);
        playerColors[BLUE] = new Color(0, 0, 0.9f);
        playerColors[WHITE] = new Color(1, 1, 1);
        playerColors[BLACK] = new Color(0.18f, 0.15f, 0.27f);
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
           //fire weapon
        }

    }

    /***Colour Change*****/
    public void TurnRed(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            
            GetComponent<SpriteRenderer>().color = playerColors[RED];
            Debug.Log("Changing to red");
        }
    }

    public void TurnBlue(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            
            GetComponent<SpriteRenderer>().color = playerColors[BLUE];
            Debug.Log("Changing to blue");
        }
    }

    public void TurnBlack(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            
            GetComponent<SpriteRenderer>().color = playerColors[BLACK];
            Debug.Log("Changing to black");
        }
    }

    public void TurnWhite(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
           
            GetComponent<SpriteRenderer>().color = playerColors[WHITE];
            Debug.Log("Changing to white");
        }
    }
}
