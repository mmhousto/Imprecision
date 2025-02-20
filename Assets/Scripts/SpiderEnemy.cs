using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class SpiderEnemy : Enemy
    {
        private Animator anim;
        private Rigidbody rb;
        private Coroutine attacking;

        // Start is called before the first frame update
        void Start()
        {
            AgentSetup();
            anim = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();
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
                StartCoroutine(PushBack());

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
                StartCoroutine(ShowDamageTaken(myCollider.GetComponent<MeshRenderer>()));
                Player.Instance.HitTarget();
                Destroy(collision.gameObject);
            }
        }

        public override void Die()
        {
            if(name.Contains("MiniBoss"))
                Score.Instance.AddPoints(1000);
            else
                Score.Instance.AddPoints(500);
            currentState = AIState.Dead;
            
            DisableAgent();
            Destroy(agent);
            Destroy(rb);

            anim.SetTrigger("Death");
            Destroy(gameObject, 1.5f);
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
                        // do idle behavior (e.g. stay in place and wait for player to get close)
                        if (distanceToPlayer <= followDistance)
                        {
                            currentState = AIState.Follow;
                            break;
                        }


                        if (Vector3.Distance(transform.position, spawnLocation) > 1)
                        {
                            FollowTarget(spawnLocation);
                        }
                        break;
                    case AIState.Follow:
                        // do follow behavior (e.g. move towards player)
                        FollowTarget(target.position);
                        if (distanceToPlayer < attackDistance)
                        {
                            currentState = AIState.Attack;
                        }

                        if (agent.isStopped && attacking == null)
                        {
                            agent.Warp(transform.position);
                            agent.updatePosition = true;
                            agent.updateRotation = true;
                            agent.isStopped = false;
                            FollowTarget(target.position);
                        }
                        break;
                    case AIState.Attack:
                        // If not in attack zone but in follow distance, switch to follow state
                        if (distanceToPlayer > attackDistance && distanceToPlayer <= followDistance)
                        {
                            currentState = AIState.Follow;
                            break;
                        }

                        if (canAttack && attacking == null)
                            attacking = StartCoroutine(JumpAndAttack());

                        if (!canAttack && agent.isStopped)
                        {
                            agent.Warp(transform.position);
                            agent.updatePosition = true;
                            agent.updateRotation = true;
                            agent.isStopped = false;
                        }
                        if(attacking == null)
                            FollowTarget(target.position);
                        break;
                    case AIState.Patrol:
                        break;
                    case AIState.Dead:
                        //FollowTarget(transform.position);
                        break;
                    default:
                        currentState = AIState.Idle;
                        break;
                }
            }

        }

        private void DisableAgent()
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
        }

        private IEnumerator JumpAndAttack()
        {
            DisableAgent();

            // make the spider jump towards the player
            Vector3 directionToPlayer = (target.position - transform.position).normalized;
            rb.AddForce(Vector3.up * 3, ForceMode.VelocityChange);
            yield return new WaitForSeconds(.1f);
            rb.AddForce(new Vector3(directionToPlayer.x, 0, directionToPlayer.z) * 15, ForceMode.Impulse);

            // check if the player is within attack range
            float distanceToPlayer = Vector3.Distance(transform.position, target.position);
            if (distanceToPlayer <= 1.5f)
            {
                // perform the attack
                target.GetComponent<Health>().TakeDamage(DetermineDamageToDeal());
            }

            yield return new WaitForSeconds(.9f);

            attackTime = attackSpeed;
            canAttack = false;
            attacking = null;
        }
    }
}
