using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingMenuController : MonoBehaviour
{
    [SerializeField] private AudioMixerGroup _masterMixer;
    [SerializeField] private Slider _volumeSlider;
    [SerializeField] private GameObject _deaf;
    [SerializeField] private string _nameVolume = "MasterVolume";

    private void OnEnable()
    {
        if (_masterMixer.audioMixer.GetFloat(_nameVolume, out var tmp))
        {
            _deaf.SetActive(tmp == _volumeSlider.minValue);
            _volumeSlider.value = tmp;
        }
        _volumeSlider.onValueChanged.AddListener(OnVolume);
    }

    private void OnDisable()
    {
        _volumeSlider.onValueChanged.RemoveListener(OnVolume);
    }

    private void OnVolume(float value)
    {
        _deaf.SetActive(value == _volumeSlider.minValue);
        _masterMixer.audioMixer.SetFloat(_nameVolume, value);
    }
}
