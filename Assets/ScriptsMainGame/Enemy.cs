using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float _speed = 3f;
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _maxHealth = 50f;
    [SerializeField] private float _attackRate = 0.5f;
    [SerializeField] private string _targetTag = "Player";

    private float _currentHealth;
    private float _nextAttackTime;
    private Transform _target;

    private void Start()
    {
        _currentHealth = _maxHealth;
        FindTarget();
    }

    private void FindTarget()
    {
        PlayerControll player = Object.FindAnyObjectByType<PlayerControll>();
        if (player != null)
        {
            _target = player.transform;
            return;
        }

        GameObject targetObj = GameObject.FindWithTag(_targetTag);
        if (targetObj != null)
        {
            _target = targetObj.transform;
        }
    }

    private void Update()
    {
        if (_target == null)
        {
            FindTarget();
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, _target.position, _speed * Time.deltaTime);
    }

    public void TakeDamage(float amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DealDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        DealDamage(other);
    }

    private void DealDamage(Collider2D other)
    {
        if (Time.time < _nextAttackTime) return;

        if (other.TryGetComponent(out PlayerControll player))
        {
            player.currentHealth_Player -= _damage;
            _nextAttackTime = Time.time + _attackRate;
        }
    }
}
