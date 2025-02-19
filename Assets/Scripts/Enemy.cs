using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Com.MorganHouston.Imprecision
{
    public class Enemy : MonoBehaviour
    {
        [Tooltip("The player")]
        public Transform target;
        [Tooltip("The distance at which the enemy will start following the player.")]
        public float followDistance = 5f; // the distance at which the enemy will start following the player
        [Tooltip("The distance at which the enemy will start attacking the player.")]
        public float attackDistance = 2f; // the distance at which the enemy will start attacking the player
        [Tooltip("Tells wether the enemy is attacking or not.")]
        public bool attacking = false;

        protected Health health;
        protected enum AIState { Idle, Follow, Attack, Patrol, Dead }; // enum for the enemy states
        protected AIState currentState = AIState.Idle; // current state of the enemy
        protected Vector3 spawnLocation;
        protected NavMeshAgent agent;
        protected float attackTime = 0;
        protected bool canAttack;
        public bool CanAttack { get { return canAttack; } set { canAttack = value; } }

        [SerializeField]
        protected float scalingfactor = 10;

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
                StartCoroutine(PushBack());
                StartCoroutine(ShowDamageTaken(myCollider.GetComponent<MeshRenderer>()));
                Player.Instance.HitTarget();
                Destroy(collision.gameObject);
            }
        }

        protected IEnumerator ShowDamageTaken(MeshRenderer mesh)
        {
            mesh.material.EnableKeyword("_EMISSION");
            yield return new WaitForSeconds(0.5f);
            mesh.material.DisableKeyword("_EMISSION");
        }

        protected IEnumerator ShowDamageTaken(SkinnedMeshRenderer mesh)
        {
            mesh.material.EnableKeyword("_EMISSION");
            yield return new WaitForSeconds(0.5f);
            mesh.material.DisableKeyword("_EMISSION");
        }

        protected IEnumerator PushBack()
        {
            if (agent != null)
            {
                agent.SetDestination(transform.position);
                // disable the agent
                agent.updatePosition = false;
                agent.updateRotation = false;
                agent.isStopped = true;
            }
            yield return new WaitForSeconds(1f);
            if (agent != null)
            {
                agent.updatePosition = true;
                agent.updateRotation = true;
                agent.isStopped = false;
            }

        }

        public virtual void Die()
        {
            Score.Instance.AddPoints(500);
            currentState = AIState.Dead;
            Destroy(gameObject);
        }

        protected void AgentSetup()
        {
            target = GameObject.FindWithTag("Player").transform;
            agent = transform.root.GetComponentInChildren<NavMeshAgent>();
            spawnLocation = transform.position;
            NavMeshHit closestHit;

            if (NavMesh.SamplePosition(spawnLocation, out closestHit, 500f, NavMesh.AllAreas))
                gameObject.transform.position = closestHit.position;
            else
                Debug.LogError("Could not find position on NavMesh!");
            //agent.Warp(spawnLocation);
            agent.speed = movementSpeed;
            health = GetComponent<Health>();
            attackSpeed = 5 - (attackSpeed / 100) * 4;
            attackTime = 0;
        }

        protected virtual void DetermineState()
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

                        if (agent.isStopped)
                        {
                            agent.Warp(transform.position);
                            agent.updatePosition = true;
                            agent.updateRotation = true;
                            agent.isStopped = false;
                            FollowTarget(target.position);
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
                            agent.Warp(transform.position);
                            agent.updatePosition = true;
                            agent.updateRotation = true;
                            agent.isStopped = false;
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

        protected void CheckCanAttack()
        {
            if (attackTime <= 0)
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
            if (targetPos != null && agent != null && agent.isOnNavMesh)
                agent.SetDestination(targetPos);
        }

        public int DetermineDamageToDeal()
        {
            Player player = Player.Instance;

            int damage = 0;
            float critValue = UnityEngine.Random.value;
            float randValue = UnityEngine.Random.Range(1, 12);
            float crit = CritChance / 100f;
            int dp = player != null ? player.DefensePower : 10;
            int ul = player != null ? player.UserLevel : 1;

            if (critValue < (1f - crit))
            {
                damage = (int)randValue + AttackPower + (ul * (1 / 6)) - dp;
            }
            else
            {
                damage = (int)(randValue + AttackPower + (ul * (1 / 6)) - dp) * 2;
            }

            if (damage < 0)
            {
                damage = (int)randValue;
            }

            return damage;
        }

        protected int DetermineDamageToTake(int hitZone)
        {
            Player player = Player.Instance;
            float randValue = 10;
            float crit = player != null ? player.CritChance / 100f : 5 / 100f;
            int ap = player != null ? player.AttackPower : 10;
            int ul = player != null ? player.UserLevel : 1;

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
                damage = (int)randValue + ap - (DefensePower + (ul * (1 / 6)));
            }
            else
            {
                damage = (int)(randValue + ap - (DefensePower + (ul * (1 / 6)))) * 2;
            }

            if (damage < 0)
            {
                damage = (int)randValue;
            }

            return damage;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position, followDistance);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, attackDistance);
        }
    }
}
