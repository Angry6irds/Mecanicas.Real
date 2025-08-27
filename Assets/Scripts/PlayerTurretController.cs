using UnityEngine;

public class PlayerTurretController : MonoBehaviour
{
    public Transform turret, barrel;
    public float rotSpeed, bulletSpeed, bulletLifeSpan;
    public Transform shootPoint;
    public GameObject bulletPrefab;
    public Transform crossHair;

    public float fireRate = 4f;
    public float range = 40f;
    public LayerMask targetMask;
    public LayerMask obstructionMask;
    [Range(0f, 1f)] public float aimDotToShoot = 0.985f;

    private float angleX;
    float fireCD;
    Transform currentTarget;

    void Update()
    {
        if (!currentTarget || !EsValido(currentTarget))
            currentTarget = BuscarObjetivo();

        if (currentTarget)
        {
            Vector3 to = currentTarget.position - turret.position;
            to.y = 0f;
            if (to.sqrMagnitude > 0.0001f)
            {
                Quaternion yaw = Quaternion.LookRotation(to.normalized, Vector3.up);
                turret.rotation = Quaternion.RotateTowards(turret.rotation, yaw, rotSpeed * Time.deltaTime);
            }

            Vector3 toPitch = currentTarget.position - barrel.position;
            Vector3 localDir = transform.InverseTransformDirection(toPitch.normalized);
            float desiredPitch = Mathf.Atan2(localDir.y, new Vector2(localDir.x, localDir.z).magnitude) * Mathf.Rad2Deg;
            angleX = Mathf.Clamp(desiredPitch, -90f, 0f);
            barrel.localRotation = Quaternion.Euler(angleX, 0, 0);
        }

        fireCD -= Time.deltaTime;
        if (currentTarget && fireCD <= 0f && ListoParaDisparar())
        {
            Fire();
            fireCD = 1f / Mathf.Max(0.01f, fireRate);
        }

        if (crossHair)
        {
            Vector3 g = new Vector3(0, -9.8f, 0);
            Vector3 P0 = shootPoint.position;
            Vector3 V0 = bulletSpeed * shootPoint.forward;
            float T = FlyingTime();
            crossHair.position = 0.5f * g * T * T + V0 * T + P0;
        }
    }

    bool ListoParaDisparar()
    {
        float dot = Vector3.Dot(barrel.forward, (currentTarget.position - shootPoint.position).normalized);
        if (dot < aimDotToShoot) return false;
        Vector3 dir = (currentTarget.position - shootPoint.position).normalized;
        int mask = obstructionMask | targetMask;
        if (Physics.Raycast(shootPoint.position, dir, out var hit, range, mask, QueryTriggerInteraction.Ignore))
            return hit.transform == currentTarget || hit.transform.IsChildOf(currentTarget);
        return false;
    }

    Transform BuscarObjetivo()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range, targetMask, QueryTriggerInteraction.Ignore);
        float best = float.MaxValue;
        Transform choice = null;
        foreach (var h in hits)
        {
            if (!h) continue;
            float d = (h.transform.position - transform.position).sqrMagnitude;
            if (d < best) { best = d; choice = h.transform; }
        }
        return choice;
    }

    bool EsValido(Transform t)
    {
        if (!t) return false;
        return t.gameObject.activeInHierarchy && (t.position - transform.position).sqrMagnitude <= range * range;
    }

    void Fire()
    {
        Vector3 position = shootPoint.position;
        Quaternion rotation = shootPoint.rotation;
        GameObject bullet = Instantiate(bulletPrefab, position, rotation);
        var tp = bullet.GetComponent<TiroParabolico>();
        if (tp != null)
        {
            tp.P0 = shootPoint.position;
            tp.V0 = bulletSpeed * shootPoint.forward;
        }
        else
        {
            if (bullet.TryGetComponent<Rigidbody>(out var rb))
                rb.velocity = shootPoint.forward * bulletSpeed;
        }
        Destroy(bullet, bulletLifeSpan);
    }

    float FlyingTime()
    {
        float y0 = shootPoint.position.y;
        float Vy0 = bulletSpeed * shootPoint.forward.y;
        float g = 9.8f;
        return (Vy0 + Mathf.Sqrt(Vy0 * Vy0 + 2f * g * y0)) / g;
    }
}