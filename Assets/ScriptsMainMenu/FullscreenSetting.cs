using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class FullscreenSetting : MonoBehaviour
{
    private Toggle _toggle;

    private void Awake()
    {
        _toggle = GetComponent<Toggle>();
        bool isFull = PlayerPrefs.GetInt("SavedFullscreen", Screen.fullScreen ? 1 : 0) == 1;
        
        Screen.fullScreen = isFull;
        _toggle.isOn = isFull;
        
        _toggle.onValueChanged.AddListener(SetFullscreen);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("SavedFullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }
}
