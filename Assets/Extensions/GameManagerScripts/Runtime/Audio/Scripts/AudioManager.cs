using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

#if UNITY_EDITOR
using UnityEditor.Presets;
#endif

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioMixer mainMixer;
    
    
    [SerializeField]private AudioSource[] sources = new AudioSource[0];
    
    [Search(typeof(ReferenceAudio))] public ReferenceAudio[] refernceAudioArray;

    [Header("Fade")]
    [SerializeField] private float musicFadeFloor = -40;
     private float musicBaseVolume;
    [SerializeField] private float fadeSpeed = 5;
    [Header("Pitch")]
    [SerializeField, Range(1,2f)] private float maxPitchVariation = 2;


    private Coroutine randomMusicCoroutine = null;
    private Coroutine musicFade = null;

    #region Awake Methods
   
    void Awake()
    {
        //Sets all the dictionarys
        foreach(ReferenceAudio audioRefenerce in refernceAudioArray)
        {
            audioRefenerce.OnPlay();
        }
        mainMixer.GetFloat("MusicVolume", out musicBaseVolume);
    }
    public static AudioManager GetInstance()
    {
        //Singleton pattern
        if (instance == null)
        {
            return new AudioManager();
        }
        else
        {
            return instance;
        }
    }
    AudioManager()
    {
        instance = this;
    }
    /// <summary>
    /// Generates an audio source for each type of audio
    /// </summary>
    public void GenerateAudioManager()
    {
        if(sources.Length != 0) { return; }
        #if UNITY_EDITOR
        CreateAduioSources();
        SetRefernceArray();
        SetAudioMixer();
        
        if (instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            #if UNITY_EDITOR
            DestroyImmediate(gameObject);
            return;
            #endif

            #pragma warning disable CS0162 // Unreachable code detected
            Destroy(gameObject);
            #pragma warning restore CS0162 // Unreachable code detected
        }
        #endif


    }
    /// <summary>
    /// Sets teh refernce array
    /// </summary>
