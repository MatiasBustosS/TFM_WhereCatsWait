using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    [Header("Video")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Slider brightnessSlider;

    [Header("Audio")]
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Mixer")]
    public AudioMixer audioMixer;

    private Resolution[] resolutions;

    private void Start()
    {
        SetupResolutions();
        LoadSettings();
    }
    
    void SetupResolutions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("Resolution", resolutionIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }
    
    public void SetBrightness(float value)
    {
        RenderSettings.ambientIntensity = value;
        PlayerPrefs.SetFloat("Brightness", value);
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("Master", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
    
    void LoadSettings()
    {
        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        fullscreenToggle.isOn = fullscreen;
        Screen.fullScreen = fullscreen;

        float brightness = PlayerPrefs.GetFloat("Brightness", 1f);
        brightnessSlider.value = brightness;
        RenderSettings.ambientIntensity = brightness;

        float master = PlayerPrefs.GetFloat("MasterVolume", 1f);
        masterSlider.value = master;
        SetMasterVolume(master);

        float music = PlayerPrefs.GetFloat("MusicVolume", 1f);
        musicSlider.value = music;
        SetMusicVolume(music);

        float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);
        sfxSlider.value = sfx;
        SetSFXVolume(sfx);

        int resolutionIndex = PlayerPrefs.GetInt("Resolution", resolutions.Length - 1);
        resolutionDropdown.value = resolutionIndex;
        SetResolution(resolutionIndex);
    }
}
