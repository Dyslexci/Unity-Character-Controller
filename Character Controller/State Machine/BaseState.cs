using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MovementState", menuName = "MovementStates/BaseMovementState")]
public class BaseState : ScriptableObject
{
    public MovementState state;
    public StateFactory parentFactory;
    public FirstPersonMovementController controller;
    public float targetSpeed = 6f;
    public float cameraOffsetFromColliderTop = 0.2f;
    public float colliderHeight = 2f;
    public float fovMultiplier = 1f;

    public void Init(StateFactory parentFactory, FirstPersonMovementController controller)
    {
        this.parentFactory = parentFactory;
        this.controller = controller;
    }

    public void EnterState()
    {

    }
    public void UpdateState()
    {

    }
    public void ExitState()
    {

    }
}
