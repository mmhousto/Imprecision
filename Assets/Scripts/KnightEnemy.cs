using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class KnightEnemy : Enemy
    {
        private Animator anim;
        private bool attacking = false;

        // Start is called before the first frame update
        void Start()
        {
            AgentSetup();
            anim = GetComponentInChildren<Animator>();
            anim.SetInteger("State", 0); // IDLE ANIM
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
                Debug.Log(myCollider.name);
                if (myCollider.name.Contains("Head"))
                {
                    health.TakeDamage(DetermineDamageToTake((int)HitArea.Body));
                }
                else if (myCollider.name.Contains("Body"))
                {
                    health.TakeDamage(DetermineDamageToTake((int)HitArea.Body));
                }
                else
                {
                    health.TakeDamage(DetermineDamageToTake((int)HitArea.Legs));
                }
                anim.SetTrigger("Hurt");
                Destroy(collision.gameObject);
            }
        }

        public override void Die()
        {
            currentState = AIState.Dead;
            anim.SetTrigger("Death");
            Destroy(transform.root.gameObject, 2.3f);
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
                        
                        
                        if(Vector3.Distance(transform.position, spawnLocation) > 1)
                        {
                            FollowTarget(spawnLocation);
                            anim.SetInteger("State", 1); // WALK ANIM to spawn
                        }
                        else if (anim.GetInteger("State") != 0)
                        {
                            anim.SetInteger("State", 0); // IDLE ANIM in place
                        }
                        
                        break;
                    case AIState.Follow:
                        // Check to see if close enough to player to attack, changes states and breaks case
                        if (distanceToPlayer <= attackDistance)
                        {
                            currentState = AIState.Attack;
                            break;
                        }

                        //if animation state is not set to walk, transition to walking state
                        if(anim.GetInteger("State") != 1)
                        {
                            anim.SetInteger("State", 1); // WALK ANIM
                        }
                        
                        // do follow behavior (e.g. move towards player)
                        FollowTarget(target.position);
                        
                        break;
                    case AIState.Attack:
                        // If not in attack zone but in follow distance, switch to follow state
                        if (distanceToPlayer > attackDistance && distanceToPlayer <= followDistance)
                        {
                            anim.SetInteger("State", 1);
                            currentState = AIState.Follow;
                            // do follow behavior (e.g. move towards player)
                            FollowTarget(target.position);
                            break;
                        }

                        // Checks can attack and if we are not currently attacking, then attacks
                        if (canAttack == true && attacking == false)
                            StartCoroutine(JumpAndAttack());
                        // Checks if we cant attack and aren't attacking, then follow player
                        else if (canAttack == false && attacking == false)
                        {
                            anim.SetInteger("State", 1);
                            // do follow behavior (e.g. move towards player)
                            FollowTarget(target.position);
                        }
                        break;
                    case AIState.Patrol:
                        break;
                    case AIState.Dead:
                        FollowTarget(transform.position);
                        break;
                    default:
                        currentState = AIState.Idle;
                        break;
                }
            }

        }

        IEnumerator JumpAndAttack()
        {
            attacking = true;
            anim.SetFloat("Attack", Random.Range(0, 5));
            anim.SetInteger("State", 2);
            

            // make the spider jump towards the player
            //Vector3 directionToPlayer = (target.position - transform.position).normalized;

            // check if the player is within attack range
            float distanceToPlayer = Vector3.Distance(transform.position, target.position);
            if (distanceToPlayer <= 1.5f)
            {
                // perform the attack
                target.GetComponent<Health>().TakeDamage(DetermineDamageToDeal());
            }

            yield return new WaitForSeconds(3.5f);
            attacking = false;
            attackTime = attackSpeed;
            canAttack = false;

        }


    }
}
