using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Knockbackable : MonoBehaviour
{
    public float killThreshold = 15f;
    private Rigidbody rb;
    private PlayerController3P owner;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        owner = GetComponent<PlayerController3P>();
    }

    public void ApplyExplosion(Vector3 epicenter, Vector3 impulse)
    {
        rb.AddForce(impulse, ForceMode.Impulse);
        CheckSoon();
    }

    public void ApplyKnockback(Vector3 hitPoint, Vector3 impulse)
    {
        rb.AddForceAtPosition(impulse, hitPoint, ForceMode.Impulse);
        CheckSoon();
    }

    void CheckSoon()
    {
        if (owner && owner.IsAlive) Invoke(nameof(CheckKill), 0.12f);
    }

    void CheckKill()
    {
        if (!owner || !owner.IsAlive) return;
        if (rb.linearVelocity.magnitude >= killThreshold) owner.Eliminate();
    }
}