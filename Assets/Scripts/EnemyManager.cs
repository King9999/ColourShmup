using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Handles enemy movement patterns and tracks all living enemies.
public class EnemyManager : MonoBehaviour
{
    public List<Enemy> enemies = new List<Enemy>();
    public float enemyShotChance;
    public float enemyMoveSpeed;
    

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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
