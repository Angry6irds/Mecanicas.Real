using UnityEngine;

public class CameraShake : MonoBehaviour
{
    Vector3 originalPos;
    float timeLeft;
    float intensity;

    void Awake() { originalPos = transform.localPosition; }

    public void Shake(float duration, float strength)
    {
        timeLeft = duration;
        intensity = strength;
    }

    void LateUpdate()
    {
        if (timeLeft > 0f)
        {
            timeLeft -= Time.deltaTime;
            transform.localPosition = originalPos + Random.insideUnitSphere * intensity;
        }
        else transform.localPosition = originalPos;
    }
}