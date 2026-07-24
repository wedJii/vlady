using UnityEngine;

public class Bullet : MonoBehaviour
{
    private PlayerControll playerScript;
    private Rigidbody2D rb;

    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Start()
    {
        playerScript = FindFirstObjectByType<PlayerControll>();

        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0f;
        
        Vector2 direction = (mouseWorldPos - transform.position).normalized;
        
        rb.linearVelocity = direction * playerScript.bulletSpeed;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player"))
        {
            if (other.gameObject.CompareTag("Enemy"))
            {
                other.GetComponent<Enemy>().currentHealth_Enemy -= playerScript.bulletDamage;
            }
            Destroy(gameObject);
        }
    }
}