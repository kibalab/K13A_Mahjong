
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AudioQueue : UdonSharpBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;

    [SerializeField] private KList KList;

    public void AddQueue(string clip)
    {
        setClip(clip);
        audioSource.Play();
        //KList.Add(clip);
    }

    public void ClearQueue()
    {
        KList.Clear();
    }

    public int GetQueueCount()
    {
        return KList.Count();
    }

    public void PlayOnTime(string clip, float startTime)
    {
        setClip(clip);
        audioSource.Play();
        audioSource.time = startTime;
    }

    public float GetCurrentTime()
    {
        return audioSource.time;
    }

    public void set(float volume, float pitch)
    {
        audioSource.volume = volume;
        audioSource.pitch = pitch;
    }

    void setClip(string clip)
    {
        foreach (AudioClip a in audioClips)
        {
            if (a.name == clip)
            {
                audioSource.clip = a;
            }
        }
    }

    private void Update()
    {
        /*
        if (audioSource.isPlaying || KList.Count() == 0)
        {
            return;
        }
        var clip = KList.RemoveAt(0).ToString();
        setClip(clip);
        audioSource.Play();
        */
    }
}
