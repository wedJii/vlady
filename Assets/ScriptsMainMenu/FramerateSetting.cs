using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Dropdown))]
public class FramerateSetting : MonoBehaviour
{
    private TMP_Dropdown _dropdown;

    private void Awake()
    {
        _dropdown = GetComponent<TMP_Dropdown>();
        int savedIndex = PlayerPrefs.GetInt("SavedFPSIndex", 3);
        ChangeFrameRate(savedIndex);

        if (_dropdown != null)
        {
            _dropdown.value = savedIndex;
            _dropdown.RefreshShownValue();
            _dropdown.onValueChanged.AddListener(ChangeFrameRate);
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
}
