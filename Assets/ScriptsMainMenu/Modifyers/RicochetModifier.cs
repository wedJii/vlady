using UnityEngine;

public class RicochetModifier : MonoBehaviour
{
    public int bounceCount = 1;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Enemy"))
        {
            if (bounceCount > 0)
            {
                bounceCount--;
                
                ContactPoint2D contact = collision.contacts[0];
                Vector2 reflectDirection = Vector2.Reflect(rb.linearVelocity, contact.normal);
                
                rb.linearVelocity = reflectDirection;
                
                float angle = Mathf.Atan2(reflectDirection.y, reflectDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
} 