using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Fire Plate", menuName = "Inventory/Plates/Fire Plate")]
public class BurnPlate : UbgradePlate
{
    [SerializeField] private float burnDamage = 5f;
    [SerializeField] private float waitBetweenBurns = 1f; // Время в секундах
    [SerializeField] private int burnTimes = 5;

    // Ссылка на игрока для запуска корутин
    private PlayerControll cachedPlayer;

    public override void OnApply(PlayerControll player)
    {
        cachedPlayer = player;
        player.OnBulletSpawned += SubscribeToBullet;
    }

    public override void OnRemove(PlayerControll player)
    {
        player.OnBulletSpawned -= SubscribeToBullet;
        cachedPlayer = null;
    }
    
    private void SubscribeToBullet(Bullet bullet)
    {
        bullet.OnHitEnemy += ApplyBurnEffect;
    }

    private void ApplyBurnEffect(Enemy enemy)
    {
        if (cachedPlayer != null && enemy != null)
        {
            cachedPlayer.StartCoroutine(BurnRoutine(enemy));
        }
    }

    private IEnumerator BurnRoutine(Enemy enemy)
    {
        for (int i = 0; i < burnTimes; i++)
        {
            yield return new WaitForSeconds(waitBetweenBurns);
            
            if (enemy == null) yield break;

            enemy.currentHealth_Enemy -= burnDamage;
            Debug.Log($"Тик огня! Урон: {burnDamage}, HP врага: {enemy.currentHealth_Enemy}");

            if (enemy.currentHealth_Enemy <= 0)
            {
                Destroy(enemy.gameObject);
                yield break;
            }
        }
    }
}