using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int DirectionHash = Animator.StringToHash("Direction");
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");

    private Animator _animator;
    private Rigidbody2D _rb;
    private int _direction = 0; // 0: Down, 1: Left, 2: Right, 3: Up

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
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

        if (_animator != null)
        {
            _animator.SetBool(IsMovingHash, isMoving);
            _animator.SetInteger(DirectionHash, _direction);
            _animator.SetFloat(SpeedHash, isMoving ? (vel.sqrMagnitude > 0.01f ? vel.magnitude : inputDir.magnitude) : 0f);
            _animator.SetFloat(MoveXHash, inputDir.x);
            _animator.SetFloat(MoveYHash, inputDir.y);
        }
    }

    private int CalculateDirection(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return dir.x < 0 ? 1 : 2; // 1 = Влево, 2 = Вправо
        if (Mathf.Abs(dir.y) > 0.01f)
            return dir.y > 0 ? 3 : 0; // 3 = Вверх, 0 = Вниз
        if (Mathf.Abs(dir.x) > 0.01f)
            return dir.x < 0 ? 1 : 2;

        return _direction;
    }
}
