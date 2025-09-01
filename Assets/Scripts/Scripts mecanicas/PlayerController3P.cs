using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController3P : MonoBehaviour
{
    [Header("Meta")]
    public string displayName = "Px";
    public Transform handSocket;
    public Renderer[] tintRenderers;

    [Header("Movimiento")]
    public float moveSpeed = 7f;
    public float rotateSpeed = 720f;

    [Header("Lanzamiento")]
    public float throwSpeed = 12f;
    public float lobArc = 5f;

    [Header("Estado")]
    public bool IsAlive { get; private set; } = true;
    public bool HasBomb { get; set; }

    public event Action<PlayerController3P> OnEliminated;

    private Rigidbody rb;
    private List<PlayerController3P> rivals = new();
    private int targetIndex = 0;
    private Vector3 moveWorld;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!handSocket)
        {
            handSocket = new GameObject("HandSocket").transform;
            handSocket.SetParent(transform);
            handSocket.localPosition = new Vector3(0.35f, 1.1f, 0.3f);
        }
    }

    public void RegisterRivals(List<PlayerController3P> all)
    {
        rivals = all.Where(x => x != this).ToList();
        targetIndex = 0;
    }

    public void SetTint(Material m)
    {
        foreach (var r in tintRenderers) if (r) r.material = m;
    }

    public void SetMoveInput(Vector2 move)
    {
        moveWorld = new Vector3(move.x, 0, move.y);
        moveWorld = Vector3.ClampMagnitude(moveWorld, 1f);
    }

    void FixedUpdate()
    {
        if (!IsAlive) return;

        Vector3 vel = moveWorld * moveSpeed;
        rb.velocity = new Vector3(vel.x, rb.velocity.y, vel.z);

        if (vel.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(vel);
            rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeed * Time.fixedDeltaTime));
        }
    }

    public void CycleTarget(int dir)
    {
        if (rivals == null || rivals.Count == 0) return;
        var vivos = rivals.Where(r => r && r.IsAlive).ToList();
        if (vivos.Count == 0) return;

        var current = (targetIndex >= 0 && targetIndex < rivals.Count) ? rivals[targetIndex] : null;
        if (current == null || !current.IsAlive) current = vivos[0];

        int idx = vivos.IndexOf(current);
        idx = (idx + dir + vivos.Count) % vivos.Count;
        var nuevo = vivos[idx];
        targetIndex = rivals.IndexOf(nuevo);
        // aquí puedes encender un indicador visual sobre 'nuevo'
    }

    public void TryThrow()
    {
        if (!IsAlive || !HasBomb) return;
        var bomb = FindObjectOfType<Bomb>();
        if (!bomb) return;

        var target = GetCurrentTargetAlive();
        if (target == null) return;

        HasBomb = false;
        bomb.DetachAndThrow(CalcThrowVelocity(target.transform.position));
    }

    PlayerController3P GetCurrentTargetAlive()
    {
        if (rivals == null || rivals.Count == 0) return null;
        if (targetIndex < 0 || targetIndex >= rivals.Count) targetIndex = 0;
        var t = rivals[targetIndex];
        if (t && t.IsAlive) return t;
        return rivals.FirstOrDefault(x => x && x.IsAlive);
    }

    Vector3 CalcThrowVelocity(Vector3 targetPos)
    {
        Vector3 start = handSocket.position;
        Vector3 to = targetPos - start;
        Vector3 toXZ = new Vector3(to.x, 0f, to.z);
        float time = Mathf.Max(0.25f, toXZ.magnitude / throwSpeed);
        float vy = (to.y + 0.5f * Physics.gravity.magnitude * time * time) / time;
        Vector3 vxz = toXZ / time;
        return vxz + Vector3.up * vy + Vector3.up * lobArc;
    }

    public void Eliminate()
    {
        if (!IsAlive) return;
        IsAlive = false;

        foreach (var r in tintRenderers) if (r) r.enabled = false;
        var col = GetComponent<Collider>(); if (col) col.enabled = false;
        rb.isKinematic = true;

        OnEliminated?.Invoke(this);
    }
}