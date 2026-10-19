using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;

    public Transform floorCheck;
    public float floorCheckRadius = 0.2f;
    public LayerMask floorLayer;

    public float maxHelath = 100f;
    public float curHelath;
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float climbSpeed = 3f;
    public float jumpForce = 7f;
    public float bulletSpeed;
    public int life;
    public int maxLife = 2;

    public bool isGrounded = true;
    bool isMove = false;
    bool isCrouch = false;
    bool isRun = false;
    bool isJump = false;
    bool isClimb;
    bool isLadder;
    bool isShooting = false;

    private bool isConfined = false;
    private float minX_confined, maxX_confined;

    float climbInputY;
    float originalGravity;

    public SpriteRenderer sprite;
    public Rigidbody2D rigid;
    public Animator anim;

    [Header("Attack")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Transform crouchFirePoint;
    public float fireCooldown = 0.5f;
    public bool isInvincible = false;
    public float knockbackForce = 5f;
    private bool isKnockedBack = false;

    private float lastFireTime;

    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        originalGravity = rigid.gravityScale;

        curHelath = maxHelath;
        isLadder = false;
        isClimb = false;
        life = maxLife;
    }

    void Start()
    {
        transform.position = new Vector2((float)-7.24, -1);
        curHelath = maxHelath;
        if (GameManager.instance != null)
            GameManager.instance.UpdateHealth(curHelath, maxHelath);
    }

    void FixedUpdate()
    {
        if (isKnockedBack) return;

        isGrounded = Physics2D.OverlapCircle(floorCheck.position, floorCheckRadius, floorLayer);

        if (isClimb)
        {
            rigid.gravityScale = 0f;

            float verticalMove = climbInputY * climbSpeed;
            float horizontalMove = inputVec.x * (moveSpeed * 0.5f);

            rigid.velocity = new Vector2(horizontalMove, verticalMove);

            anim.SetFloat("Speed", Mathf.Abs(horizontalMove));
            anim.SetBool("isClimb", true);
        }
        else
        {
            rigid.gravityScale = originalGravity;

            isShooting = anim.GetCurrentAnimatorStateInfo(0).IsTag("Shooting");

            if (isGrounded && (isCrouch || (isShooting && !isRun)))
            {
                rigid.velocity = new Vector2(0, rigid.velocity.y);
            }
            else
            {
                float currentSpeed = isRun ? runSpeed : moveSpeed;
                rigid.velocity = new Vector2(inputVec.x * currentSpeed, rigid.velocity.y);
            }
        }
    }

    void Update()
    {
        isMove = inputVec.x != 0;
        if (isLadder && climbInputY != 0)
        {
            isClimb = true;
        }

        if ((isRun && isMove && anim.GetCurrentAnimatorStateInfo(0).IsTag("Shooting")) || isCrouch || isJump)
        {
            anim.ResetTrigger("Fire");
        }

        if (transform.position.y < -8)
        {
            if (!isInvincible)
            {
                curHelath -= 20;
                GameManager.instance.UpdateHealth(curHelath, maxHelath);
            }

            if (curHelath <= 0)
            {
                StartCoroutine(Die());
            }
            else
            {
                transform.position = GameManager.instance.GetRespawnPoint();
                rigid.velocity = Vector2.zero;
            }
        }
        if(!isJump && !isCrouch)
            isShooting = anim.GetCurrentAnimatorStateInfo(0).IsTag("Shooting");

        anim.SetFloat("Speed", Mathf.Abs(inputVec.x));
        anim.SetBool("isRun", isRun);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isCrouch", isCrouch);
        anim.SetBool("isJump", isJump);
        anim.SetBool("isClimb", isClimb);
    }

    void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();
        if (inputVec.x != 0)
        {
            Vector3 currentScale = transform.localScale;
            currentScale.x = Mathf.Abs(currentScale.x) * (inputVec.x < 0 ? -1 : 1);
            transform.localScale = currentScale;
        }
    }

    void OnRun(InputValue value)
    {
        isRun = value.isPressed && !isCrouch && inputVec.x != 0;
    }

    void OnFire(InputValue value)
    {
        if (isClimb) return;
        if (Time.time < lastFireTime + fireCooldown) return;

        lastFireTime = Time.time;
        Transform currentFirePoint = isCrouch ? crouchFirePoint : firePoint;

        GameObject bullet = Instantiate(bulletPrefab, currentFirePoint.position, currentFirePoint.rotation);
        Rigidbody2D bulletRigid = bullet.GetComponent<Rigidbody2D>();
        SpriteRenderer bulletSprite = bullet.GetComponent<SpriteRenderer>();

        float direction = Mathf.Sign(transform.localScale.x);
        bulletSprite.flipX = (direction == -1);

        bulletRigid.velocity = new Vector2(direction * bulletSpeed, 0);
        anim.SetTrigger("Fire");
    }

    void OnJump(InputValue value)
    {
        if (isLadder)
        {
            climbInputY = value.isPressed ? 1f : 0f;
        }
        else if (value.isPressed && isGrounded && !isCrouch && !isClimb)
        {
            rigid.velocity = new Vector2(rigid.velocity.x, jumpForce);
            isJump = true;
        }
    }

    void OnCrouch(InputValue value)
    {
        if (isLadder && !isGrounded)
        {
            climbInputY = value.isPressed ? -1f : 0f;
        }
        else if (value.isPressed)
        {
            if (isGrounded)
            {
                isCrouch = true;
                isRun = false;
                rigid.velocity = new Vector2(0, rigid.velocity.y);
            }
        }
        else
        {
            isCrouch = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & floorLayer) != 0)
        {
            isJump = false;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null && !isInvincible)
            {
                TakeDamage(enemy.damage, enemy.transform.position);
            }
        }
        if (collision.gameObject.CompareTag("Ladder"))
        {
            isLadder = true;
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !isInvincible)
        {
            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                TakeDamage(enemy.damage, enemy.transform.position);
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ladder"))
        {
            isLadder = false;
            isClimb = false;
            climbInputY = 0f;
        }
    }

    public void TakeDamage(float damage, Vector3 attackerPosition)
    {
        if (isInvincible || isKnockedBack || curHelath <= 0)
        {
            return;
        }
        StartCoroutine(Damaged(damage, attackerPosition));
    }

    IEnumerator Damaged(float damageAmount, Vector3 attackerPosition)
    {
        isInvincible = true;
        isKnockedBack = true;
        isClimb = false;
        isCrouch = false;

        curHelath -= damageAmount;
        GameManager.instance.UpdateHealth(curHelath, maxHelath);

        rigid.velocity = Vector2.zero;
        int knockbackDirection = transform.position.x > attackerPosition.x ? 1 : -1;
        rigid.AddForce(new Vector2(knockbackDirection, 0.5f) * knockbackForce, ForceMode2D.Impulse);

        if(!isJump && !isClimb)
            anim.SetTrigger("isDamaged");

        if (curHelath <= 0)
        {
            StartCoroutine(Die());
            yield break;
        }

        yield return new WaitForSeconds(0.3f);
        isKnockedBack = false;

        for (int i = 0; i < 5; i++)
        {
            sprite.enabled = false;
            yield return new WaitForSeconds(0.05f);
            sprite.enabled = true;
            yield return new WaitForSeconds(0.05f);
        }

        isInvincible = false;
    }

    IEnumerator Die()
    {
        yield return new WaitForSeconds(0.5f);
        GameManager.instance.LoadScene("GameOver");
    }

    void LateUpdate()
    {
        if (isConfined)
        {
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, minX_confined, maxX_confined),
                transform.position.y,
                transform.position.z
            );
        }
    }

    public void ConfineMovement(float minX, float maxX)
    {
        minX_confined = minX;
        maxX_confined = maxX;
        isConfined = true;
    }

    public void UpdateConfinement(float minX, float maxX)
    {
        minX_confined = minX;
        maxX_confined = maxX;
    }
}
