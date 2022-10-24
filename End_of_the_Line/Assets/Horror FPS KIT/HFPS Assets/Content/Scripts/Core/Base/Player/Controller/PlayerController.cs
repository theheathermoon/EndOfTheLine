using System;
using UnityEngine;
using System.Collections;
using ThunderWire.Utility;
using ThunderWire.Helpers;
using ThunderWire.Input;
using HFPS.Systems;

namespace HFPS.Player
{
    /// <summary>
    /// Main HFPS Player Movement Script
    /// </summary>
    [RequireComponent(typeof(CharacterController), typeof(HealthManager), typeof(FootstepsController))]
    public class PlayerController : Singleton<PlayerController>
    {
        public enum CharacterState { Stand, Crouch, Prone }
        public enum MovementState { Normal, Ladder }

        #region Structures
        [Serializable]
        public sealed class BasicSettings
        {
            public float walkSpeed = 3;
            public float runSpeed = 7;
            public float crouchSpeed = 2;
            public float proneSpeed = 1;
            public float inWaterSpeed = 2;
            public float climbSpeed = 1.5f;
            public float pushSpeed = 2;
            public float jumpHeight = 7;
            public float waterJumpHeight = 5;
            public float stateChangeSpeed = 3f;
            public float runTransitionSpeed = 8f;
        }

        [Serializable]
        public sealed class ControllerFeatures
        {
            public bool enableJump = true;
            public bool enableRun = true;
            public bool enableCrouch = true;
            public bool enableProne = true;
            public bool enableSliding = false;
            public bool airControl = false;
            public bool enableFly = false;
            public bool enableWallRicochet = true;
            public bool enableStamina = false;
            public bool enableJumpSounds = false;
        }

        [Serializable]
        public sealed class ControllerSettings
        {
            public float baseGravity = 24;
            public float inputSmoothing = 3f;
            public float minFallDistance = 0.5f;
            public float fallDamageMultiplier = 5.0f;
            public float standFallTreshold = 8;
            public float crouchFallTreshold = 4;
            public float consoleToProneTime = 0.5f;
            public float wallRicochetCap = 0.2f;
        }

        [Serializable]
        public sealed class SlidingSettings
        {
            public float slideRayLength = 1f;
            public float sphereCastRadius = 0.2f;
            public float maxSlideSpeed = 20f;
            public float slideFriction = 2f;
            public float slideSpeedIncrease = 5f;
            public float maxAdditionalFriction = 1f;
            public float additionalFrictionSpeed = 100f;
            public float slideControlMultiplier = 1f;
            public bool slideControl = true;
        }

        [Serializable]
        public sealed class PlayerStamina
        {
            public float playerMaxStamina = 1f;
            public float staminaRegenSpeed = 1f;
            public float runExhaustionSpeed = 1f;
            public float jumpExhaustion = 1f;
            public float waterJumpExhaustion = 1f;
            public float regenerateAfter = 2f;
            public bool autoRegenerate = true;
        }

        [Serializable]
        public sealed class ControllerAutoMove
        {
            public float globalAutoMove = 10f;
            public float climbUpAutoMove = 15f;
            public float climbDownAutoMove = 10f;
            public float climbFinishAutoMove = 10f;
            public float globalAutoLook = 3f;
            public float climbUpAutoLook = 3f;
            public float climbDownAutoLook = 3f;
        }

        [Serializable]
        public sealed class ControllerAdjustments
        {
            public float normalHeight = 2.0f;
            public float crouchHeight = 1.4f;
            public float proneHeight = 0.6f;
            [Space(5)]
            public float camNormalHeight = 0.9f;
            public float camCrouchHeight = 0.2f;
            public float camProneHeight = -0.4f;
            [Space(5)]
            public Vector3 normalCenter = Vector3.zero;
            public Vector3 crouchCenter = new Vector3(0, -0.3f, 0);
            public Vector3 proneCenter = new Vector3(0, -0.7f, 0);
        }

        [Serializable]
        public sealed class CameraHeadBob
        {
            public Animation cameraAnimations;
            public string cameraIdle = "CameraIdle";
            public string cameraWalk = "CameraWalk";
            public string cameraRun = "CameraRun";
            [Range(0, 5)] public float walkAnimSpeed = 1f;
            [Range(0, 5)] public float runAnimSpeed = 1f;
        }

        [Serializable]
        public sealed class ArmsHeadBob
        {
            public Animation armsAnimations;
            public string armsIdle = "ArmsIdle";
            public string armsBreath = "ArmsBreath";
            public string armsWalk = "ArmsWalk";
            public string armsRun = "ArmsRun";
            [Range(0, 5)] public float walkAnimSpeed = 1f;
            [Range(0, 5)] public float runAnimSpeed = 1f;
            [Range(0, 5)] public float breathAnimSpeed = 1f;
        }
        #endregion

        #region Public Variables
        [ReadOnly] public CharacterState characterState = CharacterState.Stand;
        [ReadOnly] public MovementState movementState = MovementState.Normal;

        private CharacterController m_characterControl;
        public CharacterController CharacterControl
        {
            get
            {
                if (!m_characterControl)
                    return m_characterControl = GetComponent<CharacterController>();

                return m_characterControl;
            }
        }

