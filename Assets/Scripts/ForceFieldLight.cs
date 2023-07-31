using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class ForceFieldLight : MonoBehaviour
    {
        public ForceFieldManager forceField;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.transform.CompareTag("arrow"))
            {
                forceField.RemoveLight(GetComponent<Light>());
                Destroy(this.gameObject);
            }
        }
    }
}
