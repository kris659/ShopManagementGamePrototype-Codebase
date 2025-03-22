using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum Sound
{
    UIButtonClicked,
    UIEmpty_1,
    UIEmpty_2,
    UIEmpty_3,
    UIEmpty_4,

    ProductPickup,
    ProductDrop,
    BoxPickup,
    BoxDrop,

    BoxOpen,
    BoxClose,

    WoodDoorOpen,
    WoodDoorClose,

    AutomaticDoorOpen,
    AutomaticDoorClose,

    FreezerDoorOpen,
    FreezerDoorClose,

    VanDoorOpen,
    VanDoorClose,

    EngineStartVan,
    EngineStartTruck,
    EngineStartForklift,
    Empty_4,

    PlayerOrder,

    ProductScan,
    CheckoutFinished,
    CustomerPutOnRegister,

    ThrowAway,
    Build,
    Destroy,

    PositiveNotification,
    NegativeNotification,
    RewardClaim
}

public class AudioManager : MonoBehaviour
{
    public static float musicVolume
    {
        get { return _musicVolume; }
        set {            
            _musicVolume = value;
            UpdateMusicVolume();
        }
    }
    public static float masterVolume
    {
        get { return _masterVolume; }
        set
        {
            _masterVolume = value;
            UpdateMusicVolume();
        }
    }

    private static float _musicVolume;
    private static float _masterVolume;
    public static float sfxVolume;

    public SoundAudioClip[] soundAudioClips;
    static SoundAudioClip[] _soundAudioClips;

    public float mainMusicVolume = 1;
    static float _mainMusicVolume;
    static GameObject mainMusicGameObject;

    [SerializeField] private AudioMixerGroup masterMixerGroup;
    private static AudioMixerGroup _masterMixerGroup;

    [SerializeField] private AnimationCurve volumeRolloff;
    private static AnimationCurve _volumeRolloff;


    [System.Serializable]
    public class SoundAudioClip
    {
        public Sound sound;
        public AudioClip audioClip;
        [Range(0, 1)]
        public float volume;
        //public AudioMixerGroup mixerGroup;
        [Range(0, 1f)]
        public float pitchRandomness;
    }

    private void Awake()
    {
        _soundAudioClips = soundAudioClips;
        mainMusicGameObject = gameObject;
        _mainMusicVolume = mainMusicVolume;
        _masterMixerGroup = masterMixerGroup;
        _volumeRolloff = volumeRolloff;

        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        sfxVolume = PlayerPrefs.GetFloat("sfxVolume", 0.5f);

    }

    public static void PlaySound(Sound sound)
    {
        PlaySound(GetSoundInfo(sound));
    }
    public static void PlaySound(Sound sound, Vector3 position, Transform transform = null)
    {
        PlaySound(GetSoundInfo(sound), position, transform);
    }

    public static void PlayMusic(AudioClip audioClip, float volume)
    {
        GameObject soundGameObject = new GameObject("Music");
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
        audioSource.volume = volume * musicVolume * masterVolume;
        audioSource.PlayOneShot(audioClip);
    }

    public static void PlaySound(SoundAudioClip soundInfo)
    {
        Transform camera = PlayerInteractions.Instance.mainCamera.transform;
        PlaySound(soundInfo, camera.position - camera.forward, camera);
    }
    public static void PlaySound(SoundAudioClip soundInfo, Vector3 position, Transform parent = null)
    {
        GameObject soundGameObject = new GameObject("Sound");
        soundGameObject.transform.SetParent(parent);
        soundGameObject.transform.position = position;
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = _masterMixerGroup;
        audioSource.rolloffMode = AudioRolloffMode.Custom;
        audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, _volumeRolloff);
        audioSource.maxDistance = 25;
        audioSource.dopplerLevel = 0.2f;
        audioSource.spatialBlend = 1f;
        audioSource.pitch = 1 + UnityEngine.Random.Range(-soundInfo.pitchRandomness, soundInfo.pitchRandomness);
        audioSource.clip = soundInfo.audioClip;
        audioSource.volume = sfxVolume * soundInfo.volume * masterVolume;
        audioSource.Play();
        Destroy(soundGameObject, soundInfo.audioClip.length);

    }
    static SoundAudioClip GetSoundInfo(Sound sound)
    {
        foreach (SoundAudioClip soundAudioClip in _soundAudioClips){
            if (soundAudioClip.sound == sound) return soundAudioClip;
        }
        Debug.LogError("Sound not found!");
        return null;
    }
    static void UpdateMusicVolume()
    {
         //mainMusicGameObject.GetComponent<AudioSource>().volume = musicVolume * _mainMusicVolume * masterVolume;
    }
}