        // references
        public StabilizeKickback baseKickback;
        public StabilizeKickback weaponKickback;
        public Transform mouseLook;
        public ParticleSystem waterParticles;

        // layers
        public LayerMask surfaceCheckMask;
        public LayerMask slidingMask;

        // ground settings
        public float groundCheckOffset;
        public float groundCheckRadius;

        // controller main
        public BasicSettings basicSettings = new BasicSettings();
        public ControllerFeatures controllerFeatures = new ControllerFeatures();
        public ControllerSettings controllerSettings = new ControllerSettings();
        public SlidingSettings slidingSettings = new SlidingSettings();
        public PlayerStamina staminaSettings = new PlayerStamina();
        public ControllerAutoMove autoMoveSettings = new ControllerAutoMove();
        public ControllerAdjustments controllerAdjustments = new ControllerAdjustments();

        // head bob
        public CameraHeadBob cameraHeadBob = new CameraHeadBob();
        public ArmsHeadBob armsHeadBob = new ArmsHeadBob();

        // sounds
        public AudioClip[] jumpSounds;
        public float jumpVolume = 1f;
        #endregion

        #region Input
        private bool JumpPressed;
        private bool RunPressed;
        private bool CrouchPressed;
        private bool PronePressed;
        private bool ZoomPressed;

        private float inputX;
        private float inputY;
        private Vector2 inputMovement;

        private bool proneTimeStart;
        private float proneTime;
        private bool inProne;

        protected RandomHelper random = new RandomHelper();
        #endregion

        #region Hidden Variables
        [HideInInspector] public bool ladderReady;
        [HideInInspector] public bool isControllable;
        [HideInInspector] public bool isRunning;
        [HideInInspector] public bool isInWater;
        [HideInInspector] public bool sliding;
        [HideInInspector] public bool shakeCamera;
        [HideInInspector] public bool steadyArms;
        [HideInInspector] public float velMagnitude;
        [HideInInspector] public float movementSpeed;
        #endregion

        #region Private Variables
        protected Timekeeper timekeeper = new Timekeeper();
        private (string, string) ExitLadder = ("GameUI.ExitLadder", "Exit Ladder");

        private HFPS_GameManager gameManager;
        private ScriptManager scriptManager;
        private ItemSwitcher itemSwitcher;
        private HealthManager healthManager;
        private FootstepsController footsteps;
        private InputHandler.Device inputDevice = InputHandler.Device.None;

        private Vector3 moveDirection = Vector3.zero;
        private Vector3 slideDirection = Vector3.zero;
        private Vector3 currPosition;
        private Vector3 lastPosition;
        private Vector3 slopeNormal;

        private bool flyControl;
        private Vector3 flyDirection;
        private Vector3 flyModifier;

        private readonly float antiBumpFactor = .75f;
        private readonly float spamWaitTime = 0.5f;

        private float gravity;
        private float slideAngleLimit;

        private float fallDamageThreshold;
        private float fallDistance;
        private float highestPoint;

        private float currentStamina;
        private float staminaRegenWait;

        private bool antiSpam;
        private bool isGrounded;
        private bool isSliding;
        private bool isFalling;
        private bool isFoamRemoved;
        private bool isStaminaShown;
        private bool isPauseMenu;
        private bool onLadder;
        private bool wallRicochet;

        private Ladder ladder;
        private Vector3 ladderExit;

        private ControllerColliderHit colliderHit;
        private ParticleSystem foamParticles;
        #endregion

        void Awake()
        {
            footsteps = GetComponent<FootstepsController>();
            healthManager = GetComponent<HealthManager>();
            gameManager = HFPS_GameManager.Instance;
            scriptManager = ScriptManager.Instance;
            itemSwitcher = scriptManager.C<ItemSwitcher>();

            TextsSource.Subscribe(OnInitTexts);
            gravity = controllerSettings.baseGravity * 2;
        }

        void OnInitTexts()
        {
            string defaultText = ExitLadder.Item2;
            ExitLadder.Item2 = TextsSource.GetText(ExitLadder.Item1, defaultText);

            if(movementState == MovementState.Ladder)
            {
                gameManager.ShowHelpButtons(new HelpButton(ExitLadder.Item2, InputHandler.CompositeOf("Jump").GetBindingPath()), null, null, null);
            }
        }

        void Start()
        {
            slideAngleLimit = CharacterControl.slopeLimit - 0.2f;

            if (cameraHeadBob.cameraAnimations)
            {
                cameraHeadBob.cameraAnimations.wrapMode = WrapMode.Loop;
                cameraHeadBob.cameraAnimations[cameraHeadBob.cameraWalk].speed = cameraHeadBob.walkAnimSpeed;
                cameraHeadBob.cameraAnimations[cameraHeadBob.cameraRun].speed = cameraHeadBob.runAnimSpeed;
            }

            if (armsHeadBob.armsAnimations)
            {
                armsHeadBob.armsAnimations.wrapMode = WrapMode.Loop;
                armsHeadBob.armsAnimations.Stop();
                armsHeadBob.armsAnimations[armsHeadBob.armsWalk].speed = armsHeadBob.walkAnimSpeed;
                armsHeadBob.armsAnimations[armsHeadBob.armsRun].speed = armsHeadBob.runAnimSpeed;
                armsHeadBob.armsAnimations[armsHeadBob.armsBreath].speed = armsHeadBob.breathAnimSpeed;
            }

            SetPlayerMaxStamina(staminaSettings.playerMaxStamina);
        }

