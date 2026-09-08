using UnityEngine;

public class RicochetModifier : MonoBehaviour
{
    public int bounceCount = 1;
    private Rigidbody2D rb;
    private Collider2D col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    public bool Bounce(Collider2D obstacle)
    {
        if (bounceCount <= 0 || rb == null) return false;

        Vector2 vel = rb.linearVelocity;
        if (vel.sqrMagnitude < 0.001f) return false;

        Vector2 normal = Vector2.zero;

        // 1. Пытаемся получить нормаль через ColliderDistance2D
        if (col != null && obstacle != null)
        {
            var dist = col.Distance(obstacle);
            if (dist.isValid && dist.normal != Vector2.zero)
            {
                normal = dist.normal;
            }
        }

        // 2. Если нормаль не найдена, пускаем короткий луч в направлении движения
        if (normal == Vector2.zero && obstacle != null)
        {
            var hit = Physics2D.Raycast((Vector2)transform.position - vel.normalized * 0.4f, vel.normalized, 1.2f);
            if (hit.collider == obstacle)
            {
                normal = hit.normal;
            }
        }

        // 3. Рассчитываем отражение
        Vector2 reflectDir = normal != Vector2.zero ? Vector2.Reflect(vel, normal) : -vel;
        if (reflectDir == Vector2.zero || reflectDir == vel)
            reflectDir = -vel;

        rb.linearVelocity = reflectDir.normalized * vel.magnitude;

        // Небольшой сдвиг от стены, чтобы пуля не застревала в коллайдере
        transform.position += (Vector3)(reflectDir.normalized * 0.08f);

        float angle = Mathf.Atan2(reflectDir.y, reflectDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        bounceCount--;
        return true;
    }
}