using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class EndLevelTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && StoryManager.Instance != null)
            {
                StoryManager.Instance.LevelComplete();
            }
        }
    }
}
