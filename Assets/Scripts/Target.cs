using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{

    public class Target : MonoBehaviour {
        [SerializeField]
        private int speed = 5;

        private GameObject player;

        private int currentLevel, minSpeed, maxSpeed;

        public GameObject[] particles;

        void Start() {
            player = GameObject.FindWithTag("Player");
            currentLevel = GameManager.Instance.LevelSelected;
            minSpeed = (int)(5 + ((currentLevel/2 * 1.2f) / 2));
            maxSpeed = (int)(10 + ((currentLevel * 1.2f) / 2));

            speed = Random.Range(minSpeed, maxSpeed);

        }

        void Update() {
            transform.root.RotateAround(player.transform.position, Vector3.up, Time.deltaTime * speed);

        }

        void LateUpdate() {
            transform.parent.transform.LookAt(player.transform);

        }

        void SpawnParticle(int particleToSpawn)
        {
            GameObject particle = Instantiate(particles[particleToSpawn], transform.root.position, particles[particleToSpawn].transform.rotation, transform.root.parent);
            particle.transform.parent = null;
        }

        void OnCollisionEnter(Collision col) {
            if (col.gameObject.tag == "arrow") {
                Vector3 relPos = gameObject.transform.InverseTransformPoint(col.transform.position);
                SpawnParticle(Score.Instance.GetHitPosition(relPos));
                Destroy(transform.root.gameObject);

            }
        }
    }
}
