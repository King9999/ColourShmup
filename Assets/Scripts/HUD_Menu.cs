using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUD_Menu : MonoBehaviour
{
    [Header("UI")]
    float menuAppearanceTimer;              //controls when the menu appears on screen. Title must finish animating first.
    //public Animation anim;
    public TextMeshProUGUI startGameText;
    public TextMeshProUGUI helpMenuText;
    public TextMeshProUGUI soundText;       //allows game to be muted.
    public TextMeshProUGUI soundToggleText;
    public Image cursor;
   
    void Start()
    {
        menuAppearanceTimer = 1;
        cursor.enabled = false;
        startGameText.enabled = false;
        helpMenuText.enabled = false;
        soundText.enabled = false;
        soundToggleText.enabled = false;

        //get sound setting
        if (HUD.instance.muted)
            soundToggleText.text = "Off";
        else
            soundToggleText.text = "On";
        StartCoroutine(DisplayMenu());
    }

    IEnumerator DisplayMenu()
    {
       float currentTime = Time.time;
       while (Time.time < currentTime + menuAppearanceTimer)
            yield return null;

        cursor.enabled = true;
        startGameText.enabled = true;
        helpMenuText.enabled = true;
        soundText.enabled = true;
        soundToggleText.enabled = true;
    }
}
