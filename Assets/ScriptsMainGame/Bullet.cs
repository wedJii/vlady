using System; // ОБЯЗАТЕЛЬНО для Action
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Sprite[] animFrames;
    [SerializeField] private float animFPS = 14f;

    private PlayerControll playerScript;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float animTimer;
    private int frameIndex;
    public bool isDuplicate = false;
    
    public event Action<Enemy> OnHitEnemy;

    public void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void Start()
    {
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

    private void Update()
    {
        if (animFrames != null && animFrames.Length > 0 && sr != null)
        {
            animTimer += Time.deltaTime;
            if (animTimer >= 1f / animFPS)
            {
                animTimer = 0f;
                frameIndex = (frameIndex + 1) % animFrames.Length;
                if (frameIndex < animFrames.Length && animFrames[frameIndex] != null)
                {
                    sr.sprite = animFrames[frameIndex];
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player"))
        {
            if (other.gameObject.CompareTag("Enemy"))
            {
                var enemy = other.GetComponent<Enemy>();
                if (enemy != null && playerScript != null)
                {
                    enemy.currentHealth_Enemy -= playerScript.bulletDamage;
                    OnHitEnemy?.Invoke(enemy);
                }
            }
            Destroy(gameObject);
        }
    }
}