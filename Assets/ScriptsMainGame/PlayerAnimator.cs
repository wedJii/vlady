using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("Sprite Sequences (48 frames: 4 directions x 12 frames)")]
    [SerializeField] private Sprite[] idleSprites = new Sprite[48];
    [SerializeField] private Sprite[] walkSprites = new Sprite[48];
    [SerializeField] private Sprite[] runSprites = new Sprite[48];

    [Header("Animation Speeds (FPS)")]
    [SerializeField] private float idleFPS = 8f;
    [SerializeField] private float walkFPS = 12f;
    [SerializeField] private float runFPS = 16f;

    private SpriteRenderer _sr;
    private Rigidbody2D _rb;
    private float _timer;
    private int _frame;
    private int _direction = 0; // 0 = Down (0..11), 1 = Left (12..23), 2 = Right (24..35), 3 = Up (36..47)

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        if (_sr != null) _sr.flipX = false;
    }

    private void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 inputDir = new Vector2(h, v);
        bool hasInput = inputDir.sqrMagnitude > 0.01f;

        Vector2 vel = _rb != null ? _rb.linearVelocity : Vector2.zero;
        bool isMoving = hasInput || vel.sqrMagnitude > 0.05f;

        if (isMoving)
        {
            Vector2 moveDir = hasInput ? inputDir : vel;
            _direction = CalculateDirection(moveDir);
        }

        bool isRunning = isMoving && (vel.magnitude > 4.5f || (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed));
        
        Sprite[] activeList;
        float fps;

        if (!isMoving)
        {
            activeList = idleSprites;
            fps = idleFPS;
        }
        else if (isRunning && runSprites != null && runSprites.Length >= 48 && runSprites[0] != null)
        {
            activeList = runSprites;
            fps = runFPS;
        }
        else
        {
            activeList = (walkSprites != null && walkSprites.Length >= 48 && walkSprites[0] != null) ? walkSprites : runSprites;
            fps = walkFPS;
        }

        _timer += Time.deltaTime;
        if (_timer >= 1f / fps)
        {
            _timer = 0f;
            _frame = (_frame + 1) % 12;
        }

        if (activeList != null && activeList.Length >= 48)
        {
            int index = (_direction * 12) + _frame;
            if (index >= 0 && index < activeList.Length && activeList[index] != null)
            {
                _sr.sprite = activeList[index];
            }
        }
    }

    private int CalculateDirection(Vector2 dir)
    {
        // 4 основных направления:
        // Если движение больше по горизонтали — приоритет влево/вправо
        // Если по вертикали — приоритет вверх/вниз
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            return dir.x < 0 ? 1 : 2; // 1 = Влево, 2 = Вправо
        }
        else if (Mathf.Abs(dir.y) > 0.01f)
        {
            return dir.y > 0 ? 3 : 0; // 3 = Вверх, 0 = Вниз
        }
        else if (Mathf.Abs(dir.x) > 0.01f)
        {
            return dir.x < 0 ? 1 : 2;
        }

        return _direction;
    }
}
