using UnityEngine;
[CreateAssetMenu(fileName = "Health Plate", menuName = "Inventory/Plates/Health Plate")]
public class HealthPlate : UbgradePlate
{
    [SerializeField] private float bonusHealth = 50;
    
    public override void OnApply(PlayerControll player)
    {
        player.maxHealth_Player += bonusHealth;
    }

    public override void OnRemove(PlayerControll player)
    {
        player.maxHealth_Player -= bonusHealth;
    }
}
