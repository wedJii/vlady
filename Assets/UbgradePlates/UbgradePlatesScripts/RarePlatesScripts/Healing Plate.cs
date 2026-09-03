using UnityEngine;
[CreateAssetMenu(fileName = "Heal Plate", menuName = "Inventory/Plates/Heal Plate")]
public class NewMonoBehaviourScript : UbgradePlate
{
    PlayerControll cashedPlayer;
    public override void OnApply(PlayerControll player)
    {
        cashedPlayer = player;
        player.OnBulletSpawned += SubscribeToBullet;
    }

    public override void OnRemove(PlayerControll player)
    {
        player.OnBulletSpawned -= SubscribeToBullet;
    }
    
    private void SubscribeToBullet(Bullet bullet)
    {
        bullet.OnHitEnemy += HealingAffect;
    }
    
    private void HealingAffect(Enemy enemy)
    {
        cashedPlayer.currentHealth_Player += 5f;
    }
}
