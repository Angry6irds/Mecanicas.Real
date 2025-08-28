using UnityEngine;

public class PlayerTurret : MonoBehaviour
{
    public Transform turret, barrel;
    public float rotSpeed, bulletSpeed, bulletLifeSpan;
    public Transform shootPoint;
    public GameObject bulletPrefab;
    public Transform crossHair;

    public LineRenderer aimLine;
    public LayerMask hitMask = ~0;
    public LayerMask groundMask = ~0;
    public int maxPoints = 120;
    public float simStep = 0.04f;
    public float gravityY = -9.8f;
    public float crosshairOffset = 0.02f;

    float angleX;

    void Update()
    {
        TurretRotation();
        BarrelRotation();
        UpdateAimPreview();
        if (Input.GetKeyDown(KeyCode.Space)) Fire();
    }

    void TurretRotation()
    {
        float dt = Time.deltaTime;
        float hInput = Input.GetAxis("Horizontal");
        float angle = rotSpeed * hInput * dt;
        turret.Rotate(new Vector3(0, angle, 0), Space.Self);
    }

    void BarrelRotation()
    {
        float dt = Time.deltaTime;
        float vInput = Input.GetAxis("Vertical");
        angleX -= rotSpeed * vInput * dt;
        angleX = Mathf.Clamp(angleX, -90f, 0f);
        barrel.localRotation = Quaternion.Euler(angleX, 0, 0);
    }

    void UpdateAimPreview()
    {
        if (!crossHair) return;

        if (aimLine)
        {
            aimLine.enabled = true;
            aimLine.positionCount = 1;
            aimLine.SetPosition(0, shootPoint.position);
        }

        Vector3 g = new Vector3(0f, gravityY, 0f);
        Vector3 p0 = shootPoint.position;
        Vector3 v0 = bulletSpeed * shootPoint.forward;

        Vector3 p = p0;
        Vector3 v = v0;
        Vector3 lastPoint = p0;

        for (int i = 1; i < maxPoints; i++)
        {
            v += g * simStep;
            Vector3 next = p + v * simStep;
            Vector3 dir = next - p;
            float dist = dir.magnitude;

            if (dist > 0.0001f && Physics.Raycast(p, dir.normalized, out var hit, dist, hitMask, QueryTriggerInteraction.Ignore))
            {
                if (aimLine) { aimLine.positionCount = i + 1; aimLine.SetPosition(i, hit.point); }
                lastPoint = hit.point;
                break;
            }

            if (aimLine) { aimLine.positionCount = i + 1; aimLine.SetPosition(i, next); }
            lastPoint = next;
            p = next;
        }

        Vector3 place = lastPoint;
        if (Physics.Raycast(lastPoint + Vector3.up * 10f, Vector3.down, out var groundHit, 50f, groundMask, QueryTriggerInteraction.Ignore))
            place = groundHit.point;

        crossHair.gameObject.SetActive(true);
        crossHair.position = place + Vector3.up * crosshairOffset;
        crossHair.rotation = Quaternion.Euler(90f, 0f, 0f);
        if (crossHair.TryGetComponent<Renderer>(out var r)) r.enabled = true;
    }

    void Fire()
    {
        GameObject bullet = Instantiate(bulletPrefab);
        bullet.transform.SetPositionAndRotation(shootPoint.position, shootPoint.rotation);
        if (bullet.TryGetComponent<TiroParabolico>(out var tp))
        {
            tp.Init(shootPoint.position, bulletSpeed * shootPoint.forward);
            tp.gravedad = new Vector3(0f, gravityY, 0f);
        }
        Destroy(bullet, bulletLifeSpan);
    }
}