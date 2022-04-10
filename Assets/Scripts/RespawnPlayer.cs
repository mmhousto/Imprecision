using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MorganHouston.Imprecision
{
    public class RespawnPlayer : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<Rigidbody>().velocity = Vector3.zero;
                other.transform.position = new Vector3(0, 4, 0);
            }
        }
    }
}
