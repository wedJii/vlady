using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(Inventory))]
[RequireComponent(typeof(Collider2D))]
public class Chest : MonoBehaviour
{
    [Header("Настройки сундука")]
    [SerializeField] private int capacity = 15;
    [SerializeField] private float interactionRadius = 2.8f;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("Стартовые вещи")]
    [SerializeField] private List<LobbyMenuManager.StarterItemEntry> starterItems = new();

    [Header("Внешний вид")]
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private GameObject interactionPrompt;

    [Header("Настройки лута")]
    [Tooltip("Спавнить вещи на старте")]
    [SerializeField] private bool enableRandomLoot = true;

    [Tooltip("Макс предметов")]
    [SerializeField, Min(1)] private int maxLootItems = 2;

    [Tooltip("Шанс на след предмет")]
    [SerializeField, Range(0f, 100f)] private float nextItemChance = 20f;

    [Header("Шанс 50 на 50")]
    [Tooltip("Шанс на пластину")]
    [SerializeField, Range(0f, 100f)] private float plateChance = 50f;

    [Tooltip("Шанс на монеты")]
    [SerializeField, Range(0f, 100f)] private float coinChance = 50f;

    [Header("Шансы пластин")]
    [Tooltip("Обычная (80%)")]
    [SerializeField, Range(0f, 100f)] private float commonPlateChance = 80f;

    [Tooltip("Редкая (17%)")]
    [SerializeField, Range(0f, 100f)] private float rarePlateChance = 17f;

    [Tooltip("Легендарка (2.5%)")]
    [SerializeField, Range(0f, 100f)] private float legendaryPlateChance = 2.5f;

    [Tooltip("Мифик (0.5%)")]
    [SerializeField, Range(0f, 100f)] private float mythicPlateChance = 0.5f;

    [Header("Настройки монет")]
    [Tooltip("Мин монет")]
    [SerializeField, Min(1)] private int minCoins = 5;

    [Tooltip("Макс монет")]
    [SerializeField, Min(1)] private int maxCoins = 10;

    [Tooltip("Предмет монеты")]
    [SerializeField] private Item coinItem;

    [Header("Списки пластин")]
    [SerializeField] private List<UbgradePlate> commonPlates = new();
    [SerializeField] private List<UbgradePlate> rarePlates = new();
    [SerializeField] private List<UbgradePlate> legendaryPlates = new();
    [SerializeField] private List<UbgradePlate> mythicPlates = new();

    private Inventory _inventory;
    private SpriteRenderer _sr;
    private Animator _animator;
    private PlayerControll _player;
    private bool _isOpen;
    private bool _isPlayerNear;
    private bool _initialized;
    private float _lastInteractTime;

    private static readonly int IsOpenHash = Animator.StringToHash("IsOpen");

    public Inventory ChestInventory => _inventory;
    public bool IsOpen => _isOpen;
    public int Capacity => capacity;

    private void OnValidate()
    {
        if (minCoins < 1) minCoins = 1;
        if (maxCoins < minCoins) maxCoins = minCoins;

#if UNITY_EDITOR
        if (!Application.isPlaying && commonPlates.Count == 0 && rarePlates.Count == 0)
        {
            AutoPopulatePlates();
        }
#endif
    }

