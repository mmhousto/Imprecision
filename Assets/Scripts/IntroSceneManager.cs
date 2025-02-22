using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Playables;

namespace Com.MorganHouston.Imprecision
{
    public class IntroSceneManager : MonoBehaviour
    {
        public GameObject[] cams;
        public GameObject cutsceneCanvas;
        public StarterAssetsInputs inputs;
        public PauseManager pausedManager;
        
        private PlayableDirector _currentDirector;
        private bool _sceneSkipped = false;
        private float _timeToSkipTo;
        public bool _cutsceneTriggered = false;

        private void Awake()
        {
            GetDirector(GetComponent<PlayableDirector>());
        }

        private void Start()
        {
            _sceneSkipped = false;
        }

        private void Update()
        {
            if(_sceneSkipped == false && inputs.isSkipping && _cutsceneTriggered && pausedManager.isPaused == false)
            {
                SkipScene();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && _cutsceneTriggered == false)
            {
                other.GetComponent<RigBuilder>().enabled = false;
                _cutsceneTriggered = true;
                _currentDirector.Play();
            }
        }

        public void DeleteCams()
        {
            foreach(GameObject cam in cams)
            {
                Destroy(cam);
            }
        }

        public void StartGame()
        {
            GameManager.Instance.gameStarted = true;
        }

        public void GetDirector(PlayableDirector director)
        {
            _sceneSkipped = false;
            _currentDirector = director;
        }
        public void GetSkipTime(float skipTime)
        {
            _timeToSkipTo = skipTime;
        }

        public void SkipScene()
        {
            if (_sceneSkipped == false)
            {
                _currentDirector.time = _timeToSkipTo;
                _sceneSkipped = true;
                inputs.isSkipping = false;
                //cutsceneCanvas.SetActive(false);
            }
            
        }

        public void PlayMusic()
        {
            AudioManager.Instance.GetComponent<AudioSource>().Play();
        }
    }
}
