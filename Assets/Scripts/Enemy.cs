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
    public GameObject explosionPrefab;                      //called when enemy is destroyed


    [Header("Enemy Properties")]
    public float moveSpeed;              //this increases as the game progresses
    public float bulletSpeed;            //this too
    public float shotChance;               //probability that enemy fires a shot. Only applicable after player reaches certain level.
    public float shotCooldown;
    float currentTime;
    const float INIT_COOLDOWN = 2;
    public int enemyID;                        //used to track which path to destroy when enemy is destroyed.

    public byte currentColor;
    const byte RED = 0;
    const byte BLUE = 1;
    const byte WHITE = 2;
    const byte BLACK = 3;

    //path variables
    public List<Vector3> enemyPathPoints;
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

        //set speed
        moveSpeed = EnemyManager.instance.enemyMoveSpeed;

        //set shot chance. Get current time so that enemy doesn't shoot immediately.
        shotChance = EnemyManager.instance.enemyShotChance;
        currentTime = Time.time;

        //shot cooldown is random
        shotCooldown = Random.Range(INIT_COOLDOWN - INIT_COOLDOWN, INIT_COOLDOWN);
        //shotCooldown = 0;

        //set path
        enemyPathPoints = new List<Vector3>();

       // Vector3 screenPos = Camera.main.WorldToViewportPoint(GameManager.instance.transform.position);
       // float boundary = GameManager.instance.ScreenBoundaryX();
       // int randomPath = Random.Range(0, pathPrefab.Length);
       // EnemyManager.instance.pathList.Add(Instantiate(pathPrefab[randomPath], new Vector3(Random.Range(-screenPos.x * boundary, screenPos.x * boundary), 0, 0), Quaternion.identity));

        //get a path and populate the list with points.
        int lastPath = EnemyManager.instance.pathList.Count - 1;
        enemyID = lastPath;
        Transform transform = EnemyManager.instance.pathList[lastPath].GetComponent<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            enemyPathPoints.Add(new Vector3(transform.GetChild(i).position.x, transform.GetChild(i).position.y, 0));
            //Debug.Log("Enemy Point: " + enemyPathPoints[i]);
        }

       
        
        //path = EnemyManager.instance.path;
       // Vector3 screenPos = Camera.main.WorldToViewportPoint(GameManager.instance.transform.position);
        //enemyPathPoints = SetPath(Path.PathType.LinearVertical, EnemyManager.instance.EnemyPath());
        //enemyPathPoints = SetPath(path, EnemyManager.instance.EnemyPath());
        currentPoint = 0;
        destinationPoint = currentPoint + 1;

        //set up enemy's first position, which should always be offscreen
        //transform.position = enemyPathPoints[currentPoint];
    }

    // Update is called once per frame
    void Update()
    {
        //Enemies start shooting at the player at higher levels. I multiply value by 20 to reduce the frequency of shots. If it's still too high
        //I may reduce the shot chance.
        float shotRoll = Random.value * 20;
        if (shotRoll > 1) 
            shotRoll = 1;
        if (shotRoll <= shotChance && Time.time > currentTime + shotCooldown)
        {
            Debug.Log("Random value " + shotRoll);
            currentTime = Time.time;
            ShootBullet();
            //randomize the next cooldown to vary timing of shots
            shotCooldown = Random.Range(INIT_COOLDOWN - 1, INIT_COOLDOWN);
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
           
        //get the direction of the destination point from enemy's current position.
        Vector3 direction = (enemyPathPoints[destinationPoint] - enemyPathPoints[currentPoint]).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
     

        //if the enemy is close to the destination, want them to "snap" to the destination point so they don't overshoot
        float diffX = Mathf.Abs(enemyPathPoints[destinationPoint].x - transform.position.x);
        float diffY = Mathf.Abs(enemyPathPoints[destinationPoint].y - transform.position.y);
        //Debug.Log("DiffX: " + diffX + " DiffY: " + diffY);
        if (diffX >= 0 && diffX < 0.05f && diffY >= 0 && diffY < 0.05f)
        {
            enemyPathPoints[currentPoint] = enemyPathPoints[destinationPoint];
        }

        if (enemyPathPoints[currentPoint] == enemyPathPoints[destinationPoint])
        {
            //move to next point in path if it exists
            if (destinationPoint == enemyPathPoints.Count - 1)
            {
                //made it to end, destroy enemy.
                Destroy(gameObject);
                //Destroy(EnemyManager.instance.pathList[enemyID]);
            }
            else
            {
                currentPoint = destinationPoint;
                destinationPoint++;
                //Debug.Log("New Destination " + enemyPathPoints[destinationPoint]);
            }
        }

        //destroy enemy if off screen
        Vector3 screenPos = Camera.main.WorldToViewportPoint(GameManager.instance.transform.position);   //converting screen pixels to units
        if (transform.position.y + (GetComponent<SpriteRenderer>().bounds.extents.y * 2) < screenPos.y * -GameManager.instance.ScreenBoundaryY())
        {
            Destroy(gameObject);
            //Destroy(EnemyManager.instance.pathList[enemyID]);
            // Debug.Log("Enemy off screen");
        }

        //if enemy spawned from left or right side, destroy enemy once they reach opposite side of screen
        //TODO: May need to change this once more complex paths are introduced
        if (direction.x > 0 && transform.position.x + (GetComponent<SpriteRenderer>().bounds.extents.x * 2) > screenPos.x * GameManager.instance.ScreenBoundaryX() + 1)
        {
            Destroy(gameObject);
            //Destroy(EnemyManager.instance.pathList[enemyID]);
        }
        else if (direction.x < 0 && transform.position.x + (GetComponent<SpriteRenderer>().bounds.extents.x * 2) < screenPos.x * -GameManager.instance.ScreenBoundaryX())
        {
            Destroy(gameObject);
            //Destroy(EnemyManager.instance.pathList[enemyID]);
        }
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
        EnemyManager.instance.enemyBullets.Add(Instantiate(enemyBulletPrefab, new Vector3(transform.position.x,
            transform.position.y - GetComponent<SpriteRenderer>().bounds.extents.y, -1), Quaternion.identity)); //bullet is generated at the enemy's nose

        //change bullet colour accordingly
        int lastBullet = EnemyManager.instance.enemyBullets.Count - 1;
        EnemyBullet bulletColor = EnemyManager.instance.enemyBullets[lastBullet].GetComponent<EnemyBullet>();
        SpriteRenderer sr = EnemyManager.instance.enemyBullets[lastBullet].GetComponent<SpriteRenderer>();
        sr.sprite = GetBulletColor(bulletColor);
        EnemyManager.instance.enemyBullets[lastBullet].GetComponent<EnemyBullet>().BulletSpeed = bulletSpeed;
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
        //Destroy(EnemyManager.instance.pathList[enemyID]);
    }

    /*public IEnumerator FollowPath(int routeNum)
    {
        pathCoroutineRunning = true;
        Vector3 p0 = flightPath.routes[routeNum].GetChild(0).position;
        Vector3 p1 = flightPath.routes[routeNum].GetChild(1).position;
        Vector3 p2 = flightPath.routes[routeNum].GetChild(2).position;
        Vector3 p3 = flightPath.routes[routeNum].GetChild(3).position;

        //enemy follows assigned path until they're off screen or player kills them
        while (t < 1)
        {
            t += Time.deltaTime * moveSpeed;

            transform.position = Mathf.Pow(1 - t, 3) * p0 + 3 * Mathf.Pow(1 - t, 2) * t * p1
                + 3 * (1 - t) * Mathf.Pow(t, 2) * p2 + Mathf.Pow(t, 3) * p3;

            transform.position = Camera.main.WorldToViewportPoint(GameManager.instance.transform.position);
            Debug.Log("Enemy moving to " + transform.position);
            yield return new WaitForFixedUpdate();
        }

        t = 0f;
        routeCounter++;
        if (routeCounter > flightPath.routes.Length - 1)
            routeCounter = 0;

        pathCoroutineRunning = false;
    }*/
}
