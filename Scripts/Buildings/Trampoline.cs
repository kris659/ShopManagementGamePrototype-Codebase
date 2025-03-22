using UnityEngine;

public class Trampoline : MonoBehaviour
{
    public float bounceForce = 10f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 6)
        {
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = new Vector2(rb.velocity.x, bounceForce);
            }
        }
    }
}
