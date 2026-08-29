using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [SerializeField] private Item item;
    [SerializeField] private int amount = 1;
    [SerializeField] private string customPickupID;
    [SerializeField] private bool bobbingAnimation = true;

    private Vector3 _startPos;
    private SpriteRenderer _spriteRenderer;
    private bool _isPickedUp = false;

    public string UniqueID
    {
        get
        {
            if (string.IsNullOrEmpty(customPickupID))
                customPickupID = $"{gameObject.scene.name}_{gameObject.name}_{transform.position.x:F1}_{transform.position.y:F1}";
            return customPickupID;
        }
    }

    private void Awake()
    {
        _startPos = transform.position;
        _spriteRenderer = GetComponent<SpriteRenderer>();

        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        // Если этот предмет уже был экстракнут — уничтожаем его сразу
        if (RaidLoadoutManager.IsWorldItemCollected(UniqueID))
        {
            Destroy(gameObject);
            return;
        }

        UpdateVisuals();
    }

    private void Update()
    {
        if (bobbingAnimation && !_isPickedUp)
        {
            float yOffset = Mathf.Sin(Time.time * 3f) * 0.08f;
            transform.position = _startPos + new Vector3(0f, yOffset, 0f);
        }
    }

    private void UpdateVisuals()
    {
        if (_spriteRenderer != null && item != null && item.icon != null)
        {
            _spriteRenderer.sprite = item.icon;
        }
    }

    private void TryPickup(GameObject target)
    {
        if (_isPickedUp) return;

        bool isPlayer = target.CompareTag("Player") || 
                        target.GetComponent<PlayerControll>() != null || 
                        target.GetComponentInParent<PlayerControll>() != null ||
                        target.name.ToLower().Contains("player");

        if (!isPlayer) return;

        if (item == null)
        {
            Debug.LogError($"<color=red>[ItemPickup]</color> На объекте {gameObject.name} в поле 'Item' не назначен предмет!", gameObject);
            return;
        }

        var inventory = target.GetComponent<Inventory>() ?? 
                        target.GetComponentInParent<Inventory>() ?? 
                        UI_Inventory.Instance?.Inventory ?? 
                        FindFirstObjectByType<Inventory>();

        if (inventory == null)
        {
            Debug.LogError("<color=red>[ItemPickup]</color> В сцене не найден компонент Inventory!", gameObject);
            return;
        }

        if (inventory.AddItem(item, amount))
        {
            _isPickedUp = true;
            RaidLoadoutManager.MarkWorldItemCollected(UniqueID);
            UI_Inventory.Instance?.UpdateUI();
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("<color=yellow>[ItemPickup]</color> Инвентарь полон!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other) => TryPickup(other.gameObject);
    private void OnTriggerStay2D(Collider2D other) => TryPickup(other.gameObject);
    private void OnCollisionEnter2D(Collision2D collision) => TryPickup(collision.gameObject);

    private void OnValidate()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisuals();
    }
}
