using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("# Audio Master")]
    public AudioMixer masterMixer;
    public AudioMixerGroup bgmGroup;
    public AudioMixerGroup sfxGroup;

    //public AudioClip[] bgmClip;
    //AudioSource bgmPlayer;
    //public Bgm bgm;
    //AudioHighPassFilter bgmEffect;
    //public float bgmVolume;
    //public bool bgmVolumeMute;


    [Header("# BGM")]
    public AudioData[] bgmDatas;
    private Dictionary<string, AudioData> bgmDictionary = new Dictionary<string, AudioData>();
    private AudioSource bgmPlayer;
    private string currentBgm = "";
    
    //public AudioClip[] sfxClips;
    //public float sfxVolume;
    //public bool sfxVolumeMute;
    //public int channels;
    //AudioSource[] sfxPlayer;
    //int channelIndex;

    [Header("# SFX")]
    public AudioData[] sfxDatas;
    private Dictionary<string, AudioData> sfxDictionary = new Dictionary<string, AudioData>();
    public int channels = 16;
    AudioSource[] sfxPlayer;
    int channelIndex;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
        }
        else
        {
            Destroy(gameObject);
        }
        
    }

    private void Start()
    {
        Init();
    }

    void Init()
    {      
        // BGM 플레이어 초기화
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.SetParent(transform);
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.outputAudioMixerGroup = bgmGroup;


        // BGM 딕셔너리 데이터 변환
        foreach(var data in bgmDatas)
        {
            if (!bgmDictionary.ContainsKey(data.key))
                bgmDictionary.Add(data.key, data);
        }

        //SFX 플레이어 초기화
        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.SetParent(transform);
        sfxPlayer = new AudioSource[channels];

        for (int index = 0; index < channels; index++)
        {
            sfxPlayer[index] = sfxObject.AddComponent<AudioSource>();
            sfxPlayer[index].playOnAwake = false;
            sfxPlayer[index].bypassListenerEffects = true;
            sfxPlayer[index].outputAudioMixerGroup = sfxGroup;
        }

        foreach(var data in sfxDatas)
        {
            if (!sfxDictionary.ContainsKey(data.key))
                sfxDictionary.Add(data.key, data);
        }

        if (SettingsManager.instance != null)
            SettingsManager.instance.ApplySettings();

    }  

    public void PlayBgm(string key, bool isPlay = true)
    {
        if (currentBgm == key && isPlay)
            return;

        if (isPlay)
        {
            if (!bgmDictionary.TryGetValue(key, out AudioData data)) return;
            currentBgm = key;
            bgmPlayer.clip = data.clip;
            bgmPlayer.volume = data.volume;
            bgmPlayer.Play();
        }
        else
        {
            bgmPlayer.Stop();
            this.currentBgm = "";
        }
    }

    //public void EffectBgm(bool isPlay)
    //{
    //    bgmEffect.enabled = isPlay;
    //}

    public void PlaySfx(string key)
    {
        //if (sfxClips == null || (int)sfx >= sfxClips.Length) return;
        if (!sfxDictionary.TryGetValue(key, out AudioData data)) return;

        for (int index = 0; index < channels; index++)
        {
            int loopIndex = (index + channelIndex) % sfxPlayer.Length;

            if (sfxPlayer[loopIndex].isPlaying)
            {
                continue;
            }

            //int ranIndex = 0;

            channelIndex = loopIndex;
            sfxPlayer[loopIndex].clip = data.clip;
            sfxPlayer[loopIndex].volume = data.volume;
            sfxPlayer[loopIndex].Play();
            break;
        }

    }
    
    public void StopBgm()
    {
        bgmPlayer.Stop();
        currentBgm = "";
    }

    //public void PlayBgm(AudioClip clip)
    //{
    //    if(clip == null) return;

    //    bgmPlayer.clip = clip;
    //    bgmPlayer.Play();       
    //}

    //public void PlaySfx(AudioClip clip)
    //{
    //    if (clip == null) return;
    //    for (int index = 0; index < channels; index++)
    //    {
    //        int loopIndex = (index + channelIndex) % sfxPlayer.Length;
    //        if (sfxPlayer[loopIndex].isPlaying) continue;

    //        channelIndex = loopIndex;
    //        sfxPlayer[loopIndex].clip = clip;
    //        sfxPlayer[loopIndex].Play();
    //        break;
    //    }
    //}

    // 볼륨 제어 (오디오 믹서)
    public void SetMasterVolume(float volume) => SetMixerVolume("Master", volume);
    public void SetBgmVolume(float volume) => SetMixerVolume("BGM", volume);
    public void SetSfxVolume(float volume) => SetMixerVolume("SFX", volume);

    private void SetMixerVolume(string name, float volume)
    {
        if (masterMixer == null)
        {
            Debug.Log($"[Audio] masterMixer NULL!");
            return;
        }

        if (volume <= 0.0001f)
        {
            masterMixer.SetFloat(name, -80f);
            Debug.Log($"[Audio] {name} = {volume} (무음)");
        }
        else
        {
            float db = Mathf.Log10(volume) * 20f;
            masterMixer.SetFloat(name, db);
            Debug.Log($"[Audio] {name} = {volume} → {db}dB");
        }
    }
    //public void SetMasterVolume(float volume)
    //{
    //    AudioListener.volume = volume;
    //}

    //public void SetBgmVolume(float volume)
    //{
    //    bgmPlayer.volume = volume;
    //}

    //public void SetSfxVolume(float volume)
    //{
    //    for (int i = 0; i < channels; i++)
    //    {
    //        sfxPlayer[i].volume = volume;
    //    }
    //}
}
