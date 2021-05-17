using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//generates projectiles that deal damage. May also use this as a parent class to the super attack.
public class Bullet : MonoBehaviour
{
    //public Sprite bulletSprite;
    public AudioClip bulletSound;
    AudioSource audioSource;
    float bulletSpeed = 12f;

    private void Update()
    {
        //bullet travels in a fixed direction every time it's generated.
        gameObject.transform.position = new Vector3(0, gameObject.transform.position.y + bulletSpeed * Time.deltaTime, 0);

        //if bullet flies off screen, it must be either destroyed or recycled. Would be better to use a set number
        //of bullets and re-use those to prevent garbage collection calls.
    }

}
