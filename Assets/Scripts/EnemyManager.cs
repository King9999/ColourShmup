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
    public Path enemyPath;

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
        enemyPath.pathPoints.Add(Vector3.zero);
        enemies.Add(Instantiate(enemyPrefab, enemyPath.pathPoints[0], Quaternion.identity));
    }

    // Update is called once per frame
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
