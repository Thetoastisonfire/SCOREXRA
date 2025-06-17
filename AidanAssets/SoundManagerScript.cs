using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SoundManagerScript : MonoBehaviour
{

    #region variables

    public static SoundManagerScript Instance { get; private set; }

    /*
        The current plan is to have all sound effects play on av sources 1-5;
        Each effect will move down the stack, checking if each audio source is active.
        If one is available, it plays on that one. If none are open, maybe just end the first source? 
        Or dump the new source... That'd either be FIFO circular buffer or buffer with drop policy

        Currently going to do a buffer with drop policy I think, cause .isPlaying is lightweight and
        it would probably be heavier to track what audiosource is oldest than just going through em; 
        cause audio sources will all end at random times

        BUT if whoever is reading this wants to prevent dropping new sounds, then please either add more sources
        or switch to a circular buffer (good luck!)
    */
    public AudioSource avSource1; //audio source 1, 2, 3, etc.
    public AudioSource avSource2;
    public AudioSource avSource3;
    public AudioSource avSource4;
    public AudioSource avSource5;
    public AudioSource backMusicSource1; //background music source
    public AudioSource backMusicSource2; //background ambient music source

    [Space(20)]

    private AudioSource[] audioSources = new AudioSource[7] { null, null, null, null, null, null, null};
    private float[] originalVolumes; //track default volume settings for each source
    public static float theVolume = 1f;

    private Dictionary<string, AudioClip> dialogueClips = new Dictionary<string, AudioClip>(); //in case we add dialogue for npcs
    private Dictionary<string, AudioClip> SEClips = new Dictionary<string, AudioClip>(); //sound effects
    private Dictionary<string, AudioClip> backgroundClips = new Dictionary<string, AudioClip>(); //background music / ambience

    public bool initComplete = false; //sound init is complete

    #endregion

    #region initialization

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }//endof if

        Instance = this;
        DontDestroyOnLoad(gameObject);

        int j = 0; //init-ed source counter
        for (int i = 1; i <= 7; i++)
        { //ensure audio sources are init-ed and store in array
            switch (i)
            {
                case 1: //av source 1
                    if (avSource1 == null) Debug.LogError("audio source " + i + " not set."); //if not set, error
                    else avSource1 = audioSources[j]; j++; //set audio source and increment counter
                    break;
                case 2: //av source 2
                    if (avSource2 == null) Debug.LogError("audio source " + i + " not set."); //if not set, error
                    else avSource2 = audioSources[j]; j++; //set audio source and increment counter
                    break;
                case 3: //av source 3
                    if (avSource3 == null) Debug.LogError("audio source " + i + " not set."); //if not set, error
                    else avSource3 = audioSources[j]; j++; //set audio source and increment counter
                    break;
                case 4: //av source 4
                    if (avSource4 == null) Debug.LogError("audio source " + i + " not set."); //if not set, error
                    else avSource4 = audioSources[j]; j++; //set audio source and increment counter
                    break;
                case 5: //av source 5
                    if (avSource5 == null) Debug.LogError("audio source " + i + " not set."); //if not set, error
                    else avSource5 = audioSources[j]; j++; //set audio source and increment counter
                    break;
                case 6: //av source 4
                    if (backMusicSource1 == null) Debug.LogError("backMusicSource1 not set."); //if not set, error
                    else backMusicSource1 = audioSources[j]; j++; //set audio source and increment counter
                    break;
                case 7: //av source 5
                    if (backMusicSource2 == null) Debug.LogError("backMusicSource2 not set."); //if not set, error
                    else backMusicSource2 = audioSources[j]; j++; //set audio source and increment counter
                    break;
            } //endof switch
        } //endof for

        StartCoroutine(InitializeAudioClips()); //init clips to be used
    }

    private IEnumerator InitializeAudioClips()
    {
        StartCoroutine(LoadAudioClips("Sound/dialogueClips", dialogueClips));
        StartCoroutine(LoadAudioClips("Sound/SEClips", SEClips));
        yield return StartCoroutine(LoadAudioClips("Sound/backgroundClips", backgroundClips));
        //waits on last load
        initComplete = true; //set initComplete to true so things can use sound now
    }

    //please run the install before this happens or stuff will break HARD!!
    private IEnumerator LoadAudioClips(string path, Dictionary<string, AudioClip> clipDictionary)
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>(path);

        if (clips.Length <= 0) Debug.LogWarning("No clips found at path " + path);

        int batchSize = 4; //windowing
        for (int i = 0; i < clips.Length; i += batchSize) //in batches of 4, load up audio clips
        {
            for (int j = 0; j < batchSize && (i + j) < clips.Length; j++)
            {
                AudioClip clip = clips[i + j];
                clipDictionary[clip.name] = clip; //assign clip and its name to the dictionary
            }
            yield return null; //yield control back to main to avoid freezing, and to give clips time to assign
        }
    }//endof method

    //this method is for audio that has a name like "1,5"; usually used for dialogue
    private int TryParseClipName(string clipName, out int num1, out int num2)
    {
        string[] parts = clipName.Split(',');

        if (parts.Length == 2 && int.TryParse(parts[0], out num1) && int.TryParse(parts[1], out num2))
        {
            return 1; //returning 'true'
        }

        //if parsing fails, return default values
        num1 = 0;
        num2 = 0;
        return 0; //returning 'false'
    }


    //should set the volume in relation to preset volume levels
    public void SetGlobalVolume(float volume)
    {
        volume = Mathf.Clamp(volume, 0f, 1f); //clamps the volume between 0 and 1

        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i] != null) audioSources[i].volume = originalVolumes[i] * volume;
        }//endof for
    }//endof method

    void Start()
    {
        audioSources = new AudioSource[] {
            avSource1, avSource2,
            avSource3, avSource3,
            avSource4, avSource5,
            backMusicSource1,
            backMusicSource2
        };

        originalVolumes = new float[audioSources.Length];

        //should be 7 audio sources, null/unassigned ones get ignored
        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i] != null)
            {
                originalVolumes[i] = audioSources[i].volume;
                Debug.Log("Assigned AudioSource at index " + i + ": " + audioSources[i].name);
            }//endof if
        } //endof for

        SetGlobalVolume(theVolume);
    }//endof method

    #endregion

    #region playing sounds

    public IEnumerator PlayAudioClip(string clipName, bool isDialogue)
    {
        Debug.LogWarning("PLAYING CLIP!!!!!!!!!");
        if (isDialogue) //if is dialogue, pass that along to PlayClip
        {
            //if clip exists, play it; else, send a warning that it failed
            if (dialogueClips.TryGetValue(clipName, out AudioClip clip))
            {
                yield return PlayClip(clip, true);
            }
            else Debug.LogWarning($"Dialogue Audio clip not found: {clipName}");
        }
        else //if its a sound effect (background music is handled elsewhere)
        {
            if (SEClips.TryGetValue(clipName, out AudioClip clip))
            {
                yield return PlayClip(clip, false);
            }
            else Debug.LogWarning($"Dialogue Audio clip not found: {clipName}");
        }//endof else
    }//endof method

    private IEnumerator PlayClip(AudioClip clip, bool isDialogue)
    {
        AudioSource selectedSource;

        if (!avSource1.isPlaying) selectedSource = avSource1;
        else if (!avSource2.isPlaying) selectedSource = avSource2;
        else if (!avSource3.isPlaying) selectedSource = avSource3;
        else if (!avSource4.isPlaying) selectedSource = avSource4;
        else if (!avSource5.isPlaying) selectedSource = avSource5;
        else
        {
            Debug.LogError("all players full, unable to play clip.");
            yield break;
        }

        selectedSource.clip = clip;
        selectedSource.loop = false;
        selectedSource.Play(); //play clip, no loop

        while (selectedSource.isPlaying) //while its playing, stay on this method instance
        {
            yield return null;
        }

        selectedSource.Stop(); //ensure the clip has stopped

    }

    private IEnumerator FadeOutAndStop(AudioSource source, float fadeDuration)
    {
        float startVolume = source.volume;

        while (source.volume > 0)
        {
            source.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        source.Stop();
        source.volume = startVolume; //reset volume after stopping
    }

    #endregion


    //add stop all clips, stop background music, stop specific sound effects (maybe track when & where one is playing)
    //add fade out and stop audio clip method
    //add play ambient
    //add playNewBackgroundMusic
}
