using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    public Transform playerTransform;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    [Header("Camera Bounds")]
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    [Header("Boss Fight Settings")]
    public Boss boss;
    public float bossAreaTriggerX = 80f;
    public float bossFightCameraX = 87f;
    public float nextStageTriggerX = 94f;

    private enum CameraState { FollowingPlayer, BossFight, StageClear }
    private CameraState currentState;

    private float cameraBoundMinX;
    private float cameraBoundMaxX;

    void Start()
    {
        transform.position = new Vector3(-1.55f, 0.66f, -10f);
        currentState = CameraState.FollowingPlayer;
    }

    void FixedUpdate()
    {
        if (playerTransform == null) return;

        switch (currentState)
        {
            case CameraState.FollowingPlayer:
                FollowPlayer();
                if (playerTransform.position.x >= bossAreaTriggerX)
                {
                    EnterBossFight(); // 보스방 진입
                }
                break;

            case CameraState.BossFight:
                LockCameraForBossFight();
                if (boss != null && boss.isDead)
                {
                    ExitBossFight();
                }
                break;

            case CameraState.StageClear:
                LockCameraForBossFight();
                if (playerTransform.position.x > cameraBoundMaxX + 2f)
                {
                    GameManager.instance.LoadNextScene();
                    this.enabled = false;
                }
                break;
        }
    }

    void EnterBossFight()
    {
        currentState = CameraState.BossFight;

        if (GameManager.instance != null)
        {
            GameManager.instance.SetBossHpBarActive(true);
            if (boss != null)
            {
                GameManager.instance.UpdateBossHealth(boss.currentHealth, boss.maxHealth);
            }
            Debug.Log("보스전 시작: BossHp 켜짐");
        }

        // 카메라 범위 고정
        float cameraHeight = Camera.main.orthographicSize * 2;
        float cameraWidth = cameraHeight * Camera.main.aspect;
        cameraBoundMinX = bossFightCameraX - cameraWidth / 2;
        cameraBoundMaxX = bossFightCameraX + cameraWidth / 2;

        // 플레이어 이동 제한
        playerTransform.GetComponent<Player>().ConfineMovement(cameraBoundMinX, cameraBoundMaxX);
    }

    void ExitBossFight()
    {
        currentState = CameraState.StageClear;
        if (playerTransform != null)
        {
            playerTransform.GetComponent<Player>().UpdateConfinement(cameraBoundMinX, Mathf.Infinity);
        }
    }

    void FollowPlayer()
    {
        Vector3 desiredPosition = new Vector3(playerTransform.position.x, playerTransform.position.y, transform.position.z);
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    void LockCameraForBossFight()
    {
        Vector3 targetPosition = new Vector3(bossFightCameraX, transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);
    }
}
