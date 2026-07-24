using UnityEngine;
using System;
using TMPro;

public class WavesManager : MonoBehaviour
{
    [SerializeField] private GameObject enemyForCloning;
    private int waveIndex;
    public int enemyLeft;
    public GameObject UbgradePanel;
    private Action[] _actions;
    
    [SerializeField] private TMP_Text waveCounter;

    private void Awake()
    {
        waveIndex = 0;
        waveCounter.text = waveIndex + 1.ToString();
        _actions = new Action[]
        {
            wave1,
            wave2,
            wave3,
            wave4,
            wave5
        };
        
        _actions[waveIndex].Invoke();
    }
    public void StartNextRoundForUbgrade()
    {
        waveIndex++;
        UbgradePanel.SetActive(false);
        _actions[waveIndex].Invoke();
        waveCounter.text = waveIndex + 1.ToString();
    }
    void wave1()
    {
        enemyLeft = 1;
        Instantiate(enemyForCloning, new Vector2(0, 5), Quaternion.identity);
    }

    void wave2()
    {
        enemyLeft = 3;
        for (int i = 0; i < 3; i++)
        {
            Instantiate(enemyForCloning, new Vector2(0, 5), Quaternion.identity);
        }
    }

    void wave3()
    {
        enemyLeft = 5;
        for (int i = 0; i < 5; i++)
        {
            Instantiate(enemyForCloning, new Vector2(0, 5), Quaternion.identity);
        }
    }

    void wave4()
    {
        enemyLeft = 8;
        for (int i = 0; i < 8; i++)
        {
            Instantiate(enemyForCloning, new Vector2(0, 5), Quaternion.identity);
        }
    }

    void wave5()
    {
        enemyLeft = 10;
        for (int i = 0; i < 10; i++)
        {
            Instantiate(enemyForCloning, new Vector2(0, 5), Quaternion.identity);
        }
    }
}
