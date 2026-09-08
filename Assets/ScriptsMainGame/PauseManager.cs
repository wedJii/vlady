using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _gameUI;
    [SerializeField] private GameObject _losePanel;
    [SerializeField] private GameObject _settingsPanel;
    private bool _openedPauseMenu;

    public bool IsPauseActive => _openedPauseMenu;

    private void Start()
    {
        Time.timeScale = 1f;
        ClosePauseMenu();
    }

    private void Update()
    {
        if (_losePanel != null && _losePanel.activeSelf) return;

        bool escPressed = false;
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            escPressed = true;

        if (!escPressed)
        {
            try
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    escPressed = true;
            }
            catch { }
        }

        if (escPressed)
        {
            if (Time.unscaledTime - UI_Chest.LastCloseTime < 0.15f)
            {
                return;
            }

            var chestUI = UI_Chest.Instance ?? FindFirstObjectByType<UI_Chest>(FindObjectsInactive.Include);
            if (chestUI != null && chestUI.IsOpen)
            {
                chestUI.Close();
                return;
            }

            var invUI = UI_Inventory.Instance ?? FindFirstObjectByType<UI_Inventory>(FindObjectsInactive.Include);
            if (invUI != null && invUI.IsOpen)
            {
                invUI.Close();
                return;
            }

            if (_settingsPanel != null && _settingsPanel.activeSelf)
            {
                CloseSettings();
            }
            else if (_openedPauseMenu)
            {
                ClosePauseMenu();
            }
            else
            {
                OpenPauseMenu();
            }
        }
    }

    public void OpenPauseMenu()
    {
        if (_losePanel != null && _losePanel.activeSelf) return;

        if (_settingsPanel != null) _settingsPanel.SetActive(false);
        if (_pauseMenu != null) _pauseMenu.SetActive(true);
        if (_gameUI != null) _gameUI.SetActive(false);
        _openedPauseMenu = true;
        Time.timeScale = 0f;
    }

    public void ClosePauseMenu()
    {
        if (_settingsPanel != null) _settingsPanel.SetActive(false);
        if (_pauseMenu != null) _pauseMenu.SetActive(false);
        if (_gameUI != null && (_losePanel == null || !_losePanel.activeSelf)) _gameUI.SetActive(true);
        _openedPauseMenu = false;
        Time.timeScale = 1f;
    }

    public void OpenSettings()
    {
        if (_settingsPanel != null) _settingsPanel.SetActive(true);
        if (_pauseMenu != null) _pauseMenu.SetActive(false);
    }

    public void CloseSettings()
    {
        if (_settingsPanel != null) _settingsPanel.SetActive(false);
        if (_pauseMenu != null) _pauseMenu.SetActive(true);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        RaidLoadoutManager.OnPlayerDied();
        SceneManager.LoadScene("MainMenu");
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        RaidLoadoutManager.OnRaidAbandoned();
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        RaidLoadoutManager.OnRaidAbandoned();
        Application.Quit();
    }
}
