using UnityEngine;

public class DungeonRaidInitializer : MonoBehaviour
{
    [SerializeField] private Inventory dungeonInventory;

    private void Start()
    {
        if (dungeonInventory == null)
            dungeonInventory = GetComponent<Inventory>() ?? FindFirstObjectByType<Inventory>();

        var player = FindFirstObjectByType<PlayerControll>();

        if (dungeonInventory != null)
        {
            dungeonInventory.capacity = 15;
            dungeonInventory.plateCapacity = 3;

            // Загружаем только реальные вещи и пластины, которые игрок взял с собой в рейд
            RaidLoadoutManager.ApplyToDungeon(dungeonInventory, player);

            UI_Inventory.Instance?.UpdateUI();
        }
    }

    public void SaveCurrentLoot()
    {
        if (dungeonInventory != null)
            RaidLoadoutManager.OnSuccessfulExtraction(dungeonInventory);
    }
}
