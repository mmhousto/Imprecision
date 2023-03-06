using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Com.MorganHouston.Imprecision
{
    public class Enemy : MonoBehaviour
    {

        public Transform target;
        private Vector3 spawnLocation;
        private NavMeshAgent agent;
        private bool following;

        // Start is called before the first frame update
        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            spawnLocation = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            Follow();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                following = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                following = false;
            }
        }

        private void Follow()
        {
            if (following)
            {
                FollowTarget(transform.position);
            }
            else
            {
                FollowTarget(spawnLocation);
            }
        }

        private void FollowTarget(Vector3 targetPos)
        {
            agent.SetDestination(targetPos);
        }
    }
}
