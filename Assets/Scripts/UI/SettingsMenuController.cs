namespace GoblinzMechanics.Game
{
    using UnityEngine;
    using UnityEngine.Audio;
    using UnityEngine.UI;

    public class SettingMenuController : MonoBehaviour
    {
        [SerializeField] private AudioMixerGroup _masterMixer;
        [SerializeField] private Slider _volumeSlider;
        [SerializeField] private Slider _brightnessSlider;
        [SerializeField] private Slider _contrastSlider;
        [SerializeField] private GameObject _deaf;
        [SerializeField] private GameObject _blind;
        [SerializeField] private GameObject _birdEye;
        [SerializeField] private string _nameVolume = "MasterVolume";
        [SerializeField] private CameraSettings cameraSettings;

        private void OnEnable()
        {
            if (_masterMixer.audioMixer.GetFloat(_nameVolume, out var tmp))
            {
                _deaf.SetActive(tmp == _volumeSlider.minValue);
                _volumeSlider.value = tmp;
            }
            _blind.SetActive(cameraSettings.Brightness == _brightnessSlider.minValue);
            _birdEye.SetActive(cameraSettings.Contrast == _contrastSlider.maxValue);

            _volumeSlider.onValueChanged.AddListener(OnVolume);
            _brightnessSlider.onValueChanged.AddListener(OnBrightness);
            _contrastSlider.onValueChanged.AddListener(OnContrast);
        }

        private void OnDisable()
        {
            _volumeSlider.onValueChanged.RemoveListener(OnVolume);
            _brightnessSlider.onValueChanged.RemoveListener(OnBrightness);
            _contrastSlider.onValueChanged.RemoveListener(OnContrast);
        }

        private void OnVolume(float value)
        {
            _deaf.SetActive(value == _volumeSlider.minValue);
            _masterMixer.audioMixer.SetFloat(_nameVolume, value);
        }

        private void OnBrightness(float value)
        {
            _blind.SetActive(value == _brightnessSlider.minValue);
            cameraSettings.Brightness = _brightnessSlider.value;
        }

        private void OnContrast(float value)
        {
            _birdEye.SetActive(value == _contrastSlider.maxValue);
            cameraSettings.Contrast = _contrastSlider.value;
        }
    }
}