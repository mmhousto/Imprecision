using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class Footsteps : MonoBehaviour
    {

        public AudioClip[] audioClips;
        public AudioSource audioSource;

        private void OnTriggerEnter(Collider other)
        {
            if(other.gameObject.layer == 6)
            {
                int rand = Random.Range(0, audioClips.Length);
                audioSource.clip = audioClips[rand];
                audioSource.PlayOneShot(audioSource.clip);
            }
            
        }
    }
}
