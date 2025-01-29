using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class Footsteps : MonoBehaviour
    {

        public AudioClip[] audioClips;
        public AudioSource audioSource;
        private float delay = 1f;

        private void Start()
        {
            delay = 0;
        }

        private void Update()
        {
            if (delay >= 0)
            {
                delay -= Time.deltaTime;
            }
            

        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.layer == 6 && delay <= 0)
            {
                int rand = Random.Range(0, audioClips.Length);
                audioSource.clip = audioClips[rand];
                audioSource.PlayOneShot(audioSource.clip);
                delay = 1f;
            }
            
        }
    }
}
