using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class BowSounds : MonoBehaviour
    {
        public AudioClip[] clips;
        private AudioSource audioSource;

        // Start is called before the first frame update
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public void PlayRandomBowSound()
        {
            audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)], 0.3f);
        }
    }
}
