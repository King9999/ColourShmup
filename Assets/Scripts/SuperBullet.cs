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
        transform.localScale = new Vector3(1, screenPos.y * GameManager.instance.ScreenBoundaryY() + 1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        /* the super bullet animates as follows:
         * -Starts out as a thin line
         * -widens over time
         * -Continuously remains on screen until rainbow meter runs out
         * -when gauge runs out, line shrinks and then disappears */
        Vector3 screenPos = Camera.main.WorldToViewportPoint(GameManager.instance.transform.position);
        /*bulletPoints[0] = new Vector3(GameManager.instance.playerPos.x, GameManager.instance.playerPos.y +
            GameManager.instance.player.GetComponent<SpriteRenderer>().bounds.extents.y - 0.1f, -1);           //laser positioned at player's nose
        bulletPoints[1] = new Vector3(GameManager.instance.playerPos.x, screenPos.y * GameManager.instance.ScreenBoundaryY() + 1, -1);
        bullet.SetPositions(bulletPoints);*/

        transform.position = new Vector3(GameManager.instance.playerPos.x, GameManager.instance.playerPos.y +
            GetComponent<SpriteRenderer>().bounds.extents.y, -1);
    }
}
