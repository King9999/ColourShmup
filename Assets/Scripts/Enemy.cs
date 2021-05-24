using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [Header("Enemy Colours")]
    public Sprite enemyRed;
    public Sprite enemyBlue;
    public Sprite enemyBlack;
    public Sprite enemyWhite;

    [Header("Prefabs")]
    public GameObject enemyBulletPrefab;
    GameObject bullet;
    Animator anim;                      //used for explosions

    [Header("Enemy Properties")]
    private float vx, vy;                //velocity. Both values should be the same
    public float moveSpeed;              //this increases as the game progresses
    public float bulletSpeed;            //this too
    public float shotChance;               //probability that enemy fires a shot. Only applicable after player reaches certain level.
    public float shotCooldown;
    float currentTime;
    const float INIT_COOLDOWN = 2;
    const float SHOT_INC_AMT = 0.04f;

    public byte currentColor;
    const byte RED = 0;
    const byte BLUE = 1;
    const byte WHITE = 2;
    const byte BLACK = 3;

    Path flightPath;                    //the path the enemy follows when they're generated.
    List<Vector3> pathPoints;
    Vector3 direction;
    int currentPoint;
    int destinationPoint;               //tracks where enemy is along the flight path. These contain the indexes of the path vectors.

    // Start is called before the first frame update
    void Start()
    {
        //enemies are always instantiated with a random colour
        currentColor = (byte)Random.Range(RED, BLACK + 1);

        switch(currentColor)
        {
            case RED:
                GetComponent<SpriteRenderer>().sprite = enemyRed;
                break;
            case BLUE:
                GetComponent<SpriteRenderer>().sprite = enemyBlue;
                break;
            case WHITE:
                GetComponent<SpriteRenderer>().sprite = enemyWhite;
                break;
            case BLACK:
                GetComponent<SpriteRenderer>().sprite = enemyBlack;
                break;
            default:
                break;
        }

        //set shot chance according to current level. shot chance goes up the higher the level.
        shotChance = EnemyManager.instance.enemyShotChance;
        if (GameManager.instance.level % 2 == 0)
        {
            shotChance += SHOT_INC_AMT;  //increase shot chance by 4% every 2 levels
            
            if (shotChance > 1)
                shotChance = 1;

            EnemyManager.instance.enemyShotChance = shotChance;
            //Debug.Log("New enemy shot chance: " + shotChance);
        }

        //shot cooldown is random
        shotCooldown = Random.Range(INIT_COOLDOWN, INIT_COOLDOWN + INIT_COOLDOWN);

        //set path
        pathPoints = new List<Vector3>();
        Vector3 screenPos = Camera.main.WorldToViewportPoint(GameManager.instance.transform.position);
        //SetPath(flightPath.pathPoints[0]);
        pathPoints.Add(Vector3.zero);
        pathPoints.Add(new Vector3(transform.position.x, screenPos.y * -GameManager.instance.ScreenBoundaryY(), 0));
        currentPoint = 0;
        destinationPoint = currentPoint + 1;
    }

    // Update is called once per frame
    void Update()
    {
        //Enemies start shooting at the player at higher levels. Cooldown is randomized to vary the timing of shots
        if (Random.value <= shotChance && Time.time > currentTime + shotCooldown)
        {
            currentTime = Time.time;
            ShootBullet();
            //randomize the next cooldown
            shotCooldown = Random.Range(INIT_COOLDOWN, INIT_COOLDOWN + INIT_COOLDOWN);
        }

    }

    Sprite GetBulletColor(EnemyBullet bullet)
    {
        Sprite spriteColor = null;

        switch(currentColor)    //current color of the enemy
        {
            case RED:
                spriteColor = bullet.bulletRed;
                break;
            case BLUE:
                spriteColor = bullet.bulletBlue;
                break;
            case BLACK:
                spriteColor = bullet.bulletBlack;
                break;
            case WHITE:
                spriteColor = bullet.bulletWhite;
                break;
            default:
                break;
        }

        return spriteColor;
    }

    public void Move()
    {
        //default pattern is to move in a straight line until off screen.
        //transform.position = new Vector3(transform.position.x, transform.position.y - (moveSpeed * Time.deltaTime), 0);

        //get the direction of the destination point from enemy's current position.
        Vector3 direction = (pathPoints[destinationPoint] - pathPoints[currentPoint]).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        if (transform.position == pathPoints[destinationPoint])
        {
            //move to next point in path if it exists

        }

        //destroy enemy if off screen
        Vector3 screenPos = Camera.main.WorldToViewportPoint(GameManager.instance.transform.position);   //converting screen pixels to units
        if (transform.position.y + (GetComponent<SpriteRenderer>().bounds.extents.y * 2) < screenPos.y * -GameManager.instance.ScreenBoundaryY())
        {
            Destroy(gameObject);
            Debug.Log("Enemy off screen");
        }      
    }

    private void FixedUpdate()
    {
        //enemies move in different ways. Their default is to move in a straight line, but as the game progresses
        //their movement becomes more complex. 
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = GameManager.instance.player.GetComponent<Player>();
        //if enemy touches a player bullet, different interactions occur based on the enemy and player's colour.
        if (collision.CompareTag("Bullet_Player") && collision.GetComponent<SpriteRenderer>().enabled == true)  //bullet has to be on screen
        {
            if (currentColor != player.currentColor)
            {
                GameManager.instance.audioSource.PlayOneShot(GameManager.instance.explodeSound);

                //add to score
                GameManager.instance.enemyCount++;

                //check if enemy is an opposing colour. If true, then generate a powerup
                if (PlayerIsOpposingColour(player.currentColor, currentColor))
                {
                    //create powerup. Check which one to create
                    float rollValue = Random.value;
                    if (rollValue <= GameManager.instance.energyPowerUpChance)
                    {
                        Instantiate(GameManager.instance.energyPowerupPrefab, transform.position, Quaternion.identity);
                        Debug.Log("Energy powerup created, drop chance " + rollValue);
                    }
                    else
                    {
                        Instantiate(GameManager.instance.speedPowerupPrefab, transform.position, Quaternion.identity);
                        Debug.Log("Speed powerup created");
                    }
                }

                //play explosion animation
                //StartCoroutine(Explode());
                //anim = Instantiate(GameManager.instance.explosionAnim, transform.position, Quaternion.identity);
                //GameManager.instance.animController.ChangeAnimationState(anim, GameManager.instance.ExplosionState());
                Destroy(gameObject);
                //Destroy(anim, 0.5f);
                
                
                //Debug.Log("Enemy destroyed");
            }
            else
            {
                //enemy took no damage
                GameManager.instance.audioSource.PlayOneShot(GameManager.instance.blockSound);
            }

            //send bullet back to player
            collision.GetComponent<Bullet>().BulletHit = true;
            
        }
       
    }

    bool PlayerIsOpposingColour(byte playerColour, byte enemyColour)
	{
		bool coloursOpposed = false;
			
		switch (playerColour)
		{
			case WHITE:
				coloursOpposed = (enemyColour == BLACK) ? true : false;
				break;
			case BLACK:
				coloursOpposed = (enemyColour == WHITE) ? true : false;
				break;
			case RED:
				coloursOpposed = (enemyColour == BLUE) ? true : false;
				break;
			case BLUE:
				coloursOpposed = (enemyColour == RED) ? true : false;
				break;
            default:
                break;

		}

        return coloursOpposed;
	}

    public void ShootBullet()
    {
        bullet = Instantiate(enemyBulletPrefab, new Vector3(transform.position.x,
            transform.position.y - GetComponent<SpriteRenderer>().bounds.extents.y, -1), Quaternion.identity); //bullet is generated at the enemy's nose

        //change bullet colour accordingly
        EnemyBullet bulletColor = bullet.GetComponent<EnemyBullet>();
        SpriteRenderer sr = bullet.GetComponent<SpriteRenderer>();
        sr.sprite = GetBulletColor(bulletColor);
        bullet.GetComponent<EnemyBullet>().BulletSpeed = bulletSpeed;
    }

    IEnumerator Explode()
    {
        //anim = Instantiate(GameManager.instance.explosionAnim, transform.position, Quaternion.identity);
        GameManager.instance.animController.ChangeAnimationState(anim, GameManager.instance.ExplosionState());
        //while (anim.)
        yield return new WaitForSeconds(1);

        Destroy(anim);
        Destroy(gameObject);
    }
}
