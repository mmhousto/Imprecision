using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{
    public class CyclopsEnemy : Enemy
    {
        private Animator anim;
        private float[] animTimes =
        {
            3.167f,
            2.267f,
            2.4f,
            1.867f,
            4.667f,
            4.2f
        };

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
                if (myCollider.name == "Head")
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
                Player.Instance.HitTarget();
                anim.SetTrigger("Hurt");
                Destroy(collision.gameObject);
            }
        }

        public override void Die()
        {
            if (name.Contains("MiniBoss"))
                Score.Instance.AddPoints(300);
            else
                Score.Instance.AddPoints(250);
            currentState = AIState.Dead;
            anim.SetTrigger("Death");
            Destroy(gameObject, 4f);
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
                            anim.SetTrigger("Battlecry");
                            currentState = AIState.Follow;
                            break;
                        }


                        if (Vector3.Distance(transform.position, spawnLocation) > 1)
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
                        // switches to attack state if close enogh
                        if (distanceToPlayer < attackDistance)
                        {
                            currentState = AIState.Attack;
                        }

                        // transitions to walking state if not there
                        if (anim.GetInteger("State") != 1)
                        {
                            anim.SetInteger("State", 1); // Walking ANIM
                        }

                        // follows player
                        FollowTarget(target.position);
                        
                        break;
                    case AIState.Attack:
                        // If not in attack zone but in follow distance, switch to follow state
                        if (distanceToPlayer > attackDistance && distanceToPlayer <= followDistance)
                        {
                            currentState = AIState.Follow;
                            break;
                        }

                        // Checks can attack and if we are not currently attacking, then attacks
                        if (canAttack == true && attacking == false)
                            StartCoroutine(JumpAndAttack());
                        // Checks if we cant attack and aren't attacking, then follow player
                        else if (canAttack == false && attacking == false)
                        {
                            // transitions to walking state if not there
                            if (anim.GetInteger("State") != 1)
                            {
                                anim.SetInteger("State", 1); // Walking ANIM
                            }
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
            int randomAtt = Random.Range(0, 6);
            anim.SetFloat("Attack", randomAtt);
            anim.SetInteger("State", 2);


            // make the spider jump towards the player
            //Vector3 directionToPlayer = (target.position - transform.position).normalized;

            // check if the player is within attack range
            /*float distanceToPlayer = Vector3.Distance(transform.position, target.position);
            if (distanceToPlayer <= 1.5f)
            {
                // perform the attack
                target.GetComponent<Health>().TakeDamage(DetermineDamageToDeal());
            }*/
            //Debug.Log(anim.GetCurrentAnimatorStateInfo(0).length);
            Debug.Log(animTimes[randomAtt]);
            yield return new WaitForSeconds(animTimes[randomAtt]);
            attacking = false;
            attackTime = attackSpeed;
            canAttack = false;

        }
    }
}
