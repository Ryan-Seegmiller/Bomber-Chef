using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [Header("Auido Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [Header("Audio Sliders")]
    [SerializeField] private Slider masterVolume;
    [SerializeField] private Slider musicVolume;
    [SerializeField] private Slider dialougeVolume;
    [SerializeField] private Slider sfxVolume;
    [Header("Button")]
    [SerializeField] private Button closeButton;

    private Dictionary<string, Slider> volumeType;

    public UnityEvent closeButtonEvent;

    private void Start()
    {
        
        //Initailizes the dictionary
        volumeType = new Dictionary<string, Slider>
        {
            {"MusicVolume", musicVolume },
            { "DialougeVolume", dialougeVolume},
            { "SFXVolume", sfxVolume},
            { "MasterVolume", masterVolume}};
        //Adds all the volume slider values to what was saved
        foreach (var volume in volumeType)
        {
            if (PlayerPrefs.HasKey(volume.Key))
            {
                LoadVolume(volume.Key, volume.Value);
            }
            else
            {
                SetVolume(volume.Key, volume.Value);
            }
            volume.Value.minValue = .00001f;
            volume.Value.onValueChanged.AddListener(delegate {SetVolume(volume.Key, volume.Value); });
        }
        //Adds any functinoality set in the inspector to the button
        closeButton.onClick.AddListener(() => { closeButtonEvent.Invoke(); });
    }
    /// <summary>
    /// Sets the volume for the mixer and save it
    /// </summary>
    /// <param name="volumePath"></param>
    /// <param name="slider"></param>
    public void SetVolume(string volumePath, Slider slider)
    {
        float volume = slider.value;
        audioMixer.SetFloat(volumePath, MathF.Log10(volume) * 20);
        PlayerPrefs.SetFloat(volumePath, volume);//Saves the player data
    }
    /// <summary>
    /// Loads the volume
    /// </summary>
    /// <param name="volumePath"></param>
    /// <param name="slider"></param>
    public void LoadVolume(string volumePath, Slider slider)
    {
        slider.value = PlayerPrefs.GetFloat(volumePath);//Gets the player data
        SetVolume(volumePath, slider);
    }
    
}
