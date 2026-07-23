using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string _playSceneName = "SampleScene";
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private TMP_Dropdown _fpsDropdown;

    private void Start()
    {
        int savedIndex = PlayerPrefs.GetInt("SavedFPSIndex", 3);

        ChangeFrameRate(savedIndex);
        
        if (_fpsDropdown != null)
        {
            _fpsDropdown.value = savedIndex;
            _fpsDropdown.RefreshShownValue();
        }
    }

    public void ChangeFrameRate(int index)
    {
        QualitySettings.vSyncCount = 0;
        
        switch (index)
        {
            case 0: Application.targetFrameRate = 60; break;
            case 1: Application.targetFrameRate = 144; break;
            case 2: Application.targetFrameRate = 240; break;
            case 3: Application.targetFrameRate = -1; break;
        }
        
        PlayerPrefs.SetInt("SavedFPSIndex", index);
        PlayerPrefs.Save();
    }

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