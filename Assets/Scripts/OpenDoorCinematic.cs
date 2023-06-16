using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace Com.MorganHouston.Imprecision
{
    public class OpenDoorCinematic : MonoBehaviour
    {

        public CinemachineVirtualCamera doorCam;
        public Transform door;
        public HingeJoint doorHinge;
        public Rigidbody doorRb;
        bool doorUnlocked = false;
        bool doorOpen = false;

        public void OpenDoor()
        {
            doorRb.isKinematic = false;
            doorUnlocked = true;
            doorCam.enabled = true;
            doorHinge.useMotor = true;
            doorOpen = false;
            StartCoroutine(DisableCam());
        }

        IEnumerator DisableCam()
        {
            yield return new WaitForSeconds(4f);
            doorOpen = true;
            doorHinge.useMotor = false;
            doorCam.enabled = false;
        }
    }
}
