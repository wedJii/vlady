using UnityEngine;

[CreateAssetMenu(fileName = "Speed Plate", menuName = "Inventory/Plates/Speed Plate")]
public class SpeedPlateScript : UbgradePlate
{ 
    [Header("Speed Plate Settings")]
    [SerializeField] private float bonusSpeed = 5f;
    
    public override void OnApply(PlayerControll player)
    {
        if (player == null) return;
        player.speed = player.basicSpeed + bonusSpeed;
        Debug.Log($"<color=cyan>[SpeedPlate]</color> Пластина скорости применена! Скорость: {player.speed}");
    }

    public override void OnRemove(PlayerControll player)
    {
        if (player == null) return;
        player.speed = player.basicSpeed;
        Debug.Log($"<color=orange>[SpeedPlate]</color> Пластина скорости снята! Скорость: {player.speed}");
    }
}
