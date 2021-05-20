using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//increases bullet speed
public class SpeedPowerup : MonoBehaviour
{

    public float cooldownMod;       //reduces shot cooldown
    public float vy;                //how fast powerup falls
    public float speedAmount;       //how much to increase bullet speed
    AudioSource audioSource;
    //public AudioClip pickupSound;
    //public GameObject pickupLabel;

    private void Start()
    {
        //audioSource = GetComponent<AudioSource>();
    }
    private void FixedUpdate()
    {
        //when a powerup is generated, it travels downward until it's off the screen.
        transform.position = new Vector3(transform.position.x, transform.position.y - (vy * Time.deltaTime), 0);
    }

    private void Update()
    {
        //remove powerup if it goes offscreen
        Vector3 screenPos = Camera.main.WorldToViewportPoint(GameManager.instance.transform.position);

        if (transform.position.y + (GetComponent<SpriteRenderer>().sprite.bounds.extents.y * 2) < screenPos.y * -GameManager.instance.ScreenBoundaryY())
        {
            Destroy(gameObject);
            Debug.Log("Powerup went off screen");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {

            //play sound. Game manager must play the sound because sound will not play if it's attached to an object that's about to be destroyed.
            GameManager.instance.audioSource.PlayOneShot(GameManager.instance.pickupSound);

            Player player = collision.GetComponent<Player>();

            if (player.bulletSpeed < player.MaxBulletSpeed())
            {
                player.bulletSpeed += speedAmount;
                foreach (GameObject bullet in player.playerBullets)
                {
                    bullet.GetComponent<Bullet>().BulletSpeed = player.bulletSpeed;
                }

                //display pickup label
                GameManager.instance.speedUpLabelList.Add(Instantiate(GameManager.instance.speedUpLabelPrefab, transform.position, Quaternion.identity));

                //cooldown is reduced slightly every time speed goes up so there isn't a large gap between shots.
                player.shotCooldown -= cooldownMod;

                //Debug.Log("Bullet Speed +" + speedAmount + ", cooldown is now " + player.shotCooldown);
            }

         
            Destroy(gameObject);
            //Debug.Log("Touched Powerup");
        }
    }


}
