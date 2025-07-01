using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal; // ���� URP
using System.Linq;
using TMPro;

public class SettingsMenu : MonoBehaviour
{
    [Header("Graphics")]
    public TMP_Dropdown shadowQualityDropdown;
    public Slider shadowDistanceSlider;
    public TMP_Dropdown antiAliasingDropdown;
    public TMP_Dropdown textureQualityDropdown;
    public TMP_Dropdown vSyncDropdown;
    public TMP_Dropdown anisotropicFilteringDropdown;

    [Header("Audio")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public AudioMixer audioMixer; // ������� � ����������

    [Header("Screen")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    [Header("Performance")]
    public TMP_Dropdown targetFrameRateDropdown;

    [Header("UI")]
    public Slider uiScaleSlider;
    public CanvasScaler canvasScaler;

    [Header("Controls")]
    public Slider mouseSensitivitySlider;
    public Toggle invertXToggle;
    public Toggle invertYToggle;

    private Resolution[] resolutions;

    private void Start()
    {
        InitResolutionDropdown();
        InitShadowQualityDropdown();
        InitAntiAliasingDropdown();
        InitTextureQualityDropdown();
        InitVSyncDropdown();
        InitAnisotropicFilteringDropdown();
        InitTargetFrameRateDropdown();

        LoadSettings();

        // ������������� �� ��������� UI
        shadowQualityDropdown.onValueChanged.AddListener(SetShadowQuality);
        shadowDistanceSlider.onValueChanged.AddListener(SetShadowDistance);
        antiAliasingDropdown.onValueChanged.AddListener(SetAntiAliasing);
        textureQualityDropdown.onValueChanged.AddListener(SetTextureQuality);
        vSyncDropdown.onValueChanged.AddListener(SetVSync);
        anisotropicFilteringDropdown.onValueChanged.AddListener(SetAnisotropicFiltering);

        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);

        targetFrameRateDropdown.onValueChanged.AddListener(SetTargetFrameRate);

        uiScaleSlider.onValueChanged.AddListener(SetUIScale);

        mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
        invertXToggle.onValueChanged.AddListener(SetInvertX);
        invertYToggle.onValueChanged.AddListener(SetInvertY);
    }

    #region Init Dropdowns

    void InitResolutionDropdown()
    {
        resolutions = Screen.resolutions.Distinct().ToArray();
        resolutionDropdown.ClearOptions();
        var options = resolutions.Select(r => $"{r.width} x {r.height} @ {r.refreshRate}Hz").ToList();
        resolutionDropdown.AddOptions(options);

        int currentResIndex = System.Array.FindIndex(resolutions, r => r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height);
        resolutionDropdown.value = currentResIndex >= 0 ? currentResIndex : 0;
        resolutionDropdown.RefreshShownValue();

        fullscreenToggle.isOn = Screen.fullScreen;
    }

    void InitShadowQualityDropdown()
    {
        var names = System.Enum.GetNames(typeof(UnityEngine.ShadowResolution));
        shadowQualityDropdown.ClearOptions();
        shadowQualityDropdown.AddOptions(names.ToList());
        shadowQualityDropdown.value = (int)QualitySettings.shadowResolution;
        shadowQualityDropdown.RefreshShownValue();

        shadowDistanceSlider.minValue = 0;
        shadowDistanceSlider.maxValue = 150;
        shadowDistanceSlider.value = QualitySettings.shadowDistance;
    }


    void InitAntiAliasingDropdown()
    {
        antiAliasingDropdown.ClearOptions();
        antiAliasingDropdown.AddOptions(new System.Collections.Generic.List<string> { "Off", "2x", "4x", "8x" });
        int aa = QualitySettings.antiAliasing;
        int value = 0;
        switch (aa)
        {
            case 0: value = 0; break;
            case 2: value = 1; break;
            case 4: value = 2; break;
            case 8: value = 3; break;
            default: value = 0; break;
        }
        antiAliasingDropdown.value = value;
        antiAliasingDropdown.RefreshShownValue();
    }

    void InitTextureQualityDropdown()
    {
        textureQualityDropdown.ClearOptions();
        textureQualityDropdown.AddOptions(new System.Collections.Generic.List<string> { "Full Res", "Half Res", "Quarter Res", "Eighth Res" });
        textureQualityDropdown.value = QualitySettings.globalTextureMipmapLimit;
        textureQualityDropdown.RefreshShownValue();
    }

    void InitVSyncDropdown()
    {
        vSyncDropdown.ClearOptions();
        vSyncDropdown.AddOptions(new System.Collections.Generic.List<string> { "Off", "Every V Blank", "Every Second V Blank" });
        vSyncDropdown.value = QualitySettings.vSyncCount;
        vSyncDropdown.RefreshShownValue();
    }

    void InitAnisotropicFilteringDropdown()
    {
        anisotropicFilteringDropdown.ClearOptions();
        anisotropicFilteringDropdown.AddOptions(new System.Collections.Generic.List<string> { "Disable", "Enable", "Force Enable" });
        anisotropicFilteringDropdown.value = (int)QualitySettings.anisotropicFiltering;
        anisotropicFilteringDropdown.RefreshShownValue();
    }

    void InitTargetFrameRateDropdown()
    {
        targetFrameRateDropdown.ClearOptions();
        targetFrameRateDropdown.AddOptions(new System.Collections.Generic.List<string> { "30", "60", "120", "144", "Unlimited" });

        int currentFPS = Application.targetFrameRate;
        int index = 1; // default 60
        switch (currentFPS)
        {
            case 30: index = 0; break;
            case 60: index = 1; break;
            case 120: index = 2; break;
            case 144: index = 3; break;
            case -1: index = 4; break;
            default: index = 1; break;
        }
        targetFrameRateDropdown.value = index;
        targetFrameRateDropdown.RefreshShownValue();
    }

    #endregion

    #region Setters

    public void SetShadowQuality(int val)
    {
        QualitySettings.shadowResolution = (UnityEngine.ShadowResolution)val;
        PlayerPrefs.SetInt("ShadowQuality", val);
    }

    public void SetShadowDistance(float val)
    {
        QualitySettings.shadowDistance = val;
        PlayerPrefs.SetFloat("ShadowDistance", val);
    }

    public void SetAntiAliasing(int val)
    {
        int aa = 0;
        switch (val)
        {
            case 0: aa = 0; break;
            case 1: aa = 2; break;
            case 2: aa = 4; break;
            case 3: aa = 8; break;
        }
        QualitySettings.antiAliasing = aa;
        PlayerPrefs.SetInt("AntiAliasing", aa);
    }

    public void SetTextureQuality(int val)
    {
        QualitySettings.globalTextureMipmapLimit = val;
        PlayerPrefs.SetInt("TextureQuality", val);
    }

    public void SetVSync(int val)
    {
        QualitySettings.vSyncCount = val;
        PlayerPrefs.SetInt("VSync", val);
    }

    public void SetAnisotropicFiltering(int val)
    {
        QualitySettings.anisotropicFiltering = (AnisotropicFiltering)val;
        PlayerPrefs.SetInt("AnisotropicFiltering", val);
    }

    public void SetMasterVolume(float val)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(val) * 20); // ��������� � ���������
        PlayerPrefs.SetFloat("MasterVolume", val);
    }

    public void SetMusicVolume(float val)
    {
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(val) * 20);
        PlayerPrefs.SetFloat("MusicVolume", val);
    }

    public void SetSFXVolume(float val)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(val) * 20);
        PlayerPrefs.SetFloat("SFXVolume", val);
    }

    public void SetResolution(int val)
    {
        Resolution res = resolutions[val];
        Screen.SetResolution(res.width, res.height, fullscreenToggle.isOn, res.refreshRate);
        PlayerPrefs.SetInt("ResolutionIndex", val);
    }

    public void SetFullscreen(bool val)
    {
        Screen.fullScreen = val;
        PlayerPrefs.SetInt("Fullscreen", val ? 1 : 0);
    }

    public void SetTargetFrameRate(int val)
    {
        int fps = 60;
        switch (val)
        {
            case 0: fps = 30; break;
            case 1: fps = 60; break;
            case 2: fps = 120; break;
            case 3: fps = 144; break;
            case 4: fps = -1; break; // unlimited
        }
        Application.targetFrameRate = fps;
        PlayerPrefs.SetInt("TargetFrameRate", fps);
    }

    public void SetUIScale(float val)
    {
        if (canvasScaler != null)
        {
            canvasScaler.scaleFactor = val;
            PlayerPrefs.SetFloat("UIScale", val);
        }
    }

    public void SetMouseSensitivity(float val)
    {
        PlayerPrefs.SetFloat("MouseSensitivity", val);
        // � ����� ����������� ������ ���� ��� ��������
    }

    public void SetInvertX(bool val)
    {
        PlayerPrefs.SetInt("InvertX", val ? 1 : 0);
        // ��������� � �����������
    }

    public void SetInvertY(bool val)
    {
        PlayerPrefs.SetInt("InvertY", val ? 1 : 0);
        // ��������� � �����������
    }

    #endregion

    public void LoadSettings()
    {
        // ��������� � ��������� ����������� ���������

        SetShadowQuality(PlayerPrefs.GetInt("ShadowQuality", (int)QualitySettings.shadowResolution));
        SetShadowDistance(PlayerPrefs.GetFloat("ShadowDistance", QualitySettings.shadowDistance));
        SetAntiAliasing(PlayerPrefs.GetInt("AntiAliasing", QualitySettings.antiAliasing));
        SetTextureQuality(PlayerPrefs.GetInt("TextureQuality", QualitySettings.globalTextureMipmapLimit));
        SetVSync(PlayerPrefs.GetInt("VSync", QualitySettings.vSyncCount));
        SetAnisotropicFiltering(PlayerPrefs.GetInt("AnisotropicFiltering", (int)QualitySettings.anisotropicFiltering));

        SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume", 1f));
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 1f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", 1f));

        int resIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        SetResolution(resIndex);
        SetFullscreen(fullscreenToggle.isOn);

        int targetFPS = PlayerPrefs.GetInt("TargetFrameRate", 60);
        int targetFPSIndex = targetFrameRateDropdown.options.FindIndex(o => o.text == targetFPS.ToString());
        if (targetFPSIndex == -1) targetFPSIndex = 1; // default 60
        targetFrameRateDropdown.value = targetFPSIndex;
        SetTargetFrameRate(targetFPSIndex);

        float uiScale = PlayerPrefs.GetFloat("UIScale", 1f);
        uiScaleSlider.value = uiScale;
        SetUIScale(uiScale);

        float mouseSens = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        mouseSensitivitySlider.value = mouseSens;

        bool invertX = PlayerPrefs.GetInt("InvertX", 0) == 1;
        invertXToggle.isOn = invertX;

        bool invertY = PlayerPrefs.GetInt("InvertY", 0) == 1;
        invertYToggle.isOn = invertY;
    }
}
