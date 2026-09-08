using UnityEngine;
[CreateAssetMenu(fileName = "Heal Plate", menuName = "Inventory/Plates/Heal Plate")]
public class HealingPlate : UbgradePlate
{
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
        bullet.OnHitEnemy += HealingAffect;
    }
    
    private void HealingAffect(Enemy enemy)
    {
        if (cachedPlayer != null)
        {
            cachedPlayer.currentHealth_Player = Mathf.Min(cachedPlayer.currentHealth_Player + 5f, cachedPlayer.maxHealth_Player);
        }
    }
}
