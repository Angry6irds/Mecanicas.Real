using UnityEngine;

public class PlayerTurretController : MonoBehaviour
{
    [Header("Refs")]
    public Transform baseYaw;     // nodo que rota en Y
    public Transform cannonPitch; // nodo que rota en X
    public Transform muzzle;      // punta del ca��n para instanciar proyectiles
    public GameObject projectilePrefab;

    [Header("Ajustes de giro")]
    public float yawSpeed = 120f;     // grados/seg
    public float pitchSpeed = 90f;    // grados/seg
    public Vector2 pitchLimits = new Vector2(-10f, 45f); // min,max en grados locales

    [Header("Disparo")]
    public float fireCooldown = 0.25f;
    public float projectileSpeed = 40f;

    float cd;

    void Update()
    {
        // Giro horizontal (A/D o flechas o mouse X)
        float yaw = Input.GetAxis("Horizontal");              // A/D o flechas
        float yawMouse = Input.GetAxis("Mouse X");            // mouse opcional
        float finalYaw = (yaw + yawMouse) * yawSpeed * Time.deltaTime;
        baseYaw.Rotate(0f, finalYaw, 0f, Space.Self);

        // Elevaci�n (W/S o mouse Y invertido)
        float pitch = -Input.GetAxis("Vertical");             // W/S
        float pitchMouse = -Input.GetAxis("Mouse Y");         // mouse
        float finalPitch = (pitch + pitchMouse) * pitchSpeed * Time.deltaTime;

        var e = cannonPitch.localEulerAngles;
        // Normalizar a -180..180
        if (e.x > 180f) e.x -= 360f;
        e.x = Mathf.Clamp(e.x + finalPitch, pitchLimits.x, pitchLimits.y);
        cannonPitch.localEulerAngles = new Vector3(e.x, 0f, 0f);

        // Cooldown
        cd -= Time.deltaTime;

        // Disparar con Click Izquierdo o Espacio
        if (cd <= 0f && (Input.GetButton("Fire1") || Input.GetKey(KeyCode.Space)))
        {
            Fire();
            cd = fireCooldown;
        }
    }

    void Fire()
    {
        var go = Instantiate(projectilePrefab, muzzle.position, muzzle.rotation);
        if (go.TryGetComponent<Rigidbody>(out var rb))
            rb.linearVelocity = muzzle.forward * projectileSpeed;
    }
}