using System.Collections;
using UnityEngine;

public class Boss : MonoBehaviour
{
    [Header("Core Components")]
    private SpriteRenderer spriteRenderer;
    private Transform playerTransform;

    [Header("Patrol Settings")]
    public float patrolDistance = 5f;
    public float speed = 2f;
    public float hoverAmplitude = 0.5f;
    public float hoverFrequency = 1f;

    [Header("Battle Stats")]
    public float maxHealth = 500f;
    public float attackRange = 15f;
    public float damage = 20f;
    public float deathEffectDuration = 1.5f;
    public float currentHealth;

    [Header("Machine Gun Attack")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public int machineGunBurstCount = 10;
    public float machineGunFireRate = 0.1f;
    public Color machineGunTelegraphColor = Color.red;
    public float machineGunTelegraphDuration = 0.5f;
    public ParticleSystem muzzleFlashParticle;

    public ParticleSystem engineParticle;
    public ParticleSystem hitParticle;
    public ParticleSystem deathExplosionParticle;

    private enum BossState { Entering, Patrolling, Attacking, Cooldown, Dying }
    private BossState currentState;
    private Vector2 startPosition;
    private Vector2 pointA_pos;
    private Vector2 pointB_pos;
    private Vector2 currentTarget;
    public bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }

        startPosition = transform.position;
        pointA_pos = new Vector2(startPosition.x - patrolDistance, startPosition.y);
        pointB_pos = new Vector2(startPosition.x + patrolDistance, startPosition.y);

        currentTarget = pointB_pos;
        FaceTarget();
        currentState = BossState.Patrolling;
        GameManager.instance.UpdateBossHealth(currentHealth, maxHealth);

        if (engineParticle != null) engineParticle.Play();
    }

    void Update()
    {
        if (isDead || playerTransform == null) return;

        // 호버링 효과 (Y축 미세 진동)
        transform.position = new Vector2(transform.position.x, startPosition.y + Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude);

        switch (currentState)
        {
            case BossState.Patrolling:
                Patrol();
                if (Vector2.Distance(transform.position, playerTransform.position) <= attackRange)
                {
                    StartCoroutine(MachineGunRoutine());
                }
                break;
            case BossState.Dying:
                break;
        }
    }

    void Patrol()
    {
        if (Vector2.Distance(transform.position, currentTarget) < 0.1f)
        {
            currentTarget = (currentTarget == pointB_pos) ? pointA_pos : pointB_pos;
            FaceTarget();
        }
        transform.position = Vector2.MoveTowards(transform.position, currentTarget, speed * Time.deltaTime);
    }

    IEnumerator MachineGunRoutine()
    {
        currentState = BossState.Attacking;
        FacePlayer();

        // 예비동작: 발사 전 경고 색상
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = machineGunTelegraphColor;
        yield return new WaitForSeconds(machineGunTelegraphDuration);
        spriteRenderer.color = originalColor;

        Vector2 directionToPlayer = (playerTransform.position - firePoint.position).normalized;

        for (int i = 0; i < machineGunBurstCount; i++)
        {
            if (muzzleFlashParticle != null) muzzleFlashParticle.Play();

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            bullet.GetComponent<Rigidbody2D>().velocity = directionToPlayer * 10f;

            if (bullet.GetComponent<SpriteRenderer>() != null)
            {
                bullet.GetComponent<SpriteRenderer>().flipX = (directionToPlayer.x < 0);
            }

            yield return new WaitForSeconds(machineGunFireRate);
        }

        if (muzzleFlashParticle != null) muzzleFlashParticle.Stop();
        currentState = BossState.Cooldown;
        StartCoroutine(CooldownRoutine());
    }

    IEnumerator CooldownRoutine()
    {
        yield return new WaitForSeconds(Random.Range(1f, 2f));
        currentState = BossState.Patrolling;
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;
        currentHealth -= damageAmount;

        GameManager.instance.UpdateBossHealth(currentHealth, maxHealth);

        if (hitParticle != null) hitParticle.Play();

        if (currentHealth <= 0)
        {
            StopAllCoroutines();
            StartCoroutine(Die());
        }
    }

    IEnumerator Die()
    {
        isDead = true;
        currentState = BossState.Dying;

        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().simulated = false;
        if (engineParticle != null) engineParticle.Stop();
        if (muzzleFlashParticle != null) muzzleFlashParticle.Stop();

        for (int i = 0; i < 5; i++)
        {
            if (deathExplosionParticle != null)
            {
                Vector3 randomPos = transform.position + (Vector3)Random.insideUnitCircle * 1.5f;
                Instantiate(deathExplosionParticle, randomPos, Quaternion.identity);
            }
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(deathEffectDuration / 5f);
        }

        spriteRenderer.enabled = false;
        GameManager.instance.SetBossHpBarActive(false);
        Destroy(gameObject, 0.5f);
    }

    // localScale.x 반전으로 좌우 반전 처리 (단일 스프라이트, 별도 좌우 스프라이트 없음)
    void FaceTarget()
    {
        Vector3 currentScale = transform.localScale;
        if (currentTarget.x > transform.position.x)
        {
            currentScale.x = -Mathf.Abs(transform.localScale.x);
        }
        else
        {
            currentScale.x = Mathf.Abs(transform.localScale.x);
        }
        transform.localScale = currentScale;
    }

    void FacePlayer()
    {
        if (playerTransform == null) return;

        Vector3 currentScale = transform.localScale;
        if (playerTransform.position.x > transform.position.x)
        {
            currentScale.x = -Mathf.Abs(transform.localScale.x);
        }
        else
        {
            currentScale.x = Mathf.Abs(transform.localScale.x);
        }
        transform.localScale = currentScale;
    }
}
