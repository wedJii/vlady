using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject _pauseMenu;
    [SerializeField] private GameObject _gameUI;
    [SerializeField] private GameObject _losePanel;
    private bool _openedPauseMenu;

    private void Start()
    {
        Time.timeScale = 1f;
        ClosePauseMenu();
    }

    private void Update()
    {
        if (_losePanel != null && _losePanel.activeSelf) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (_openedPauseMenu)
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

        if (_pauseMenu != null) _pauseMenu.SetActive(true);
        if (_gameUI != null) _gameUI.SetActive(false);
        _openedPauseMenu = true;
        Time.timeScale = 0f;
    }

    public void ClosePauseMenu()
    {
        if (_pauseMenu != null) _pauseMenu.SetActive(false);
        if (_gameUI != null && (_losePanel == null || !_losePanel.activeSelf)) _gameUI.SetActive(true);
        _openedPauseMenu = false;
        Time.timeScale = 1f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
