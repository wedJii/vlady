using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class ExtractionZone : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string targetSceneName = "viborDungeon";
    [SerializeField] private float extractionDuration = 2.5f;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private TextMeshPro statusText;
    [SerializeField] private Color defaultZoneColor = new(0.35f, 0.38f, 0.42f, 0.45f);
    [SerializeField] private Color activeZoneColor = new(0.2f, 0.85f, 0.4f, 0.6f);

    private float _timer;
    private bool _playerInZone;
    private bool _isExtracted;
    private PlayerControll _player;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        spriteRenderer ??= GetComponent<SpriteRenderer>();
        statusText ??= GetComponentInChildren<TextMeshPro>();

        if (spriteRenderer != null)
            spriteRenderer.color = defaultZoneColor;
        if (statusText != null)
            statusText.text = "<b>[ЗОНА ЭВАКУАЦИИ]</b>";
    }

    private void Update()
    {
        if (_isExtracted || !_playerInZone) return;

        _timer -= Time.deltaTime;

        if (statusText != null)
            statusText.text = $"<color=#FFE600><b>ЭВАКУАЦИЯ:</b></color>\n{Mathf.Max(0f, _timer):F1} сек";

        if (_timer <= 0f)
        {
            _isExtracted = true;
            Extract();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isExtracted || !IsPlayer(other)) return;

        _playerInZone = true;
        _timer = extractionDuration;

        if (spriteRenderer != null)
            spriteRenderer.color = activeZoneColor;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_isExtracted || !IsPlayer(other)) return;

        _playerInZone = false;
        _timer = extractionDuration;

        if (spriteRenderer != null)
            spriteRenderer.color = defaultZoneColor;

        if (statusText != null)
            statusText.text = "<b>[ЗОНА ЭВАКУАЦИИ]</b>";
    }

    private bool IsPlayer(Collider2D other) =>
        other.CompareTag("Player") ||
        other.GetComponent<PlayerControll>() != null ||
        other.GetComponentInParent<PlayerControll>() != null ||
        other.name.ToLower().Contains("player");

    public void Extract()
    {
        _player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerControll>() ?? FindFirstObjectByType<PlayerControll>();
        if (_player != null)
            CurrencyManager.SavedCoins += _player.CurrentCoins;

        var inv = (UI_Inventory.Instance != null && UI_Inventory.Instance.Inventory != null)
                  ? UI_Inventory.Instance.Inventory
                  : FindFirstObjectByType<Inventory>();

        if (inv != null)
            RaidLoadoutManager.OnSuccessfulExtraction(inv);

        Time.timeScale = 1f;
        SceneManager.LoadScene(targetSceneName);
    }
}
