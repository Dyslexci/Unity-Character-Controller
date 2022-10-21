using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Cinemachine;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerInputs))]
[RequireComponent(typeof(CharacterController))]
public class FirstPersonMovementController : MonoBehaviour
{
    // Private variables

    public bool grounded = true;
    float verticalLookRotation;
    public Transform cameraTransform;
    private PlayerInputs inputs;
    PlayerInput inputManager;
    private float speed;
    private float targetRotation = 0.0f;
    private float verticalVelocity;
    private float terminalVelocity = 53.0f;
    public Vector3 targetDirection { get; private set; }
    private float fallTimeoutDelta;
    private bool hasAnimator;
    public Animator animator;
    bool hasUsedDoubleJump;
    CapsuleCollider capsuleCollider;


    // System variables
    public CharacterController controller { get; private set; }
    private GameObject mainCamera;
    public CinemachineVirtualCamera virtualCamera { get; private set; }
    public MovementInfo movementInfo { get; private set; }
    StateFactory stateFactory;

    void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main.gameObject;
        }
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
        inputs = GetComponent<PlayerInputs>();
        inputManager = GetComponent<PlayerInput>();
        fallTimeoutDelta = fallTimeout;
        jumpsRemaining = maxNumberOfJumps;
        inheritedVelocity = Vector3.zero;
        movementInfo = new MovementInfo();
        stateFactory = GetComponent<StateFactory>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        colliderOrigiRadius = capsuleCollider.radius;
        virtualCamera = cameraTransform.gameObject.GetComponent<CinemachineVirtualCamera>();
        baseCameraFOV = virtualCamera.m_Lens.FieldOfView;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        hasAnimator = animator;
        if (hasAnimator)
            animator.ResetTrigger("Landing");
        StartCoroutine(DebugCurve());
    }

    void Update()
    {
        if (inputs.pause)
        {
            if (PauseManager.instance.paused)
            {
                PauseManager.instance.Unpause();
            }
            else
            {
                PauseManager.instance.Pause();
            }
        }
        inputs.pause = false;

        if (PauseManager.instance.paused) return;

        if (inputs.jump)
        {
            _lastJumpPressed = Time.time;
        }
        stateFactory.DetermineState();
        RunCollisionChecks();
        

        CalculateJumpApex();
        CalculateJump();

        PlayerMovement();
        if (teleporting)
            Teleport();
    }

    private void LateUpdate()
    {
        MouseLook();
        //GetGroundedObj();
    }

    private void FixedUpdate()
    {
        //GetGroundedObj();
    }

    [Header("MOVEMENT")]
    [Tooltip("The velocity bonus provided by reaching the jump apex")]
    [SerializeField] private float apexBonus = 50;
    [Tooltip("The gravity applied when falling - must be negative")]
    public float gravity = -35f;
    [Tooltip("The maximum jump height")]
    [SerializeField] private float jumpHeight = 2.5f;
    [Tooltip("The default horizontal velocity")]
    public float moveSpeed = 6f;
    [Tooltip("The horizontal velocity when sprinting")]
    public float sprintSpeed = 8f;
    public float doubleSprintSpeed = 10f;
    public float crouchSpeed = 4f;
    public float crouchSprintSpeed = 5f;
    public float proneSpeed = 2f;
    public float proneSprintSpeed = 3f;
    [Tooltip("Enable the stamina system")]
    public bool enableStamina = false;
    [Tooltip("Maximum stamina in seconds - the amount of time which can be spent sprinting")]
    public float staminaInSeconds = 10f;
    [Tooltip("The multiplier to horizontal velocity")]
    public float movementModifier = 1f;
    [Tooltip("Movement acceleration and deceleration")]
    public float acceleration = 20f;
    [Tooltip("The time after falling before which the player is marked as falling for animation purposes")]
    public float fallTimeout = 0.15f;
    [Tooltip("The time after falling before which the player is marked as falling for animation purposes")]
    public float staminaUsageThreshold = 50f;
    public float staminaRechargeDelay = 3f;
    public float staminaRechargeTime = 3f;
    public float stamina = 100f;
    public bool enableCrouch = true;
    public bool enableProne = true;
    public bool enableProneSprinting = true;
    public bool enableCrouchSprinting = true;
    public bool enableSprint = true;
    public bool enableDoubleSprint = true;
    public bool enableSliding = true;

    public float targetSpeed;
    public float cameraHeight;
    public float colliderHeight;
    public float fovMultiplier;
    float staminaDrainRate => 100f / staminaInSeconds;
    float staminaRechargeRate => 100f / staminaRechargeTime;
    public bool canSprint => recharging ? stamina > staminaUsageThreshold : (stamina > 0 || !enableStamina);
    bool recharging = false;
    IEnumerator recharger;
    bool aiming;
    public float currentSpeed { get; private set; }
    public Vector3 currentMotion { get; private set; }

    /// <summary>
    /// Moves the player based on current input, varying dependant on sprint state, use of a gamepad. Also changes animations.
    /// </summary>
    private void PlayerMovement()
    {
        float _targetSpeed = targetSpeed * movementModifier;
        bool walking = false;
        bool sprinting = false;

        ManageStamina();

        if (inputs.move == Vector2.zero)
        {
            _targetSpeed = 0.0f;
        }
        if (inputs.crouch)
        {
            inputs.crouch = false;
        }

        if (hasAnimator)
        {
            animator.SetBool("Walking", walking);
            animator.SetBool("Running", sprinting);
        }


        float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = !inputManager.currentControlScheme.Equals("KeyboardMouse") ? inputs.move.magnitude : 1f;

        if (_targetSpeed != 0.0f)
        {
            var _apexBonus = Mathf.Sign(currentHorizontalSpeed) * apexBonus * apexPoint;
            currentHorizontalSpeed += _apexBonus * Time.deltaTime;
        }

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < _targetSpeed - speedOffset || currentHorizontalSpeed > _targetSpeed + speedOffset)
        {
            // apply speed to non-linear gradient
            speed = Mathf.Lerp(currentHorizontalSpeed, _targetSpeed * inputMagnitude, Time.deltaTime * acceleration);
            speed = Mathf.Round(speed * 1000f) / 1000f;
        }
        else
        {
            speed = _targetSpeed;
        }

        Vector3 inputDirection = new Vector3(inputs.move.x, 0.0f, inputs.move.y).normalized;

        if (inputs.move != Vector2.zero)
        {
            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
        }
        //DownhillSliding();
        targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;

        Vector3 inputMotion = targetDirection.normalized * (speed * Time.deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime;
        if (stateFactory.movementState == MovementState.Sliding && DownhillSliding() && currentSpeed < maxSlideSpeed)
            inputMotion += new Vector3(slopeNormal.x, -slopeNormal.y, slopeNormal.z) * (slidingSpeed * Time.deltaTime);

        currentMotion = inputMotion;
        //inputMotion += new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime;
        // move the player
        GetGroundedObj();
        controller.Move(inputMotion);
        currentSpeed = speed;
        
        
    }
    Vector3 slopeNormal;
    float slidingSpeed = 0;
    float slideAccel = 1f;
    float maxSlideSpeed = 15f;
    float slideT = 0;
    public AnimationCurve slideSpeed = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve walkAcceleration = AnimationCurve.Linear(0, 0, 1, 1);

    IEnumerator DebugCurve()
    {
        float t = 0;
        while(t < 1.1f)
        {
            print("t = " + t + ", eval = " +slideSpeed.Evaluate(t));
            t += .1f;
            yield return new WaitForEndOfFrame();
        }
    }
    bool DownhillSliding()
    {
        if (grounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, (controller.height / 2) + .5f, groundLayers)) {
            slopeNormal = hit.normal;
            float coefficient = stateFactory.movementState == MovementState.Sliding ? 1 : 0;
            float targetSpeed = maxSlideSpeed * coefficient;
            slideT += slideAccel * Time.deltaTime;
            if(slidingSpeed < targetSpeed - .1f)
            {
                slidingSpeed = Mathf.Lerp(0, targetSpeed, slideT);
                slidingSpeed = Mathf.Round(slidingSpeed * 1000f) / 1000f;
            }
            print("Target speed: " + targetSpeed);
            print("Actual speed: " + slidingSpeed);
            return true;
        }
        return false;
    }

    void ManageStamina()
    {
        bool doubleSprinting = stateFactory.movementState == MovementState.DoubleSprinting;
        bool sprinting = stateFactory.movementState == MovementState.Sprinting;
        if (enableDoubleSprint)
        {
            if(doubleSprinting && stamina > 0)
            {
                stamina -= staminaDrainRate * Time.deltaTime;
            }
            if (stamina < 100f && !doubleSprinting && !recharging)
            {
                recharger = RegenerateStamina();
                StartCoroutine(recharger);
                recharging = true;
                print("starting to recharge");
            }
            else if (doubleSprinting && recharging)
            {
                StopCoroutine(recharger);
                recharger = null;
                recharging = false;
                print("canceling recharge");
            }
        } else
        {
            if(sprinting && stamina > 0)
            {
                stamina -= staminaDrainRate * Time.deltaTime;
            }
            if (stamina < 100f && !sprinting && !recharging)
            {
                recharger = RegenerateStamina();
                StartCoroutine(recharger);
                recharging = true;
                print("starting to recharge");
            }
            else if (sprinting && recharging)
            {
                StopCoroutine(recharger);
                recharger = null;
                recharging = false;
                print("canceling recharge");
            }
        }


    }

    IEnumerator RegenerateStamina()
    {
        yield return new WaitForSeconds(staminaRechargeTime);
        while (stamina < 100)
        {
            stamina += staminaRechargeRate * Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        stamina = 100f;
    }

    bool teleporting;
    Vector3 teleportPos;
    /// <summary>
    /// Teleports the player to the given position at the end of the next frame, in line with the order of operations of movement.
    /// </summary>
    /// <param name="newPosition">New position.</param>
    public void TeleportPlayer(Vector3 newPosition)
    {
        teleporting = true;
        teleportPos = newPosition;
    }

    void Teleport()
    {
        teleporting = false;
        controller.enabled = false;
        transform.position = teleportPos;
        controller.enabled = true;
        teleportPos = Vector3.zero;
    }

    [Header("JUMP")]
    [Tooltip("Clamp the fall velocity")]
    [SerializeField] private float fallClamp = -40f;
    [Tooltip("The maximum jump apex value")]
    [SerializeField] private float _jumpApexThreshold = 10f;
    [Tooltip("The time after walking off an edge where the player can still jump")]
    [SerializeField] private float coyoteTimeThreshold = 0.2f;
    [Tooltip("The time before landing within which a player can line up a second jump")]
    [SerializeField] private float _jumpBuffer = 0.1f;

    public int maxNumberOfJumps = 1;
    public int jumpsRemaining;
    bool walkedOffEdge;

    public bool jumpingThisFrame { get; private set; }

    // private jump variables
    private bool _coyoteUsable;
    //private bool _endedJumpEarly = true;
    private float apexPoint; // Becomes 1 at the apex of a jump
    private float _lastJumpPressed;
    private bool canUseCoyote => _coyoteUsable && !collisionDown && timeLeftGrounded + coyoteTimeThreshold > Time.time;
    private bool hasBufferedJump => grounded && _lastJumpPressed + _jumpBuffer > Time.time;
    private bool hasExtraJumps => jumpsRemaining > 0;

    private void CalculateJumpApex()
    {
        if (!collisionDown)
        {
            // Gets stronger the closer to the top of the jump
            apexPoint = Mathf.InverseLerp(_jumpApexThreshold, 0, Mathf.Abs(controller.velocity.y));
        }
        else
        {
            apexPoint = 0;
        }
    }

    /// <summary>
    /// Performs any jump action, with limitations based on contact with the ground, buffered jumps, double jumping and coyote time.
    /// </summary>
    private void CalculateJump()
    {
        if (grounded || canUseCoyote || hasExtraJumps)
        {
            // reset the fall timeout timer
            fallTimeoutDelta = fallTimeout;

            // stop our velocity dropping infinitely when grounded
            if (verticalVelocity < 0.0f && grounded) verticalVelocity = -50f;
            if (walkedOffEdge) { verticalVelocity = 0f; walkedOffEdge = false; }
            // Jump
            if (inputs.jump && (canUseCoyote || hasBufferedJump || hasExtraJumps))
            {
                _coyoteUsable = false;
                timeLeftGrounded = float.MinValue;
                jumpingThisFrame = true;
                jumpsRemaining -= 1;
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

                // update animator if using character
                if (hasAnimator)
                {
                    animator.ResetTrigger("Landing");
                    animator.SetTrigger("Jump");
                    animator.SetBool("Jumping", true);
                }
                inputs.jump = false;
            } else
            {
                jumpingThisFrame = false;
            }
        }
        else
        {
            // fall timeout
            if (fallTimeoutDelta >= 0.0f)
            {
                fallTimeoutDelta -= Time.deltaTime;
            }
            // if we are not grounded, do not jump
            inputs.jump = false;
        }
        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (verticalVelocity < terminalVelocity) verticalVelocity += gravity * Time.deltaTime;

        if (collisionUp) if (verticalVelocity > 0) verticalVelocity = 0;

        if (verticalVelocity < fallClamp) verticalVelocity = fallClamp;
    }

    [Header("ROTATION")]
    [Tooltip("The horizontal mouse sensitivity multiplier")]
    public float mouseSensX = 1;
    [Tooltip("The vertical mouse sensitivity multiplier")]
    public float mouseSensY = 1;
    [Tooltip("The minimum and maximum angles the player can rotate vertically")]
    public Vector2 lookAngleMinMax = new Vector2(-75, 80);

    public Vector3 cameraRot { get; private set; }
    public bool lookingAtEnemy;

    /// <summary>
    /// Moves the camera in relation to mouse or gamepad input. If using a gamepad, the sensitivity of movement is halved, and halved again if looking at an enemy
    /// in order to support a basic level of aim assist.
    /// </summary>
    private void MouseLook()
    {
        float sensitivity = GlobalCharacterVariables.cameraSensitivity;
        string inputScheme = inputManager.currentControlScheme;
        if (!inputScheme.Equals("KeyboardMouse"))
        {
            sensitivity = sensitivity * .75f;
        }
        if (!inputScheme.Equals("KeyboardMouse") && lookingAtEnemy)
        {
            sensitivity = sensitivity * .5f;
        }
        transform.Rotate(Vector3.up * inputs.look.x * Time.deltaTime * sensitivity);
        //cameraTransform.Rotate(Vector3.up * inputs.look.x * Time.deltaTime * sensitivity);
        verticalLookRotation -= inputs.look.y * Time.deltaTime * sensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, lookAngleMinMax.x, lookAngleMinMax.y);
        cameraTransform.localEulerAngles = Vector3.left * verticalLookRotation;
        cameraRot = cameraTransform.forward;
    }

    [Header("COLLISION")]
    [Tooltip("Useful for rough ground")]
    public float groundedOffset = 0.68f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float groundedRadius = 0.5f;
    [Tooltip("What layers the character uses as ground")]
    public LayerMask groundLayers;
    [Tooltip("What layers the character inherits velocity from while in contact")]
    public LayerMask inheritVelocityLayers;
    bool collisionDown, collisionUp;
    float timeLeftGrounded;
    GameObject touchedObj;
    Vector3 touchedObjLastPosition, touchedObjectContactPoint, inheritedVelocity;

    /// <summary>
    /// Checks whether the player is in contact with either the ground or ceiling, and modifiers animations.
    /// </summary>
    void RunCollisionChecks()
    {
        bool groundedCheck = CheckGrounded();
        if (collisionDown && !groundedCheck)
        {
            timeLeftGrounded = Time.time;
        }
        else if (!collisionDown && groundedCheck)
        {
            jumpsRemaining = maxNumberOfJumps;
            _coyoteUsable = true; // Only trigger when first touching
            if (hasAnimator) {
                animator.SetBool("Jumping", false);
                animator.ResetTrigger("Jump");
                animator.SetTrigger("Landing");
            }

            // MovementAnimationSoundEvents.instance.landSource.Play();
        }
        if (grounded && !groundedCheck) walkedOffEdge = true;
        collisionDown = groundedCheck;
        collisionUp = CheckUpCollision();
        grounded = groundedCheck;
        if (hasUsedDoubleJump && grounded)
        {
            hasUsedDoubleJump = false;
        }
    }

    private bool CheckGrounded()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
        return Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

    float getMovingPlatformDataRayLength = .15f;

    private bool GetGroundedObj()
    {
        //print("Player calculating movement");
        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit hit;
        if (grounded && Physics.Raycast(ray, out hit, (controller.height / 2) + 5, inheritVelocityLayers))
        {
            if (hit.collider.TryGetComponent(out MovingObject obj))
            {                
                if (obj.velocity.y > 0) getMovingPlatformDataRayLength = .1f;
                else getMovingPlatformDataRayLength = .15f;

                Vector3 A = Vector3.Cross(obj.angularVel.normalized, (hit.point - hit.collider.transform.position).normalized);
                float r = Vector3.Distance(hit.point, obj.centrePoint);
                Vector3 rotationalVelocity = A * r;
                transform.position += obj.velocity;
                transform.position += rotationalVelocity * Time.deltaTime;

            } else
            {
                getMovingPlatformDataRayLength = .15f;
            }
            return true;
        }
        else
        {
            getMovingPlatformDataRayLength = .15f;
            return false;
        }
    }
    bool stanceChanging;
    public float stanceSmoothing = 6f;
    float colliderOrigiRadius;

    public IEnumerator ChangeHeight(float targetColliderHeight, float cameraDistanceBelowColliderTop)
    {
        float targetColliderRadius = (targetColliderHeight < capsuleCollider.radius * 2) ? targetColliderHeight / 2
            : colliderOrigiRadius;

        float t = 0;
        while (!Mathf.Approximately(capsuleCollider.height, targetColliderHeight))
        {
            t += stanceSmoothing * Time.deltaTime;
            stanceChanging = true;
            capsuleCollider.height = (stanceSmoothing > 0 ? Mathf.MoveTowards(capsuleCollider.height, targetColliderHeight, stanceSmoothing * Time.deltaTime) : targetColliderHeight);
            controller.height = capsuleCollider.height;
            cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, (capsuleCollider.height / 2) - cameraDistanceBelowColliderTop, cameraTransform.localPosition.z);
            cameraHeight = cameraTransform.localPosition.y;
            capsuleCollider.radius = Mathf.MoveTowards(capsuleCollider.radius, targetColliderRadius, stanceSmoothing * Time.deltaTime);
            controller.radius = capsuleCollider.radius;
            colliderHeight = capsuleCollider.height;
            yield return new WaitForEndOfFrame();
        }
        stanceChanging = false;
        yield return null;
    }

    public float FOVSmoothing = 1f;
    public float baseCameraFOV { get; private set; }
    public IEnumerator ChangeFOV(float newFOVMultiplier)
    {
        float targetFOV = baseCameraFOV * newFOVMultiplier;
        float startFOV = virtualCamera.m_Lens.FieldOfView;
        float t = 0;
        while(!Mathf.Approximately(virtualCamera.m_Lens.FieldOfView, targetFOV))
        {
            t += FOVSmoothing * Time.deltaTime;
            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            yield return new WaitForEndOfFrame();
        }
        virtualCamera.m_Lens.FieldOfView = targetFOV;
        yield return null;
    }

    private bool CheckUpCollision()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y + groundedOffset, transform.position.z);
        return Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

    



    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z), groundedRadius);
        Gizmos.color = Color.red;
        if (controller == null) return;
        Gizmos.DrawLine(transform.position, transform.position - new Vector3(0, 1 + .1f, 0));
    }
}

public struct MovementInfo
{
    public Vector3 inheritedVelocity;
    public float targetSpeed;

    public void Reset()
    {
        inheritedVelocity = new Vector3(0, 0, 0);
    }
}

public enum MovementState
{
    Swimming,
    SwimSprinting,
    Climbing,
    ClimbSprinting,
    Prone,
    ProneSprinting,
    Crouching,
    CrouchSprinting,
    Walking,
    Sprinting,
    DoubleSprinting,
    Sliding
}