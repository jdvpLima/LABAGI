using Assets.Scripts.Settings;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
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
    public int colorblindMode = 0;

    private float old_musicVolume = 1.0f;
    private float old_sfxVolume = 1.0f;
    private bool old_lowSensoryModeEnabled = false;
    private bool old_hapticsEnabled = true;
    private int old_colorblindMode = 0;

    private PersistentSettingsManager persistentSettings;

    [Header("Accessibility")]
    public TMP_Dropdown colorblindModeDropdown;

    private const string COLORBLIND_MODE_KEY = "Accessibility.ColorblindType";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // Find PersistentSettingsManager and attach corresponding listeners to sliders/toggles/dropdown
        persistentSettings = PersistentSettingsManager.Instance;

        if (persistentSettings == null)
        {
            Debug.LogError("PersistentSettingsManager instance not found in the scene.");
            return;
        }

        LoadSettings(persistentSettings);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
        lowSensoryMode.onValueChanged.AddListener(SetLowSensoryMode);
        enableHaptics.onValueChanged.AddListener(SetHaptics);
        if (colorblindModeDropdown != null)
        {
            colorblindModeDropdown.onValueChanged.AddListener(SetColorblindMode);
        }
    }

    public void OnBackButtonPressed()
    {
        // Only restore previous settings
        // Unloading the scene is handled by QuitScene script
        if (this.persistentSettings == null)
        {
            Debug.LogError("PersistentSettingsManager instance not found in the scene.");
            return;
        }
        RestorePreviousSettings(this.persistentSettings);
    }

    private void RestorePreviousSettings(PersistentSettingsManager persistentSettings)
    {
        musicVolume = old_musicVolume;
        sfxVolume = old_sfxVolume;
        lowSensoryModeEnabled = old_lowSensoryModeEnabled;
        hapticsEnabled = old_hapticsEnabled;
        colorblindMode = old_colorblindMode;
        SaveSettingsToPersistent(persistentSettings);
    }

    private void SaveSettingsToPersistent(PersistentSettingsManager persistentSettings)
    {
        persistentSettings.SetMusicVolume(musicVolume);
        persistentSettings.SetSfxVolume(sfxVolume);
        persistentSettings.SetLowSensoryMode(lowSensoryModeEnabled);
        persistentSettings.SetHaptics(hapticsEnabled);
        persistentSettings.SetColorblindMode(colorblindMode);
        PlayerPrefs.Save();

    }

    public void OnSaveButtonPressed()
    {
        if (persistentSettings == null)
        {
            Debug.LogError("PersistentSettingsManager instance not found in the scene.");
            return;
        }
        
        SaveSettingsToPersistent(persistentSettings);
        UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("Settings");
    }
    public void SetColorblindMode(int modeIndex)
    {
        colorblindMode = modeIndex;
        this.persistentSettings.SetColorblindMode(modeIndex);
    }

    public void SetMusicVolume(float volume)
    {
        this.musicVolume = volume;
        this.persistentSettings.SetMusicVolume(volume);
    }

    public void SetSfxVolume(float volume)
    {
        this.sfxVolume = volume;
        this.persistentSettings.SetSfxVolume(volume);
    }

    public void SetHaptics(bool isEnabled)
    {
        this.hapticsEnabled = isEnabled;
        this.persistentSettings.SetHaptics(isEnabled);

        Haptics.SetEnabled(isEnabled);
    }

    public void SetLowSensoryMode(bool isEnabled)
    {
        this.lowSensoryModeEnabled = isEnabled;
        this.persistentSettings.SetLowSensoryMode(isEnabled);
    }


    private void LoadSettings(PersistentSettingsManager persistentSettings)
    {
        // Load Volume

        musicVolumeSlider.value = persistentSettings.musicVolume;
        sfxVolumeSlider.value = persistentSettings.sfxVolume;

        // Load Toggles
        enableHaptics.isOn = persistentSettings.hapticsEnabled;
        lowSensoryMode.isOn = persistentSettings.lowSensoryModeEnabled;

        if (colorblindModeDropdown != null)
        {
            colorblindModeDropdown.value = persistentSettings.colorblindMode;
            colorblindModeDropdown.RefreshShownValue();
        }

        this.musicVolume = persistentSettings.musicVolume;
        this.sfxVolume = persistentSettings.sfxVolume;
        this.lowSensoryModeEnabled = persistentSettings.lowSensoryModeEnabled;
        this.hapticsEnabled = persistentSettings.hapticsEnabled;
        this.colorblindMode = persistentSettings.colorblindMode;

        // Store old values to allow restoring on cancel
        old_musicVolume = this.musicVolume;
        old_sfxVolume = this.sfxVolume;
        old_lowSensoryModeEnabled = this.lowSensoryModeEnabled;
        old_hapticsEnabled = this.hapticsEnabled;
        old_colorblindMode = this.colorblindMode;
    }
    //public void OnColorblindModeChanged(int index)
    //{
    //    // guardar na mesma key que o plugin usa
    //    PlayerPrefs.SetInt(COLORBLIND_MODE_KEY, index);
    //    PlayerPrefs.Save();

    //    // chamar diretamente o SOHNE.Colorblindness
    //    if (SOHNE.Accessibility.Colorblindness.Colorblindness.Instance != null)
    //    {
    //        SOHNE.Accessibility.Colorblindness.Colorblindness.Instance.Change(index);
    //    }
    //}
}
