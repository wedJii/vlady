using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    private WavesManager wavesManager;
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
                    wavesManager.enemyLeft--;
                    if (wavesManager.enemyLeft == 0)
                    {
                        wavesManager.UbgradePanel.SetActive(true);
                    }
                    Destroy(gameObject);
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

    [SerializeField] private float _damage = 10f;
    [SerializeField] private Slider healthSlider_Enemy;
    private bool _isPlayerTouched;

    private void Awake()
    {
        wavesManager = FindObjectOfType<WavesManager>();
    }

    private async void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            await Task.Delay(250);
            _isPlayerTouched = true;
    
            while (_isPlayerTouched)
            {
                if (Time.timeScale > 0f)
                {
                   other.GetComponent<PlayerControll>().currentHealth_Player -= _damage;
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
