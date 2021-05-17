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
    public bool CanFireBullet { get; set; } = true;             //if there are 5 bullets on screen at once, then this is false and player cannot shoot

    private void Update()
    {
        //bullet travels in a fixed direction every time it's generated.
        gameObject.transform.position = new Vector3(0, gameObject.transform.position.y + bulletSpeed * Time.deltaTime, 0);

    }
   

}
