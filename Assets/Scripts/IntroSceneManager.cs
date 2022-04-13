using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MorganHouston.Imprecision
{
    public class IntroSceneManager : MonoBehaviour
    {

        public GameObject[] cams;

        public void DeleteCams()
        {
            foreach(GameObject cam in cams)
            {
                Destroy(cam);
            }
        }
    }
}
