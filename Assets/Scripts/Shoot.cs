using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StarterAssets;

namespace Com.MorganHouston.Imprecision
{

    public class Shoot : MonoBehaviour
    {

        public GameObject arrow, bow1, bow2, bow3, bow4, bow5, bowFull;
        public Transform playerLookingDirection;
        public Transform arrowSpawn;
        public float shootForce = 10f;
        public Slider power;
        public AudioSource audioSource;
        private StarterAssetsInputs _input;
        private PlayerAnimatorManager _anim;
        private Player player;


        private float shotTimer = 1.75f;
        private float timer;
        public static bool canFire = true;
        private float shotStrength = 0.0f;
        private float shotSpeed = 5f;
        private float shotStamina = 75f;
        private float pullbackTimeLeft;
        private bool aiming = false;
        private bool pullingBack = false;
        private bool hasReleased = false;
        private bool startedPullingBack = false;
        private Cinemachine.CinemachineImpulseSource source;



        // Start is called before the first frame update
        void Start()
        {
            timer = 0f;
            _input = GetComponent<StarterAssetsInputs>();
            source = GetComponent<Cinemachine.CinemachineImpulseSource>();
            _anim = GetComponent<PlayerAnimatorManager>();
            player = Player.Instance;

            shootForce = player != null ? player.AttackPower : 10;
            shotSpeed = player != null ? player.AttackSpeed : 5;
            shotStamina = player != null ? player.Stamina : 75;
            pullbackTimeLeft = shotStrength / 5f;
        }

        // Update is called once per frame
        void Update()
        {
            CheckIfCanFire();

            Fire();

        }

        private void CheckIfCanFire()
        {
            // Shot timer
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = 0f;
                canFire = true;

            }
            else
            {
                canFire = false;
                shotStrength = 0f;
                startedPullingBack = false;
            }
        }

        private void Fire()
        {
            aiming = _input.aiming;
            _anim.SetDraw(aiming);
            if (!aiming) {
                shotStrength = 0f;
                power.value = shotStrength;
                return; 
            }
            pullingBack = _input.isPullingBack;

            if (pullingBack && startedPullingBack == false)
            {
                startedPullingBack = true;
                
            }

            if (pullingBack && canFire && startedPullingBack)
            {
                pullbackTimeLeft -= Time.deltaTime;
                if(pullbackTimeLeft <= 0)
                {
                    pullingBack = false;
                    pullbackTimeLeft = shotStamina / 5f;
                    RumbleManager.instance.StopRumbleNow();
                }

                shotStrength += (.95f + (shotSpeed/150)) * Time.deltaTime;
                var pullBack = Mathf.Clamp(shotStrength, 0f, 5f); // clamps shotStrength if greater than 5
                RumbleManager.instance.RumblePulse(pullBack / 6f, pullBack / 6f, shotStamina / 5f);

            }
            else if (pullingBack == false && canFire && startedPullingBack)
            {
                var pullBack = Mathf.Clamp(shotStrength, 0f, 5f); // clamps shotStrength if greater than 5
                power.value = shotStrength;

                timer = shotTimer; // reset timer to reset canFire

                Ray ray = new Ray(arrowSpawn.position, playerLookingDirection.forward);
                RaycastHit hit;
                Vector3 targetPoint;

                if (Physics.Raycast(ray, out hit, 100f))
                {
                    targetPoint = hit.point;
                }
                else
                {
                    targetPoint = ray.GetPoint(90f);
                }

                Vector3 directionWithoutSpread = targetPoint - arrowSpawn.position;

                GameObject clone = Instantiate(arrow, arrowSpawn.position, arrowSpawn.rotation) as GameObject; // spawns arrow
                Rigidbody rb = clone.GetComponent<Rigidbody>(); // gets Rigidbody of cloned arrow
                                                                //rb.velocity = arrowSpawn.forward * shootForce * pullBack; // applies velocity to it in direction facing
                rb.AddForce(directionWithoutSpread.normalized *  (10 + shootForce/25) * pullBack, ForceMode.Impulse);

#pragma warning disable CS1633 // Unrecognized #pragma directive
                clone.transform.rotation = Quaternion.LookRotation(rb.velocity); // adds arc to arrow (rotates arrow down)
#pragma warning restore CS1633 // Unrecognized #pragma directive

                Physics.IgnoreCollision(clone.GetComponent<Collider>(), GetComponent<Collider>()); // ignore collision w/ player

                source.GenerateImpulse(playerLookingDirection.forward);

                _anim.SetShot();
                startedPullingBack = false;

                if(player!= null)
                    player.FiredArrow();

                audioSource.Play();
                RumbleManager.instance.StopRumbleNow();
            }

        }

        private void AnimatePullBack()
        {
            if (pullingBack && canFire)
            {
                _anim.SetShotStrength(shotStrength);
                power.value = shotStrength;
            }
            else
            {
                _anim.SetShotStrength(shotStrength);
                power.value = shotStrength;
                bow1.SetActive(false);
                bow2.SetActive(false);
                bow3.SetActive(false);
                bow4.SetActive(false);
                bow5.SetActive(false);
                bowFull.SetActive(false);
            }
        }

        private void LateUpdate()
        {
            AnimatePullBack();
        }

    }

}
