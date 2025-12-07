using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class PersistentSettingsManager : MonoBehaviour
{
    public static PersistentSettingsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

    }

    [Header("Audio")]
    public AudioMixer audioMixer;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        // Load Volume
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 1f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_VOL_KEY, 1f);

        // Load Toggles
        bool hapticsEnabled = PlayerPrefs.GetInt(ENABLE_HAPTCIS_KEY, 1) == 1;
        bool lowSensoryModeEnabled = PlayerPrefs.GetInt(LOW_SENSORY_MODE_KEY, 0) == 1;
        int savedColorblindMode = PlayerPrefs.GetInt(COLORBLIND_MODE_KEY, 0);

        SetMusicVolume(musicVol);
        SetSfxVolume(sfxVol);
        SetLowSensoryMode(lowSensoryModeEnabled);
        SetHaptics(hapticsEnabled);
        SetColorblindMode(savedColorblindMode);

        PlayerPrefs.Save();
    }

    public void SetColorblindMode(int modeIndex)
    {
        colorblindMode = modeIndex;
        PlayerPrefs.SetInt(COLORBLIND_MODE_KEY, modeIndex);

        // chamar diretamente o SOHNE.Colorblindness
        if (SOHNE.Accessibility.Colorblindness.Colorblindness.Instance != null)
        {
            SOHNE.Accessibility.Colorblindness.Colorblindness.Instance.Change(modeIndex);
        }
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
}
