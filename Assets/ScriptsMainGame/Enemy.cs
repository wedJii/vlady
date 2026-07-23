using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float damage;
    private bool isPlayerTouched = false;
    
    private async void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player") 
        {
            isPlayerTouched = true;
            while (isPlayerTouched)
            {
                other.GetComponent<PlayerControll>().currentHealth_Player -= damage;
                await Task.Delay(1000);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player") isPlayerTouched = false;
    }
}
