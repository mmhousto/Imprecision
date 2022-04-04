using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class RotateThisObject : MonoBehaviour
    {

        // Update is called once per frame
        void Update()
        {
            transform.Rotate(Vector3.up, 25.0f * Time.deltaTime);
        }
    }
}