#if UNITY_EDITOR
    [ContextMenu("Reset Refernce Array")]
    public void SetRefernceArray()
    {
        //Adds all the referneces into an array
        string[] guidsForRfenernceAudio = AssetDatabase.FindAssets($"t:{nameof(ReferenceAudio)}");
        refernceAudioArray = new ReferenceAudio[guidsForRfenernceAudio.Length];

        for (int i = 0; i < guidsForRfenernceAudio.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guidsForRfenernceAudio[i]);
            refernceAudioArray[i] = AssetDatabase.LoadAssetAtPath<ReferenceAudio>(path);
        }
    }

    /// <summary>
    /// Gets the presets from the assets folder
    /// </summary>
    /// <param name="names"></param>
    /// <returns></returns>
    private Preset[] GetAudioSourcePresets(string[] names)
    {
        //Sets the presets in the same order as the array
        Preset[] presets = new Preset[names.Length];
        foreach (string guid in AssetDatabase.FindAssets("t:preset"))//Gets the GUIDs of all the presets
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Preset preset = AssetDatabase.LoadAssetAtPath<Preset>(path);
            if (preset.GetTargetFullTypeName() == "UnityEngine.AudioSource")
            {
                for (int i = 0; i < names.Length; i++)
                {
                    if (names[i].Contains(preset.name))
                    {
                        presets[i] = preset;
                    }
                }
            }
        }
        return presets;
    }
    /// <summary>
    /// Creates the audio sources and assigns the coresponding presets
    /// </summary>
    /// <param name="names"></param>
    public void CreateAduioSources()
    {
        string[] names = new string[] { "Dialouge AudioSource", "Music AudioSource", "SoundEffect AudioSource", "Foley AudioSource", "Background AudioSource" };

        Preset[] presets = GetAudioSourcePresets(names);

        sources = new AudioSource[5];

        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i] == null)
            {
                Type audioSource = typeof(AudioSource);
                GameObject go = new GameObject(names[i], audioSource);
                go.transform.parent = transform;
                //Sets the preset
                presets[i].ApplyTo(go.GetComponent<AudioSource>());
            }

            sources[i] = GetComponentsInChildren<AudioSource>()[i];
        }

    }

    public void SetAudioMixer()
    {
        string[] guids = AssetDatabase.FindAssets($"t:{nameof(AudioMixer)}");
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        mainMixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(path);
    }
    #endif
    #endregion

    #region Play Methods
    /// <summary>
    /// Play that takes in a string value
    /// </summary>
    /// <param name="audio"></param>
    /// <param name="tr"></param>
    public void PlaySound(string audio, Transform tr)
    {
       
        var audioClip = GetAudioClip(audio); 
        AudioSource audioSource = GetAudioScource(audioClip.audioClipType);
        audioSource.transform.position = tr.position;
        if(!audioClip.Equals(default(ReferenceAudio.AudioClips)))
        {
            audioSource.PlayOneShot(audioClip.clip);
        }
    }
    /// <summary>
    /// Plays a sound
    /// </summary>
    /// <param name="audio"></param>
    /// <param name="tr"></param>
    /// <param name="audioSource"></param>
    public void PlaySound(string audio, Transform tr, AudioSource audioSource)
    {

        var audioClip = GetAudioClip(audio);
        audioSource.transform.position = tr.position;
        if (!audioClip.Equals(default(ReferenceAudio.AudioClips)))
        {
            audioSource.PlayOneShot(audioClip.clip);
        }
    }
    public void PlayRandom(string[] audio, Transform tr)
    {

        PlaySound(audio[UnityEngine.Random.Range(0, audio.Length)], tr);
    }
    public void PlaySequence(string[] audio, Transform tr)
    {
        StartCoroutine(PlaySequenceCourtine(audio, tr));
    }
    /// <summary>
    /// Plays audio in sequence
    /// </summary>
    /// <param name="audio"></param>
    /// <param name="tr"></param>
    /// <returns></returns>
    IEnumerator PlaySequenceCourtine(string[] audio, Transform tr)
    {
        for (int i = 0; i < audio.Length; i++)
        {
            PlaySound(audio[i], tr);
            yield return new WaitForSeconds(GetAudioClip(audio[i]).clip.length);
        }
    }
    /// <summary>
    /// Plays music looping
    /// </summary>
    /// <param name="audio"></param>
    public void PlayMusicLooping(string audio)
    {
        AudioSource musicSource = GetAudioScource(AudioClipType.MX);
        if (musicSource.isPlaying)
        {
           musicFade = StartCoroutine(ChangeMusic(audio, musicSource, true));
        }
        else
        {
            musicSource.loop = true;
            musicSource.clip = GetAudioClip(audio).clip;
            musicSource.Play();
        }
    }
    /// <summary>
    /// Plays music not looping
    /// </summary>
    /// <param name="audio"></param>
    public void PlayMusic(string audio)
    {
        mainMixer.GetFloat("MusicVolume", out musicBaseVolume);

        AudioSource musicSource = GetAudioScource(AudioClipType.MX);
        if (musicSource.isPlaying)
        {
            musicFade = StartCoroutine(ChangeMusic(audio, musicSource, false));
        }
        else
        {
            musicSource.clip = GetAudioClip(audio).clip;
            musicSource.Play();
        }
    }
    /// <summary>
    /// Fades the music to play the nect one
    /// </summary>
    /// <param name="valueToChangeTo"></param>
    /// <param name="fadeIn"></param>
    /// <returns></returns>
    IEnumerator FadeMusic(float valueToChangeTo, bool fadeIn)
    {
        mainMixer.GetFloat("MusicVolume", out float outValue); ;
        while ((fadeIn)? outValue < valueToChangeTo : outValue > valueToChangeTo)
        {
            mainMixer.GetFloat("MusicVolume", out outValue);
            float inValue = (fadeIn) ? outValue + Time.deltaTime * fadeSpeed : outValue - Time.deltaTime * fadeSpeed;
            mainMixer.SetFloat("MusicVolume", inValue);
                
            yield return null;
            
        }
        mainMixer.SetFloat("MusicVolume", valueToChangeTo);
    }
    /// <summary>
    /// Changes the music 
    /// </summary>
    /// <param name="audio"></param>
    /// <param name="musicSource"></param>
    /// <returns></returns>
    IEnumerator ChangeMusic(string audio, AudioSource musicSource, bool isLooping)
    {
        Coroutine fadedMusic = StartCoroutine(FadeMusic(musicFadeFloor, false));
        yield return fadedMusic;

        musicSource.loop = isLooping;
        musicSource.clip = GetAudioClip(audio).clip;
        musicSource.Play();
        musicFade = StartCoroutine(FadeMusic(musicBaseVolume, true));
    }
    public void PlayMusicAtRandom(string[] audio)
    {
        if(randomMusicCoroutine != null)
        {
            StopCoroutine(randomMusicCoroutine);
        }
       randomMusicCoroutine = StartCoroutine(RandomMusicPicker(audio));
        
    }
    /// <summary>
    /// Gets a random music clip from a string and plays it but returns an error if its not a music clip
    /// </summary>
    /// <param name="audio"></param>
    /// <returns></returns>
    IEnumerator RandomMusicPicker(string[] audio)
    {
        AudioSource audioSoruce = GetAudioScource(AudioClipType.MX);
        audioSoruce.Stop();
        int lastIndex = -1;
        while(true)
        {
            int randomIndex = UnityEngine.Random.Range(0, audio.Length);
            //Makes sure it never plays the same one twice in a row
            if (randomIndex != lastIndex)
            {
                 lastIndex = randomIndex;

                var audioClipName = audio[randomIndex];
                //Adds handling to be sure these scripts are all MX
                if (GetAudioClip(audioClipName).audioClipType != AudioClipType.MX)
                {
                    Debug.LogError(audioClipName + " is not of type " + AudioClipType.MX.ToString());
                    yield break;
                }
                PlayMusic(audioClipName);

                yield return new WaitForSeconds(GetAudioClip(audioClipName).clip.length - 3);


            }
            yield return null;
        }
    }
    public void PlayWithPitchVariation(string audio, Transform tr)
    {
        var audioClip = GetAudioClip(audio);
        AudioSource audioSource = GetAudioScource(audioClip.audioClipType);

        audioSource.pitch = UnityEngine.Random.Range(1, maxPitchVariation);

        audioSource.transform.position = tr.position;
        if (!audioClip.Equals(default(ReferenceAudio.AudioClips)))
        {
            audioSource.PlayOneShot(audioClip.clip);
        }
    }
    #endregion

    #region Get Audio Clips Methods
    /// <summary>
    /// Gets the clip value within the refernece audio array
    /// </summary>
    /// <param name="audioLabel"></param>
    /// <returns></returns>
    public ReferenceAudio.AudioClips GetAudioClip(string audioLabel)
    {
        for(int i = 0; i < refernceAudioArray.Length; i++)
        {
            if (!refernceAudioArray[i].clipsDic.ContainsKey(audioLabel)) { continue; }
            return refernceAudioArray[i].clipsDic[audioLabel];
        }
        Debug.LogError(audioLabel + " Not Found");
        return new ReferenceAudio.AudioClips();
    }
    #endregion

    #region Get Audio Source Methods
    public AudioSource GetAudioScource(AudioClipType type)
    {
        return sources[(int)type];
    }
    #endregion
    public enum AudioClipType
    {
        DX = 0,
        MX = 1, 
        SFX = 2,
        FOL = 3,
        BG = 4
    }
    
}
