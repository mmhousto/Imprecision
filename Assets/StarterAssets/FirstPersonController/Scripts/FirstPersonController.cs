using UnityEngine;
using UnityEngine.InputSystem;
using Com.MorganHouston.Imprecision;
using UnityEngine.Animations.Rigging;
using System;

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
	[RequireComponent(typeof(PlayerInput))]
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 1.0f;
		[Tooltip("Sprint speed of the character in m/s")]
		public float SprintSpeed = 2f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;
		[Tooltip("Acceleration and deceleration")]
		public float SpeedChangeRate = 10.0f;

		[Space(10)]
		[Tooltip("The height the player can jump")]
		public float JumpHeight = 1.2f;
		[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
		public float Gravity = -15.0f;

		[Space(10)]
		[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
		public float JumpTimeout = 0.1f;
		[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
		public float FallTimeout = 0.15f;

		[Header("Player Grounded")]
		[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
		public bool Grounded = true;
		[Tooltip("Useful for rough ground")]
		public float GroundedOffset = -0.14f;
		[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
		public float GroundedRadius = 0.5f;
		[Tooltip("What layers the character uses as ground")]
		public LayerMask GroundLayers;

		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
        [Tooltip("The follow target's parent for First Person")]
        public Transform cinemachineFPCamParent;
        [Tooltip("The follow target's parent for Third Person")]
        public Transform cinemachineTPCamParent;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		public MultiRotationConstraint leftArmRig, headRig;

		public GameObject fpCam, tpCam, tpAimCam;

		// cinemachine
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _rotationVelocity;
		private float _verticalVelocity;
		private float _terminalVelocity = 53.0f;

		// timeout deltatime
		private float _jumpTimeoutDelta;
		private float _fallTimeoutDelta;

		[SerializeField]
		private bool isFP;

		private Vector3 _cinemachineCamFollowPos; 
		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private PlayerAnimatorManager _anim;
		private GameObject _mainCamera;
		private Player player;

		private const float _threshold = 0.01f;

		[SerializeField]
		private float currentX = 0, currentY = 0;

		private void Awake()
		{
			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
		}

		private void Start()
		{
			_controller = GetComponent<CharacterController>();
			_input = GetComponent<StarterAssetsInputs>();
			_anim = GetComponent<PlayerAnimatorManager>();
			_cinemachineCamFollowPos = CinemachineCameraTarget.transform.localPosition;

			player = Player.Instance;
			float movementSpeed = player != null ? player.MovementSpeed : 5;
            MoveSpeed += movementSpeed / 250;
			SprintSpeed += (movementSpeed / 250) * 3;

			RotationSpeed = PlayerPrefs.GetFloat("Sensitivity", 20) * 0.05f;

			//isFP = Convert.ToBoolean(PlayerPrefs.GetInt("FP", 1));
			isFP = true;

			// reset our timeouts on start
			_jumpTimeoutDelta = JumpTimeout;
			_fallTimeoutDelta = FallTimeout;
		}

		private void Update()
		{
			
			JumpAndGravity();
			GroundedCheck();
			Move();

			if (!_input.aiming)
				leftArmRig.weight = 0;
			else
				leftArmRig.weight = 1;//playerLeftArm.transform.Rotate(Vector3.right, _cinemachineTargetPitch);
		}

		private void LateUpdate()
		{
			CameraRotation();
			UpdateCamera();
		}

		public void UpdateSensitivity(float value)
        {
			RotationSpeed = value * 0.05f;
		}

		private void UpdateCamera()
        {

			if (isFP && (!fpCam.activeInHierarchy || tpAimCam.activeInHierarchy || tpCam.activeInHierarchy))
            {
				fpCam.SetActive(true);
				tpCam.SetActive(false);
				tpAimCam.SetActive(false);
				CinemachineCameraTarget.transform.SetParent(cinemachineFPCamParent);
				CinemachineCameraTarget.transform.localPosition = _cinemachineCamFollowPos;
            }
            else if(isFP == false)
            {
				if(CinemachineCameraTarget.transform.parent != cinemachineTPCamParent)
					CinemachineCameraTarget.transform.SetParent(cinemachineTPCamParent);
                if (fpCam.activeInHierarchy)
					fpCam.SetActive(false);
                if (_input.aiming && !tpAimCam.activeInHierarchy)
                {
					tpAimCam.SetActive(true);
					tpCam.SetActive(false);
                }else if (!_input.aiming && !tpCam.activeInHierarchy)
                {
					tpAimCam.SetActive(false);
					tpCam.SetActive(true);
				}
            }
			
			isFP = Convert.ToBoolean(PlayerPrefs.GetInt("FP", 1));
        }

		private void GroundedCheck()
		{
			// set sphere position, with offset
			Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
			Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
			_anim.SetInAir(!Grounded);
		}

		private void CameraRotation()
		{
			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold)
			{
				_cinemachineTargetPitch += _input.look.y * RotationSpeed * Time.deltaTime;
				_rotationVelocity = _input.look.x * RotationSpeed * Time.deltaTime;

				// clamp our pitch rotation
				if(isFP)
					_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
				else
                    _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, -80, 80f);

                // Update Cinemachine camera target pitch
                CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				//playerHead.transform.Rotate(Vector3.right, _cinemachineTargetPitch); 

				if (!_input.aiming)
					leftArmRig.weight = 0;
				else
					leftArmRig.weight = 1;

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

		private void Move()
		{
			// set target speed based on move speed, sprint speed and if sprint is pressed
			float targetSpeed = _input.sprint && !_input.aiming ? SprintSpeed : MoveSpeed;

			// a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

			// note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is no input, set the target speed to 0
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;
			_anim.SetSpeed(_input.sprint && !_input.aiming ? 2 : 1);

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

			float speedOffset = 0.1f;
			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

			// accelerate or decelerate to target speed
			if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
			{
				// creates curved result rather than a linear one giving a more organic speed change
				// note T in Lerp is clamped, so we don't need to clamp our speed
				_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

				// round speed to 3 decimal places
				_speed = Mathf.Round(_speed * 1000f) / 1000f;
			}
			else
			{
				_speed = targetSpeed;
			}

			currentX = 0;
			currentX = Mathf.MoveTowards(_input.move.x, currentX, 0.0001f * Time.deltaTime);
			currentY = 0;
			currentY = Mathf.MoveTowards(_input.move.y, currentY, 0.0001f * Time.deltaTime);

			// normalise input direction
			Vector3 inputDirection = new Vector3(currentX, 0.0f, currentY);


			_anim.SetHorz(inputDirection.x);
			_anim.SetVert(inputDirection.z);

			// note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
			// if there is a move input rotate player when the player is moving
			if (_input.move != Vector2.zero)
			{
				// move
				inputDirection = transform.right * currentX + transform.forward * currentY;
			}

			// move the player
			_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
		}

		private void JumpAndGravity()
		{
			if (Grounded)
			{
				// reset the fall timeout timer
				_fallTimeoutDelta = FallTimeout;

				// stop our velocity dropping infinitely when grounded
				if (_verticalVelocity < 0.0f)
				{
					_verticalVelocity = -2f;
				}

				// Jump
				if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				{
					// the square root of H * -2 * G = how much velocity needed to reach desired height
					_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
				}

				// jump timeout
				if (_jumpTimeoutDelta >= 0.0f)
				{
					_jumpTimeoutDelta -= Time.deltaTime;
				}
			}
			else
			{
				// reset the jump timeout timer
				_jumpTimeoutDelta = JumpTimeout;

				// fall timeout
				if (_fallTimeoutDelta >= 0.0f)
				{
					_fallTimeoutDelta -= Time.deltaTime;
				}

				// if we are not grounded, do not jump
				_input.jump = false;
			}

			// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
			if (_verticalVelocity < _terminalVelocity)
			{
				_verticalVelocity += Gravity * Time.deltaTime;
			}
		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		private void OnDrawGizmosSelected()
		{
			Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
			Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

			if (Grounded) Gizmos.color = transparentGreen;
			else Gizmos.color = transparentRed;

			// when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
			Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
		}
	}
}