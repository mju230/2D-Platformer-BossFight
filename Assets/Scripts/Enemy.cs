using System.Collections;
using UnityEngine;

public enum EnemyType { Drone, Slime, Turret }

[RequireComponent(typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    [Header("공통 설정")]
    public EnemyType enemyType;
    public float damage = 15f;
    public float maxHealth = 100f;
    public float moveSpeed = 2f;
    public float patrolDistance = 4f;

    private float currentHealth;
    private Vector2 startPos;
    private Vector2 targetPos;
    private bool facingRight = true;
    private bool isDead = false;

    [Header("드론 전용 설정 (스프라이트 교체용)")]
    public Sprite sideSprite;
    public Sprite diagonalSprite;
    public Sprite frontSprite;

    [Header("애니메이션/렌더러")]
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    // 드론 상태
    private enum DroneState { Patrolling, Turning }
    private DroneState droneState;

    void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        startPos = transform.position;
        targetPos = new Vector2(startPos.x + patrolDistance, startPos.y);

        if (enemyType == EnemyType.Drone)
        {
            FaceTarget();
            droneState = DroneState.Patrolling;
        }
    }

    void Update()
    {
        if (isDead) return;

        switch (enemyType)
        {
            case EnemyType.Drone:
                if (droneState == DroneState.Patrolling)
                    DronePatrol();
                break;

            case EnemyType.Slime:
                SlimePatrol();
                break;

            case EnemyType.Turret:
                // 터렛은 고정 (움직이지 않음)
                break;
        }
    }

    // ===== 드론 =====
    void DronePatrol()
    {
        if (Vector2.Distance(transform.position, targetPos) < 0.1f)
        {
            droneState = DroneState.Turning;
            StartCoroutine(TurnRoutine());
            return;
        }

        transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
    }

    IEnumerator TurnRoutine()
    {
        droneState = DroneState.Turning;

        // Animator를 잠시 꺼서 Idle 루프 방지
        if (anim != null)
            anim.enabled = false;

        // 스프라이트로 회전 연출
        spriteRenderer.sprite = diagonalSprite;
        yield return new WaitForSeconds(0.2f);

        spriteRenderer.sprite = frontSprite;
        yield return new WaitForSeconds(0.2f);

        // 타겟 갱신
        if (Mathf.Abs(transform.position.x - (startPos.x + patrolDistance)) < 0.1f)
            targetPos = new Vector2(startPos.x - patrolDistance, startPos.y);
        else
            targetPos = new Vector2(startPos.x + patrolDistance, startPos.y);

        FaceTarget();

        spriteRenderer.sprite = diagonalSprite;
        yield return new WaitForSeconds(0.2f);

        // 다시 기본 자세
        spriteRenderer.sprite = sideSprite;

        // Animator 다시 활성화
        if (anim != null)
            anim.enabled = true;

        droneState = DroneState.Patrolling;
    }

    void FaceTarget()
    {
        if (targetPos.x > transform.position.x)
            spriteRenderer.flipX = true;
        else
            spriteRenderer.flipX = false;
    }

    // ===== 슬라임 =====
    void SlimePatrol()
    {
        float leftLimit = startPos.x - patrolDistance;
        float rightLimit = startPos.x + patrolDistance;

        transform.position += new Vector3((facingRight ? 1 : -1) * moveSpeed * Time.deltaTime, 0, 0);

        if (facingRight && transform.position.x > rightLimit)
            Flip();
        else if (!facingRight && transform.position.x < leftLimit)
            Flip();

        if (anim != null)
            anim.Play("Slimer-Animation"); // 루프 애니메이션
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // ===== 공통 =====
    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;

        // HP가 0 이하 → 즉시 사망 처리
        if (currentHealth <= 0)
        {
            StopAllCoroutines(); // 회전 중이었으면 멈춤

            // Animator 꺼져 있으면 다시 켜기
            if (anim != null)
                anim.enabled = true;

            StartCoroutine(Die());
            return;
        }
    }

    IEnumerator Die()
    {
        isDead = true;

        GetComponent<Collider2D>().enabled = false;

        // 폭발 애니메이션 실행
        if (anim != null)
        {
            anim.enabled = true;
            anim.SetTrigger("Dead");
        }

        // 혹시 다른 스프라이트로 덮여 있으면 복원
        spriteRenderer.sprite = sideSprite;

        yield return new WaitForSeconds(0.5f);

        spriteRenderer.enabled = false;
        gameObject.SetActive(false);
    }
}
