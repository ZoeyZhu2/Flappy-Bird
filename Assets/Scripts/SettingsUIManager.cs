using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
using TMPro;

public class SettingsUIManager : MonoBehaviour
{
    [SerializeField] private Toggle soundFXToggle;
    [SerializeField] private Slider soundFXSlider;
    [SerializeField] private TMP_Text soundFXVolumeText;

    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private TMP_Text musicVolumeText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //null checks
        if (soundFXToggle == null || soundFXSlider == null)
        {
            Debug.LogError("SettingsUIManager: One or more UI components are not assigned.");
            return;
        }   

        if (AudioManager.Instance == null)
        {
            Debug.LogError("SettingsUIManager: AudioManager instance is not found.");
            return;
        }
        // AudioManager.Instance.ReapplyVolumeSettings();

        // float currentMusicVol = AudioManager.Instance.GetMusicVolume();
        // float currentSoundFXVol = AudioManager.Instance.GetSoundFXVolume();
    
        // Debug.Log("SettingsUIManager: After re-applying, volumes are - Music=" + currentMusicVol + " SoundFX=" + currentSoundFXVol);



        //Setting initial states and Listeners
        soundFXToggle.isOn = !AudioManager.Instance.IsSoundFXMuted(); //on means soundFX is on, so soundFXMuted is false
        soundFXSlider.value = AudioManager.Instance.GetSoundFXVolume();
        soundFXToggle.onValueChanged.AddListener(OnSoundFXToggled);
        soundFXSlider.onValueChanged.AddListener(OnSoundFXVolumeChanged);

        soundFXVolumeText.text = soundFXSlider.value.ToString("0.00", CultureInfo.InvariantCulture);

        // Debug.Log("SettingsUIManager Start: GetMusicVolume returned=" + musicVol);
        // Debug.Log("SettingsUIManager Start: musicSlider.minValue=" + musicSlider.minValue + " maxValue=" + musicSlider.maxValue);
        // Debug.Log("SettingsUIManager Start: Setting musicSlider.value to " + musicVol);
        musicToggle.isOn = !AudioManager.Instance.IsMusicMuted();
        musicSlider.value = AudioManager.Instance.GetMusicVolume();
        musicToggle.onValueChanged.AddListener(OnMusicToggled);
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        // Debug.Log("SettingsUIManager Start: musicSlider.value is now=" + musicSlider.value);

        musicVolumeText.text = musicSlider.value.ToString("0.00", CultureInfo.InvariantCulture);
    }
    
    private void OnSoundFXToggled(bool isOn)
    {
        AudioManager.Instance.SoundFXMute(!isOn);
    }

    private void OnSoundFXVolumeChanged(float volume)
    {
        AudioManager.Instance.SetSoundFXVolume(volume);
        float newVolume = AudioManager.Instance.GetSoundFXVolume();
        soundFXVolumeText.text = newVolume.ToString("0.00", CultureInfo.InvariantCulture);
    }

    private void OnDestroy()
    {
        //Remove listeners to prevent memory leaks when the settings scene is unloaded
        if (soundFXToggle != null)
        {
            soundFXToggle.onValueChanged.RemoveListener(OnSoundFXToggled);
        }
        if (soundFXSlider != null)
        {
            soundFXSlider.onValueChanged.RemoveListener(OnSoundFXVolumeChanged);
        }
        if (musicToggle != null)
        {
            musicToggle.onValueChanged.RemoveListener(OnMusicToggled);
        }
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        }
    }

    private void OnMusicToggled(bool isOn)
    {
        AudioManager.Instance.MusicMute(!isOn);
    }

    private void OnMusicVolumeChanged(float volume)
    {
        AudioManager.Instance.SetMusicVolume(volume);
        float newVolume = AudioManager.Instance.GetMusicVolume();
        musicVolumeText.text = newVolume.ToString("0.00", CultureInfo.InvariantCulture);
    }

    // Update is called once per frame
    void Update()
    {

        
    }
}
