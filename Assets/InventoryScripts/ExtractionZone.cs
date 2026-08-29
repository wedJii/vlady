using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class ExtractionZone : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "viborDungeon";

    private bool _isExtracting = false;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isExtracting) return;
        if (!other.CompareTag("Player") && other.GetComponent<PlayerControll>() == null && !other.name.ToLower().Contains("player")) return;

        _isExtracting = true;
        Extract();
    }

    public void Extract()
    {
        var inv = FindFirstObjectByType<Inventory>();
        if (inv != null)
            RaidLoadoutManager.SaveDungeonLoot(inv);

        Time.timeScale = 1f;
        SceneManager.LoadScene(targetSceneName);
    }
}
