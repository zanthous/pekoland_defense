using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private bool triggerOnly = false;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int numberToSpawn = 0;
    
    //if player in range
    private GameObject player;
        
    private float spawnInterval;

    private float timer = 0.0f;

    private bool playerInRange = false;


    // Start is called before the first frame update
    void Start()
    {
        timer = spawnInterval; //immediately spawn one on start
        player = FindObjectOfType<Player>().gameObject;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(!triggerOnly)
        {
            if(playerInRange)
            {

            }
        }
    }

    void CheckPlayerInRange()
    {

    }

    public void SpawnEnemy()
    {

    }

    public void ResetSpawner()
    {
        
    }
}
