using Assets.Scripts.Settings;
using TMPro;
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

    [Header("Accessibility")]
    public TMP_Dropdown colorblindModeDropdown;

    public float musicVolume = 1.0f;
    public float sfxVolume = 1.0f;
    public bool lowSensoryModeEnabled = false;
    public bool hapticsEnabled = true;
    public int colorblindMode = 0;

    private const string MUSIC_VOL_KEY = "MusicVolume";
    private const string SFX_VOL_KEY = "SfxVolume";
    private const string LOW_SENSORY_MODE_KEY = "LowSensoryModeEnabled";
    private const string ENABLE_HAPTCIS_KEY = "HapticsEnabled";
    private const string COLORBLIND_MODE_KEY = "Accessibility.ColorblindType";

    private const string MUSIC_VOL_MIXER_VARIABLE = "MusicVolume";
    private const string SFX_VOL_MIXER_VARIABLE = "SfxVolume";

    //public static SettingsManager Instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        LoadSettings();
        /*

         if (Instance != null && Instance != this)
         {
             Destroy(gameObject);
             return;
         }

         // Registrar esta instância
         Instance = this;

         // Fazer persistir entre cenas
         DontDestroyOnLoad(gameObject);*/
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
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
        SetColorblindMode(colorblindModeDropdown.value);

        PlayerPrefs.Save();
        OnBackButtonPressed();
    }
    public void SetColorblindMode(int modeIndex)
    {
        colorblindMode = modeIndex;
        PlayerPrefs.SetInt(COLORBLIND_MODE_KEY, modeIndex);
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
        int savedColorblindMode = PlayerPrefs.GetInt(COLORBLIND_MODE_KEY, 0);

        enableHaptics.isOn = hapticsEnabled;
        lowSensoryMode.isOn = lowSensoryModeEnabled;

        if (colorblindModeDropdown != null)
        {
            colorblindModeDropdown.value = savedColorblindMode;
            colorblindModeDropdown.RefreshShownValue();
        }

        SetMusicVolume(musicVolumeSlider.value);
        SetSfxVolume(sfxVolumeSlider.value);
        SetLowSensoryMode(lowSensoryMode.isOn);
        SetHaptics(enableHaptics.isOn);

        PlayerPrefs.Save();
    }
    public void OnColorblindModeChanged(int index)
    {
        // guardar na mesma key que o plugin usa
        PlayerPrefs.SetInt(COLORBLIND_MODE_KEY, index);
        PlayerPrefs.Save();

        // chamar diretamente o SOHNE.Colorblindness
        if (SOHNE.Accessibility.Colorblindness.Colorblindness.Instance != null)
        {
            SOHNE.Accessibility.Colorblindness.Colorblindness.Instance.Change(index);
        }
    }
}
