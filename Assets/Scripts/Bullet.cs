using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 20f;
    public GameObject hitEffectPrefab;
 

    void Start()
    {
        
        Destroy(gameObject, 2f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            // 먼저, 부딪힌 오브젝트에 Enemy 스크립트(일반 드론)가 있는지 확인
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }

            // 만약 Enemy 스크립트가 없다면, BossAI 스크립트(보스)가 있는지 확인
            Boss boss = other.GetComponent<Boss>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
            }

            // 타격 효과를 생성하고 총알 자신은 파괴
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }

    }
}
