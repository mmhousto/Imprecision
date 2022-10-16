using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace com.MorganHouston.Imprecision
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager instance;

        public static AudioManager Instance { get { return instance; } }

        public AudioMixer masterMixer;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(Instance.gameObject);
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume", 1));
            SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 1));
            SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", 1));
        }

        public void SetMasterVolume(float value)
        {
            masterMixer.SetFloat("MasterVolume", value);
        }

        public void SetMusicVolume(float value)
        {
            masterMixer.SetFloat("MusicVolume", value);
        }

        public void SetSFXVolume(float value)
        {
            masterMixer.SetFloat("SFXVolume", value);
        }
    }
}
