using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is a special attack that's accessible when the rainbow meter is full. I call it a bullet, but it's more
//like a laser in the updated game.
public class SuperBullet : MonoBehaviour
{
    //LineRenderer bullet;
    //BoxCollider2D hitbox;
    //public Vector3[] bulletPoints;

    public bool BulletFired { get; set; } = false;
    float drainValue = 15f;              //used to reduce rainbow gauge while firing

    // Start is called before the first frame update
    void Start()
    {
        Vector3 screenPos = Camera.main.WorldToViewportPoint(GameManager.instance.transform.position);
        /*bullet = gameObject.AddComponent<LineRenderer>();
        hitbox = gameObject.AddComponent<BoxCollider2D>();
        hitbox.isTrigger = true;
        hitbox.size = new Vector2(0.1f, screenPos.y * GameManager.instance.ScreenBoundaryY() + 1);
        hitbox.offset = new Vector2(0, 2.5f);
        
        bulletPoints = new Vector3[2];
        //bulletPoints[0] = new Vector3(GameManager.instance.playerPos.x, GameManager.instance.playerPos.y +
        //GameManager.instance.player.GetComponent<SpriteRenderer>().bounds.extents.y, -1);           //laser positioned at player's nose
        //bulletPoints[1] = new Vector3(GameManager.instance.playerPos.x, screenPos.y * GameManager.instance.ScreenBoundaryY(), -1);
        bulletPoints[0] = Vector3.zero;
        bulletPoints[1] = Vector3.zero;
        bullet.positionCount = bulletPoints.Length;
        bullet.startWidth = 0.05f;
        bullet.endWidth = 0.05f;
        //bullet.SetPositions(bulletPoints);*/

        //set scale of bullet to reach top of screen
        transform.localScale = new Vector3(0.5f, screenPos.y * GameManager.instance.ScreenBoundaryY() + 1, 1);

        //super bullet is hidden by default
        GetComponent<SpriteRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        /* the super bullet animates as follows:
         * -Starts out as a thin line
         * -widens over time
         * -Continuously remains on screen until rainbow meter runs out
         * -when gauge runs out, line shrinks and then disappears */

        if (BulletFired)
        {
            GetComponent<SpriteRenderer>().enabled = true;
            Vector3 screenPos = Camera.main.WorldToViewportPoint(GameManager.instance.transform.position);
            /*bulletPoints[0] = new Vector3(GameManager.instance.playerPos.x, GameManager.instance.playerPos.y +
                GameManager.instance.player.GetComponent<SpriteRenderer>().bounds.extents.y - 0.1f, -1);           //laser positioned at player's nose
            bulletPoints[1] = new Vector3(GameManager.instance.playerPos.x, screenPos.y * GameManager.instance.ScreenBoundaryY() + 1, -1);
            bullet.SetPositions(bulletPoints);*/

            StartCoroutine(ExpandBullet());
            transform.position = new Vector3(GameManager.instance.playerPos.x, GameManager.instance.playerPos.y +
                GetComponent<SpriteRenderer>().bounds.extents.y + 0.45f, -1);

            //reduce rainbow meter
            HUD.instance.fillRainbowMeter.value -= drainValue * Time.deltaTime;

            //when rainbow gauge runs out, shrink bullet and then destroy it.
            if (HUD.instance.fillRainbowMeter.value <= 0)
                StartCoroutine(ShrinkBullet());
        }
    }

    IEnumerator ExpandBullet()
    {
        //the bullet's x scale expands to a certain point. The bullet's colour also changes colour.
        float xScale = 10;
        
        while (transform.localScale.x < xScale)
        {
            transform.localScale = new Vector3(transform.localScale.x + 0.2f * Time.deltaTime, transform.localScale.y, 1);
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator ShrinkBullet()
    {
        float xScale = 0;

        while (transform.localScale.x > xScale)
        {
            transform.localScale = new Vector3(transform.localScale.x - 1.2f * Time.deltaTime, transform.localScale.y, 1);
            yield return new WaitForFixedUpdate();
        }

        BulletFired = false;
        GetComponent<SpriteRenderer>().enabled = false;
        //Destroy(gameObject);
    }
}
