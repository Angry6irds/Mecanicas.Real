using System.Runtime.InteropServices;
using UnityEngine;

public class JumperPlayer : MonoBehaviour
{
    public float jumpImpulse, gravity;
    private bool grounded;
    private Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        Physics.gravity = new Vector3(0 , gravity, 0);
        if(grounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(jumpImpulse * transform.up, ForceMode.Impulse);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            grounded = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        {
            if (collision.collider.CompareTag("Ground"))
            {
                grounded = false;
            }
        }
    }
}
