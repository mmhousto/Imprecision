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
        public AudioMixerGroup sfx;

        private float lifeTimer = 6f;
        private float timer;
        public bool hit;

        // Start is called before the first frame update
        void Start()
        {
            timer = lifeTimer;
            arrowRB = GetComponent<Rigidbody>();
            hit = false;
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

            if (collision.collider.tag == "target")
            {
                
                GameObject audioExplosion = new GameObject("AudioExplosion");
                audioExplosion.transform.position = collision.transform.position;
                AudioSource audio = audioExplosion.AddComponent<AudioSource>();
                audio.clip = explosionTarget;
                audio.playOnAwake = false;
                audio.spatialBlend = 1;
                audio.minDistance = 8;
                audio.maxDistance = 75;
                audio.outputAudioMixerGroup = sfx;
                audio.Play();
                Destroy(audioExplosion, 1f);

            }

            // Sets arrow rb to kinematic
            arrowRB.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            arrowRB.isKinematic = true;

            gameObject.GetComponent<Collider>().enabled = false;

        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("apple"))
            {
                if (Player.Instance.AppleShotOnLevels[GameManager.Instance.LevelSelected] != 1)
                {
                    Player.Instance.AppleShotOnLevels[GameManager.Instance.LevelSelected] = 1;

#if (UNITY_IOS || UNITY_ANDROID)
                    LeaderboardManager.CheckAppleAchievements();
#endif

                }
                Score.Instance.AddExtraPoints();
                //other.gameObject.GetComponent<Rigidbody>().AddExplosionForce(500f, other.transform.position, 5f);
                other.GetComponent<AudioSource>().Play();
                GameObject particle = Instantiate(applePS, transform.root.position, applePS.transform.rotation);
                Destroy(particle, 2f);
                Destroy(other.gameObject, 2f);
                Destroy(this.gameObject); // destroy arrow
                
            }
        }


    }
}
