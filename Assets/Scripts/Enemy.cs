using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace Com.MorganHouston.Imprecision
{
    public class Enemy : MonoBehaviour
    {        
        public Transform target;
        public float followDistance = 5f; // the distance at which the spider will start following the player
        public float attackDistance = 2f; // the distance at which the spider will start attacking the player
        public float attackTime = 0;

        protected Health health;
        protected enum AIState { Idle, Follow, Attack, Patrol }; // enum for the spider's states
        protected AIState currentState = AIState.Idle; // current state of the spider
        protected Vector3 spawnLocation;
        protected NavMeshAgent agent;
        protected bool canAttack;

        [SerializeField]
        protected int scalingfactor = 10;

        [SerializeField]
        protected int attackPower;
        public int AttackPower { get { return attackPower; } protected set { attackPower = value; } }

        [SerializeField]
        protected int defensePower;
        public int DefensePower { get { return defensePower; } protected set { defensePower = value; } }

        [SerializeField]
        protected float attackSpeed;
        public float AttackSpeed { get { return attackSpeed; } protected set { attackSpeed = value; } }

        [SerializeField]
        protected int movementSpeed;
        public int MovementSpeed { get { return movementSpeed; } protected set { movementSpeed = value; } }

        [SerializeField]
        protected int stamina;
        public int Stamina { get { return stamina; } protected set { stamina = value; } }

        [SerializeField]
        protected int critChance;
        public int CritChance { get { return critChance; } protected set { critChance = value; } }

        protected enum HitArea
        {
            Head,
            Body,
            Legs
        }

        // Start is called before the first frame update
        void Start()
        {
            AgentSetup();
        }

        // Update is called once per frame
        void Update()
        {
            CheckCanAttack();
            DetermineState();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("arrow"))
            {
                Collider myCollider = collision.GetContact(0).thisCollider;
                if(myCollider.name == "Head")
                {
                    health.TakeDamage(DetermineDamageToTake((int)HitArea.Head));
                }
                else if (myCollider.name == "Body")
                {
                    health.TakeDamage(DetermineDamageToTake((int)HitArea.Body));
                }
                else
                {
                    health.TakeDamage(DetermineDamageToTake((int)HitArea.Legs));
                }
                Destroy(collision.gameObject);
            }
        }

        protected void AgentSetup()
        {
            agent = GetComponent<NavMeshAgent>();
            spawnLocation = transform.position;
            agent.Warp(spawnLocation);
            agent.speed = movementSpeed;
            health = GetComponent<Health>();
            attackSpeed = 5 - (attackSpeed / 100) * 4;
            attackTime = 0;
        }

        protected virtual void DetermineState()
        {
            if(target == null)
            {
                FollowTarget(spawnLocation);
            }
            else
            {
                float distanceToPlayer = Vector3.Distance(transform.position, target.position);

                switch (currentState)
                {
                    case AIState.Idle:
                        FollowTarget(spawnLocation);
                        // do idle behavior (e.g. stay in place and wait for player to get close)
                        if (distanceToPlayer < followDistance)
                        {
                            currentState = AIState.Follow;
                        }
                        break;
                    case AIState.Follow:
                        // do follow behavior (e.g. move towards player)
                        FollowTarget(target.position);
                        if (distanceToPlayer < attackDistance)
                        {
                            currentState = AIState.Attack;
                        }
                        break;
                    case AIState.Attack:
                        if (canAttack)
                        {
                            target.GetComponent<Health>().TakeDamage(DetermineDamageToDeal());
                            attackTime = attackSpeed;
                            canAttack = false;
                        }
                        else
                        {
                            agent.updatePosition = true;
                            agent.updateRotation = true;
                            agent.isStopped = false;
                            FollowTarget(target.position);
                        }
                        break;
                    default:
                        currentState = AIState.Idle;
                        break;
                }
            }
            
        }

        protected void CheckCanAttack()
        {
            if(attackTime <= 0)
            {
                attackTime = 0;
                canAttack = true;
            }
            else
            {
                attackTime -= Time.deltaTime;
                canAttack = false;
            }
        }

        protected void FollowTarget(Vector3 targetPos)
        {
            if(targetPos != null)
                agent.SetDestination(targetPos);
        }

        protected int DetermineDamageToDeal()
        {
            Player player = Player.Instance;

            int damage = 0;
            float critValue = UnityEngine.Random.value;
            float randValue = UnityEngine.Random.Range(1, 12);
            float crit = CritChance / 100f;

            if (critValue < (1f - crit))
            {
                damage = (int)(randValue + AttackPower) - (player.DefensePower * (player.UserLevel / scalingfactor));
            }
            else
            {
                damage = ((int)(randValue + AttackPower) - (player.DefensePower * (player.UserLevel / scalingfactor))) * 2;
            }

            if (damage < 0)
            {
                damage = 0;
            }

            return damage;
        }

        protected int DetermineDamageToTake(int hitZone)
        {
            Player player = Player.Instance;
            float randValue = 10;
            float crit = player.CritChance / 100f;

            switch (hitZone)
            {
                case 0:
                    randValue = 22;
                    crit = 1;
                    break;
                case 1:
                    randValue = UnityEngine.Random.Range(12, 22);
                    break;
                case 2:
                    randValue = UnityEngine.Random.Range(1, 12);
                    break;
                default:
                    randValue = UnityEngine.Random.Range(1, 22);
                    break;
            }

            int damage = 0;
            float critValue = UnityEngine.Random.value;
            
            

            if (critValue < (1f - crit))
            {
                damage = (int)(randValue + player.AttackPower) - (DefensePower * (player.UserLevel / scalingfactor));
            }
            else
            {
                damage = (int)((randValue + player.AttackPower) - (DefensePower * (player.UserLevel / scalingfactor)))*2;
            }

            if(damage < 0)
            {
                damage = 0;
            }

            return damage;
        }
    }
}
