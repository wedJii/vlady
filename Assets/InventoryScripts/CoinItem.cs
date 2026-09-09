using UnityEngine;

[CreateAssetMenu(fileName = "CoinItem", menuName = "Inventory/Coin Item")]
public class CoinItem : Item
{
    private void OnValidate()
    {
        id = "coin";
        if (string.IsNullOrEmpty(displayName))
            displayName = "Монеты";
        isStackable = true;
        maxStack = 999;
    }
}
