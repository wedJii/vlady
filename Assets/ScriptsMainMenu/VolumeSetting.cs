using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class VolumeSetting : MonoBehaviour
{
    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
        float volume = PlayerPrefs.GetFloat("SavedVolume", 1f);
        
        AudioListener.volume = volume;
        _slider.value = volume;
        
        _slider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("SavedVolume", volume);
        PlayerPrefs.Save();
    }
}
