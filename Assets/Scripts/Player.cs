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
    public GameObject chargeUpPrefab;
    public GameObject explosionPrefab;      //plays when player dies

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
    const float INIT_BULLET_SPEED = 6f;

    [Header("Bullet Data")]
    public List<GameObject> playerBullets;
    public bool[] playerBulletClip;             //controls how many bullets are fired. When true, bullet can be fired.
    public int currentBullet;
    bool superBulletEngaged;                    //if true, cannot fire regular bullets.
    GameObject superBullet;
    public GameObject superBulletPrefab;

    [Header("Audio")]
    //public AudioSource playerSource;
    //public AudioClip rainbowShot;

    //colours
    [HideInInspector]
    public byte currentColor;
    const byte RED = 0;
    const byte BLUE = 1;
    const byte WHITE = 2;
    const byte BLACK = 3;

    //variable to prevent pulse coroutine from activating more than once at a time
    bool isPulseCoroutineRunning = false;

    //disable control when dead
    bool playerDead;

    // Start is called before the first frame update
    void Start()
    {

        //set up player colours
        currentColor = WHITE;

        //bullets set up
        playerBullets = new List<GameObject>();
        playerBulletClip = new bool[BULLET_LIMIT];
        superBulletEngaged = false;
        bulletSpeed = INIT_BULLET_SPEED;

        for (int i = 0; i < BULLET_LIMIT; i++)
        {
            playerBulletClip[i] = true;
            playerBullets.Add(Instantiate(bulletPrefab, transform.position, Quaternion.identity));
            playerBullets[i].GetComponent<Bullet>().BulletSpeed = bulletSpeed;
        }

        currentBullet = 0;
        shotCooldown = INIT_COOLDOWN;

        superBullet = Instantiate(superBulletPrefab, transform.position, Quaternion.identity);
        //superBullet.GetComponent<SpriteRenderer>().enabled = false;

        playerDead = false;

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
        var pad = Gamepad.current;
        if (!playerDead)
        {
            if (kb.spaceKey.isPressed || (pad != null && pad.rightTrigger.isPressed))
            {
                //fire weapon           
                if (!superBulletEngaged && !superBullet.GetComponent<SuperBullet>().BulletFired &&
                    Time.time > currentTime + shotCooldown && playerBulletClip[currentBullet] == true)
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
                    //fire!
                    StartCoroutine(ActivateSuperBullet());
                    //superBullet.GetComponent<SuperBullet>().BulletFired = true;

                    //player is invincible while super bullet is engaged.
                    StartCoroutine(BeginInvincibility(true));
                    /*if (!isPulseCoroutineRunning)
                    {
                        StartCoroutine(Pulse(Color.yellow, true));
                        StartCoroutine(BeginInvincibility(true));
                    }*/
                }
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
           

            //adjust the rainbow gauge
            HUD.instance.AdjustRainbowGauge(gaugeAmount);

            if (HUD.instance.fillRainbowMeter.value <= 0)
            {
                //player dead, reduce life
                if (GameManager.instance.playerLives - 1 >= 0)
                    GameManager.instance.playerLives--;
                else
                    GameManager.instance.isGameOver = true;
                StartCoroutine(ExplodePlayer());               
                //Debug.Log("Player dead");
            }
            else if (gaugeAmount < 0 && Time.time > currentInvulTime + invulDuration)
            {
                //not dead but took damage, play approproate sound
                GameManager.instance.audioSource.PlayOneShot(GameManager.instance.playerHit, GameManager.instance.SoundEffectVolume() + 0.1f);
                StartCoroutine(BeginInvincibility());
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
            //Destroy(EnemyManager.instance.pathList[collision.GetComponent<Enemy>().enemyID]);
            Destroy(collision.gameObject);

            //add to score
            GameManager.instance.enemyCount++;

            //if enemy was absorbed, generate absorb label
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


            //adjust the rainbow gauge
            HUD.instance.AdjustRainbowGauge(gaugeAmount);

            if (HUD.instance.fillRainbowMeter.value <= 0)
            {
                //player dead, lose a life
                if (GameManager.instance.playerLives - 1 >= 0)
                    GameManager.instance.playerLives--;
                else
                    GameManager.instance.isGameOver = true;
                StartCoroutine(ExplodePlayer());
                //Debug.Log("Player dead");
            }
            else if (gaugeAmount < 0 && Time.time > currentInvulTime + invulDuration)
            {
                //not dead but took damage, play approproate sound
                GameManager.instance.audioSource.PlayOneShot(GameManager.instance.playerHit, GameManager.instance.SoundEffectVolume() + 0.1f);
                StartCoroutine(BeginInvincibility());
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

    IEnumerator BeginInvincibility(bool infiniteDuration = false)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (infiniteDuration == true)
        {
            while (HUD.instance.fillRainbowMeter.value > 0)
            {
                currentInvulTime = Time.time;
                //player sprite visibility alternates between 0 and 1.
                if (sr.enabled)
                    sr.enabled = false;
                else if (!sr.enabled)
                    sr.enabled = true;
                yield return new WaitForSeconds(0.05f);
            }
        }
        else
        {
            currentInvulTime = Time.time;
            while (Time.time < currentInvulTime + invulDuration)
            {
                //player sprite visibility alternates
                if (sr.enabled)
                    sr.enabled = false;
                else if (!sr.enabled)
                    sr.enabled = true;
                yield return new WaitForSeconds(0.05f);
            }
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

        if (pulseRepeat == true)
        {
            //lerp continuously until rainbow gauge is empty
            while (HUD.instance.fillRainbowMeter.value > 0)
            {
                while (!firstLerpComplete && sr.color != pulseColor)
                {
                    p += 0.1f;
                    sr.color = Color.Lerp(originalColor, pulseColor, p);
                    yield return new WaitForSeconds(0.05f); //1 fps = 16ms
                }

                //once we get here, reset p so we can use it again for the next lerp
                firstLerpComplete = true;
                p = 0;

                while (firstLerpComplete && sr.color != originalColor)
                {
                    //reverse the pulse until back to original color
                    p += 0.1f;
                    sr.color = Color.Lerp(pulseColor, originalColor, p);
                    yield return new WaitForSeconds(0.05f);
                }

                firstLerpComplete = false;
                p = 0;
            }

            isPulseCoroutineRunning = false;
        }
        else
        {
            //change player colour to pulse colour and then return.
            while (!secondLerpComplete)
            {
                while (!firstLerpComplete && sr.color != pulseColor)
                {
                    p += 0.1f;
                    sr.color = Color.Lerp(originalColor, pulseColor, p);
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
        
    }

    //used to pause the game briefly before super bullet is activated.
    /*NOTE: must use Time.unscaledTime to check time because setting 0 to time scale will
     * result in the game being paused, and Time.time won't tick. 
     I found a bug where player is invincible and the super bullet never fires. It occurs when
    player fires at the same they're hit while rainbow gauge is full. I think 
    this pause will help prevent that bug, so I restored it.*/
    IEnumerator ActivateSuperBullet()
    {
        float duration = 0.05f;
        float currentTime = Time.unscaledTime;
        //Instantiate(chargeUpPrefab, new Vector3(transform.position.x, transform.position.y + GetComponent<SpriteRenderer>().bounds.extents.y + 1, 0), Quaternion.identity);
        while (Time.unscaledTime < currentTime + duration)
        {
            Time.timeScale = 0; //game paused
            yield return null;
        }

        Time.timeScale = 1;
        superBullet.GetComponent<SuperBullet>().BulletFired = true;
    }

    IEnumerator ExplodePlayer()
    {
        //hide player sprite and hitbox, play explosion and sound, wait a few seconds then bring back player
        //if they have remaining lives.
        playerDead = true;
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<BoxCollider2D>().enabled = false;
        GameManager.instance.audioSource.PlayOneShot(GameManager.instance.explodeSound);
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(2f);

        //set up player at bottom of screen
        if (!GameManager.instance.isGameOver)
        {
            Vector3 screenPos = Camera.main.WorldToViewportPoint(GameManager.instance.transform.position);   //converting screen pixels to units
            transform.position = new Vector3(0, screenPos.y * -GameManager.instance.ScreenBoundaryY(), 0);
            GetComponent<SpriteRenderer>().enabled = true;
            GetComponent<BoxCollider2D>().enabled = true;
            playerDead = false;

            //reset bullet speed
            bulletSpeed = INIT_BULLET_SPEED;
            shotCooldown = INIT_COOLDOWN;
            for (int i = 0; i < BULLET_LIMIT; i++)
            {
                playerBulletClip[i] = true;
                playerBullets[i].GetComponent<Bullet>().BulletSpeed = bulletSpeed;
            }

            StartCoroutine(BeginInvincibility());
        }
      
        //if the above condition is true, game is over so we do nothing. 
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
    
    /*public void Fire(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
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

    }*/


    /***Colour Change*****/
    public void TurnRed(InputAction.CallbackContext context)
    {
        if (!playerDead && currentColor != RED && !isPulseCoroutineRunning && context.phase == InputActionPhase.Performed)
        {           
            StartCoroutine(Pulse(Color.clear));
            GetComponent<SpriteRenderer>().sprite = playerRed;
            HUD.instance.livesImage.sprite = HUD.instance.livesSpriteRed;
            currentColor = RED;
            GameManager.instance.audioSource.PlayOneShot(GameManager.instance.colourChange, GameManager.instance.SoundEffectVolume() + 0.2f);          
           // Debug.Log("Changing to red");
        }
    }

    public void TurnBlue(InputAction.CallbackContext context)
    {
        if (!playerDead && currentColor != BLUE && !isPulseCoroutineRunning && context.phase == InputActionPhase.Performed)
        {        
            StartCoroutine(Pulse(Color.clear));
            GetComponent<SpriteRenderer>().sprite = playerBlue;
            HUD.instance.livesImage.sprite = HUD.instance.livesSpriteBlue;
            currentColor = BLUE;
            GameManager.instance.audioSource.PlayOneShot(GameManager.instance.colourChange, GameManager.instance.SoundEffectVolume() + 0.2f);
            //Debug.Log("Changing to blue");
        }
    }

    public void TurnBlack(InputAction.CallbackContext context)
    {
        if (!playerDead && currentColor != BLACK && !isPulseCoroutineRunning && context.phase == InputActionPhase.Performed)
        {
            StartCoroutine(Pulse(Color.clear));
            GetComponent<SpriteRenderer>().sprite = playerBlack;
            HUD.instance.livesImage.sprite = HUD.instance.livesSpriteBlack;
            currentColor = BLACK;
            GameManager.instance.audioSource.PlayOneShot(GameManager.instance.colourChange, GameManager.instance.SoundEffectVolume() + 0.2f);
            //Debug.Log("Changing to black");
        }
    }

    public void TurnWhite(InputAction.CallbackContext context)
    {
        if (!playerDead && currentColor != WHITE && !isPulseCoroutineRunning && context.phase == InputActionPhase.Performed)
        {          
            StartCoroutine(Pulse(Color.clear));
            GetComponent<SpriteRenderer>().sprite = playerWhite;
            HUD.instance.livesImage.sprite = HUD.instance.livesSpriteWhite;
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
