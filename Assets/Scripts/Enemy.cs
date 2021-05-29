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
    public GameObject explosionPrefab;                      //called when enemy is destroyed

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
    public List<Vector3> enemyPathPoints;
    Vector3 direction;
    int currentPoint;
    int destinationPoint;               //tracks where enemy is along the flight path. These contain the indexes of the path vectors.
    Path.PathType path;                 //randomly chosen path to follow when spawned.
    float duration = 3;

    // Start is called before the first frame update
    void Start()
    {
        Path flightPath = new Path();
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

        //set speed
        moveSpeed = EnemyManager.instance.enemyMoveSpeed;

        //set shot chance. Get current time so that enemy doesn't shoot immediately.
        shotChance = EnemyManager.instance.enemyShotChance;
        currentTime = Time.time;

        //shot cooldown is random
        shotCooldown = Random.Range(INIT_COOLDOWN - INIT_COOLDOWN, INIT_COOLDOWN + INIT_COOLDOWN);
        //shotCooldown = 0;

        //set path
        enemyPathPoints = new List<Vector3>();
        path = EnemyManager.instance.path;
        Vector3 screenPos = Camera.main.WorldToViewportPoint(GameManager.instance.transform.position);
        //enemyPathPoints = SetPath(Path.PathType.LinearVertical, EnemyManager.instance.EnemyPath());
        enemyPathPoints = SetPath(path, EnemyManager.instance.EnemyPath());
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

        //float time = 0;
        
        //get the direction of the destination point from enemy's current position.
        Vector3 direction = (enemyPathPoints[destinationPoint] - enemyPathPoints[currentPoint]).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        /*while (time < duration)
        {
            transform.position = Vector3.Lerp(enemyPathPoints[currentPoint], enemyPathPoints[destinationPoint], time / duration);
            time += Time.deltaTime;
        }*/
        if (enemyPathPoints[currentPoint] == enemyPathPoints[destinationPoint])
        {
            //Debug.Log("Got here");
            //move to next point in path if it exists
            if (destinationPoint == enemyPathPoints.Count - 1)
                return; //made it to end

            currentPoint = destinationPoint;
            destinationPoint++;
            Debug.Log("New Destination " + enemyPathPoints[destinationPoint]);
        }

        //destroy enemy if off screen
        Vector3 screenPos = Camera.main.WorldToViewportPoint(GameManager.instance.transform.position);   //converting screen pixels to units
        if (transform.position.y + (GetComponent<SpriteRenderer>().bounds.extents.y * 2) < screenPos.y * -GameManager.instance.ScreenBoundaryY())
        {           
            Destroy(gameObject);
            Debug.Log("Enemy off screen");
        }

        //if enemy spawned from left or right side, destroy enemy once they reach opposite side of screen
        //TODO: May need to change this once more complex paths are introduced
        if (direction.x > 0 && transform.position.x + (GetComponent<SpriteRenderer>().bounds.extents.x * 2) > screenPos.x * GameManager.instance.ScreenBoundaryX() + 1)
            Destroy(gameObject);
        else if (direction.x < 0 && transform.position.x + (GetComponent<SpriteRenderer>().bounds.extents.x * 2) < screenPos.x * -GameManager.instance.ScreenBoundaryX())
            Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        //enemies move in different ways. Their default is to move in a straight line, but as the game progresses
        //their movement becomes more complex. 
    }

    #region Collision Check
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = GameManager.instance.player.GetComponent<Player>();
        //if enemy touches a player bullet, different interactions occur based on the enemy and player's colour.
        if (collision.CompareTag("Bullet_Player") && collision.GetComponent<SpriteRenderer>().enabled == true)  //bullet has to be on screen
        {
            if (currentColor != player.currentColor)
            {
                //GameManager.instance.audioSource.PlayOneShot(GameManager.instance.explodeSound);

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
                        //Debug.Log("Energy powerup created, drop chance " + rollValue);
                    }
                    else
                    {
                        Instantiate(GameManager.instance.speedPowerupPrefab, transform.position, Quaternion.identity);
                        //Debug.Log("Speed powerup created");
                    }
                }

                //play death coroutine. Object also destroyed in this coroutine
                StartCoroutine(DestroyEnemy());

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

        if (collision.CompareTag("SuperBullet"))
        {
            //add to score
            GameManager.instance.enemyCount++;

            //All enemies are destroyed, regardless of colour.
            //powerups drop chance is altered. There's a chance nothing is spawned.
            float rollValue = Random.value;
            if (rollValue <= GameManager.instance.energyPowerUpChance / 2)  //penalty applied since we don't want the player to extend the gauge too much
            {
                Instantiate(GameManager.instance.energyPowerupPrefab, transform.position, Quaternion.identity);
            }
            else if (rollValue <= GameManager.instance.speedPowerUpChance)
            {
                Instantiate(GameManager.instance.speedPowerupPrefab, transform.position, Quaternion.identity);
            }

            //play death coroutine. Object also destroyed in this coroutine
            StartCoroutine(DestroyEnemy());
        }
       
    }
    #endregion
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

    public List<Vector3> SetPath(Path.PathType pathType, List<Vector3>[] path)
    {
        return path[(int)pathType];
    }

    public IEnumerator DestroyEnemy()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        GameManager.instance.audioSource.PlayOneShot(GameManager.instance.explodeSound);
        yield return null;

        Destroy(gameObject);
    }
}
