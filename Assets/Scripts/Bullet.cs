using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//generates projectiles that deal damage. May also use this as a parent class to the super attack.
public class Bullet : MonoBehaviour
{
    public float BulletSpeed { get; set; }
    public bool BulletFired { get; set; } = false;     

    private void FixedUpdate()
    {
        //bullet travels in a fixed direction every time it's generated.
        if (BulletFired)
            gameObject.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y + BulletSpeed * Time.deltaTime, 1);

    }
   
}
