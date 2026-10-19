using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    private Transform cameraTransform;
    private Vector3 lastCameraPosition;

    [Tooltip("패럴랙스 효과의 강도입니다. 0에 가까울수록 천천히 움직입니다.")]
    [Range(0f, 1f)]
    public float parallaxEffectMultiplier;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        transform.position += deltaMovement * parallaxEffectMultiplier;
        lastCameraPosition = cameraTransform.position;
    }
}