#if UNITY_EDITOR
    [ContextMenu("Найти пластины")]
    public void AutoPopulatePlates()
    {
        commonPlates.Clear();
        rarePlates.Clear();
        legendaryPlates.Clear();
        mythicPlates.Clear();

        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:UbgradePlate");
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var plate = UnityEditor.AssetDatabase.LoadAssetAtPath<UbgradePlate>(path);
            if (plate == null) continue;

            string rarityStr = plate.PlateRarity?.Trim().ToLowerInvariant() ?? "";
            if (rarityStr.Contains("common")) commonPlates.Add(plate);
            else if (rarityStr.Contains("rare")) rarePlates.Add(plate);
            else if (rarityStr.Contains("legend")) legendaryPlates.Add(plate);
            else if (rarityStr.Contains("mythic")) mythicPlates.Add(plate);
            else commonPlates.Add(plate);
        }

        if (coinItem == null)
        {
            string[] coinGuids = UnityEditor.AssetDatabase.FindAssets("t:CoinItem");
            if (coinGuids.Length > 0)
            {
                string cPath = UnityEditor.AssetDatabase.GUIDToAssetPath(coinGuids[0]);
                coinItem = UnityEditor.AssetDatabase.LoadAssetAtPath<Item>(cPath);
            }
        }

        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif

    private void Awake()
    {
        _inventory = GetComponent<Inventory>() ?? gameObject.AddComponent<Inventory>();
        _inventory.capacity = capacity;
        _inventory.plateCapacity = 0;
        _inventory.EnsureCapacity();
        _inventory.EnsurePlateCapacity();

        _sr = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        if (_animator == null && closedSprite != null && _sr != null)
            _sr.sprite = closedSprite;

        EnsurePrompt();

        if (enableRandomLoot)
        {
            GenerateRandomLoot();
        }
    }

    private void Start()
    {
        if (!enableRandomLoot)
            InitStarterItems();

        if (_sr != null)
            _sr.sortingOrder = 1000 + Mathf.RoundToInt(-transform.position.y * 100);
    }

    public void GenerateRandomLoot()
    {
        if (_initialized) return;
        _initialized = true;

        if (_inventory == null)
        {
            _inventory = GetComponent<Inventory>() ?? gameObject.AddComponent<Inventory>();
            _inventory.capacity = capacity;
            _inventory.EnsureCapacity();
        }

        if (maxLootItems <= 0) return;

        int spawnedCount = 0;

        SpawnSingleLootItem();
        spawnedCount++;

        while (spawnedCount < maxLootItems)
        {
            float roll = Random.Range(0f, 100f);
            if (roll <= nextItemChance)
            {
                SpawnSingleLootItem();
                spawnedCount++;
            }
            else
            {
                break;
            }
        }
    }

    private void SpawnSingleLootItem()
    {
        float totalTypeChance = plateChance + coinChance;
        if (totalTypeChance <= 0f) return;

        float rollType = Random.Range(0f, totalTypeChance);
        if (rollType < plateChance)
        {
            SpawnPlate();
        }
        else
        {
            SpawnCoins();
        }
    }

    private void SpawnPlate()
    {
        PlateRarity selectedRarity = RollRarity();
        UbgradePlate plate = GetPlateByRarityWithFallback(selectedRarity);

        if (plate != null)
        {
            _inventory.AddItem(plate, 1);
        }
    }

    private PlateRarity RollRarity()
    {
        float totalRarity = commonPlateChance + rarePlateChance + legendaryPlateChance + mythicPlateChance;
        if (totalRarity <= 0f) totalRarity = 100f;

        float roll = Random.Range(0f, totalRarity);

        if (roll < commonPlateChance)
            return PlateRarity.Common;
        roll -= commonPlateChance;

        if (roll < rarePlateChance)
            return PlateRarity.Rare;
        roll -= rarePlateChance;

        if (roll < legendaryPlateChance)
            return PlateRarity.Legendary;

        return PlateRarity.Mythic;
    }

    private UbgradePlate GetPlateByRarityWithFallback(PlateRarity rarity)
    {
        List<UbgradePlate> pool = GetPoolForRarity(rarity);

        if (pool == null || pool.Count == 0)
        {
            if (rarity == PlateRarity.Mythic)
                pool = GetPoolForRarity(PlateRarity.Legendary);

            if ((pool == null || pool.Count == 0) && (rarity == PlateRarity.Mythic || rarity == PlateRarity.Legendary))
                pool = GetPoolForRarity(PlateRarity.Rare);

            if (pool == null || pool.Count == 0)
                pool = GetPoolForRarity(PlateRarity.Common);
        }

        if (pool != null && pool.Count > 0)
        {
            var validPlates = pool.FindAll(p => p != null);
            if (validPlates.Count > 0)
            {
                int index = Random.Range(0, validPlates.Count);
                return validPlates[index];
            }
        }

        return null;
    }

    private List<UbgradePlate> GetPoolForRarity(PlateRarity rarity)
    {
        return rarity switch
        {
            PlateRarity.Common => commonPlates,
            PlateRarity.Rare => rarePlates,
            PlateRarity.Legendary => legendaryPlates,
            PlateRarity.Mythic => mythicPlates,
            _ => commonPlates
        };
    }

    private void SpawnCoins()
    {
        if (coinItem == null)
        {
            Debug.LogWarning("Не выбрана монета!");
            return;
        }

        int amount = Random.Range(minCoins, maxCoins + 1);
        _inventory.AddItem(coinItem, amount);
    }

    private void InitStarterItems()
    {
        if (_initialized || starterItems == null || starterItems.Count == 0) return;
        _initialized = true;

        bool hasItems = false;
        foreach (var s in _inventory.Slots)
        {
            if (s != null && !s.IsEmpty) { hasItems = true; break; }
        }

        if (!hasItems)
        {
            foreach (var entry in starterItems)
            {
                if (entry.item != null && entry.amount > 0)
                    _inventory.AddItem(entry.item, entry.amount);
            }
        }
    }

    private void EnsurePrompt()
    {
        if (interactionPrompt == null)
            interactionPrompt = transform.Find("InteractionPrompt")?.gameObject;

        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }

    private void SetPromptActive(bool active)
    {
        if (interactionPrompt != null && interactionPrompt.activeSelf != active)
            interactionPrompt.SetActive(active);
    }

    private void Update()
    {
        if (_player == null)
            _player = FindFirstObjectByType<PlayerControll>();

        if (_player == null) return;

        float dist = Vector2.Distance(transform.position, _player.transform.position);

        if (_isOpen)
        {
            if (dist > interactionRadius + 1.5f)
            {
                CloseChest();
                return;
            }

            bool closeInteract = false;
            if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame))
                closeInteract = true;

            if (!closeInteract)
            {
                try
                {
                    if (Input.GetKeyDown(interactionKey) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
                        closeInteract = true;
                }
                catch { }
            }

            if (closeInteract && (Time.unscaledTime - _lastInteractTime >= 0.2f))
            {
                CloseChest();
                return;
            }

            return;
        }

        if (dist <= interactionRadius)
        {
            if (!_isPlayerNear)
            {
                _isPlayerNear = true;
                SetPromptActive(true);
            }

            bool interactPressed = false;
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                interactPressed = true;

            if (!interactPressed)
            {
                try
                {
                    if (Input.GetKeyDown(interactionKey) || Input.GetKeyDown(KeyCode.E))
                        interactPressed = true;
                }
                catch { }
            }

            if (interactPressed && (Time.unscaledTime - _lastInteractTime >= 0.25f) && (Time.unscaledTime - UI_Chest.LastCloseTime >= 0.25f))
            {
                OpenChest();
            }
        }
        else
        {
            if (_isPlayerNear)
            {
                _isPlayerNear = false;
                SetPromptActive(false);
            }
        }
    }

    private void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        if (_player != null && Vector2.Distance(transform.position, _player.transform.position) <= interactionRadius)
        {
            if (_isOpen) CloseChest();
            else OpenChest();
        }
    }

    public void OpenChest()
    {
        if (_isOpen) return;
        _lastInteractTime = Time.unscaledTime;
        _isOpen = true;

        SetPromptActive(false);

        if (_animator != null)
        {
            _animator.SetBool(IsOpenHash, true);
        }
        else
        {
            if (openSprite != null && _sr != null)
                _sr.sprite = openSprite;

            transform.DOKill();
            transform.DOPunchScale(new Vector3(0.12f, 0.12f, 0f), 0.2f).SetUpdate(true);
        }

        var ui = UI_Chest.GetOrCreate();
        ui?.Open(this);
    }

    public void CloseChest()
    {
        if (!_isOpen) return;
        _lastInteractTime = Time.unscaledTime;
        _isOpen = false;

        if (_animator != null)
        {
            _animator.SetBool(IsOpenHash, false);
        }
        else
        {
            if (closedSprite != null && _sr != null)
                _sr.sprite = closedSprite;
        }

        if (_isPlayerNear)
            SetPromptActive(true);

        var ui = UI_Chest.Instance;
        if (ui != null && ui.IsOpen && ui.CurrentChest == this)
            ui.Close();
    }

    public void OnOpened()
    {
        _lastInteractTime = Time.unscaledTime;
        _isOpen = true;
        SetPromptActive(false);
        if (_animator != null)
            _animator.SetBool(IsOpenHash, true);
        else if (openSprite != null && _sr != null)
            _sr.sprite = openSprite;
    }

    public void OnClosed()
    {
        _lastInteractTime = Time.unscaledTime;
        _isOpen = false;
        if (_animator != null)
            _animator.SetBool(IsOpenHash, false);
        else if (closedSprite != null && _sr != null)
            _sr.sprite = closedSprite;

        if (_isPlayerNear)
            SetPromptActive(true);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}