        void Update()
        {
            velMagnitude = CharacterControl.velocity.magnitude;

            void GetInput()
            {
                Vector2 movement;

                if ((movement = InputHandler.ReadInput<Vector2>("Move")) != null)
                {
                    if (InputHandler.CurrentDevice != InputHandler.Device.MouseKeyboard)
                    {
                        inputX = movement.x;
                        inputY = movement.y;
                        inputMovement = movement;
                    }
                    else
                    {
                        inputY = Mathf.MoveTowards(inputY, movement.y, Time.deltaTime * controllerSettings.inputSmoothing);
                        inputX = Mathf.MoveTowards(inputX, movement.x, Time.deltaTime * controllerSettings.inputSmoothing);
                        inputMovement.y = inputY;
                        inputMovement.x = inputX;
                    }
                }
            }

            //Break update when player is dead and ragdoll is activated
            if (healthManager.isDead)
            {
                if (cameraHeadBob.cameraAnimations && cameraHeadBob.cameraAnimations.transform.childCount < 1)
                    cameraHeadBob.cameraAnimations.gameObject.SetActive(false);

                return;
            }

            if (InputHandler.InputIsInitialized && !isPauseMenu)
            {
                inputDevice = InputHandler.CurrentDevice;
                ZoomPressed = InputHandler.ReadButton("Zoom");

                if (controllerFeatures.enableJump)
                {
                    JumpPressed = InputHandler.ReadButtonOnce(this, "Jump") &&
                        (!controllerFeatures.enableStamina || currentStamina > 0);
                }

                if (controllerFeatures.enableRun)
                {
                    if (inputDevice != InputHandler.Device.MouseKeyboard)
                    {
                        if (!controllerFeatures.enableStamina)
                        {
                            if (InputHandler.ReadButtonOnce(this, "Run"))
                            {
                                RunPressed = !RunPressed;
                            }
                        }
                        else
                        {
                            if (InputHandler.ReadButtonOnce(this, "Run"))
                            {
                                if (!RunPressed && currentStamina > 0)
                                {
                                    RunPressed = true;
                                }
                            }
                            else if (currentStamina < 0)
                            {
                                RunPressed = false;
                            }
                        }
                    }
                    else
                    {
                        if (!controllerFeatures.enableStamina)
                        {
                            RunPressed = InputHandler.ReadButton("Run");
                        }
                        else
                        {
                            RunPressed = InputHandler.ReadButton("Run") && currentStamina > 0;
                        }
                    }
                }

                if (!InputHandler.IsCompositesSame("Crouch", "Prone"))
                {
                    CrouchPressed = InputHandler.ReadButtonOnce(this, "Crouch");
                    PronePressed = InputHandler.ReadButtonOnce(this, "Prone");
                }
                else
                {
                    bool prone = InputHandler.ReadButton("Prone");

                    if (prone && !inProne)
                    {
                        proneTimeStart = true;
                        proneTime += Time.deltaTime;

                        if (proneTime >= controllerSettings.consoleToProneTime)
                        {
                            PronePressed = true;
                            inProne = true;
                        }
                    }
                    else if (proneTimeStart && proneTime < controllerSettings.consoleToProneTime)
                    {
                        CrouchPressed = true;
                        proneTimeStart = false;
                        proneTime = 0;
                    }
                    else
                    {
                        CrouchPressed = false;
                        PronePressed = false;
                        proneTime = 0;

                        if (!prone && inProne)
                        {
                            inProne = false;
                        }
                    }
                }

                if (isControllable)
                {
                    GetInput();

                    if (inputDevice != InputHandler.Device.MouseKeyboard && inputY < 0.7f)
                    {
                        RunPressed = false;
                    }
                }
                else
                {
                    RunPressed = false;
                    inputX = 0f;
                    inputY = 0f;
                    inputMovement = Vector2.zero;
                }
            }

            if (inputDevice.IsGamepadDevice() == 1)
            {
                if (!isPauseMenu)
                {
                    isPauseMenu = gameManager.isPaused || gameManager.isInventoryShown;
                }
                else if (!gameManager.isPaused && !gameManager.isInventoryShown)
                {
                    isPauseMenu = InputHandler.AnyInputPressed();
                }
            }

            if (controllerFeatures.enableStamina)
            {
                if (isRunning && !isInWater)
                {
                    currentStamina = Mathf.MoveTowards(currentStamina, 0f, Time.deltaTime * staminaSettings.runExhaustionSpeed);
                    staminaRegenWait = staminaSettings.regenerateAfter;

                    if(RunPressed && currentStamina <= 0f)
                    {
                        RunPressed = false;
                    }

                    if (!isStaminaShown)
                    {
                        gameManager.ShowStaminaUI(currentStamina);
                        isStaminaShown = true;
                    }
                }
                else if (staminaRegenWait <= 0 && staminaSettings.autoRegenerate)
                {
                    staminaRegenWait = 0;
                    currentStamina = Mathf.MoveTowards(currentStamina, staminaSettings.playerMaxStamina, Time.deltaTime * staminaSettings.staminaRegenSpeed);

                    if (currentStamina >= staminaSettings.playerMaxStamina && isStaminaShown)
                    {
                        gameManager.ShowStaminaUI(currentStamina, false);
                        isStaminaShown = false;
                    }
                }
                else
                {
                    staminaRegenWait -= Time.deltaTime;
                }

                if (isStaminaShown)
                {
                    gameManager.UpdateSliderValue(1, currentStamina);
                }
            }

            if (movementState == MovementState.Ladder)
            {
                //Apply ladder movement physics
                isRunning = false;
                highestPoint = transform.position.y;

                if (armsHeadBob.armsAnimations)
                    armsHeadBob.armsAnimations.CrossFade(armsHeadBob.armsIdle);

                if (cameraHeadBob.cameraAnimations)
                    cameraHeadBob.cameraAnimations.CrossFade(cameraHeadBob.cameraIdle);

                if (onLadder)
                {
                    Vector3 verticalMove = Vector3.up;
                    verticalMove *= inputY > 0.1f ? 1 : inputY < -0.1f ? -1 : 0;
                    verticalMove *= basicSettings.climbSpeed;

                    if (CharacterControl.enabled)
                        isGrounded = (CharacterControl.Move(verticalMove * Time.deltaTime) & CollisionFlags.Below) != 0;

                    if (ladder != null && Vector3.Distance(transform.position, ladder.LadderUp) < 0.2f)
                    {
                        LerpPlayerLadder(ladderExit);
                        onLadder = false;
                    }

                    if (isGrounded && inputY < 0 || JumpPressed)
                        LadderExit();
                }
            }
            else
            {
                if (isGrounded)
                {
                    isSliding = false;
                    try
                    {
                        if (SlopeCast(out slopeNormal, out float slopeAngle))
                        {
                            isSliding = slopeAngle > slideAngleLimit;
                        }
                    }
                    catch { }
                    sliding = isSliding;

                    //Change player affect type to running when they are not in water
                    if (characterState == CharacterState.Stand && !isInWater)
                    {
                        isRunning = isControllable && RunPressed && inputY > 0.5f && !ZoomPressed;
                    }
                    else
                    {
                        isRunning = false;
                    }

                    if (isSliding)
                    {
                        //Apply sliding physics
                        highestPoint = transform.position.y;

                        float maxSpeed = slidingSettings.maxSlideSpeed;
                        float friction = slidingSettings.slideFriction;
                        float increaseSpeed = slidingSettings.slideSpeedIncrease;
                        float speed = Mathf.Clamp(moveDirection.magnitude, 1, maxSpeed);

                        if (moveDirection.y < 0.1f)
                            speed = Mathf.MoveTowards(speed, maxSpeed, Time.deltaTime * increaseSpeed);

                        moveDirection.x += (1f - slopeNormal.y) * slopeNormal.x * friction;
                        moveDirection.z += (1f - slopeNormal.y) * slopeNormal.z * friction;

                        if (slidingSettings.slideControl)
                            moveDirection.z += inputMovement.x * slidingSettings.slideControlMultiplier;

                        moveDirection = moveDirection.normalized * speed;
                        slideDirection = moveDirection;

                        isSliding = false;
                    }
                    else if(slideDirection.magnitude > slidingSettings.maxAdditionalFriction)
                    {
                        //Apply additional slide friction
                        slideDirection = Vector3.MoveTowards(slideDirection, Vector3.zero, Time.deltaTime * slidingSettings.additionalFrictionSpeed);
                        moveDirection = slideDirection;
                    }
                    else
                    {
                        //Apply fall damage and play footstep land sounds
                        if (isFalling && !isSliding)
                        {
                            fallDistance = highestPoint - currPosition.y;

                            if (fallDistance > fallDamageThreshold)
                            {
                                ApplyFallingDamage(fallDistance);
                            }

                            if (fallDistance < fallDamageThreshold && fallDistance > controllerSettings.minFallDistance)
                            {
                                footsteps.OnJump();
                                StartCoroutine(ApplyKickback(new Vector3(7, UnityEngine.Random.Range(-1.0f, 1.0f), 0), 0.15f));
                            }

                            isFalling = false;
                        }

                        //Assign movement speed
                        if (characterState == CharacterState.Stand)
                        {
                            if (!ZoomPressed)
                            {
                                if (!isInWater && !isRunning)
                                {
                                    movementSpeed = basicSettings.walkSpeed;
                                }
                                else if (!isInWater && isRunning)
                                {
                                    movementSpeed = Mathf.MoveTowards(movementSpeed, basicSettings.runSpeed, Time.deltaTime * basicSettings.runTransitionSpeed);
                                }
                                else if (isInWater)
                                {
                                    movementSpeed = basicSettings.inWaterSpeed;
                                }
                            }
                            else
                            {
                                movementSpeed = basicSettings.crouchSpeed;
                            }
                        }
                        else if (characterState == CharacterState.Crouch)
                        {
                            movementSpeed = basicSettings.crouchSpeed;
                        }
                        else if (characterState == CharacterState.Prone)
                        {
                            movementSpeed = basicSettings.proneSpeed;
                        }

                        //Apply normal movement physics
                        moveDirection = new Vector3(inputMovement.x, -antiBumpFactor, inputMovement.y);
                        moveDirection = transform.TransformDirection(moveDirection);
                        moveDirection *= movementSpeed;

                        //Jump player
                        if (isControllable && JumpPressed && movementState != MovementState.Ladder)
                        {
                            if (characterState == CharacterState.Stand)
                            {
                                if (controllerFeatures.enableStamina)
                                {
                                    if (!isStaminaShown)
                                    {
                                        gameManager.ShowStaminaUI(currentStamina);
                                        isStaminaShown = true;
                                    }

                                    currentStamina -= isInWater ? staminaSettings.waterJumpExhaustion : staminaSettings.jumpExhaustion;
                                    if (currentStamina < 0) currentStamina = 0;
                                    staminaRegenWait = staminaSettings.regenerateAfter;
                                }

                                if (!isInWater)
                                {
                                    moveDirection.y = Mathf.Sqrt(basicSettings.jumpHeight * -2f * gravity);

                                    if (controllerFeatures.enableJumpSounds && jumpSounds.Length > 0)
                                    {
                                        int jumpIndex = random.Range(0, jumpSounds.Length);
                                        Utilities.PlayOneShot2D(transform.position, jumpSounds[jumpIndex], jumpVolume);
                                    }
                                }
                                else
                                {
                                    moveDirection.y = Mathf.Sqrt(basicSettings.waterJumpHeight * -2f * gravity);
                                }
                            }
                            else
                            {
                                if (CheckDistance() > 1.6f)
                                {
                                    characterState = CharacterState.Stand;
                                    StartCoroutine(AntiSpam());
                                }
                            }
                        }
                    }

                    //Play camera head bob animations
                    if (!shakeCamera)
                    {
                        if (!steadyArms)
                        {
                            if (!isRunning && velMagnitude > basicSettings.crouchSpeed)
                            {
                                if (armsHeadBob.armsAnimations)
                                    armsHeadBob.armsAnimations.CrossFade(armsHeadBob.armsWalk);

                                if (cameraHeadBob.cameraAnimations)
                                    cameraHeadBob.cameraAnimations.CrossFade(cameraHeadBob.cameraWalk);
                            }
                            else if (isRunning && velMagnitude > basicSettings.walkSpeed)
                            {
                                if (armsHeadBob.armsAnimations)
                                    armsHeadBob.armsAnimations.CrossFade(armsHeadBob.armsRun);

                                if (cameraHeadBob.cameraAnimations)
                                    cameraHeadBob.cameraAnimations.CrossFade(cameraHeadBob.cameraRun);
                            }
                            else if (velMagnitude < basicSettings.crouchSpeed)
                            {
                                if (armsHeadBob.armsAnimations)
                                    armsHeadBob.armsAnimations.CrossFade(armsHeadBob.armsBreath);

                                if (cameraHeadBob.cameraAnimations)
                                    cameraHeadBob.cameraAnimations.CrossFade(cameraHeadBob.cameraIdle);
                            }
                        }
                        else
                        {
                            if (armsHeadBob.armsAnimations)
                                armsHeadBob.armsAnimations.CrossFade(armsHeadBob.armsIdle);

                            if (cameraHeadBob.cameraAnimations)
                                cameraHeadBob.cameraAnimations.CrossFade(cameraHeadBob.cameraIdle);
                        }
                    }
                }
                else
                {
                    currPosition = transform.position;

                    if (!isFalling)
                    {
                        highestPoint = transform.position.y;
                    }

                    if (currPosition.y > lastPosition.y)
                    {
                        highestPoint = transform.position.y;
                    }

                    if (controllerFeatures.airControl)
                    {
                        moveDirection.x = inputX * movementSpeed;
                        moveDirection.z = inputY * movementSpeed;
                        moveDirection = transform.TransformDirection(moveDirection);
                    }

                    if (!shakeCamera)
                    {
                        if (armsHeadBob.armsAnimations)
                            armsHeadBob.armsAnimations.CrossFade(armsHeadBob.armsIdle);

                        if (cameraHeadBob.cameraAnimations)
                            cameraHeadBob.cameraAnimations.CrossFade(cameraHeadBob.cameraIdle);
                    }

                    isFalling = true;
                }

                if (!isInWater && isControllable && !antiSpam)
                {
                    //Crouch Player
                    if (CrouchPressed && !flyControl && controllerFeatures.enableCrouch)
                    {
                        if (characterState != CharacterState.Crouch)
                        {
                            if (CheckDistance() > 1.6f)
                            {
                                characterState = CharacterState.Crouch;
                            }
                        }
                        else if (characterState != CharacterState.Stand)
                        {
                            if (CheckDistance() > 1.6f)
                            {
                                characterState = CharacterState.Stand;
                            }
                        }

                        StartCoroutine(AntiSpam());
                    }

                    //Prone Player
                    if (PronePressed && !flyControl && controllerFeatures.enableProne)
                    {
                        if (characterState != CharacterState.Prone)
                        {
                            characterState = CharacterState.Prone;
                        }
                        else if (characterState == CharacterState.Prone)
                        {
                            if (CheckDistance() > 1.6f)
                            {
                                characterState = CharacterState.Stand;
                            }
                        }

                        StartCoroutine(AntiSpam());
                    }
                }
            }

            //Play foam particles when player is in water
            if (foamParticles && !isFoamRemoved)
            {
                if (isInWater && !ladderReady)
                {
                    if (velMagnitude > 0.01f)
                    {
                        if (foamParticles.isStopped) foamParticles.Play(true);
                    }
                    else
                    {
                        if (foamParticles.isPlaying) foamParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    }
                }
                else
                {
                    foamParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    StartCoroutine(RemoveFoam());
                    isFoamRemoved = true;
                }
            }

            if (characterState == CharacterState.Stand)
            {
                //Stand Position
                CharacterControl.height = controllerAdjustments.normalHeight;
                CharacterControl.center = controllerAdjustments.normalCenter;
                fallDamageThreshold = controllerSettings.standFallTreshold;

                Vector3 camPosition = mouseLook.localPosition;
                camPosition.y = Mathf.MoveTowards(camPosition.y, controllerAdjustments.camNormalHeight, Time.smoothDeltaTime * basicSettings.stateChangeSpeed);
                mouseLook.localPosition = camPosition;
            }
            else if (characterState == CharacterState.Crouch)
            {
                //Crouch Position
                CharacterControl.height = controllerAdjustments.crouchHeight;
                CharacterControl.center = controllerAdjustments.crouchCenter;
                fallDamageThreshold = controllerSettings.crouchFallTreshold;

                Vector3 camPosition = mouseLook.localPosition;
                camPosition.y = Mathf.MoveTowards(camPosition.y, controllerAdjustments.camCrouchHeight, Time.smoothDeltaTime * basicSettings.stateChangeSpeed);
                mouseLook.localPosition = camPosition;
            }
            else if (characterState == CharacterState.Prone)
            {
                //Prone Position
                CharacterControl.height = controllerAdjustments.proneHeight;
                CharacterControl.center = controllerAdjustments.proneCenter;
                fallDamageThreshold = controllerSettings.crouchFallTreshold;

                Vector3 camPosition = mouseLook.localPosition;
                camPosition.y = Mathf.MoveTowards(camPosition.y, controllerAdjustments.camProneHeight, Time.smoothDeltaTime * basicSettings.stateChangeSpeed);
                mouseLook.localPosition = camPosition;
            }

            if (movementState != MovementState.Ladder && CharacterControl.enabled)
            {
                if (!flyControl)
                {
                    //Apply movement physics and gravity
                    moveDirection.y += gravity * Time.deltaTime;
                }
                else
                {
                    //Apply flying physics
                    float flyingSpeed = 2f;
                    float dirChangeSpeed = 1.5f;
                    float startedFlyingSpeed = 5f;

                    if (Mathf.Abs(inputX) > 0)
                    {
                        flyModifier.x = Mathf.Lerp(flyModifier.x, inputX, Time.deltaTime * dirChangeSpeed);
                    }
                    if (Mathf.Abs(inputY) > 0)
                    {
                        flyModifier.z = Mathf.Lerp(flyModifier.z, inputY, Time.deltaTime * dirChangeSpeed);
                    }

                    if (InputHandler.ReadButton("Jump"))
                    {
                        flyModifier.y = Mathf.Lerp(flyModifier.y, 1, Time.deltaTime * dirChangeSpeed);
                    }
                    if (InputHandler.ReadButton("Crouch"))
                    {
                        flyModifier.y = Mathf.Lerp(flyModifier.y, -1, Time.deltaTime * dirChangeSpeed);
                    }

                    flyDirection = flyModifier;
                    flyDirection *= flyingSpeed;
                    flyDirection = transform.TransformDirection(flyDirection);
                    moveDirection = Vector3.Lerp(moveDirection, flyDirection, startedFlyingSpeed * Time.deltaTime);
                }

                isGrounded = (CharacterControl.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;

                if (controllerFeatures.enableFly)
                {
                    if (IsGrounded())
                    {
                        flyDirection = Vector3.zero;
                        flyControl = false;
                    }
                    else if (!flyControl)
                    {
                        flyControl = true;
                    }
                }

                if (wallRicochet && isGrounded)
                    wallRicochet = false;
            }
        }

        void LateUpdate()
        {
            lastPosition = currPosition;
        }

        float CheckDistance()
        {
            Vector3 pos = transform.position + CharacterControl.center - new Vector3(0, CharacterControl.height / 2, 0);

            if (Physics.SphereCast(pos, CharacterControl.radius, transform.up, out RaycastHit hit, 10, surfaceCheckMask))
            {
                Debug.DrawLine(pos, hit.point, Color.yellow, 2.0f);
                return hit.distance;
            }
            else
            {
                Debug.DrawLine(pos, hit.point, Color.yellow, 2.0f);
                return 3;
            }
        }

        bool SlopeCast(out Vector3 normal, out float angle)
        {
            Vector3 origin = new Vector3(transform.position.x,
                transform.position.y - (CharacterControl.height / 2),
                transform.position.z);

            if (colliderHit != null)
            {
                if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, slidingSettings.slideRayLength, slidingMask, QueryTriggerInteraction.Ignore))
                {
                    if (colliderHit != null && hit.collider != null && colliderHit.gameObject == hit.collider.gameObject)
                    {
                        normal = hit.normal;
                        angle = Vector3.Angle(hit.normal, Vector3.up);
                        return true;
                    }
                }
                else if (colliderHit != null)
                {
                    normal = colliderHit.normal;
                    angle = Vector3.Angle(colliderHit.normal, Vector3.up);
                    return true;
                }
            }

