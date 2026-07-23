using System.Threading.Tasks;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float _damage = 10f;
    private bool _isPlayerTouched;

    public float damage
    {
        get => _damage;
        set => _damage = value;
    }

    private async void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerTouched = true;
            PlayerControll playerControl = other.GetComponent<PlayerControll>();
            while (_isPlayerTouched && playerControl != null)
            {
                if (Time.timeScale > 0f)
                {
                    playerControl.currentHealth_Player -= _damage;
                }
                await Task.Delay(1000);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerTouched = false;
        }
    }
}
