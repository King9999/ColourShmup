using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//Handles enemy movement patterns and tracks all living enemies.
public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Data")]
    public List<GameObject> enemies = new List<GameObject>();
    public List<GameObject> enemyBullets = new List<GameObject>();
    public GameObject enemyPrefab;
    public float enemyShotChance;
    public float enemyMoveSpeed;
    public float bulletSpeed;
    Path enemyPath;
    public Path.PathType path;
    public int currentEnemy;
    float currentTime;
    public float SpawnTimer;                       //controls how fast enemies are spawned. can be random. Value is in seconds
    int totalEnemyCount;          //limits how many enemies can be on screen at once. Number increases as level increases.
    int currentEnemyCount;

    //consts
    const int INIT_ENEMY_COUNT = 4;
    const float INIT_SPAWN_TIME = 2;
    const float SHOT_INC_AMT = 0.04f;

    public static EnemyManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);    //Only want one instance of enemy manager
            return;
        }

        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        enemyPath = new Path();
        totalEnemyCount = INIT_ENEMY_COUNT;
        SpawnTimer = INIT_SPAWN_TIME;
    }

    // Update is called once per frame
    private void Update()
    {
        //check if it's time to spawn another enemy.
        if (Time.time > currentTime + SpawnTimer && currentEnemyCount < totalEnemyCount)
        {
            currentTime = Time.time;
            SpawnTimer = INIT_SPAWN_TIME;           //reset spawn timer in case it changed previously.
            //path = Path.PathType.LinearVertical;
            path = (Path.PathType)UnityEngine.Random.Range((int)Path.PathType.LinearVertical, (int)Path.PathType.LinearHorizontal + 1);
            //enemyPath.pathPoints[(int)Path.PathType.LinearVertical] = enemyPath.SetPath((int)Path.PathType.LinearVertical);
            enemyPath.SetPath(enemyPath.pathPoints, path);
            enemies.Add(Instantiate(enemyPrefab, enemyPath.pathPoints[(int)path][0], Quaternion.identity));
            //enemies.Add(In/stantiate(enemyPrefab, enemyPath.pathPoints[(int)Path.PathType.LinearVertical][0], Quaternion.identity));
            currentEnemyCount++;
        }

        CleanupEnemyList();
        CleanupBulletList();
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
    public List<Vector3>[] EnemyPath()
    {
        return enemyPath.pathPoints;
    }

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
            enemyBullets.RemoveAt(i);
        }
    }

    public void AdvanceLevel()
    {
        DestroyAllEnemies();
        currentTime = Time.time;
        SpawnTimer = 5;             //giving player a breather before level begins
        currentEnemyCount = 0;
        totalEnemyCount++;
    }

    IEnumerator MoveEnemies()
    {
        yield return new WaitForFixedUpdate();  //update every 0.02 seconds

        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
                enemy.GetComponent<Enemy>().Move();
        }
    }
}
