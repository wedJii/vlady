using UnityEngine;

public abstract class UbgradePlate : Item
{
    public string PlateRarity;

    public PlateRarity RarityTier => System.Enum.TryParse<PlateRarity>(PlateRarity, true, out var r) ? r : global::PlateRarity.Common;
    private void OnValidate()
    {
        isStackable = false;
        maxStack = 1;
    }
    
    public abstract override void OnApply(PlayerControll player);
    public abstract override void OnRemove(PlayerControll player);
    
}
