using System;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider), typeof(AudioSource))]
public class Bomb : MonoBehaviour
{
    public float explosionRadius = 5f;
    public float explosionForce = 1200f;
    public float upwardModifier = 0.5f;
    public LayerMask hitMask = ~0;
    public ParticleSystem explosionVFX;
    public AudioClip explosionSFX;
    public float fuseAfterGround = 0.05f;
    public float fuseMultiplier = 1f;

    public Action<Vector3> OnExploded;

    private Rigidbody rb;
    private Collider col;
    private Transform attachedTo;
    private bool exploded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        SetPhysicsActive(true);
    }

    void LateUpdate()
    {
        if (attachedTo)
        {
            transform.position = attachedTo.position;
            transform.rotation = attachedTo.rotation;
        }
    }

    public void AttachTo(PlayerController3P player)
    {
        attachedTo = player.handSocket;
        transform.SetParent(attachedTo);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        SetPhysicsActive(false);
        player.HasBomb = true;
    }

    public void DetachAndThrow(Vector3 velocity)
    {
        transform.SetParent(null);
        attachedTo = null;
        SetPhysicsActive(true);
        rb.linearVelocity = velocity;
        rb.angularVelocity = UnityEngine.Random.insideUnitSphere * 6f;
    }

    void SetPhysicsActive(bool active)
    {
        rb.isKinematic = !active;
        col.enabled = true;
    }

    // ---- DEFLECT (Parry con sartén) opcional ----
    public void Deflect(Vector3 normal, float multiplier = 1.1f)
    {
        rb.velocity = Vector3.Reflect(rb.linearelocity, normal) * multiplier;
        // pequeño spin para el drama
        rb.angularVelocity = UnityEngine.Random.insideUnitSphere * 7f;
    }

    void OnCollisionEnter(Collision c)
    {
        if (exploded) return;

        // Parry con sartén (si existe y está activo)
        var pan = c.collider.GetComponentInParent<PanParry>();
        if (pan && pan.IsActive)
        {
            Deflect(c.GetContact(0).normal, pan.deflectMultiplier);
            pan.PlayClank();
            return;
        }

        if (c.collider.CompareTag("Ground"))
        {
            Invoke(nameof(Explode), fuseAfterGround * fuseMultiplier);
        }
        else
        {
            var kb = c.collider.GetComponentInParent<Knockbackable>();
            if (kb)
            {
                kb.ApplyKnockback(c.GetContact(0).point,
                    (kb.transform.position - transform.position).normalized * (explosionForce * 0.6f) + Vector3.up * 6f);
                Explode();
            }
            else
            {
                Explode();
            }
        }
    }

    public void TuneDifficulty(float fuseScale)
    {
        fuseMultiplier = Mathf.Clamp(fuseMultiplier * fuseScale, 0.5f, 1.2f);
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;

        Vector3 pos = transform.position;

        if (explosionVFX)
        {
            var fx = Instantiate(explosionVFX, pos, Quaternion.identity);
            fx.gameObject.SetActive(true);
            fx.Play();
        }

        var audio = GetComponent<AudioSource>();
        if (audio && explosionSFX) audio.PlayOneShot(explosionSFX);

        var shaker = Camera.main ? Camera.main.GetComponent<CameraShake>() : null;
        if (shaker) shaker.Shake(0.3f, 0.4f);

        foreach (var h in Physics.OverlapSphere(pos, explosionRadius, hitMask, QueryTriggerInteraction.Ignore))
        {
            var kb = h.GetComponentInParent<Knockbackable>();
            if (kb)
            {
                Vector3 dir = (kb.transform.position - pos).normalized + Vector3.up * upwardModifier;
                kb.ApplyExplosion(pos, dir * explosionForce);
            }
        }

        OnExploded?.Invoke(pos);

        var mr = GetComponent<MeshRenderer>(); if (mr) mr.enabled = false;
        col.enabled = false;
        rb.isKinematic = true;
        Destroy(gameObject, 1.2f);
    }
}