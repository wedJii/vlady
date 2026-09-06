using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class CoinScript : MonoBehaviour
{
    private PlayerControll playerScript;
    private GameObject player;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerScript = player.GetComponent<PlayerControll>();
    }

    async private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            transform.DOMove(player.transform.position, 0.10f).SetEase(Ease.Linear);
            await Task.Delay(100);
            playerScript.CurrentCoins += playerScript.coinIncome;
            Destroy(gameObject);    
        }
    }
}
