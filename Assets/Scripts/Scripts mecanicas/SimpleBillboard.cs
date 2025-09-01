using UnityEngine;

using UnityEngine;

public class SimpleBillboard : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main) transform.forward = Camera.main.transform.forward;
    }
}
