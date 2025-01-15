using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class HolAudio : MonoBehaviour
    {
        private AudioSource audioSource;
        public AudioClip[] clips;

        // Start is called before the first frame update
        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            InvokeRepeating(nameof(DirectPlayer), 120f, 120f);
        }

        void DirectPlayer()
        {
            audioSource.clip = clips[Random.Range(0, clips.Length)];
            audioSource.Play();
        }
    }
}
