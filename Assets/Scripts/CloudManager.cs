using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MorganHouston.Imprecision
{
    public class CloudManager : MonoBehaviour
    {
        [SerializeField]
        private float cloudSpeed = 5f;
        [SerializeField]
        private Vector3 respawnPoint = new Vector3(-32f, 30f, -150f);

        // Update is called once per frame
        void Update()
        {
            transform.Translate(Vector3.forward * cloudSpeed * Time.deltaTime);

            if(transform.position.z >= 200)
            {
                transform.position = respawnPoint;
            }
        }
    }
}
