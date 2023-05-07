using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class Tree : MonoBehaviour
    {
        public float minScale = 0.9f;
        public float maxScale = 1.5f;
        public float yPosition = -0.1f;


        // Start is called before the first frame update
        void Awake()
        {
            transform.position = new Vector3(transform.position.x, yPosition, transform.position.z);

            float randomRot = Random.Range(0.0f, 360.0f);
            transform.rotation = Quaternion.Euler(new Vector3(0, randomRot, 0));

            float randomScale = Random.Range(minScale, maxScale);
            transform.localScale = Vector3.one * randomScale;
        }
    }
}
