using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    [SerializeField] private float minCooldown;
    [SerializeField] private float maxCooldown;
    [SerializeField] private GameObject enemy;
    [Range(0f, 100f)] [SerializeField] private float spawnRate;             //Chance to spawn an enemy every check
    [SerializeField] private float minSpawnDistanceFromPlayer = 1;
    //[SerializeField] private float minX = 0f;
    //[SerializeField] private float maxX = 0f;

    private float cooldown = 0f;
    private GameObject player;
    private Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        // get player and main camera
        player = GameObject.FindGameObjectWithTag("Player");
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // if cooldown isn't reached, continue
        if (cooldown > 0) {
            cooldown -= Time.deltaTime * 1000;
            return;
        }

        // check if spawn
        float check = Random.Range(0f, 100f);

        // if spawn, spawn one 
        if (check < spawnRate) {
            Vector2 cameraPos = camera.transform.position;
            float rangeY = Camera.main.orthographicSize;
            float rangeX = rangeY * Camera.main.aspect;

            Vector2 playerPos = player.transform.position;

            // make sure there's enough space between spawned enemy and player. Depends on monster type.
            // determine if there's land few pixel below the spawn and drop them. If there are no land, find another random
            int count = 0;
            bool valid = true;
            Vector2 spawn;
            do {
                float posX = Random.Range(cameraPos.x - rangeX, cameraPos.x + rangeX);
                float posY = Random.Range(cameraPos.y - rangeY, cameraPos.y + rangeY);
                spawn = new Vector2(posX, posY);

                // check if the spawn is valid
                // 1. check if the position is far enough
                if (posX > playerPos.x - this.minSpawnDistanceFromPlayer && posX < playerPos.x + this.minSpawnDistanceFromPlayer) {
                    valid = false;
                }

                // check if there's something to collide to below and also has enough distance for the height
                RaycastHit2D groundInfo = Physics2D.Raycast(spawn, Vector2.down, rangeY * 2);

                if (groundInfo.collider == null || groundInfo.distance < enemy.GetComponent<Collider2D>().bounds.size.y) {
                    valid = false;
                }

                count++;
            } while (!valid && count < 100);

            // it tried to make a valid spawn 10 times but failed, so stop the spawning process
            if (!valid) {
                return;
            }

            Instantiate(enemy, spawn, Quaternion.identity);
        }

        cooldown = Random.Range(minCooldown, maxCooldown);
    }
}
