using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class TorchFlicker : MonoBehaviour
    {
        private Light torchLight;

        public float minIntensity = 0.8f;
        public float maxIntensity = 1.2f;
        public float flickerSpeed = 0.5f;

        private float targetIntensity;
        private float currentIntensity;

        private void Start()
        {
            if (torchLight == null)
            {
                torchLight = GetComponent<Light>();
            }

            currentIntensity = torchLight.intensity;
            targetIntensity = Random.Range(minIntensity, maxIntensity);
        }

        private void Update()
        {
            // Smoothly move the torch light intensity towards the target intensity
            currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * flickerSpeed);
            torchLight.intensity = currentIntensity;

            // Check if the light intensity is close to the target, then set a new target
            if (Mathf.Abs(currentIntensity - targetIntensity) < 0.05f)
            {
                targetIntensity = Random.Range(minIntensity, maxIntensity);
            }
        }
    }
}
