using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.MorganHouston.Imprecision
{

    public class Arrow : MonoBehaviour
    {
        Rigidbody arrowRB;

        private float lifeTimer = 6f;
        private float timer;
        private bool hit;

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

            //life timer
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Destroy(gameObject);
            }

        }

        // rotates arrow down to add arc
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
            if (collision.collider.tag == "target")
            {
                transform.SetParent(collision.gameObject.transform, true); // attach to target.
            }
            arrowRB.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            arrowRB.isKinematic = true;


            if (collision.collider.tag == "apple" && Player.Instance.appleShotOnLevels[GameManager.Instance.levelSelected] != 1)
            {
                Player.Instance.appleShotOnLevels[GameManager.Instance.levelSelected] = 1;
                Player.Instance.applesShot++;
                Score.AddExtraPoints();
                transform.SetParent(collision.gameObject.transform.parent, true); // attach to target.
            }



        }
    }
}
