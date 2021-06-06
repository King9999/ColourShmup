using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class HUD : MonoBehaviour
{
    [Header("Screen Fade")]
    public Animator anim;

    [Header("Rainbow Gauge")]
    public Slider fillRainbowMeter;
    public Slider fillDamage;
    public float reductionAmount;           //controls how fast damage fill depletes

    [Header("UI Text")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI enemyCountText;      //contains both the current count and the target amount
    public TextMeshProUGUI livesCountText;
    public TextMeshProUGUI readyText;
    public GameObject pauseState;           //contains a screen and text that appears when game paused

    [Header("Audio")]
    public Image muteIcon;
    //public bool muted;                       //false by default

    [Header("Player Colours")]
    public Image livesImage;
    public Sprite livesSpriteRed;
    public Sprite livesSpriteBlue;
    public Sprite livesSpriteBlack;
    public Sprite livesSpriteWhite;

    [Header("Game Over")]
    public Image gameoverImage;

    //static variable
    public static HUD instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);    //Only want one instance of game manager
            return;
        }

        instance = this;
       // DontDestroyOnLoad(this);    //want to be able to use this on multiple scenes, where sound can be disabled/enabled
    }

    private void Start()
    {
        //mute and game over icon disabled by default
        muteIcon.enabled = false;
        gameoverImage.enabled = false;
        pauseState.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //adjust damage gauge
        StartCoroutine(ReduceDamageBar());

        //check for pause
        pauseState.SetActive(GameManager.instance.gamePaused);
    }

    IEnumerator ReduceDamageBar()
    {
        while (fillDamage.value > fillRainbowMeter.value)
        {
            fillDamage.value -= reductionAmount * Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator FlashReadyText()
    {
        float currentTime = Time.time;
        while (Time.time < currentTime + EnemyManager.instance.postLevelCooldown)
        {
            if (readyText.enabled)
                readyText.enabled = false;
            else if (!readyText.enabled)
                readyText.enabled = true;

            yield return new WaitForSeconds(0.1f);
        }

        //hide text until level is cleared
        readyText.enabled = false;
    }

    public void ShowGetReadyText()
    {
        StartCoroutine(FlashReadyText());
    }

    public void AdjustRainbowGauge(float amount)
    {
        fillRainbowMeter.value += amount;

        if (fillRainbowMeter.value < fillDamage.value)  //if we took damage, do nothing more because couroutine will handle damage bar value
            return;

        fillDamage.value = fillRainbowMeter.value;
    }

    public void SetRainbowGaugeMaxValue(float amount)
    {
        fillRainbowMeter.maxValue = amount;
        fillRainbowMeter.value = 0;
        fillDamage.maxValue = fillRainbowMeter.maxValue;
        fillDamage.value = fillRainbowMeter.value;
    }

   
}
