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
        bool doorUnlocked = false;
        bool doorOpen = false;


        // Update is called once per frame
        void Update()
        {
            if(doorOpen == false && door.rotation.y >= 269 && doorUnlocked == true)
            {
                doorOpen = true;
                doorHinge.useMotor = false;
                doorCam.enabled = false;
            }
        }

        public void OpenDoor()
        {
            doorUnlocked = true;
            doorCam.enabled = true;
            doorHinge.useMotor = true;
        }
    }
}
