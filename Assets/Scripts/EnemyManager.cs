using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Handles enemy movement patterns and tracks all living enemies.
public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Data")]
    public List<GameObject> enemies;
    public List<GameObject> enemyBullets;
    public List<GameObject> pathList;
    public GameObject enemyPrefab;
    public GameObject[] pathPrefab;             //list of enemy paths that an enemy chooses from random             
    public float enemyShotChance;
    public float enemyMoveSpeed;
    public float bulletSpeed;
    //public Path enemyPath;
   //public Path.PathType path;
    public int currentEnemy;
    float currentTime;
    public float spawnTimer;                       //controls how fast enemies are spawned. can be random. Value is in seconds
    public float postLevelCooldown;                        //breather period whenever player beats level.
    public float spawnMod;                         //reduces spawn timer
    int totalEnemyCount;                         //limits how many enemies can be on screen at once. Number increases as level increases.
    int currentEnemyCount;

    //consts
    const int INIT_ENEMY_COUNT = 4;
    const float INIT_SPAWN_TIME = 2;
    const float SHOT_INC_AMT = 0.08f;
    const float INIT_MOVE_SPEED = 2f;

    //a new path unlocks after each level. After level 10, enemy speed increases.
    public enum PathType
    {
        LinearVertical,
        LinearHorizontalR,          
        LinearHorizontalL,          
        ZigzagVertical,            
        LoopR,                      
        Lasso,
        CurveUp,
        CurveDown,
        SwirlL,
        SwirlR
    };

    int pathUnlockLevel;            //index that controls which paths are unlocked.

    public static EnemyManager instance;

    private void Awake()
    {
        /*if (instance != null && instance != this)
        {
            Destroy(gameObject);    //Only want one instance of enemy manager
            return;
        }*/

        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        //enemyPath = new Path();
        currentTime = Time.time;            //in case game is restarted, time must be reset so post level cooldown begins.
        totalEnemyCount = INIT_ENEMY_COUNT;
        spawnTimer = INIT_SPAWN_TIME;
        postLevelCooldown = 5;          //game starts with cooldown so player can get ready
        pathUnlockLevel = 0;
        spawnMod = 0;
        enemyMoveSpeed = INIT_MOVE_SPEED;

        enemies = new List<GameObject>();
        enemyBullets = new List<GameObject>();
        pathList = new List<GameObject>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!GameManager.instance.isGameOver)
        {
            //check if it's time to spawn another enemy.
            if (Time.time > currentTime + (spawnTimer - spawnMod) + postLevelCooldown && currentEnemyCount < totalEnemyCount)
            {
                Vector3 screenPos = Camera.main.WorldToViewportPoint(GameManager.instance.transform.position);
                float boundaryX = GameManager.instance.ScreenBoundaryX();
                float boundaryY = GameManager.instance.ScreenBoundaryY();
                currentTime = Time.time;
                postLevelCooldown = 0;

                int randomPath = UnityEngine.Random.Range(0, pathUnlockLevel + 1);

                //randomize the path's starting X or Y position based on the path chosen
                if (randomPath == (int)PathType.LinearHorizontalL || randomPath == (int)PathType.LinearHorizontalR || randomPath == (int)PathType.LoopR
                    || randomPath == (int)PathType.CurveUp || randomPath == (int)PathType.CurveDown)
                {
                    pathList.Add(Instantiate(pathPrefab[randomPath], new Vector3(0, UnityEngine.Random.Range(-screenPos.y * boundaryY + 1, screenPos.y * boundaryY - 1), 0), Quaternion.identity));
                    enemies.Add(Instantiate(enemyPrefab, new Vector3(pathPrefab[randomPath].GetComponent<Transform>().GetChild(0).position.x,
                       pathPrefab[randomPath].GetComponent<Transform>().GetChild(0).position.y + pathList[pathList.Count - 1].GetComponent<Transform>().position.y, 0), Quaternion.identity));
                }
                else
                {
                    pathList.Add(Instantiate(pathPrefab[randomPath], new Vector3(UnityEngine.Random.Range(-screenPos.x * boundaryX + 1, screenPos.x * boundaryX - 1), 0, 0), Quaternion.identity));
                    enemies.Add(Instantiate(enemyPrefab, new Vector3(pathPrefab[randomPath].GetComponent<Transform>().GetChild(0).position.x + pathList[pathList.Count - 1].GetComponent<Transform>().position.x,
                        pathPrefab[randomPath].GetComponent<Transform>().GetChild(0).position.y, 0), Quaternion.identity));
                }

                currentEnemyCount++;
            }

            //list cleanup
            CleanupEnemyList();
            CleanupBulletList();
            CleanupPathList();
        }
    }

    //removes any null references after enemies are destroyed.
    public void CleanupEnemyList()
    {
        if (enemies.Count <= 0)
            return;

        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] == null)
            {
                enemies.RemoveAt(i);
                i--;
                currentEnemyCount--;
            }
        }

        //Debug.Log("Enemy Count: " + enemies.Count);
    }

    public void CleanupBulletList()
    {
        if (enemyBullets.Count <= 0)
            return;

        for (int i = 0; i < enemyBullets.Count; i++)
        {
            if (enemyBullets[i] == null)
            {
                enemyBullets.RemoveAt(i);
                i--;
            }
        }
    }

    public void CleanupPathList()
    {
        if (pathList.Count <= 0)
            return;

        for (int i = 0; i < pathList.Count; i++)
        {
            if (pathList[i] == null)
            {
                pathList.RemoveAt(i);
                i--;
            }
        }
    }

    public Path.PathType GetNewPath()
    {
        int path = UnityEngine.Random.Range(0, 1/*Enum.GetNames(typeof(Path.PathType)).Length + 1*/);

        return (Path.PathType)path;
    }

    public float ShotChanceAmount()
    {
        return SHOT_INC_AMT;
    }
    /*public List<Vector3>[] EnemyPath()
    {
        return enemyPath.pathPoints;
    }*/

    public void MoveAllEnemies()
    {
        StartCoroutine(MoveEnemies());
    }

    public void DestroyAllEnemies()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            StartCoroutine(enemies[i].GetComponent<Enemy>().DestroyEnemy());
        }

        //kill all bullets
        for (int i = 0; i < enemyBullets.Count; i++)
        {
            Destroy(enemyBullets[i]);
            //enemyBullets.RemoveAt(i);
        }

        //and finally, kill the path list
        for (int i = 0; i < pathList.Count; i++)
        {
            Destroy(pathList[i]);
            //pathList.RemoveAt(i);
        }     
        pathList.Clear();
        pathList.Capacity = 0;
        enemyBullets.Clear();
        enemyBullets.Capacity = 0;
    }

    public void AdvanceLevel()
    {
        DestroyAllEnemies();
        currentTime = Time.time;
        postLevelCooldown = 5;             //giving player a breather before level begins
        currentEnemyCount = 0;
        totalEnemyCount++;
        enemyMoveSpeed += 0.2f;
        bulletSpeed += 0.1f;

        //unlock new path. Ends after level 10
        if (pathUnlockLevel + 1 <= pathPrefab.Length - 1)
            pathUnlockLevel++;
            

        //set shot chance and adjust spawn timer according to current level. shot chance goes up the higher the level.
        if (GameManager.instance.level % 2 == 0)
        {
            enemyShotChance += SHOT_INC_AMT;  //increase shot chance by 8% every 2 levels

            if (enemyShotChance > 1)
                enemyShotChance = 1;

            //enemies spawn a little faster
            spawnMod += 0.2f;
        }
    }

    IEnumerator MoveEnemies()
    {
        yield return new WaitForEndOfFrame();

        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
                enemy.GetComponent<Enemy>().Move();
        }
    }
}
