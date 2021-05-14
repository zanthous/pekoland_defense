using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dagger : MonoBehaviour
{
    [SerializeField] private WeaponInfo weaponInfo;
    [SerializeField] private LayerMask collisions;
    [SerializeField] private float speed = 16.0f;
    [SerializeField] private float timeToLive = 3.0f;

    private float despawnTimer = 0.0f;

    // Update is called once per frame
    void Update()
    {
        despawnTimer += Time.deltaTime;
        if(despawnTimer > timeToLive)
        {
            Destroy(gameObject);
        }
        transform.position += speed * Time.deltaTime * transform.right;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //TODO
        if(collision.CompareTag("Enemy"))
        {
            //collision.GetComponent<IHealth>().TakeDamage(weaponInfo.damage);
        }

        if(!collision.CompareTag("Player") &&
            (collisions.value & 1 << collision.gameObject.layer) != 0)
        {
            Destroy(gameObject);
        }
    }
}
