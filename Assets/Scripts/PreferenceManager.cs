using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MorganHouston.Imprecision
{
    public class PreferenceManager : MonoBehaviour
    {

        private static PreferenceManager instance;

        public static PreferenceManager Instance { get { return instance; } }

        public float MasterVol { get; set; }
        public float MusicVol { get; set; }
        public float SFXVol { get; set; }
        public float Sensitivity { get; set; }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
            }
            else
            {
                instance = this;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            MasterVol = PlayerPrefs.GetFloat("MasterVolume", 1);
            MusicVol = PlayerPrefs.GetFloat("MusicVolume", 1);
            SFXVol = PlayerPrefs.GetFloat("SFXVolume", 1);
            Sensitivity = PlayerPrefs.GetFloat("Sensitivity", 20);
        }

        public void SetMasterValue(float value)
        {
            MasterVol = value;
            PlayerPrefs.SetFloat("MasterVolume", value);
            AudioManager.Instance.SetMasterVolume(value);
        }

        public void SetMusicValue(float value)
        {
            MusicVol = value;
            PlayerPrefs.SetFloat("MusicVolume", value);
            AudioManager.Instance.SetMusicVolume(value);
        }

        public void SetSFXValue(float value)
        {
            SFXVol = value;
            PlayerPrefs.SetFloat("SFXVolume", value);
            AudioManager.Instance.SetSFXVolume(value);
        }

        public void SetSensitivityValue(float value)
        {
            Sensitivity = value;
            PlayerPrefs.SetFloat("Sensitivity", value);
        }
    }
}
