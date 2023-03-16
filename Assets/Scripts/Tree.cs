using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class Tree : MonoBehaviour
    {

        // Start is called before the first frame update
        void Awake()
        {
            transform.position = new Vector3(transform.position.x, -0.1f, transform.position.z);

            float randomRot = Random.Range(0.0f, 360.0f);
            transform.rotation = Quaternion.Euler(new Vector3(0, randomRot, 0));

            float randomScale = Random.Range(0.9f, 1.5f);
            transform.localScale = Vector3.one * randomScale;
        }
    }
}
