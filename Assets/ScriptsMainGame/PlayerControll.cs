using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerControll : MonoBehaviour
{
    [Header("Movement")] public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;

    [Header("UI & Numbers")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private GameObject LosePanel;
    private float _currentHealth_Player = 100;
    private float _maxHealth_Player = 100;
    public int CurrentCoins;

    [Header("Camera Settings")]
    [SerializeField] private Camera cam;
    [SerializeField] private float targetZoomSize = 2.5f;
    [SerializeField] private float zoomDuration = 1f;

    [Header("Camera Shake Settings")]
    [SerializeField] private float shakeDuration = 0.15f;
    [SerializeField] private float shakeMagnitude = 0.2f;

    private bool isDead = false;
    private Coroutine shakeCoroutine;
    
    [Header("Bullet Settings")]
    public float bulletSpeed = 10f;
    public float bulletDamage = 10f;
    public float shootCooldown = 0.3f;
    private float nextShootTime = 0f; 
    [SerializeField] private GameObject bulletPrefab;
    private Bullet bulletScript;
    public Vector3 mouseWorldPos;
    
    private Animator animator;

    public float maxHealth_Player
    {
        get => _maxHealth_Player;
        set
        {
            if (_maxHealth_Player != value)
            {
                _maxHealth_Player = value;
                if (healthSlider != null)
                {
                    healthSlider.maxValue = _maxHealth_Player;
                }
            }
        }
    }

    public float currentHealth_Player
    {
        get => _currentHealth_Player;
        set
        {
            if (_currentHealth_Player != value)
            {
                if (value < _currentHealth_Player && value > 0 && !isDead)
                {
                    DamageShake();
                }

                _currentHealth_Player = value;

                if (healthSlider != null)
                {
                    healthSlider.value = _currentHealth_Player;
                    if (!healthSlider.gameObject.activeSelf)
                    {
                        healthSlider.gameObject.SetActive(true);
                    }
                }

                if (_currentHealth_Player <= 0 && !isDead)
                {
                    Die();
                }
            }
        }
    }

    private SpriteRenderer _sr;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        CurrentCoins = 0;
    }

    void Start()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = _maxHealth_Player;
            healthSlider.value = _currentHealth_Player;
            healthSlider.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (isDead) return;

        // Блокируем управление, если открыт инвентарь
        if (UI_Inventory.Instance != null && UI_Inventory.Instance.IsOpen)
        {
            movement = Vector2.zero;
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");    

        movement = new Vector2(horizontal, vertical).normalized;

        // Проверяем нажатие ЛКМ, готовность кулдауна и отсутствие клика по UI
        if (Input.GetMouseButtonDown(0) && Time.time >= nextShootTime)
        {
            if (UnityEngine.EventSystems.EventSystem.current != null && 
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;

            Shoot();
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (UI_Inventory.Instance != null && UI_Inventory.Instance.IsOpen)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            return;
        }

        if (rb != null)
        {
            rb.linearVelocity = movement * speed;
        }
    }

    public void DamageShake()
    {
        if (cam == null) return;
        
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        shakeCoroutine = StartCoroutine(ShakeCameraRoutine());
    }

    private IEnumerator ShakeCameraRoutine()
    {
        Vector3 originalPos = cam.transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            float offsetY = Random.Range(-1f, 1f) * shakeMagnitude;

            cam.transform.localPosition = new Vector3(originalPos.x + offsetX, originalPos.y + offsetY, originalPos.z);

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        cam.transform.localPosition = originalPos;
        shakeCoroutine = null;
    }

    private void Die()
    {
        isDead = true;
        RaidLoadoutManager.OnPlayerDied();

        if (rb != null) rb.linearVelocity = Vector2.zero;
        
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }

        StartCoroutine(SmoothZoomToPlayer());
        if (LosePanel != null) LosePanel.SetActive(true);
        Time.timeScale = 0;
    }

    private IEnumerator SmoothZoomToPlayer()
    {
        if (cam == null) yield break;

        float elapsedTime = 0f;
        float startSize = cam.orthographicSize;
        Vector3 startPos = cam.transform.position;

        Vector3 targetPos = new Vector3(transform.position.x, transform.position.y, startPos.z);

        while (elapsedTime < zoomDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / zoomDuration);
            float smoothT = 1f - Mathf.Pow(1f - t, 3);

            cam.orthographicSize = Mathf.Lerp(startSize, targetZoomSize, smoothT);
            cam.transform.position = Vector3.Lerp(startPos, targetPos, smoothT);

            yield return null;
        }

        cam.orthographicSize = targetZoomSize;
        cam.transform.position = targetPos;
    }

    public virtual void Shoot()
    {
        nextShootTime = Time.time + shootCooldown;
        Instantiate(bulletPrefab, transform.position, Quaternion.identity);
    }
}