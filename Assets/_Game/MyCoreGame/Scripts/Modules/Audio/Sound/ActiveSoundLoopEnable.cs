using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EventGame.MusicTour
{
    public class ActiveSoundLoopEnable : MonoBehaviour
    {
        [SerializeField] AudioSource audioSource;
        [SerializeField] bool isPlayDelay;
        [SerializeField] float timeDelay;

        private AudioSource audio
        {
            get
            {
                if(audioSource == null)
                {
                    audioSource = GetComponent<AudioSource>();
                }
                return audioSource;
            }
        }

        private void OnEnable()
        {
            if (audio != null) 
            {
                if (isPlayDelay)
                {
                    PlaySoundDelayLoop();
                }
                else
                {
                    audio.Play();
                }
            }
        }

        private void PlaySoundDelayLoop()
        {
            audio.Play();
            this.Wait(timeDelay, PlaySoundDelayLoop);
        }

        private void OnDisable()
        {
            if (audio != null) 
            { 
                audio.Stop();
            }
        }
    }
}
