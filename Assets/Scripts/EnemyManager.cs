using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Handles enemy movement patterns and tracks all living enemies.
public class EnemyManager : MonoBehaviour
{
    [HideInInspector]
    public List<GameObject> enemies = new List<GameObject>();
    public GameObject enemyPrefab;
    public float enemyShotChance;
    public float enemyMoveSpeed;
    Path enemyPath;
    public int currentEnemy;
    float currentTime;
    float SpawnTimer;                       //controls how fast enemies are spawned. can be random. Value is in seconds
    public int totalEnemyCount;          //limits how many enemies can be on screen at once. Number increases as level increases.
    public int currentEnemyCount;

    //consts
    const int INIT_ENEMY_COUNT = 4;
    const float INIT_SPAWN_TIME = 2;

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
            enemyPath.pathPoints[(int)Path.PathType.LinearVertical] = enemyPath.SetPath((int)Path.PathType.LinearVertical);
            enemies.Add(Instantiate(enemyPrefab, enemyPath.pathPoints[(int)Path.PathType.LinearVertical][0], Quaternion.identity));
            currentEnemyCount++;
        }

        CleanupEnemyList();
    }

    //removes any null references after enemies are destroyed.
    public void CleanupEnemyList()
    {
        if (enemies.Count <= 0)
            return;

        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] == null)
                enemies.RemoveAt(i);
        }

        Debug.Log("Enemy Count: " + enemies.Count);
    }

    public List<Vector3>[] EnemyPath()
    {
        return enemyPath.pathPoints;
    }

    public void MoveAllEnemies()
    {
        StartCoroutine(MoveEnemies());
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
