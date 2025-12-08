using Assets.Scripts.Settings;
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

    // Volumes "do utilizador" (0..1) – estes é que ficam em PlayerPrefs
    public float musicVolume = 1.0f;
    public float sfxVolume = 1.0f;

    public bool lowSensoryModeEnabled = false;
    public bool hapticsEnabled = true;
    public int colorblindMode = 0;

    public event Action<bool> OnLowSensoryModeChanged;

    private const string MUSIC_VOL_KEY = "MusicVolume";
    private const string SFX_VOL_KEY = "SfxVolume";
    private const string LOW_SENSORY_MODE_KEY = "LowSensoryModeEnabled";
    private const string ENABLE_HAPTCIS_KEY = "HapticsEnabled";
    private const string COLORBLIND_MODE_KEY = "Accessibility.ColorblindType";

    private const string MUSIC_VOL_MIXER_VARIABLE = "MusicVolume";
    private const string SFX_VOL_MIXER_VARIABLE = "SfxVolume";

    private void Start()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        // Lê valores de utilizador
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOL_KEY, 1f);
        hapticsEnabled = PlayerPrefs.GetInt(ENABLE_HAPTCIS_KEY, 1) == 1;
        lowSensoryModeEnabled = PlayerPrefs.GetInt(LOW_SENSORY_MODE_KEY, 0) == 1;
        colorblindMode = PlayerPrefs.GetInt(COLORBLIND_MODE_KEY, 0);

        ApplyVolumesToMixer();
        SetHaptics(hapticsEnabled);
        SetColorblindMode(colorblindMode);

        // Garante que o mute de LowSensory é aplicado por cima
        ApplyLowSensoryMuteToMixer();

        PlayerPrefs.Save();
    }

    // Helper para converter [0..1] para dB
    private float VolumeToDb(float volume)
    {
        if (volume <= 0.0001f)
            return -80f; // mute "forte"
        return Mathf.Log10(volume) * 20f;
    }

    private void ApplyVolumesToMixer()
    {
        // Se lowSensory ativo → mixer usa 0 (mute); se não → usa volumes guardados
        float effectiveMusic = lowSensoryModeEnabled ? 0f : musicVolume;
        float effectiveSfx = lowSensoryModeEnabled ? 0f : sfxVolume;

        audioMixer.SetFloat(MUSIC_VOL_MIXER_VARIABLE, VolumeToDb(effectiveMusic));
        audioMixer.SetFloat(SFX_VOL_MIXER_VARIABLE, VolumeToDb(effectiveSfx));
    }

    private void ApplyLowSensoryMuteToMixer()
    {
        // Reusa mesma lógica
        ApplyVolumesToMixer();
    }

    public void SetColorblindMode(int modeIndex)
    {
        colorblindMode = modeIndex;
        PlayerPrefs.SetInt(COLORBLIND_MODE_KEY, modeIndex);

        if (SOHNE.Accessibility.Colorblindness.Colorblindness.Instance != null)
        {
            SOHNE.Accessibility.Colorblindness.Colorblindness.Instance.Change(modeIndex);
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, musicVolume);
        ApplyVolumesToMixer();
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat(SFX_VOL_KEY, sfxVolume);
        ApplyVolumesToMixer();
    }

    public void SetHaptics(bool isEnabled)
    {
        hapticsEnabled = isEnabled;
        PlayerPrefs.SetInt(ENABLE_HAPTCIS_KEY, isEnabled ? 1 : 0);

        if (HapticsManager.Instance != null)
        {
            HapticsManager.Instance.hapticsEnabled = isEnabled;
        }
    }

    public void SetLowSensoryMode(bool isEnabled)
    {
        lowSensoryModeEnabled = isEnabled;
        PlayerPrefs.SetInt(LOW_SENSORY_MODE_KEY, isEnabled ? 1 : 0);

        // Atualiza mixer de acordo com o novo estado
        ApplyVolumesToMixer();

        OnLowSensoryModeChanged?.Invoke(isEnabled);
    }
}
