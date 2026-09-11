using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;
    private bool isLoadingScene = false;
    private Vector2 respawnPoint;

    [Header("UI Elements")]
    public Slider hpSlider;   // 플레이어 HP UI
    public Slider bossHp;     // 보스 HP UI



    public int currentStageIndex = 1;

    void Start()
    {
        respawnPoint = new Vector2((float)-7.24, -1);
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // 씬이 로드될 때마다 UI 다시 연결
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Keyboard.current.f1Key.wasPressedThisFrame && !isLoadingScene)
        {
            StartCoroutine(f1Dn());
        }
        if (Keyboard.current.f2Key.wasPressedThisFrame)
        {
            StartCoroutine(f2Dn());
        }
        if (Keyboard.current.f3Key.wasPressedThisFrame)
        {
            StartCoroutine(f3Dn());
        }
        if (Keyboard.current.f4Key.wasPressedThisFrame)
        {
            StartCoroutine(f4Dn());
        }
    }

    public void SetCheckPoint(Vector2 pos)
    {
        respawnPoint = pos;
        Debug.Log("체크포인트 갱신");        
    }

    public Vector2 GetRespawnPoint()
    {
        return respawnPoint;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Player HP UI 연결
        hpSlider = GameObject.FindWithTag("PlayerHP")?.GetComponent<Slider>();
        if (hpSlider != null)
        {
            Player player = GameObject.FindWithTag("Player")?.GetComponent<Player>();
            if (player != null)
            {
                hpSlider.maxValue = player.maxHelath;
                hpSlider.value = player.curHelath;
            }
            else
            {
                hpSlider.maxValue = 100f;
                hpSlider.value = 100f;
            }
        }

        // Boss HP UI 연결
        bossHp = GameObject.FindWithTag("BossHP")?.GetComponent<Slider>();
        if (bossHp != null)
        {
            bossHp.gameObject.SetActive(false); // 보스전 전까지는 꺼둠

        }

    }

    // === HP UI 업데이트 ===
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (hpSlider == null) return;
        hpSlider.maxValue = maxHealth;
        hpSlider.value = currentHealth;
    }

    public void UpdateBossHealth(float currentHealth, float maxHealth)
    {
        if (bossHp == null) return;
        bossHp.maxValue = maxHealth;
        bossHp.value = currentHealth;
    }

    public void SetBossHpBarActive(bool isActive)
    {
        if (bossHp != null)
            bossHp.gameObject.SetActive(isActive);
    }

    // === 씬 전환 ===
    public void LoadNextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            currentStageIndex = nextSceneIndex;
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.Log("마지막 스테이지입니다!");
        }
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void RetryCurrentStage()
    {
        SceneManager.LoadScene(currentStageIndex);
    }

    public void ResetPlayerHealthUI(float maxHealth)
    {
        if (hpSlider == null) return;
        hpSlider.maxValue = maxHealth;
        hpSlider.value = maxHealth;
    }

    IEnumerator f1Dn()
    {
        isLoadingScene = true;
        LoadNextScene();
        yield return new WaitForSeconds(0.5f);
        isLoadingScene = false;
    }

    IEnumerator f2Dn()
    {
        Player player = GameObject.FindWithTag("Player")?.GetComponent<Player>();

        if (player != null)
        {
            player.isInvincible = true;
            Debug.Log("무적 상태: On");
        }

        yield return new WaitForSeconds(0.5f);

    }
    IEnumerator f3Dn()
    {
        Player player = GameObject.FindWithTag("Player")?.GetComponent<Player>();

        if (player != null)
        {
            player.isInvincible = false;
            Debug.Log("무적 상태: Off");
        }

        yield return new WaitForSeconds(0.5f);

    }

    IEnumerator f4Dn()
    {
        Player player = GameObject.FindWithTag("Player")?.GetComponent<Player>();

        if (player != null)
        {
            player.curHelath = player.maxHelath;
            UpdateHealth(player.curHelath, player.maxHelath);
        }

        yield return new WaitForSeconds(0.5f);

    }
}