using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer audioMixer;

    [Header("UI References")]
    [InspectorName("Music Volume Slider")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle lowSensoryMode;
    public Toggle enableHaptics;
    public Button backButton;

    public float musicVolume = 1.0f;
    public float sfxVolume = 1.0f;
    public bool lowSensoryModeEnabled = false;
    public bool hapticsEnabled = true;

    private const string MUSIC_VOL_KEY = "MusicVolume";
    private const string SFX_VOL_KEY = "SfxVolume";
    private const string LOW_SENSORY_MODE_KEY = "LowSensoryModeEnabled";
    private const string ENABLE_HAPTCIS_KEY = "HapticsEnabled";

    private const string MUSIC_VOL_MIXER_VARIABLE = "MusicVolume";
    private const string SFX_VOL_MIXER_VARIABLE = "SfxVolume";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadSettings();
    }

    public void OnBackButtonPressed()
    {
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("Settings");
    }

    public void OnSaveButtonPressed()
    {
        SetMusicVolume(musicVolumeSlider.value);
        SetSfxVolume(sfxVolumeSlider.value);
        SetLowSensoryMode(lowSensoryMode.isOn);
        SetHaptics(enableHaptics.isOn);
        PlayerPrefs.Save();
        OnBackButtonPressed();
    }

    public void SetMusicVolume(float volume)
    {
        this.musicVolume = volume;
        audioMixer.SetFloat(MUSIC_VOL_MIXER_VARIABLE, Mathf.Log10(volume) * 20f);
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, volume);
    }

    public void SetSfxVolume(float volume)
    {
        this.sfxVolume = volume;
        audioMixer.SetFloat(SFX_VOL_MIXER_VARIABLE, Mathf.Log10(volume) * 20f);
        PlayerPrefs.SetFloat(SFX_VOL_KEY, volume);
    }

    public void SetHaptics(bool isEnabled)
    {
        this.hapticsEnabled = isEnabled;
        PlayerPrefs.SetInt(ENABLE_HAPTCIS_KEY, isEnabled ? 1 : 0);
    }

    public void SetLowSensoryMode(bool isEnabled)
    {
        this.lowSensoryModeEnabled = isEnabled;
        PlayerPrefs.SetInt(LOW_SENSORY_MODE_KEY, isEnabled ? 1 : 0);
    }


    private void LoadSettings()
    {
        // Load Volume
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 1f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_VOL_KEY, 1f);

        musicVolumeSlider.value = musicVol;
        sfxVolumeSlider.value = sfxVol;

        // Load Toggles
        bool hapticsEnabled = PlayerPrefs.GetInt(ENABLE_HAPTCIS_KEY, 1) == 1;
        bool lowSensoryModeEnabled = PlayerPrefs.GetInt(LOW_SENSORY_MODE_KEY, 0) == 1;

        enableHaptics.isOn = hapticsEnabled;
        lowSensoryMode.isOn = lowSensoryModeEnabled;

        SetMusicVolume(musicVolumeSlider.value);
        SetSfxVolume(sfxVolumeSlider.value);
        SetLowSensoryMode(lowSensoryMode.isOn);
        SetHaptics(enableHaptics.isOn);
        PlayerPrefs.Save();
    }
}
