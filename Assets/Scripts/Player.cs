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

    [Header("Prefabs")]
    public GameObject bulletPrefab;

    [Header("Player Properties")]
    private float vx, vy;                //velocity. Both values should be the same
    public float moveSpeed;
    public float bulletSpeed;           
    public float invulDuration;          //period of invulnerability after getting hit. Set to 3 seconds
    public float shotCooldown;           //delay in seconds in between bullets being fired.
    float currentTime;                   //gets the current time. Used to check if player can fire again.
    float currentInvulTime;              //gets current time, checks if player no longer invincible
    float gaugeAmount;                   //rainbow gauge value that changes depending on situation.

    //constants
    const byte BULLET_LIMIT = 5;         //max number of bullets that can be generated in the game
    const float MAX_SPEED = 12;          //highest bullet speed
    const float INIT_COOLDOWN = 0.4f;    //need to have this since cooldown changes over time.
    const float BULLET_GAIN_AMOUNT = 20;        //default value added to rainbow gauge if bullet of same colour touched
    const float ENEMY_GAIN_AMOUNT = 30;         //default value added to rainbow gauge if enemy of same colour touched

    [Header("Bullet Data")]
    public List<GameObject> playerBullets;
    public bool[] playerBulletClip;             //controls how many bullets are fired. When true, bullet can be fired.
    public int currentBullet;
    bool superBulletEngaged;                    //if true, cannot fire regular bullets.
    GameObject superBullet;
    public GameObject superBulletPrefab;

    //colours
    [HideInInspector]
    public byte currentColor;
    const byte RED = 0;
    const byte BLUE = 1;
    const byte WHITE = 2;
    const byte BLACK = 3;

    //variable to prevent pulse coroutine from activating more than once at a time
    bool isPulseCoroutineRunning = false;

    // Start is called before the first frame update
    void Start()
    {

        //set up player colours
        currentColor = WHITE;

        //bullets set up
        playerBullets = new List<GameObject>();
        playerBulletClip = new bool[BULLET_LIMIT];
        superBulletEngaged = false;

        for (int i = 0; i < BULLET_LIMIT; i++)
        {
            playerBulletClip[i] = true;
            playerBullets.Add(Instantiate(bulletPrefab, transform.position, Quaternion.identity));
            playerBullets[i].GetComponent<Bullet>().BulletSpeed = bulletSpeed;
        }

        currentBullet = 0;
        shotCooldown = INIT_COOLDOWN;

        superBullet = Instantiate(superBulletPrefab, transform.position, Quaternion.identity);

        //player begins the game invincible due to the game time being less than invul duration.
        StartCoroutine(BeginInvincibility());
    }

    // Update is called once per frame
    void Update()
    {
        //is rainbow meter full?
        if (HUD.instance.fillRainbowMeter.value >= HUD.instance.fillRainbowMeter.maxValue)
            superBulletEngaged = true;
        else
            superBulletEngaged = false;

        //Debug.Log("Shot cooldown " + shotCooldown);
        StartCoroutine(ManageBullets());

        //NOTE: This is currently the only way to enable "hold to shoot" with Unity's new input system.
        var kb = Keyboard.current;
        if (kb.spaceKey.isPressed)
        {
            //fire weapon           
            if (!superBulletEngaged && Time.time > currentTime + shotCooldown && playerBulletClip[currentBullet] == true)
            {
                currentTime = Time.time;                //need this to restart the cooldown

                //show bullet
                playerBullets[currentBullet].GetComponent<SpriteRenderer>().enabled = true;
                GameManager.instance.audioSource.PlayOneShot(GameManager.instance.bulletSound, GameManager.instance.SoundEffectVolume());
                playerBulletClip[currentBullet] = false;
                playerBullets[currentBullet].GetComponent<Bullet>().BulletFired = true;

                //bullet fired. move to next bullet
                currentBullet++;

                if (currentBullet >= BULLET_LIMIT)
                    currentBullet = 0;
            }
            else if (superBulletEngaged && !superBullet.GetComponent<SuperBullet>().BulletFired)
            {
                //do a short pause before firing
                superBullet.GetComponent<SuperBullet>().BulletFired = true;
            }
        }

    }
    #region Collision Check
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //check collision against enemy bullet
        if (collision.CompareTag("Bullet_Enemy") && Time.time > currentInvulTime + invulDuration)
        {
            SpriteRenderer sr = collision.GetComponent<SpriteRenderer>();
            EnemyBullet bulletColor = collision.GetComponent<EnemyBullet>();
            //if player colour is the same as bullet colour, gain energy
            switch (currentColor)
            {
                case RED:
                    if (sr.sprite == bulletColor.bulletRed)
                        gaugeAmount = BULLET_GAIN_AMOUNT;
                    else if (sr.sprite == bulletColor.bulletBlue)   //did we touch an opposing colour?
                        gaugeAmount = -BULLET_GAIN_AMOUNT * 2;
                    else
                        //take normal damage
                        gaugeAmount = -BULLET_GAIN_AMOUNT;
                    break;

                case BLUE:
                    if (sr.sprite == bulletColor.bulletBlue)
                        gaugeAmount = BULLET_GAIN_AMOUNT;
                    else if (sr.sprite == bulletColor.bulletRed)   //did we touch an opposing colour?
                        gaugeAmount = -BULLET_GAIN_AMOUNT * 2;
                    else
                        //take normal damage
                        gaugeAmount = -BULLET_GAIN_AMOUNT;
                    break;

                case WHITE:
                    if (sr.sprite == bulletColor.bulletWhite)
                        gaugeAmount = BULLET_GAIN_AMOUNT;
                    else if (sr.sprite == bulletColor.bulletBlack)   //did we touch an opposing colour?
                        gaugeAmount = -BULLET_GAIN_AMOUNT * 2;
                    else
                        //take normal damage
                        gaugeAmount = -BULLET_GAIN_AMOUNT;
                    break;

                case BLACK:
                    if (sr.sprite == bulletColor.bulletBlack)
                        gaugeAmount = BULLET_GAIN_AMOUNT;
                    else if (sr.sprite == bulletColor.bulletWhite)   //did we touch an opposing colour?
                        gaugeAmount = -BULLET_GAIN_AMOUNT * 2;
                    else
                        //take normal damage
                        gaugeAmount = -BULLET_GAIN_AMOUNT;
                    break;

                default:
                    break;
            }

            //destroy bullet
            Destroy(collision.gameObject);

            //if bullet was absorbed, generate absorb label
            if (gaugeAmount > 0)
            {
                GameManager.instance.absorbLabelList.Add(Instantiate(GameManager.instance.absorbLabelPrefab, transform.position, Quaternion.identity));
                GameManager.instance.audioSource.PlayOneShot(GameManager.instance.absorbSound);
                //run pulse coroutine
                if (!isPulseCoroutineRunning)
                {
                    Color teal = new Color(0, 0.86f, 0.98f);
                    StartCoroutine(Pulse(teal));
                }
            }
            else if (Time.time > currentInvulTime + invulDuration)
            {
                //we're taking damage
                currentInvulTime = Time.time;
                StartCoroutine(BeginInvincibility());
            }
           

            //adjust the rainbow gauge
            HUD.instance.AdjustRainbowGauge(gaugeAmount);

            if (HUD.instance.fillRainbowMeter.value <= 0)
            {
                //player dead, game over
                GameManager.instance.audioSource.PlayOneShot(GameManager.instance.explodeSound);
                Debug.Log("Player dead");
            }
            else if (Time.time < currentInvulTime + invulDuration)
            {
                //not dead but took damage, play approproate sound
                GameManager.instance.audioSource.PlayOneShot(GameManager.instance.playerHit, GameManager.instance.SoundEffectVolume() + 0.1f);
            }


        }

        //check collision against enemy. Enemy is absorbed if collision against same colour enemy
        if (collision.CompareTag("Enemy") && Time.time > currentInvulTime + invulDuration)
        {
            SpriteRenderer sr = collision.GetComponent<SpriteRenderer>();
            Enemy enemy = collision.GetComponent<Enemy>();
            //if player colour is the same as bullet colour, gain energy
            switch (currentColor)
            {
                case RED:
                    if (enemy.currentColor == RED)
                        gaugeAmount = ENEMY_GAIN_AMOUNT;
                    else if (enemy.currentColor == BLUE)   //did we touch an opposing colour?
                        gaugeAmount = -ENEMY_GAIN_AMOUNT * 2;
                    else
                        //take normal damage
                        gaugeAmount = -ENEMY_GAIN_AMOUNT;
                    break;

                case BLUE:
                    if (enemy.currentColor == BLUE)
                        gaugeAmount = ENEMY_GAIN_AMOUNT;
                    else if (enemy.currentColor == RED)   //did we touch an opposing colour?
                        gaugeAmount = -ENEMY_GAIN_AMOUNT * 2;
                    else
                        //take normal damage
                        gaugeAmount = -ENEMY_GAIN_AMOUNT;
                    break;

                case WHITE:
                    if (enemy.currentColor == WHITE)
                        gaugeAmount = ENEMY_GAIN_AMOUNT;
                    else if (enemy.currentColor == BLACK)   //did we touch an opposing colour?
                        gaugeAmount = -ENEMY_GAIN_AMOUNT * 2;
                    else
                        //take normal damage
                        gaugeAmount = -ENEMY_GAIN_AMOUNT;
                    break;

                case BLACK:
                    if (enemy.currentColor == BLACK)
                        gaugeAmount = ENEMY_GAIN_AMOUNT;
                    else if (enemy.currentColor == WHITE)   //did we touch an opposing colour?
                        gaugeAmount = -ENEMY_GAIN_AMOUNT * 2;
                    else
                        //take normal damage
                        gaugeAmount = -ENEMY_GAIN_AMOUNT;
                    break;

                default:
                    break;
            }

            //destroy enemy            
            Destroy(collision.gameObject);

            //add to score
            GameManager.instance.enemyCount++;

            //if enemy was absorbed, generate absorb label
            //NOTE: try adding a coroutine to flash player showing they absorbed something
            if (gaugeAmount > 0)
            {
                GameManager.instance.absorbLabelList.Add(Instantiate(GameManager.instance.absorbLabelPrefab, transform.position, Quaternion.identity));
                GameManager.instance.audioSource.PlayOneShot(GameManager.instance.absorbSound);
                //run pulse coroutine
                if (!isPulseCoroutineRunning)
                {
                    Color teal = new Color(0, 0.86f, 0.98f);
                    StartCoroutine(Pulse(teal));
                }
            }
            else if (Time.time > currentInvulTime + invulDuration)
            {
                //we're taking damage                
                currentInvulTime = Time.time;
                StartCoroutine(BeginInvincibility());
            }


            //adjust the rainbow gauge
            HUD.instance.AdjustRainbowGauge(gaugeAmount);

            if (HUD.instance.fillRainbowMeter.value <= 0)
            {
                //player dead, game over
                GameManager.instance.audioSource.PlayOneShot(GameManager.instance.explodeSound);
                Debug.Log("Player dead");
            }
            else if (Time.time < currentInvulTime + invulDuration)
            {
                //not dead but took damage, play approproate sound
                GameManager.instance.audioSource.PlayOneShot(GameManager.instance.playerHit, GameManager.instance.SoundEffectVolume() + 0.1f);
            }


        }
    }
    #endregion
    public float MaxBulletSpeed()
    {
        return MAX_SPEED;
    }

    #region Coroutines
    IEnumerator ManageBullets()
    {
        foreach (GameObject bullet in playerBullets)
        {
            int i = playerBullets.IndexOf(bullet);
            Bullet b = bullet.GetComponent<Bullet>();
            //if bullet is offscreen, bullet is returned to player position
            if (b.BulletFired && (!bullet.GetComponent<SpriteRenderer>().isVisible || b.BulletHit))
            {
                //Debug.Log("Bullet " + i + " is offscreen");

                playerBulletClip[i] = true;
                b.BulletHit = false;
                b.BulletFired = false;
            }

            //bullets always follow player when not fired
            if (!bullet.GetComponent<Bullet>().BulletFired && playerBulletClip[i] == true)
            {
                //hide bullet when not fired
                bullet.GetComponent<SpriteRenderer>().enabled = false;
                bullet.transform.position = new Vector3(transform.position.x,
                    transform.position.y + GetComponent<SpriteRenderer>().bounds.extents.y, -1); //bullet is positioned at player's nose
            }
        }
        yield return null;
    }

    IEnumerator BeginInvincibility()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        
        while (Time.time < currentInvulTime + invulDuration)
        {
            //player sprite visibility alternates between 0 and 1.
            if (sr.enabled)
                sr.enabled = false;
            else if (!sr.enabled)
                sr.enabled = true;
            yield return new WaitForSeconds(0.05f);
        }

        GetComponent<SpriteRenderer>().enabled = true;
    }

    IEnumerator Pulse(Color pulseColor, bool pulseRepeat = false)
    {
        isPulseCoroutineRunning = true;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color originalColor = sr.color;
        bool secondLerpComplete = false;
        bool firstLerpComplete = false;
        float p = 0;

        //change player colour to pulse colour and then return.
        while (!secondLerpComplete)
        {
            while (!firstLerpComplete && sr.color != pulseColor)
            {
                p += 0.1f;
                sr.color = Color.Lerp(originalColor, pulseColor,  p);
                yield return new WaitForSeconds(0.016f); //1 fps = 16ms
            }

            //once we get here, reset p so we can use it again for the next lerp
            firstLerpComplete = true;
            p = 0;

            while (firstLerpComplete && sr.color != originalColor)
            {
                //reverse the pulse until back to original color
                p += 0.1f;
                sr.color = Color.Lerp(pulseColor, originalColor, p);
                yield return new WaitForSeconds(0.016f);
            }

            secondLerpComplete = true;
            isPulseCoroutineRunning = false;
        }
        
    }

    #endregion

    private void FixedUpdate()
    {
        //update player movement
        transform.position = new Vector3(transform.position.x + (vx * Time.deltaTime),
            transform.position.y + (vy * Time.deltaTime), 0);
    }

    #region Movement
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

    #endregion
    
    public void Fire(InputAction.CallbackContext context)
    {
        /*if (context.phase == InputActionPhase.Performed)
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
        }*/

    }


    /***Colour Change*****/
    public void TurnRed(InputAction.CallbackContext context)
    {
        if (currentColor != RED && !isPulseCoroutineRunning && context.phase == InputActionPhase.Performed)
        {           
            StartCoroutine(Pulse(Color.clear));
            GetComponent<SpriteRenderer>().sprite = playerRed;
            currentColor = RED;
            GameManager.instance.audioSource.PlayOneShot(GameManager.instance.colourChange, GameManager.instance.SoundEffectVolume() + 0.2f);          
           // Debug.Log("Changing to red");
        }
    }

    public void TurnBlue(InputAction.CallbackContext context)
    {
        if (currentColor != BLUE && !isPulseCoroutineRunning && context.phase == InputActionPhase.Performed)
        {        
            StartCoroutine(Pulse(Color.clear));
            GetComponent<SpriteRenderer>().sprite = playerBlue;
            currentColor = BLUE;
            GameManager.instance.audioSource.PlayOneShot(GameManager.instance.colourChange, GameManager.instance.SoundEffectVolume() + 0.2f);
            //Debug.Log("Changing to blue");
        }
    }

    public void TurnBlack(InputAction.CallbackContext context)
    {
        if (currentColor != BLACK && !isPulseCoroutineRunning && context.phase == InputActionPhase.Performed)
        {
            StartCoroutine(Pulse(Color.clear));
            GetComponent<SpriteRenderer>().sprite = playerBlack;
            currentColor = BLACK;
            GameManager.instance.audioSource.PlayOneShot(GameManager.instance.colourChange, GameManager.instance.SoundEffectVolume() + 0.2f);
            //Debug.Log("Changing to black");
        }
    }

    public void TurnWhite(InputAction.CallbackContext context)
    {
        if (currentColor != WHITE && !isPulseCoroutineRunning && context.phase == InputActionPhase.Performed)
        {          
            StartCoroutine(Pulse(Color.clear));
            GetComponent<SpriteRenderer>().sprite = playerWhite;
            currentColor = WHITE;
            GameManager.instance.audioSource.PlayOneShot(GameManager.instance.colourChange, GameManager.instance.SoundEffectVolume() + 0.2f);
            //Debug.Log("Changing to white");
        }
    }
    public void ToggleMute(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            HUD.instance.muted = !HUD.instance.muted;
            //Debug.Log("Muted is " + HUD.instance.muted);

            //change audioSource
            HUD.instance.muteIcon.enabled = (HUD.instance.muted == true) ? true : false;
            GameManager.instance.audioSource.enabled = (HUD.instance.muted == false) ? true : false;
            GameManager.instance.musicSource.enabled = (HUD.instance.muted == false) ? true : false;
        }

    }
}
