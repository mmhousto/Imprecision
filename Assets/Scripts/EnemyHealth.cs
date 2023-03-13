using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class EnemyHealth : MonoBehaviour
    {
        private Enemy enemy;

        // Start is called before the first frame update
        void Start()
        {
            enemy = GetComponent<Enemy>();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

    }
}
