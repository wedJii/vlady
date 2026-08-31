using UnityEngine;

[CreateAssetMenu(fileName = "Speed Plate", menuName = "Inventory/Plates/Speed Plate")]
public class SpeedPlateScript : UbgradePlate
{ 
    private float bonusSpeed = 5f;
    
    public override void OnApply(PlayerControll player)
    {
        player.speed += bonusSpeed;
    }

    public override void OnRemove(PlayerControll player)
    {
        player.speed -= bonusSpeed;
    }
}
