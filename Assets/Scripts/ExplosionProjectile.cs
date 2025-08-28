using UnityEngine;

public class ExplosionProjectileParabolic : MonoBehaviour
{
    public float baseDamage = 80f;
    public float explosionRadius = 4f;
    public AnimationCurve damageFalloff = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public LayerMask hitMask = ~0;
    public LayerMask damageMask = ~0;
    public GameObject impactMarkerPrefab;
    public float markerLifetime = 6f;
    public GameObject explosionVFX;

    TiroParabolico tp;
    Vector3 lastPos;
    bool exploded;

    void OnEnable()
    {
        tp = GetComponent<TiroParabolico>();
        lastPos = transform.position;
    }

    void Update()
    {
        if (exploded) return;
        Vector3 cur = transform.position;
        Vector3 delta = cur - lastPos;
        float dist = delta.magnitude;
        if (dist > 0.0001f)
        {
            if (Physics.Raycast(lastPos, delta.normalized, out var hit, dist, hitMask, QueryTriggerInteraction.Ignore))
            {
                Explode(hit.point, hit.normal);
                return;
            }
        }
        lastPos = cur;
    }

    void OnTriggerEnter(Collider other)
    {
        if (exploded) return;
        if (((1 << other.gameObject.layer) & hitMask) == 0) return;
        Vector3 n = -transform.forward;
        Explode(transform.position, n);
    }

    void Explode(Vector3 point, Vector3 normal)
    {
        exploded = true;
        if (explosionVFX)
        {
            var fx = Instantiate(explosionVFX, point, Quaternion.LookRotation(normal));
            Destroy(fx, 5f);
        }
        if (impactMarkerPrefab)
        {
            var mk = Instantiate(impactMarkerPrefab, point + normal * 0.01f, Quaternion.LookRotation(normal));
            Destroy(mk, markerLifetime);
        }
        var cols = Physics.OverlapSphere(point, explosionRadius, damageMask, QueryTriggerInteraction.Ignore);
        foreach (var c in cols)
        {
            float d = Vector3.Distance(point, c.ClosestPoint(point));
            float t = Mathf.Clamp01(d / explosionRadius);
            float dmg = baseDamage * damageFalloff.Evaluate(t);
            var dmgTarget = c.GetComponentInParent<IDamageable>();
            if (dmgTarget != null && dmg > 0f) dmgTarget.ApplyDamage(dmg);
        }
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
