using UnityEngine;

public class HitEffect : MonoBehaviour
{
    void Start()
    {
        // 0.5초 뒤에 이 오브젝트를 파괴
        Destroy(gameObject, 0.3f);
    }
}