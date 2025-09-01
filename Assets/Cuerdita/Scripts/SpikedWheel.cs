using UnityEngine;

public class SpikedWheel : MonoBehaviour
{
    public float speed;
    public float radius = 1f;
    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        Vector3 velocity = speed * Vector3.right;
        float angularVelocity = (speed / radius) * Mathf.Rad2Deg;

        transform.Translate(velocity * dt, Space.World);
        transform.Rotate(0, 0, - angularVelocity * dt, Space.Self);
    }
}
