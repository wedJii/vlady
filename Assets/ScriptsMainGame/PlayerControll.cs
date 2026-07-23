using UnityEngine;
using UnityEngine.UI;

public class PlayerControll : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;

    [Header("UI & Health")]
    [SerializeField] private Slider healthSlider;
    private float _currentHealth_Player;
    private float _maxHealth_Player;

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
                _currentHealth_Player = value;
                if (healthSlider != null)
                {
                    healthSlider.value = _currentHealth_Player;
                }
            }
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void Start()
    {
        maxHealth_Player = 100f;
        currentHealth_Player = 100f;
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        movement = new Vector2(horizontal, vertical).normalized;
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = movement * speed;
        }
    }
}