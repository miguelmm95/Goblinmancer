using FMODUnity;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : BaseMenu
{
    [SerializeField] Slider MasterVolumeSlider;
    [SerializeField] Slider MusicVolumeSlider;
    [SerializeField] Slider AmbienceVolumeSlider;
    [SerializeField] Slider SFXVolumeSlider;
    public override void OpenMenu()
    {
        gameObject.SetActive(true);
        MasterVolumeSlider.value = AudioManager.instance.masterVolume;
        MusicVolumeSlider.value = AudioManager.instance.musicVolume;
        AmbienceVolumeSlider.value = AudioManager.instance.ambienceVolume;
        SFXVolumeSlider.value = AudioManager.instance.SFXVolume;
    }

    public void MuteMasterVolume()
    {
        bool isMuted = AudioManager.instance.masterMuted;
        AudioManager.instance.masterMuted = !isMuted;
        AudioManager.instance.PlayOneShot(_menuInteractionSound, transform.position);
    }

    public void MuteMusicVolume()
    {
        bool isMuted = AudioManager.instance.musicMuted;
        AudioManager.instance.musicMuted = !isMuted;
        AudioManager.instance.PlayOneShot(_menuInteractionSound, transform.position);
    }

    public void MuteAmbienceVolume()
    {
        bool isMuted = AudioManager.instance.ambienceMuted;
        AudioManager.instance.ambienceMuted = !isMuted;
        AudioManager.instance.PlayOneShot(_menuInteractionSound, transform.position);
    }

    public void MuteSFXVolume()
    {
        bool isMuted = AudioManager.instance.SFXMuted;
        AudioManager.instance.SFXMuted = !isMuted;
        AudioManager.instance.PlayOneShot(_menuInteractionSound, transform.position);
    }

    public void SetMasterVolume()
    {
        AudioManager.instance.masterVolume = MasterVolumeSlider.value;
        AudioManager.instance.masterMuted = false;
    }
    public void SetMusicVolume()
    {
        AudioManager.instance.musicVolume = MusicVolumeSlider.value;
        AudioManager.instance.musicMuted = false;
    }
    public void SetAmbienceVolume()
    {
        AudioManager.instance.ambienceVolume = AmbienceVolumeSlider.value;
        AudioManager.instance.ambienceMuted = false;
    }
    public void SetSFXVolume()
    {
        AudioManager.instance.SFXVolume = SFXVolumeSlider.value;
        AudioManager.instance.SFXMuted = false;
    }
    
    public override void CloseMenu()
    {
        AudioManager.instance.PlayOneShot(_menuInteractionSound, transform.position);
        gameObject.SetActive(false);
    }

}
