using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    public float BulletSpeed { get; set; }

    Vector3 playerLastPos;
    Vector3 bulletDirection;        //used to figure out where bullet needs to travel.

    private void Start()
    {
        //get player's last known position so bullet can travel towards it.
        playerLastPos = GameManager.instance.player.transform.position;
        bulletDirection = (playerLastPos - transform.position).normalized;
    }

    private void FixedUpdate()
    {
        //enemy bullets travel in a straight line towards the player.
        transform.position += bulletDirection * BulletSpeed * Time.deltaTime;
       
    }

    private void Update()
    {
        //destroy bullet if it goes offscreen.
        if (!GetComponent<SpriteRenderer>().isVisible)
        {
            Destroy(gameObject);
            Debug.Log("Enemy bullet destroyed");
        }
    }
}