            normal = Vector3.zero;
            angle = 0f;
            return false;
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.normal.y < 0.1f && !isGrounded && controllerFeatures.enableWallRicochet)
            {
                Vector3 tempMotion = moveDirection;
                tempMotion.y = 0f;
                tempMotion.Normalize();

                float wallDot = Vector3.Dot(tempMotion, -hit.normal);
                float ricochet = Mathf.Max(controllerSettings.wallRicochetCap, 1.0f - wallDot);

                Vector3 ricochetDir = Vector3.Reflect(moveDirection, hit.normal);
                Vector3 newMotion = ricochetDir * ricochet;

                float y = moveDirection.y;
                moveDirection = newMotion;
                moveDirection.y = y;

                wallRicochet = true;
            }

            colliderHit = hit;

            if (colliderHit != null && hit.collider != null)
            {
                Rigidbody body = hit.collider.attachedRigidbody;

                //dont move the rigidbody if the character is on top of it
                if (CharacterControl.collisionFlags == CollisionFlags.Below)
                    return;

                if (body == null || body.isKinematic)
                    return;

                Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
                body.velocity = pushDir * basicSettings.pushSpeed;
            }
        }

        void ApplyFallingDamage(float fallDistance)
        {
            healthManager.ApplyDamage((int)(fallDistance * controllerSettings.fallDamageMultiplier));
            if (characterState != CharacterState.Prone) footsteps.OnJump();
            StartCoroutine(ApplyKickback(new Vector3(12, UnityEngine.Random.Range(-2.0f, 2.0f), 0), 0.1f));
        }

        public void SetPlayerState(CharacterState state)
        {
            if (state == CharacterState.Crouch)
            {
                //Crouch Position
                CharacterControl.height = controllerAdjustments.crouchHeight;
                CharacterControl.center = controllerAdjustments.crouchCenter;
                fallDamageThreshold = controllerSettings.crouchFallTreshold;
                mouseLook.localPosition = new Vector3(mouseLook.localPosition.x, controllerAdjustments.camCrouchHeight, mouseLook.localPosition.z);
            }
            else if (state == CharacterState.Prone)
            {
                //Prone Position
                CharacterControl.height = controllerAdjustments.proneHeight;
                CharacterControl.center = controllerAdjustments.proneCenter;
                fallDamageThreshold = controllerSettings.crouchFallTreshold;
                mouseLook.localPosition = new Vector3(mouseLook.localPosition.x, controllerAdjustments.camProneHeight, mouseLook.localPosition.z);
            }

            characterState = state;
        }

        /// <summary>
        /// Restore player's stamina
        /// </summary>
        public void RestoreStamina(float stamina)
        {
            currentStamina += (stamina / 100);

            if (currentStamina > staminaSettings.playerMaxStamina)
            {
                currentStamina = staminaSettings.playerMaxStamina;
            }
        }

        /// <summary>
        /// Set player's max stamina
        /// </summary>
        public void SetPlayerMaxStamina(float maxStamina)
        {
            staminaSettings.playerMaxStamina = maxStamina;
            gameManager.userInterface.StaminaSlider.maxValue = maxStamina;
            gameManager.ShowStaminaUI(maxStamina, false);
            currentStamina = maxStamina;
            isStaminaShown = false;
        }

        /// <summary>
        /// Check if player is on the ground
        /// </summary>
        public bool IsGrounded()
        {
            Vector3 pos = transform.position + CharacterControl.center - new Vector3(0, (CharacterControl.height / 2f) + groundCheckOffset, 0);
            return Physics.CheckSphere(pos, groundCheckRadius, slidingMask) || isGrounded;
        }

        /// <summary>
        /// Get Movement Input Values
        /// </summary>
        public Vector2 GetMovementValue()
        {
            return new Vector2(inputX, inputY);
        }

        /// <summary>
        /// Spawn player water foam particles
        /// </summary>
        public void PlayerInWater(float top)
        {
            Vector3 foamPos = transform.position;
            foamPos.y = top;

            if (foamParticles == null)
            {
                foamParticles = Instantiate(waterParticles, foamPos, transform.rotation);
            }

            if (foamParticles)
            {
                foamParticles.transform.position = foamPos;
            }
        }

        /// <summary>
        /// Use ladder function
        /// </summary>
        public void UseLadder(Ladder ladder, Vector2 look, bool climbUp)
        {
            ladderReady = false;
            characterState = CharacterState.Stand;
            this.ladder = ladder;

            moveDirection = Vector3.zero;
            inputX = 0f;
            inputY = 0f;

            // adjust exit position of the ladder
            ladderExit = ladder.LadderExit;
            ladderExit.y += CharacterControl.skinWidth + (controllerAdjustments.normalHeight / 2);

            if (climbUp)
            {
                Vector3 enter = ladder.LadderCenter;
                enter.y = transform.position.y;

                // adjust enter position by groundCheckOffset
                if (Vector3.Distance(transform.position, ladder.LadderUp) > groundCheckOffset + 0.1f)
                    enter.y += Mathf.Abs(groundCheckOffset) + Physics.defaultContactOffset;

                StartCoroutine(MovePlayer(enter, autoMoveSettings.climbUpAutoMove, true));
                scriptManager.GetComponent<MouseLook>().LerpLook(look, autoMoveSettings.climbUpAutoLook, true);
            }
            else
            {
                // adjust climb down enter position
                Vector3 enter = ladder.LadderUp;
                enter.y -= 0.5f;

                StartCoroutine(MovePlayer(enter, autoMoveSettings.climbDownAutoMove, true));
                scriptManager.GetComponent<MouseLook>().LerpLook(look, autoMoveSettings.climbDownAutoLook, true);
            }

            gameManager.ShowHelpButtons(new HelpButton(ExitLadder.Item2, InputHandler.CompositeOf("Jump").GetBindingPath()), null, null, null);
            itemSwitcher.FreeHands(true);
            movementState = MovementState.Ladder;
        }

        /// <summary>
        /// Exit ladder movement
        /// </summary>
        public void LadderExit()
        {
            if (ladderReady)
            {
                movementState = MovementState.Normal;
                scriptManager.GetComponent<MouseLook>().LockLook(false);
                gameManager.HideSprites(1);
                itemSwitcher.FreeHands(false);
                ladder.Collider.enabled = true;
                ladderExit = Vector3.zero;
                ladderReady = false;
                onLadder = false;
                ladder = null;
            }
        }

        /// <summary>
        /// Lerp player from ladder to position
        /// </summary>
        public void LerpPlayerLadder(Vector3 destination)
        {
            if (ladderReady)
            {
                gameManager.HideSprites(1);
                itemSwitcher.FreeHands(false);
                StartCoroutine(MovePlayer(destination, autoMoveSettings.climbFinishAutoMove, false, true));
                ladderReady = false;
            }
        }

        /// <summary>
        /// Lerp player to position
        /// </summary>
        public void LerpPlayer(Vector3 destination, Vector2 look, bool lerpLook = true)
        {
            characterState = CharacterState.Stand;
            moveDirection = Vector3.zero;
            ladderReady = false;
            isControllable = false;
            inputX = 0f;
            inputY = 0f;

            StartCoroutine(MovePlayer(destination, autoMoveSettings.globalAutoMove, false, true));

            if (lerpLook)
            {
                scriptManager.GetComponent<MouseLook>().LerpLook(look, autoMoveSettings.globalAutoLook, true);
            }
        }

        IEnumerator MovePlayer(Vector3 pos, float speed, bool isLadder, bool unlockLook = false)
        {
            CharacterControl.enabled = false;

            while (Vector3.Distance(transform.position, pos) > 0.05f)
            {
                transform.position = Vector3.Lerp(transform.position, pos, timekeeper.deltaTime * speed);
                yield return null;
            }

            CharacterControl.enabled = true;
            isControllable = true;
            ladderReady = isLadder;
            onLadder = isLadder;
            movementState = isLadder ? MovementState.Ladder : MovementState.Normal;

            if (unlockLook) 
                scriptManager.GetComponent<MouseLook>().LockLook(false);

            if (!isLadder && ladder != null)
            {
                ladder.Collider.enabled = true;
                ladderExit = Vector3.zero;
            }
        }

        IEnumerator RemoveFoam()
        {
            yield return new WaitForSeconds(2);
            Destroy(foamParticles.gameObject);
            isFoamRemoved = false;
        }

        public IEnumerator ApplyKickback(Vector3 offset, float time)
        {
            Quaternion s = baseKickback.transform.localRotation;
            Quaternion sw = weaponKickback.transform.localRotation;
            Quaternion e = baseKickback.transform.localRotation * Quaternion.Euler(offset);
            float r = 1.0f / time;
            float t = 0.0f;

            while (t < 1.0f)
            {
                t += Time.deltaTime * r;
                baseKickback.transform.localRotation = Quaternion.Slerp(s, e, t);
                weaponKickback.transform.localRotation = Quaternion.Slerp(sw, e, t);

                yield return null;
            }
        }

        IEnumerator AntiSpam()
        {
            antiSpam = true;
            yield return new WaitForSeconds(spamWaitTime);
            antiSpam = false;
        }

        void OnDrawGizmos()
        {
            if (!CharacterControl) return;

            Vector3 pos = transform.position + CharacterControl.center - new Vector3(0, (CharacterControl.height / 2f) + groundCheckOffset, 0);
            Gizmos.DrawWireSphere(pos, groundCheckRadius);
        }
    }
}