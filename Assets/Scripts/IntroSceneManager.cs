using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Com.MorganHouston.Imprecision
{
    public class IntroSceneManager : MonoBehaviour
    {
        private static IntroSceneManager instance;

        public static IntroSceneManager Instance { get { return instance; } }

        public GameObject[] cams;

        public GameObject cutsceneCanvas;

        private PlayableDirector _currentDirector;
        private bool _sceneSkipped = false;
        private float _timeToSkipTo;

        private void Awake()
        {
            if(instance != this && instance != null)
            {
                Destroy(this.gameObject);
            }
            else
            {
                instance = this;
            }
        }

        public void DeleteCams()
        {
            foreach(GameObject cam in cams)
            {
                Destroy(cam);
            }
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
                cutsceneCanvas.SetActive(false);
            }
            
        }


    }
}
