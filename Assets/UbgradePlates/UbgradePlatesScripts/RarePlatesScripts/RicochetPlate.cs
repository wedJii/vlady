using UnityEngine;

[CreateAssetMenu(fileName = "Ricochet Plate", menuName = "Inventory/Plates/Ricochet Plate")]
public class RicochetPlate : UbgradePlate
{
    [SerializeField] private int extraBounces = 2;

    public override void OnApply(PlayerControll player)
    {
        player.OnBulletSpawned += ApplyRicochet;
    }

    public override void OnRemove(PlayerControll player)
    {
        player.OnBulletSpawned -= ApplyRicochet;
    }

    private void ApplyRicochet(Bullet bullet)
    {
        bullet.isRicochet = true;
        if (bullet.TryGetComponent(out RicochetModifier existingModifier))
        {
            existingModifier.bounceCount += extraBounces;
        }
        else
        {
            RicochetModifier newModifier = bullet.gameObject.AddComponent<RicochetModifier>();
            newModifier.bounceCount = extraBounces;
        }
    }
}