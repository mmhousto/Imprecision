using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Com.MorganHouston.Imprecision
{

    public class Arrow : MonoBehaviour
    {
        Rigidbody arrowRB;
        public GameObject applePS;
        public AudioClip explosionTarget;
        public AudioClip[] hitTree;
        public AudioClip[] hitGround;
        public AudioClip[] hitWater;
        public AudioClip[] hitEnemy;
        public AudioMixerGroup sfx;

        private GameObject audioSourceObject;
        private AudioSource audioSource;

        private float lifeTimer = 6f;
        private float timer;
        public bool hit;

        // Start is called before the first frame update
        void Start()
        {
            timer = lifeTimer;
            arrowRB = GetComponent<Rigidbody>();
            hit = false;

            if (GameObject.FindWithTag("ArrowImpact") != null)
            {
                audioSourceObject = GameObject.FindWithTag("ArrowImpact");
                audioSource = audioSourceObject.GetComponent<AudioSource>();
            }
            else
            {
                audioSourceObject = new GameObject("ArrowImpactSound");
                audioSourceObject.tag = "ArrowImpact";
                audioSource = audioSourceObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1;
            }

        }

        // Update is called once per frame
        void Update()
        {
            DestroyArrowTTL();
            
        }

        private void DestroyArrowTTL()
        {
            //life timer
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Destroy(gameObject);
            }
        }

        // Rotates arrow down to add arc
        void LateUpdate()
        {
            if (arrowRB.velocity != Vector3.zero && hit == false)
            {
                transform.rotation = Quaternion.LookRotation(arrowRB.velocity);

            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            hit = true;
            transform.SetParent(collision.gameObject.transform, true); // attach to target.

            int rand;

            switch (collision.collider.tag)
            {
                case "target":
                    audioSource.clip = explosionTarget;
                    audioSource.volume = 1;
                    audioSource.minDistance = 8;
                    audioSource.maxDistance = 75;
                    break;
                case "tree":
                    rand = Random.Range(0, hitTree.Length);
                    audioSource.clip = hitTree[rand];
                    audioSource.volume = 0.3f;
                    audioSource.minDistance = 0.5f;
                    audioSource.maxDistance = 15;
                    break;
                case "ground":
                    rand = Random.Range(0, hitGround.Length);
                    audioSource.clip = hitGround[rand];
                    audioSource.volume = 0.3f;
                    audioSource.minDistance = 0.1f;
                    audioSource.maxDistance = 10;
                    break;
                case "water":
                    rand = Random.Range(0, hitWater.Length);
                    audioSource.clip = hitWater[rand];
                    audioSource.volume = 0.3f;
                    audioSource.minDistance = 0.5f;
                    audioSource.maxDistance = 25;
                    break;
                case "Enemy":
                    rand = Random.Range(0, hitEnemy.Length);
                    audioSource.clip = hitEnemy[rand];
                    audioSource.volume = 0.3f;
                    audioSource.minDistance = 0.1f;
                    audioSource.maxDistance = 15;
                    break;
                case "apple":
                    if (Player.Instance.AppleShotOnLevels[GameManager.Instance.LevelSelected] != 1)
                    {
                        Player.Instance.AppleShotOnLevels[GameManager.Instance.LevelSelected] = 1;

#if (UNITY_IOS || UNITY_ANDROID)
                    LeaderboardManager.CheckAppleAchievements();
#endif

                    }
                    Score.Instance.AddExtraPoints();
                    //other.gameObject.GetComponent<Rigidbody>().AddExplosionForce(500f, other.transform.position, 5f);
                    collision.gameObject.GetComponent<AudioSource>().Play();
                    GameObject particle = Instantiate(applePS, transform.root.position, applePS.transform.rotation);
                    Destroy(particle, 2f);
                    Destroy(collision.gameObject, 2f);
                    Destroy(this.gameObject); // destroy arrow
                    break;
                default:
                    rand = Random.Range(0, hitGround.Length);
                    audioSource.clip = hitGround[rand];
                    audioSource.volume = 0.3f;
                    audioSource.minDistance = 0.1f;
                    audioSource.maxDistance = 10;
                    break;
            }

            audioSourceObject.transform.position = collision.GetContact(0).point;
            audioSource.outputAudioMixerGroup = sfx;
            audioSource.Play();

            // Sets arrow rb to kinematic
            arrowRB.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            arrowRB.isKinematic = true;

            gameObject.GetComponent<Collider>().enabled = false;

        }


    }
}
