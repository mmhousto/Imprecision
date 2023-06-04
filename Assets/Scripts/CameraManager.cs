using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Com.MorganHouston.Imprecision
{
    public class CameraManager : MonoBehaviour
    {
        private CinemachineVirtualCamera cam;

        private void OnEnable()
        {
            cam = GetComponent<CinemachineVirtualCamera>();
            SetCamFoVs();
        }

        private void Start()
        {
            cam = GetComponent<CinemachineVirtualCamera>();
            SetCamFoVs();
        }

        private void Update()
        {
            if (cam != null && cam.m_Lens.FieldOfView != PreferenceManager.Instance.FoV)
                SetCamFoVs();
        }

        public void SetCamFoVs()
        {
             cam.m_Lens.FieldOfView = PreferenceManager.Instance.FoV;
        }
    }
}
