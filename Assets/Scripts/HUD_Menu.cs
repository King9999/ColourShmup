using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class HUD_Menu : MonoBehaviour
{
    [Header("UI")]
    float menuAppearanceTimer;              //controls when the menu appears on screen. Title must finish animating first.
    public TextMeshProUGUI startGameText;
    public TextMeshProUGUI helpMenuText;
    public TextMeshProUGUI soundText;       //allows game to be muted.
    public TextMeshProUGUI soundToggleText;
    public Image cursor;
    //public GameObject menu;                 //container with all the menu options
    public bool muted;
    float xOffset;                          //adjusts cursor

    [Header("Screen Fade")]
    public Animator anim;

    [Header("Audio")]
    public AudioSource soundSource;         //used for sound test

    Vector3[] menus;                        //contains positions of menu elements.
    int currentMenu;
    const int START = 0;
    const int HELP = 1;
    const int SOUND = 2;

    public static HUD_Menu instance;

    //delay timer to prevent rapid input
    float delayTime;
    float currentTime;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        //DontDestroyOnLoad(this);
    }
    void Start()
    {
        currentTime = 0;
        delayTime = 0.16f;
        xOffset = 50;
        menus = new Vector3[3];
        muted = false;
        menuAppearanceTimer = 1;
        cursor.enabled = false;
        startGameText.enabled = false;
        helpMenuText.enabled = false;
        soundText.enabled = false;
        soundToggleText.enabled = false;

        //set up cursor position
        menus[START] = new Vector3(startGameText.transform.position.x - startGameText.rectTransform.rect.width / 2 - xOffset, startGameText.transform.position.y, 0);
        menus[HELP] = new Vector3(helpMenuText.transform.position.x - helpMenuText.rectTransform.rect.width / 2 - xOffset, helpMenuText.transform.position.y, 0);
        menus[SOUND] = new Vector3(soundText.transform.position.x - soundText.rectTransform.rect.width / 2 - xOffset, soundText.transform.position.y, 0);
        cursor.transform.position = menus[START];
        //cursor.transform.position = new Vector3(menu.transform.position.x - menu.GetComponent<RectTransform>().rect.width / 2, menu.transform.position.y, 0);
        currentMenu = START;

        //get sound setting
        if (muted)
            soundToggleText.text = "Off";
        else
            soundToggleText.text = "On";
        StartCoroutine(DisplayMenu());
    }

    private void Update()
    {
        //menu positions can update when the screen size is changed. Must get updated positions so the cursor position doesn't get messed up
        menus[START] = new Vector3(startGameText.transform.position.x - startGameText.rectTransform.rect.width / 2 - xOffset, startGameText.transform.position.y, 0);
        menus[HELP] = new Vector3(helpMenuText.transform.position.x - helpMenuText.rectTransform.rect.width / 2 - xOffset, helpMenuText.transform.position.y, 0);
        menus[SOUND] = new Vector3(soundText.transform.position.x - soundText.rectTransform.rect.width / 2 - xOffset, soundText.transform.position.y, 0);
        cursor.transform.position = menus[currentMenu];
    }

    public void OnPressDown(InputAction.CallbackContext context)
    {
        if (Time.time > currentTime + delayTime && context.phase == InputActionPhase.Performed)
        {
            currentTime = Time.time;
            if (currentMenu + 1 <= menus.Length - 1)
                currentMenu++;
            else
                currentMenu = 0;

            cursor.transform.position = menus[currentMenu];
            //move cursor to next menu option
            Debug.Log("Pressed Down");
        }
    }

    public void OnPressUp(InputAction.CallbackContext context)
    {
        if (Time.time > currentTime + delayTime && context.phase == InputActionPhase.Performed)
        {
            currentTime = Time.time;
            if (currentMenu - 1 >= 0)
                currentMenu--;
            else
                currentMenu = menus.Length - 1;

            cursor.transform.position = menus[currentMenu];
            //move cursor to next menu option
            Debug.Log("Pressed Up");
        }
    }

    public void OnSelectedMenu(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            //check what player selected and move to new scene. If sound was selected, toggle on or off
            if (currentMenu == START)
            {
                //load game
                StartCoroutine(OpenScene("Game"));
            }
            else if (currentMenu == HELP)
            {
                //load help
                StartCoroutine(OpenScene("Help"));
            }
            else
            {
                //toggle sound
                muted = !muted;
                soundToggleText.text = (muted == true) ? "Off" : "On";
                if (muted == false)
                    //play sound to alert player that sound is on.
                    soundSource.Play();
            }
            //Debug.Log("Selected Menu Option");
        }
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

    IEnumerator OpenScene(string sceneName)
    {
        anim.SetTrigger("Start");
        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(sceneName);
    }
}
