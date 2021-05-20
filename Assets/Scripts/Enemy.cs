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
 
    [Header("Enemy Properties")]
    private float vx, vy;                //velocity. Both values should be the same
    public float moveSpeed;              //this increases as the game progresses
    public float bulletSpeed;            //this too
    float shotChance;                    //probability that enemy fires a shot. Only applicable after player reaches certain level.
    float cooldown = 3;
    float currentTime;

    byte currentColor;
    const byte RED = 0;
    const byte BLUE = 1;
    const byte WHITE = 2;
    const byte BLACK = 3;

    // Start is called before the first frame update
    void Start()
    {
        //test bullet
        //bullet = Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);
        // bullet.GetComponent<EnemyBullet>().BulletSpeed = bulletSpeed;
        //Debug.Log("Enemy Bullet fired");

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
    }

    // Update is called once per frame
    void Update()
    {
        //Enemies start shooting at the player at higher levels.
        if (Time.time > currentTime + cooldown /*Random.value <= shotChance*/)
        {
            currentTime = Time.time;
            bullet = Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);
            bullet.GetComponent<EnemyBullet>().BulletSpeed = bulletSpeed;
            //Debug.Log("Enemy Bullet fired, shot chance: " + shotChance * 100 + "%");
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
        if (collision.CompareTag("Bullet_Player"))
        {
            if (currentColor != player.currentColor)
            {
                //check if enemy is an opposing colour. If true, then generate a powerup
                if(PlayerIsOpposingColour(player.currentColor, currentColor))
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
                //Destroy(gameObject);
                Debug.Log("Enemy destroyed");
            }
            else
            {
                //play a sound effect that indicates bullet had no effect
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
}
