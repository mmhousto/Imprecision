using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class ForceFieldManager : MonoBehaviour
    {
        public List<Light> lights;

        // Update is called once per frame
        void Update()
        {
            if (lights.Count == 0) Destroy(this.gameObject);
        }

        public void RemoveLight(Light light)
        {
            lights.Remove(light);
        }
    }
}
