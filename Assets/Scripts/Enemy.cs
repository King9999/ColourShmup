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
    float cooldown = 3;
    float currentTime = 0;


    // Start is called before the first frame update
    void Start()
    {
        //test bullet
        bullet = Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);
        bullet.GetComponent<EnemyBullet>().BulletSpeed = bulletSpeed;
        Debug.Log("Enemy Bullet fired");
    }

    // Update is called once per frame
    void Update()
    {
        //Enemies start shooting at the player at higher levels.
        if (Time.time > currentTime + cooldown)
        {
            currentTime = Time.time;
            bullet = Instantiate(enemyBulletPrefab, transform.position, Quaternion.identity);
            bullet.GetComponent<EnemyBullet>().BulletSpeed = bulletSpeed;
            Debug.Log("Enemy Bullet fired");
        }
    }

    private void FixedUpdate()
    {
        //enemies move in different ways. Their default is to move in a straight line, but as the game progresses
        //their movement becomes more complex. 
    }
}
