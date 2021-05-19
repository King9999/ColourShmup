using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//gives +20 to rainbow gauge
public class EnergyPowerup : MonoBehaviour
{
    public float gaugeAmount;       //how much meter is gained
    public float vy;                //how fast powerup falls

    private void Start()
    {
        
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

            //display pickup label
            GameManager.instance.energyLabelList.Add(Instantiate(GameManager.instance.energyLabelPrefab, transform.position, Quaternion.identity));

            Player player = collision.GetComponent<Player>();

           //Add energy to the rainbow gauge


            Destroy(gameObject);
            //Debug.Log("Touched Powerup");
        }
    }
}
