using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//parent class for all powerups in the game
public class SpeedPowerup : MonoBehaviour
{

    public float cooldownMod;       //reduces shot cooldown

    public bool IsOnScreen { get; set; }

    private void FixedUpdate()
    {
        //when a powerup is generated, it travels downward until it's off the screen.


        /*if(!GetComponent<SpriteRenderer>().isVisible)
        {
            //hide the powerup until next time it's generated.
            gameObject.SetActive(false);
        }*/
    }
    public void ActivateEffect()
    {
        //increase player's bullet speed. It should be capped.
        Bullet bullet = FindObjectOfType<Bullet>();

        /*if (bullet.bulletSpeed < MAX_SPEED)
        {
            bullet.bulletSpeed++;
            Debug.Log("Bullet Speed Up");
        }*/

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {

            Player player = collision.GetComponent<Player>();

            if (player.bulletSpeed < player.MaxBulletSpeed())
            {
                player.bulletSpeed++;
                foreach (GameObject bullet in player.playerBullets)
                {
                    bullet.GetComponent<Bullet>().BulletSpeed = player.bulletSpeed;
                }

                //cooldown is reduced slightly every time speed goes up so there isn't a large gap between shots.
                player.shotCooldown -= cooldownMod;

                //Debug.Log("Bullet Speed +1, cooldown is now " + player.shotCooldown);
            }

            Destroy(gameObject);
            //Debug.Log("Touched Powerup");
        }
    }
}
