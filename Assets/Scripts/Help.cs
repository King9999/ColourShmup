using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Help : MonoBehaviour
{
    public TextMeshProUGUI returnText;
    public Image superBulletImg;
    public Animator anim;
    bool alphaOn = true;

    // Update is called once per frame
    void Update()
    {
        //super bullet is random colours
        //pick two random colours and lerp through them.
        Color a = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 0.5f);
        Color b = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 0.5f);

        float time = 0.2f;
       // while (time < 1)
        //{
            superBulletImg.color = Color.Lerp(a, b, time);
        // time += 0.1f * Time.deltaTime;
        // }

        //return text will pulse
        if (alphaOn)
        {
            returnText.alpha += Time.deltaTime;
        }
        else
        {
            returnText.alpha -= Time.deltaTime;
        }

        if (returnText.alpha <= 0)
        {
            alphaOn = true;
        }

        if (returnText.alpha >= 1)
        {
            alphaOn = false;
        }
       
    }

    public void OnStart(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            //return to title
            StartCoroutine(Return());
        }
    }

    IEnumerator Return()
    {
        anim.SetTrigger("Start");
        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene("Menu");
    }
}
