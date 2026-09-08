using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class EasterEggTeleport : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "poshalochko2";
    [SerializeField] private float triggerRadius = 2.5f;

    private bool _playerNearby;
    private PlayerControll _player;

    private void Start() =>
        _player = FindFirstObjectByType<PlayerControll>();

    private void Update()
    {
        if (_player == null)
            _player = FindFirstObjectByType<PlayerControll>();

        if (_player != null)
            _playerNearby = Vector2.Distance(transform.position, _player.transform.position) <= triggerRadius;

        if (!_playerNearby) return;

        bool ePressed = false;
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            ePressed = true;

        if (!ePressed)
        {
            try
            {
                if (Input.GetKeyDown(KeyCode.E))
                    ePressed = true;
            }
            catch { }
        }

        if (ePressed)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(targetSceneName);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other)) _playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other)) _playerNearby = false;
    }

    private bool IsPlayer(Collider2D other) =>
        other.CompareTag("Player") ||
        other.GetComponent<PlayerControll>() != null ||
        other.GetComponentInParent<PlayerControll>() != null;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}
