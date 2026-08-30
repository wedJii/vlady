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
    [SerializeField] private Color defaultZoneColor = new(0.35f, 0.38f, 0.42f, 0.45f);
    [SerializeField] private Color activeZoneColor = new(0.2f, 0.85f, 0.4f, 0.6f);
    [SerializeField] private Vector2 zoneSize = new(3f, 3f);

    private float _timer;
    private bool _playerInZone;
    private bool _isExtracted;
    private SpriteRenderer _spriteRenderer;
    private TextMeshPro _statusText;
    
    private PlayerControll _player;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        EnsureVisuals();
    }

    private void EnsureVisuals()
    {
        var boxCol = GetComponent<BoxCollider2D>();
        if (boxCol != null)
            boxCol.size = zoneSize;

        // Создаем серый визуальный квадрат зоны эвакуации
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
            _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        if (_spriteRenderer.sprite == null)
        {
            var tex = Texture2D.whiteTexture;
            _spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 1f);
        }

        _spriteRenderer.color = defaultZoneColor;
        _spriteRenderer.sortingOrder = -1; // Лежит на полу под персонажем
        _spriteRenderer.drawMode = SpriteDrawMode.Sliced;
        _spriteRenderer.size = zoneSize;

        // Текст над зоной
        var textObj = transform.Find("ExtractText");
        if (textObj == null)
        {
            var go = new GameObject("ExtractText", typeof(TextMeshPro));
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0, (zoneSize.y / 2f) + 0.4f, 0);
            _statusText = go.GetComponent<TextMeshPro>();
            _statusText.fontSize = 3.5f;
            _statusText.alignment = TextAlignmentOptions.Center;
            _statusText.color = new Color(0.85f, 0.9f, 0.95f, 1f);
            _statusText.text = "<b>[ЗОНА ЭВАКУАЦИИ]</b>";
        }
        else
        {
            _statusText = textObj.GetComponent<TextMeshPro>();
        }
    }

    private void Update()
    {
        if (_isExtracted || !_playerInZone) return;

        _timer -= Time.deltaTime;

        if (_statusText != null)
        {
            _statusText.text = $"<color=#FFE600><b>ЭВАКУАЦИЯ:</b></color>\n{Mathf.Max(0f, _timer):F1} сек";
        }

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

        if (_spriteRenderer != null)
            _spriteRenderer.color = activeZoneColor;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_isExtracted || !IsPlayer(other)) return;

        _playerInZone = false;
        _timer = extractionDuration;

        if (_spriteRenderer != null)
            _spriteRenderer.color = defaultZoneColor;

        if (_statusText != null)
        {
            _statusText.text = "<b>[ЗОНА ЭВАКУАЦИИ]</b>";
        }
    }

    private bool IsPlayer(Collider2D other) =>
        other.CompareTag("Player") || 
        other.GetComponent<PlayerControll>() != null || 
        other.GetComponentInParent<PlayerControll>() != null ||
        other.name.ToLower().Contains("player");

    public void Extract()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerControll>();
        CurrencyManager.SavedCoins += _player.CurrentCoins;
        
        var inv = FindFirstObjectByType<Inventory>();
        if (inv != null)
        {
            RaidLoadoutManager.OnSuccessfulExtraction(inv);
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(targetSceneName);
    }
}
