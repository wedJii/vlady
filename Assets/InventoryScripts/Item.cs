using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;         
    public bool isStackable;
    public int maxStack;   
}