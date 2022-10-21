using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerInputs : MonoBehaviour
{
	[Header("Character Input Values")]
	public Vector2 move;
	public Vector2 look;
	public Vector2 scroll;
	public bool jump;
	public bool sprint;
	public bool crouch;
	public bool prone;
	public bool aim;
	public bool primaryfire;
	public bool dash;
	public bool pause;
	public bool secondary;
	public bool interact;
	public bool increasetime;
	public bool decreasetime;

	[Header("Movement Settings")]
	public bool analogMovement;

#if !UNITY_IOS || !UNITY_ANDROID
	[Header("Mouse Cursor Settings")]
	public bool cursorLocked = true;
	public bool cursorInputForLook = true;
#endif

#if ENABLE_INPUT_SYSTEM
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

	public void OnInteract(InputValue value)
	{
		InteractInput(value.isPressed);
	}

	public void OnSprint(InputValue value)
	{
		SprintInput(value.isPressed);
	}

	public void OnCrouch(InputValue value)
	{
		CrouchInput(value.isPressed);
	}

	public void OnProne(InputValue value)
	{
		ProneInput(value.isPressed);
	}

	public void OnAim(InputValue value)
	{
		AimInput(value.isPressed);
	}

	public void OnIncreaseTime(InputValue value)
    {
		IncreaseTimeInput(value.isPressed);
    }

	public void OnDecreaseTime(InputValue value)
    {
		DecreaseTimeInput(value.isPressed);
    }

	public void OnPrimaryFire(InputValue value)
    {
		PrimaryFireInput(value.isPressed);
    }

	public void OnDash(InputValue value)
	{
		DashInput(value.isPressed);
	}

	public void OnPause(InputValue value)
	{
		PauseInput(value.isPressed);
	}

	public void OnSecondary(InputValue value)
	{
		SecondaryInput(value.isPressed);
	}

	public void OnScroll(InputValue value)
    {
		ScrollInput(value.Get<Vector2>());
    }
#else
	// old input sys if we do decide to have it (most likely wont)...
#endif


	public void MoveInput(Vector2 newMoveDirection)
	{
		move = newMoveDirection;
	} 

	public void LookInput(Vector2 newLookDirection)
	{
		look = newLookDirection;
	}

	public void ScrollInput(Vector2 newScrollDirection)
    {
		scroll = newScrollDirection;
    }

	public void JumpInput(bool newJumpState)
	{
		jump = newJumpState;
	}

	public void InteractInput(bool newInteractState)
	{
		interact = newInteractState;
	}

	public void SprintInput(bool newSprintState)
	{
		sprint = newSprintState;
	}

	public void CrouchInput(bool newCrouchState)
	{
		crouch = newCrouchState;
	}

	public void ProneInput(bool newProneState)
	{
		prone = newProneState;
	}

	public void AimInput(bool newAimState)
	{
		aim = newAimState;
	}

	public void PrimaryFireInput(bool newPrimaryFireState)
    {
		primaryfire = newPrimaryFireState;
    }

	public void DashInput(bool newDashState)
	{
		dash = newDashState;
	}

	public void PauseInput(bool newPauseState)
	{
		pause = newPauseState;
	}

	public void SecondaryInput(bool newSecondaryState)
	{
		secondary = newSecondaryState;
	}

	public void IncreaseTimeInput(bool newIncreaseState)
	{
		increasetime = newIncreaseState;
	}

	public void DecreaseTimeInput(bool newDecreaseState)
	{
		decreasetime = newDecreaseState;
	}

#if !UNITY_IOS || !UNITY_ANDROID

	private void OnApplicationFocus(bool hasFocus)
	{
		SetCursorState(cursorLocked);
	}

	private void SetCursorState(bool newState)
	{
		Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
	}

#endif

	}