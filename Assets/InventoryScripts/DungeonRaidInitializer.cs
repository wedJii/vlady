using UnityEngine;

public class DungeonRaidInitializer : MonoBehaviour
{
    [SerializeField] private Inventory dungeonInventory;

    private void Start()
    {
        if (dungeonInventory == null)
            dungeonInventory = GetComponent<Inventory>() ?? FindFirstObjectByType<Inventory>();

        if (dungeonInventory != null)
        {
            RaidLoadoutManager.ApplyToDungeon(dungeonInventory);
            UI_Inventory.Instance?.UpdateUI();
        }
    }

    public void SaveCurrentLoot()
    {
        if (dungeonInventory != null)
            RaidLoadoutManager.SaveDungeonLoot(dungeonInventory);
    }
}
