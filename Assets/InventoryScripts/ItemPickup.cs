using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [SerializeField] private Item _item;
    [SerializeField] private int _amount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var inventory = other.GetComponent<Inventory>() ?? FindFirstObjectByType<Inventory>();
            if (inventory != null && inventory.AddItem(_item, _amount))
            {
                Destroy(gameObject);
            }
        }
    }
}
