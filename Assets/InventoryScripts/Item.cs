using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;
    [TextArea(2, 4)] public string description;
    public bool isStackable = true;
    public int maxStack = 99;
}