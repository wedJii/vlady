using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Pathfinding;

public class Enemy : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private Slider healthSlider_Enemy;
    private float _currentHealth_Enemy = 100;
    public float currentHealth_Enemy
    {
        get => _currentHealth_Enemy;
        set
        {
            if (_currentHealth_Enemy != value)
            {
                _currentHealth_Enemy = value;

                if (healthSlider_Enemy != null)
                {
                    healthSlider_Enemy.value = _currentHealth_Enemy;
                }

                if (_currentHealth_Enemy <= 0)
                {
                    Die();
                }
            }
        }
    }

    private float _maxHealth_Enemy = 100;
    public float maxHealth_Enemy
    {
        get => _maxHealth_Enemy;
        set
        {
            if (_maxHealth_Enemy != value)
            {
                _maxHealth_Enemy = value;
                if (healthSlider_Enemy != null) 
                {
                    healthSlider_Enemy.maxValue = _maxHealth_Enemy;
                }
            }
        }
    }

    [Header("Melee Contact Damage")]
    [SerializeField] private float _damage = 10f;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 2.2f;
    [SerializeField] private float _stoppingDistance = 4f;
    [SerializeField] private GameObject coinPrefab;

    [Header("Ranged Attack (Bow)")]
    [SerializeField] private bool _isRanged = true;
    [SerializeField] private GameObject _arrowPrefab;
    [SerializeField] private float _attackRange = 7f;
    [SerializeField] private float _attackCooldown = 2f;
    [SerializeField] private float _arrowSpeed = 8f;
    [SerializeField] private float _arrowDamage = 10f;

    private Transform _target;
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private Animator _animator;
    private AIPath _aiPath;
    private AIDestinationSetter _destinationSetter;
    private Coroutine _damageRoutine;
    private float _nextShootTime;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _aiPath = GetComponent<AIPath>();
        _destinationSetter = GetComponent<AIDestinationSetter>();
    }

    private void Start()
    {
        FindPlayerTarget();

        if (healthSlider_Enemy != null)
        {
            healthSlider_Enemy.maxValue = _maxHealth_Enemy;
            healthSlider_Enemy.value = _currentHealth_Enemy;
        }

        if (_aiPath != null)
        {
            _aiPath.maxSpeed = _moveSpeed;
            _aiPath.endReachedDistance = _isRanged ? _stoppingDistance : 0.5f;
        }
    }

    private void FindPlayerTarget()
    {
        var player = FindFirstObjectByType<PlayerControll>();
        if (player != null)
        {
            _target = player.transform;
            if (_destinationSetter != null)
            {
                _destinationSetter.target = _target;
            }
        }
    }

    private bool _isAttacking;

    private void Update()
    {
        if (_target == null)
        {
            FindPlayerTarget();
            return;
        }
        
        if (_sr != null)
        {
            float horizontalDir = 0f;

            if (_isAttacking || _target == null)
            {
                if (_target != null)
                    horizontalDir = _target.position.x - transform.position.x;
            }
            else
            {
                if (_aiPath != null && Mathf.Abs(_aiPath.velocity.x) > 0.1f)
                    horizontalDir = _aiPath.velocity.x;
                else if (_rb != null && Mathf.Abs(_rb.linearVelocity.x) > 0.1f)
                    horizontalDir = _rb.linearVelocity.x;
                else if (_target != null)
                    horizontalDir = _target.position.x - transform.position.x;
            }

            if (Mathf.Abs(horizontalDir) > 0.05f)
            {
                _sr.flipX = horizontalDir < 0f;
            }
        }

        // Стрельба из лука по кулдауну
        if (_isRanged && _arrowPrefab != null && !_isAttacking)
        {
            float distToTarget = Vector2.Distance(transform.position, _target.position);
            if (distToTarget <= _attackRange && Time.time >= _nextShootTime)
            {
                ShootArrow();
            }
        }
    }

    private void FixedUpdate()
    {
        if (_target == null) return;

        if (_isAttacking)
        {
            if (_rb != null) _rb.linearVelocity = Vector2.zero;
            if (_animator != null && _animator.isActiveAndEnabled)
            {
                _animator.SetBool("IsMoving", false);
                _animator.SetFloat("Speed", 0f);
            }
            return;
        }

        bool hasActiveAstar = AstarPath.active != null && _aiPath != null && _aiPath.canMove;

        if (!hasActiveAstar && _rb != null)
        {
            Vector2 toTarget = _target.position - transform.position;
            float distance = toTarget.magnitude;
            float desiredDistance = _isRanged ? _stoppingDistance : 0.6f;

            if (distance > desiredDistance)
            {
                _rb.linearVelocity = toTarget.normalized * _moveSpeed;
            }
            else
            {
                _rb.linearVelocity = Vector2.zero;
            }
        }

        if (_animator != null && _animator.isActiveAndEnabled)
        {
            bool isMoving = _rb != null && _rb.linearVelocity.sqrMagnitude > 0.05f;
            if (hasActiveAstar && _aiPath != null)
                isMoving = _aiPath.velocity.sqrMagnitude > 0.05f;

            _animator.SetBool("IsMoving", isMoving);
            _animator.SetFloat("Speed", isMoving ? _moveSpeed : 0f);
        }
    }

    private void LateUpdate()
    {
        if (_sr != null)
            _sr.sortingOrder = 1000 + Mathf.RoundToInt(-transform.position.y * 100);
    }

    private void ShootArrow()
    {
        _nextShootTime = Time.time + _attackCooldown;
        StartCoroutine(ShootArrowRoutine());
    }

    private IEnumerator ShootArrowRoutine()
    {
        _isAttacking = true;
        if (_animator != null && _animator.isActiveAndEnabled)
        {
            _animator.SetTrigger("Attack");
        }

        if (_aiPath != null) _aiPath.canMove = false;
        if (_rb != null) _rb.linearVelocity = Vector2.zero;

        // Ожидание кадра полного натяжения тетивы перед вылетом стрелы
        yield return new WaitForSeconds(0.35f);

        if (_target != null && _arrowPrefab != null)
        {
            Vector2 dir = (_target.position - transform.position).normalized;
            GameObject arrowObj = Instantiate(_arrowPrefab, transform.position, Quaternion.identity);
            if (arrowObj.TryGetComponent(out EnemyArrow arrow))
            {
                arrow.Init(dir, _arrowDamage, _arrowSpeed);
            }
        }

        // Доигрывание анимации спуска тетивы
        yield return new WaitForSeconds(0.25f);

        if (_aiPath != null) _aiPath.canMove = true;
        _isAttacking = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerControll>();
        if (player != null || other.CompareTag("Player"))
        {
            if (player == null) player = other.GetComponent<PlayerControll>();
            if (player != null)
            {
                _damageRoutine ??= StartCoroutine(DamagePlayerRoutine(player));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerControll>() != null || other.CompareTag("Player"))
        {
            StopDamageRoutine();
        }
    }

    private void OnDisable()
    {
        StopDamageRoutine();
        StopAllCoroutines();
        _isAttacking = false;
        if (_aiPath != null) _aiPath.canMove = true;
    }

    private void StopDamageRoutine()
    {
        if (_damageRoutine != null)
        {
            StopCoroutine(_damageRoutine);
            _damageRoutine = null;
        }
    }

    private IEnumerator DamagePlayerRoutine(PlayerControll player)
    {
        yield return new WaitForSeconds(0.25f);

        while (player != null)
        {
            player.currentHealth_Player -= _damage;
            yield return new WaitForSeconds(1f);
        }

        _damageRoutine = null;
    }

    private void Die()
    {
        StopDamageRoutine();
        Instantiate(coinPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
