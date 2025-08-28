using UnityEngine;

public class EnemyChaser : MonoBehaviour, IDamageable
{
    public Transform target;
    public float moveSpeed = 3f;
    public float stoppingDistance = 2f;
    public float turnSpeed = 360f;
    public float gravity = -20f;
    public float maxHealth = 50f;

    CharacterController cc;
    float hp;
    Vector3 velocity;

    void Awake() { cc = GetComponent<CharacterController>(); hp = maxHealth; }

    void Update()
    {
        if (!target) return;
        Vector3 to = target.position - transform.position;
        to.y = 0f;
        float dist = to.magnitude;
        if (to.sqrMagnitude > 0.001f)
        {
            var rot = Quaternion.LookRotation(to.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, turnSpeed * Time.deltaTime);
        }
        Vector3 move = dist > stoppingDistance ? transform.forward * moveSpeed : Vector3.zero;
        velocity.y += gravity * Time.deltaTime;
        cc.Move((move + velocity) * Time.deltaTime);
        if (cc.isGrounded) velocity.y = -1f;
    }

    public void ApplyDamage(float amount)
    {
        hp -= amount;
        if (hp <= 0f) Destroy(gameObject);
    }
}
