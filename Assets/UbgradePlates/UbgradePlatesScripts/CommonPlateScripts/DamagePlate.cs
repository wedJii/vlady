using UnityEngine;

[CreateAssetMenu(fileName = "Damage Plate", menuName = "Inventory/Plates/Damage Plate")]
public class DamagePlate : UbgradePlate
{
    [SerializeField] private float bonusDamage = 5;
    public override void OnApply(PlayerControll player)
    {
        player.bulletDamage += bonusDamage;
    }

    public override void OnRemove(PlayerControll player)
    {
        player.bulletDamage -= bonusDamage;
    }
}
