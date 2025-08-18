using UnityEngine;

public class Tiro_parabolico : MonoBehaviour
{
    float t;
    public Vector3 P0, V0;

    Vector3 g = new Vector3(0, -9.8f, 0);
    void Start()
    {
        
    }

    void Update()
    {
        t += Time.deltaTime;
        transform.position = PositionFuction(t);
    }
    Vector3 PositionFuction(float time)
    {
        return 0.5f * g * time * time + V0 * time + P0;
    }
}
