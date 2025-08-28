using UnityEngine;

public class TiroParabolico : MonoBehaviour
{
    public Vector3 P0;
    public Vector3 V0;
    public Vector3 gravedad = new Vector3(0, -9.81f, 0);

    Vector3 p;
    Vector3 v;
    bool inicializado;

    public void Init(Vector3 p0, Vector3 v0)
    {
        P0 = p0;
        V0 = v0;
        p = P0;
        v = V0;
        transform.position = p;
        if (v.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(v.normalized, Vector3.up);
        inicializado = true;
    }

    public Vector3 Velocity => v;
    public Vector3 Position => p;
    public bool IsInitialized => inicializado;

    void Update()
    {
        if (!inicializado) return;
        v += gravedad * Time.deltaTime;
        p += v * Time.deltaTime;
        transform.position = p;
        if (v.sqrMagnitude > 0.0001f) transform.rotation = Quaternion.LookRotation(v.normalized, Vector3.up);
    }
}