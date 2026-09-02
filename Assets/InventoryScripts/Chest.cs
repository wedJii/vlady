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
    [Header("Chest Configuration")]
    [SerializeField] private int capacity = 15;
    [SerializeField] private float interactionRadius = 2.8f;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;

    [Header("Starter Items (Optional, not random)")]
    [SerializeField] private List<LobbyMenuManager.StarterItemEntry> starterItems = new();

    [Header("Visual Feedback")]
    [SerializeField] private Sprite closedSprite;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private GameObject interactionPrompt;

    private Inventory _inventory;
    private SpriteRenderer _sr;
    private Animator _animator;
    private PlayerControll _player;
    private bool _isOpen;
    private bool _isPlayerNear;
    private bool _initialized;

    private static readonly int IsOpenHash = Animator.StringToHash("IsOpen");

    public Inventory ChestInventory => _inventory;
    public bool IsOpen => _isOpen;
    public int Capacity => capacity;

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
    }

    private void Start()
    {
        InitStarterItems();
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
        if (interactionPrompt != null) return;

        var promptGo = new GameObject("InteractionPrompt", typeof(TextMeshPro));
        promptGo.transform.SetParent(transform, false);
        promptGo.transform.localPosition = new Vector3(0f, 0.75f, 0f);

        var tmp = promptGo.GetComponent<TextMeshPro>();
        tmp.text = "<color=#FFEAA7>[<b>E</b>]</color> Открыть";
        tmp.fontSize = 3.5f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.sortingOrder = 100;

        interactionPrompt = promptGo;
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
            // Когда сундук открыт, закрытием по E/Esc/Tab управляет UI_Chest.
            // Сундук лишь проверяет, не отошел ли игрок слишком далеко.
            if (dist > interactionRadius + 1.5f)
            {
                CloseChest();
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

            if (interactPressed)
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
        _isOpen = true;
        SetPromptActive(false);
        if (_animator != null)
            _animator.SetBool(IsOpenHash, true);
        else if (openSprite != null && _sr != null)
            _sr.sprite = openSprite;
    }

    public void OnClosed()
    {
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
