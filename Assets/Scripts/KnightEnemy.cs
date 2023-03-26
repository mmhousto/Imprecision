using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class KnightEnemy : Enemy
    {
        private Animator anim;

        // Start is called before the first frame update
        void Start()
        {
            AgentSetup();
            anim = GetComponent<Animator>();
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
                if (myCollider.name == "Head")
                {
                    health.TakeDamage(DetermineDamageToTake((int)HitArea.Body));
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

        protected override void DetermineState()
        {
            if (target == null)
            {
                FollowTarget(spawnLocation);
            }
            else
            {
                float distanceToPlayer = Vector3.Distance(transform.position, target.position);

                switch (currentState)
                {
                    case AIState.Idle:
                        anim.SetInteger("State", 0);
                        FollowTarget(spawnLocation);
                        // do idle behavior (e.g. stay in place and wait for player to get close)
                        if (distanceToPlayer < followDistance)
                        {
                            currentState = AIState.Follow;
                        }
                        break;
                    case AIState.Follow:
                        // do follow behavior (e.g. move towards player)
                        anim.SetInteger("State", 1);
                        FollowTarget(target.position);
                        if (distanceToPlayer < attackDistance)
                        {
                            currentState = AIState.Attack;
                        }
                        break;
                    case AIState.Attack:
                        if (canAttack)
                            JumpAndAttack();
                        else
                        {
                            agent.updatePosition = true;
                            agent.updateRotation = true;
                            agent.isStopped = false;
                            FollowTarget(target.position);
                            anim.SetInteger("State", 1);
                        }
                        break;
                    default:
                        currentState = AIState.Idle;
                        break;
                }
            }

        }

        private void JumpAndAttack()
        {
            if (agent.enabled)
            {
                // set the agents target to where you are before the jump
                // this stops her before she jumps. Alternatively, you could
                // cache this value, and set it again once the jump is complete
                // to continue the original move
                agent.SetDestination(transform.position);
                // disable the agent
                agent.updatePosition = false;
                agent.updateRotation = false;
                agent.isStopped = true;
            }
            anim.SetInteger("State", 2);

            // make the spider jump towards the player
            Vector3 directionToPlayer = (target.position - transform.position).normalized;
            GetComponent<Rigidbody>().AddForce(directionToPlayer * 5 + Vector3.up * 5, ForceMode.Impulse);

            // check if the player is within attack range
            float distanceToPlayer = Vector3.Distance(transform.position, target.position);
            if (distanceToPlayer <= 1.5f)
            {
                // perform the attack
                target.GetComponent<Health>().TakeDamage(DetermineDamageToDeal());
            }

            attackTime = attackSpeed;
            canAttack = false;

        }


    }
}
