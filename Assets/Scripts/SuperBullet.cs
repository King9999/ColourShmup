using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is a special attack that's accessible when the rainbow meter is full. I call it a bullet, but it's more
//like a laser in the updated game.
public class SuperBullet : MonoBehaviour
{
    public bool BulletFired { get; set; } = false;
    float drainValue = 15f;              //used to reduce rainbow gauge while firing
    float defaultScale = 0.5f;              //used to reset x scale
    public Color a;
    public Color b;

    //coroutine check
    bool isShrinkBulletRunning;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 screenPos = Camera.main.WorldToViewportPoint(GameManager.instance.transform.position);

        //set scale of bullet to reach top of screen
        transform.localScale = new Vector3(defaultScale, screenPos.y * GameManager.instance.ScreenBoundaryY() + 1, 1);

        //super bullet is hidden by default since it's always active in hierarchy
        SuperBulletEnabled(false);
        isShrinkBulletRunning = false;
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
            if (AudioEnabled() && !GetComponent<AudioSource>().isPlaying)
                GetComponent<AudioSource>().Play();

            SuperBulletEnabled(true);
           // GetComponent<SpriteRenderer>().enabled = true;
            
            Vector3 screenPos = Camera.main.WorldToViewportPoint(GameManager.instance.transform.position);

            StartCoroutine(ExpandBullet());

            //use couroutine to lerp through two random colours
            StartCoroutine(LerpColors());
            transform.position = new Vector3(GameManager.instance.playerPos.x, GameManager.instance.playerPos.y +
                GetComponent<SpriteRenderer>().bounds.extents.y + 0.45f, -1);

            //reduce rainbow meter
            HUD.instance.fillRainbowMeter.value -= drainValue * Time.deltaTime;
            HUD.instance.fillDamage.value -= drainValue * Time.deltaTime;

            //when rainbow gauge runs out, shrink bullet and then destroy it. The bool is there to prevent the
            //player from gaining more meter and prolonging the bullet when it should be gone.
            if (HUD.instance.fillRainbowMeter.value <= 0 && !isShrinkBulletRunning)
                StartCoroutine(ShrinkBullet());
        }
        else
        {
            transform.localScale = new Vector3(defaultScale, transform.localScale.y, 1);
        }
    }

    bool AudioEnabled()
    {
        return GetComponent<AudioSource>().enabled = (HUD_Menu.instance.muted == false) ? true : false;
    }

    void SuperBulletEnabled(bool toggle)
    {
        GetComponent<SpriteRenderer>().enabled = toggle;
        GetComponent<BoxCollider2D>().enabled = toggle;
    }

    #region Coroutines
    IEnumerator ExpandBullet()
    {
        //the bullet's x scale expands to a certain point. The bullet's colour also changes colour.
        float xScale = 10;
        
        while (transform.localScale.x < xScale)
        {
            transform.localScale = new Vector3(transform.localScale.x + 0.4f * Time.deltaTime, transform.localScale.y, 1);
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator ShrinkBullet()
    {
        isShrinkBulletRunning = true;
        float xScale = defaultScale;

        while (transform.localScale.x > xScale)
        {
            transform.localScale = new Vector3(transform.localScale.x - 2.4f * Time.deltaTime, transform.localScale.y, 1);
            yield return new WaitForFixedUpdate();
        }

        BulletFired = false;
        SuperBulletEnabled(false);
        isShrinkBulletRunning = false;
        //GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<AudioSource>().Stop();
        //Destroy(gameObject);
    }

    IEnumerator LerpColors()
    {
        //pick two random colours and lerp through them.
        a = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 0.5f);
        b = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 0.5f);
        //b = new Color(1, 1, 1, 0.5f);
        float time = 0;

        while (HUD.instance.fillRainbowMeter.value > 0)
        {          
            GetComponent<SpriteRenderer>().color = Color.Lerp(a, b, time);
            //time += 0.1f * Time.deltaTime;
            yield return new WaitForSeconds(0.5f);
        }
    }
    #endregion
}
