using UnityEngine;

public class EnemyArrow : MonoBehaviour
{
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _lifetime = 5f;

    private Rigidbody2D _rb;

    private void Awake() => _rb = GetComponent<Rigidbody2D>();

    public void Init(Vector2 direction, float damage = 10f, float speed = 8f)
    {
        _damage = damage;
        _speed = speed;

        if (_rb != null)
            _rb.linearVelocity = direction.normalized * _speed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Destroy(gameObject, _lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Не трогаем других врагов и стрелы
        if (other.GetComponentInParent<Enemy>() != null || other.GetComponent<EnemyArrow>() != null)
            return;

        // Попадание в игрока (по компоненту или тегу)
        var player = other.GetComponentInParent<PlayerControll>();
        if (player != null || other.CompareTag("Player"))
        {
            if (player != null)
            {
                player.currentHealth_Player -= _damage;
            }
            Destroy(gameObject);
            return;
        }

        // Уничтожение о препятствия и стены
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
