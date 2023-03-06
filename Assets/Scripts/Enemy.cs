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
        [SerializeField]private bool following;

        // Start is called before the first frame update
        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            spawnLocation = transform.position;
            agent.Warp(spawnLocation);
        }

        // Update is called once per frame
        void Update()
        {
            Follow();
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player") && following != true)
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
            if (following == true)
            {
                FollowTarget(target.position);
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
