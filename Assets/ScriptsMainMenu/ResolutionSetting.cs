using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Dropdown))]
public class ResolutionSetting : MonoBehaviour
{
    private TMP_Dropdown _dropdown;
    private Resolution[] _resolutions;

    private void Awake()
    {
        _dropdown = GetComponent<TMP_Dropdown>();
        _resolutions = Screen.resolutions;

        _dropdown.ClearOptions();
        var options = new List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < _resolutions.Length; i++)
        {
            options.Add($"{_resolutions[i].width} x {_resolutions[i].height}");
            if (_resolutions[i].width == Screen.currentResolution.width &&
                _resolutions[i].height == Screen.currentResolution.height)
            {
                currentResIndex = i;
            }
        }

        int savedIndex = PlayerPrefs.GetInt("SavedResolutionIndex", currentResIndex);
        _dropdown.AddOptions(options);
        _dropdown.value = savedIndex;
        _dropdown.RefreshShownValue();

        _dropdown.onValueChanged.AddListener(SetResolution);
    }

    public void SetResolution(int index)
    {
        if (_resolutions == null || index < 0 || index >= _resolutions.Length) return;
        Resolution res = _resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        PlayerPrefs.SetInt("SavedResolutionIndex", index);
        PlayerPrefs.Save();
    }
}
