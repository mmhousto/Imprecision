using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MorganHouston.Imprecision
{
    public class RespawnPlayer : MonoBehaviour
    {
        public Vector3 spawnLocation = new Vector3(0, 4, 0);

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<CharacterController>().enabled = false;
                other.transform.position = spawnLocation;
                other.GetComponent<CharacterController>().enabled = true;
            }
        }
    }
}
