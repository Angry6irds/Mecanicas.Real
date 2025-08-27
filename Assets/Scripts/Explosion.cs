using UnityEngine;

public class ExplosionProjectile : MonoBehaviour
{
    public float baseDamage = 80f;
    public float explosionRadius = 4f;
    public AnimationCurve damageFalloff = AnimationCurve.EaseInOut(0, 1, 1, 0);
    public LayerMask damageMask = ~0;

    public GameObject impactMarkerPrefab;
    public float markerLifetime = 6f;
    public GameObject explosionVFX;

    bool exploded;

    void OnCollisionEnter(Collision col)
    {
        if (exploded) return;
        exploded = true;

        ContactPoint cp = col.contacts.Length > 0 ? col.contacts[0] : default;
        Vector3 hitPoint = cp.point != Vector3.zero ? cp.point : transform.position;
        Vector3 hitNormal = cp.normal == Vector3.zero ? -transform.forward : cp.normal;

        if (explosionVFX)
        {
            var fx = Instantiate(explosionVFX, hitPoint, Quaternion.LookRotation(hitNormal));
            Destroy(fx, 5f);
        }

        if (impactMarkerPrefab)
        {
            var mk = Instantiate(impactMarkerPrefab, hitPoint + hitNormal * 0.01f, Quaternion.LookRotation(hitNormal));
            Destroy(mk, markerLifetime);
        }

        var cols = Physics.OverlapSphere(hitPoint, explosionRadius, damageMask, QueryTriggerInteraction.Ignore);
        foreach (var c in cols)
        {
            float dist = Vector3.Distance(hitPoint, c.ClosestPoint(hitPoint));
            float t = Mathf.Clamp01(dist / explosionRadius);
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