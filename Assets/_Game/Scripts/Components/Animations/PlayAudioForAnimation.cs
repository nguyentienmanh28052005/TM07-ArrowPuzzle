using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudioForAnimation : MonoBehaviour
{
    public AudioClip[] audioClips;

    int currentClip;

    public void ResetAudio()
    {
        currentClip = 0;
    }
    
    public void PlayAudio()
    {
        if (currentClip < audioClips.Length)
        {
            AudioManager.Instance.PlayOneShot(audioClips[currentClip]);
            currentClip++;
        }
    }
}
