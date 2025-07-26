using UnityEngine;
using UnityEngine.Audio;
using System;

// Class nhỏ để định nghĩa âm thanh
[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    public bool isSfx; // Tick vào nếu đây là SFX, bỏ trống nếu là Music
    [Range(0f, 1f)] public float volume = 1f;
    [Range(.1f, 3f)] public float pitch = 1f;
    public bool loop = false;

    [HideInInspector] public AudioSource source;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioMixer audioMixer; // Kéo AudioMixer của bạn vào đây
    public Sound[] sounds;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);

        AudioMixerGroup[] sfxGroups = audioMixer.FindMatchingGroups("SFX");
        AudioMixerGroup[] musicGroups = audioMixer.FindMatchingGroups("Music");
        
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            
            // Gán vào đúng group trong Mixer
            if(s.isSfx && sfxGroups.Length > 0)
                s.source.outputAudioMixerGroup = sfxGroups[0];
            else if (!s.isSfx && musicGroups.Length > 0)
                s.source.outputAudioMixerGroup = musicGroups[0];
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) return;
        s.source.Play();
    }

    // Dành riêng cho nhạc nền để đảm bảo chỉ có 1 bài phát
    public void PlayMusic(string name)
    {
         // Dừng tất cả nhạc nền khác
        foreach(var s in sounds) {
            if(!s.isSfx) s.source.Stop();
        }
        Play(name);
    }
}