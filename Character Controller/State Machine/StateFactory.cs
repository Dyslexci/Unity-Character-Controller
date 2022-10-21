using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class StateFactory : MonoBehaviour
{
    [SerializeField]
    public BaseState walkingState, sprintingState, doubleSprintingState, crouchState, sprintCrouchState, proneState, sprintProneState,
        climbState, sprintClimbState, slidingState;
    BaseState activeState;
    PlayerInputs inputs;
    FirstPersonMovementController controller;
    MovementInfo movementInfo;
    

    private void Awake()
    {
        inputs = GetComponent<PlayerInputs>();
        controller = GetComponent<FirstPersonMovementController>();
    }

    private void Start()
    {
        movementInfo = controller.movementInfo;
        walkingState.Init(this, controller);
        sprintingState.Init(this, controller);
        doubleSprintingState.Init(this, controller);
        crouchState.Init(this, controller);
        sprintCrouchState.Init(this, controller);
        proneState.Init(this, controller);
        sprintProneState.Init(this, controller);
        climbState.Init(this, controller);
        sprintClimbState.Init(this, controller);
        movementState = MovementState.Walking;
        activeState = walkingState;
        activeState.EnterState();
        controller.targetSpeed = activeState.targetSpeed;
        controller.cameraHeight = activeState.cameraOffsetFromColliderTop;
        controller.colliderHeight = activeState.colliderHeight;
        controller.fovMultiplier = activeState.fovMultiplier;
    }

    // Update is called once per frame
    void Update()
    {
        movementInfo.targetSpeed = activeState.targetSpeed;
    }
    bool aiming;
    public MovementState movementState { get; private set; }
    public void DetermineState()
    {
        if (inputs.aim) aiming = true;
        else aiming = false;
        inputs.aim = false;
        switch (movementState)
        {
            case MovementState.Swimming:

                break;

            case MovementState.SwimSprinting:

                break;

            case MovementState.Walking:
                if (inputs.sprint) { TransferStates(sprintingState); inputs.sprint = false; }
                else if (inputs.crouch && controller.enableCrouch) { TransferStates(crouchState); inputs.crouch = false; }
                else if (inputs.prone && controller.enableProne) { TransferStates(proneState); inputs.prone = false; }
                break;

            case MovementState.Sprinting:
                if (inputs.jump) { TransferStates(sprintingState); }
                else if (inputs.sprint && controller.enableDoubleSprint && controller.canSprint) { TransferStates(doubleSprintingState); inputs.sprint = false; }
                else if (inputs.crouch)
                {
                    if (controller.enableSliding) { TransferStates(slidingState); inputs.crouch = false; }
                    else if(controller.enableCrouch) { TransferStates(crouchState); inputs.crouch = false; }
                }
                else if (inputs.prone && controller.enableProne) { TransferStates(proneState); inputs.prone = false; }
                else if (inputs.move == Vector2.zero) { TransferStates(walkingState); }
                break;

            case MovementState.Crouching:
                if (inputs.jump) { TransferStates(walkingState); inputs.jump = false; }
                else if (inputs.crouch) { TransferStates(walkingState); inputs.crouch = false; }
                else if (inputs.prone && controller.enableProne) { TransferStates(proneState); inputs.prone = false; }
                else if (inputs.sprint)
                {
                    if (controller.enableCrouchSprinting) TransferStates(sprintCrouchState);
                    else TransferStates(sprintingState);
                    inputs.sprint = false;
                }
                break;

            case MovementState.CrouchSprinting:
                if (inputs.sprint) { TransferStates(crouchState); inputs.sprint = false; }
                else if (inputs.jump) { TransferStates(walkingState); inputs.jump = false; }
                else if (inputs.prone && controller.enableProne) { TransferStates(proneState); inputs.prone = false; }
                else if (inputs.crouch) { TransferStates(sprintingState); inputs.crouch = false; }
                else if (inputs.move == Vector2.zero) { TransferStates(crouchState); }
                break;

            case MovementState.Prone:
                if (inputs.jump) { TransferStates(walkingState); inputs.jump = false; }
                else if (inputs.crouch && controller.enableCrouch) { TransferStates(crouchState); inputs.crouch = false; }
                else if (inputs.prone) { TransferStates(walkingState); inputs.prone = false; }
                else if (inputs.sprint)
                {
                    if (controller.enableProneSprinting) TransferStates(sprintProneState);
                    else TransferStates(sprintingState);
                    inputs.sprint = false;
                }
                break;

            case MovementState.ProneSprinting:
                if (inputs.sprint) { TransferStates(proneState); inputs.sprint = false; }
                else if (inputs.jump) { TransferStates(walkingState); inputs.jump = false; }
                else if (inputs.prone) { TransferStates(walkingState); inputs.prone = false; }
                else if (inputs.crouch && controller.enableCrouch) { TransferStates(crouchState); inputs.crouch = false; }
                else if (inputs.move == Vector2.zero) { TransferStates(proneState); }
                break;

            case MovementState.DoubleSprinting:
                if (inputs.jump) { TransferStates(walkingState); }
                else if (inputs.sprint) { TransferStates(sprintingState); inputs.sprint = false; }
                else if (inputs.crouch)
                {
                    if (controller.enableSliding) { TransferStates(slidingState); inputs.crouch = false; }
                    else if (controller.enableCrouch) { TransferStates(crouchState); inputs.crouch = false; }
                }
                else if (inputs.prone && controller.enableProne) { TransferStates(proneState); inputs.prone = false; }
                else if (inputs.move == Vector2.zero) { TransferStates(walkingState); }
                break;

            case MovementState.Sliding:
                if (inputs.jump) { TransferStates(walkingState); }
                else if (inputs.sprint)
                {
                    if (controller.enableCrouchSprinting) { TransferStates(sprintCrouchState); inputs.sprint = false; }
                    else { TransferStates(sprintingState); inputs.sprint = false; }
                }
                else if (inputs.crouch)
                {
                    if (controller.enableCrouch) { TransferStates(crouchState); inputs.crouch = false; }
                    else { TransferStates(walkingState); inputs.crouch = false; }
                }
                else if (inputs.prone && controller.enableProne) { TransferStates(proneState); inputs.prone = false; }
                else if (controller.currentSpeed == 0 && inputs.move == Vector2.zero)
                {
                    if (controller.enableCrouch) { TransferStates(crouchState); }
                    else { TransferStates(walkingState); }
                }
                break;
        }
    }
    Coroutine heightChangeEnumerator;
    Coroutine fovChangeEnumerator;

    void TransferStates(BaseState newState)
    {
        if (movementState == newState.state) return;
        activeState.ExitState();
        activeState = newState;
        movementState = newState.state;
        activeState.EnterState();
        if(controller.colliderHeight != activeState.colliderHeight)
        {
            if(heightChangeEnumerator != null) StopCoroutine(heightChangeEnumerator);
            heightChangeEnumerator = StartCoroutine(controller.ChangeHeight(activeState.colliderHeight, activeState.cameraOffsetFromColliderTop));
        }
        if(controller.virtualCamera.m_Lens.FieldOfView != controller.baseCameraFOV * activeState.fovMultiplier)
        {
            if (fovChangeEnumerator != null) StopCoroutine(fovChangeEnumerator);
            fovChangeEnumerator = StartCoroutine(controller.ChangeFOV(activeState.fovMultiplier));
        }
        controller.targetSpeed = activeState.targetSpeed;
        controller.fovMultiplier = activeState.fovMultiplier;
    }

    
}
