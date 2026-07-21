using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string _playSceneName = "SampleScene";
    [SerializeField] private GameObject _settingsPanel;

    public void OnPlayClick()
    {
        SceneManager.LoadScene(_playSceneName);
    }

    public void OnSettingsClick()
    {
        if (_settingsPanel != null)
            _settingsPanel.SetActive(true);
    }

    public void OnCloseSettingsClick()
    {
        if (_settingsPanel != null)
            _settingsPanel.SetActive(false);
    }

    public void OnExitClick()
    {
        Application.Quit();
    }
}
