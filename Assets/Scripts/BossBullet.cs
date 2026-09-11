using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBullet : MonoBehaviour
{
    public float damage = 10f;
    public GameObject hitEffectPrefab;

    void Start()
    {
        Destroy(gameObject, 3f); // 3초 뒤에 사라짐
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                // 총알의 데미지와 총알 자신의 위치를 전달
                Destroy(gameObject);
                player.TakeDamage(damage, transform.position);
            }

            
            
        }
    }
}