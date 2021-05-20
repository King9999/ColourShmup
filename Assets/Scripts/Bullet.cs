using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//generates projectiles that deal damage. May also use this as a parent class to the super attack.
public class Bullet : MonoBehaviour
{
    public float BulletSpeed { get; set; }
    public bool BulletFired { get; set; } = false;

    public bool BulletHit { get; set; } = false;        //used for collision checking, and to send bullet back to player

    private void FixedUpdate()
    {
        //bullet travels in a fixed direction every time it's generated.
        if (BulletFired)
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + BulletSpeed * Time.deltaTime, 1);

    }
   
}
