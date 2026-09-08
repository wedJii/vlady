using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 5f;
    private PlayerControll playerScript;
    private Rigidbody2D rb;
    public bool isDuplicate = false;
    public bool isRicochet = false;
    
    public event Action<Enemy> OnHitEnemy;

    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerScript = FindFirstObjectByType<PlayerControll>();
    }

    public void Start()
    {
        Destroy(gameObject, lifetime);

        if (playerScript == null)
            playerScript = FindFirstObjectByType<PlayerControll>();

        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 mouseWorldPos = Camera.main != null ? Camera.main.ScreenToWorldPoint(mouseScreenPos) : Vector3.zero;
        mouseWorldPos.z = 0f;
        
        Vector2 direction = (mouseWorldPos - transform.position).normalized;
        float speed = playerScript != null ? playerScript.bulletSpeed : 10f;
        
        if (rb != null)
            rb.linearVelocity = direction * speed;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Не наносим урон игроку
        if (other.GetComponentInParent<PlayerControll>() != null || other.CompareTag("Player"))
            return;

        // Попадание во врага
        var enemy = other.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            float damage = playerScript != null ? playerScript.bulletDamage : 10f;
            enemy.currentHealth_Enemy -= damage;
            OnHitEnemy?.Invoke(enemy);
            Destroy(gameObject);
            return;
        }

        // Разрушение о твердые препятствия
        if (!other.isTrigger)
        {
            if (!isRicochet) Destroy(gameObject);
        }
    }
}