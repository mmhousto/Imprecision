using UnityEngine;
using Com.MorganHouston.Imprecision;
using UnityEngine.InputSystem;

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool aiming;
		public bool isPullingBack;
		public bool isSkipping;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

        private void Start()
        {
			SetCursorState(cursorLocked);
		}

        public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnAim(InputValue value)
        {
			AimInput(value.isPressed);
        }

		public void OnAttack(InputValue value)
        {
			AttackInput(value.isPressed);
        }

		public void OnSkip(InputValue value)
        {
			isSkipping = value.isPressed;
			//IntroSceneManager.Instance.SkipScene();
        }

		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			if (!aiming)
				sprint = !sprint;
			else if (aiming)
			{
				aiming = false;
				sprint = true;
			}
				
		}

		public void AimInput(bool newAimState)
        {
			aiming = !aiming;
        }

		public void AttackInput(bool newAttackState)
        {
			isPullingBack = !isPullingBack;
        }

		public void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}

	}
	
}