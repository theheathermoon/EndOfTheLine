/*
 * BodyAnimator.cs - wirted by ThunderWire Games
 * ver. 2.1
*/

using System.Linq;
using UnityEngine;
using HFPS.Systems;
using ThunderWire.Utility;

namespace HFPS.Player
{
    public class BodyAnimator : MonoBehaviour
    {
        private ScriptManager scriptManager;
        private PlayerController controller;
        private HealthManager health;
        private MouseLook mouseLook;
        private Animator anim;
        private Transform cam;

        [Header("Main")]
        [Layer] public int InvisibleLayer;
        public float TurnSmooth;
        public float AdjustSmooth;
        public float OverrideSmooth;
        public float BackOverrideSmooth;

        [Header("Angled Body Rotation")]
        public bool angledBody;
        public float rotateBodySpeed;
        public int dirAngleIncrease;
        public int minAngleBody;
        public int maxAngleBody;
        public int minBodyMaxDeadzone;
        public int maxBodyMaxDeadzone;

        [Header("Spine Constrains")]
        public SpineBone[] SpineBones;
        [Space]
        public float aimWeight;
        public float minWeight = 0.1f;
        public float lookSmoothSpeed = 32f;

        [Header("Speed")]
        public float animWalkSpeed = 1f;
        public float animCrouchSpeed = 1f;
        public float animRunSpeed = 1f;
        public float animWaterSpeed = 0.5f;

        [Header("Misc")]
        public float turnMouseTrigger = 0.5f;
        public float animStartVelocity = 0.2f;
        public float blockStopVelocity = 0.1f;
        public float turnLeftRightDelay = 0.2f;
        public bool velocityBasedAnim = true;
        public bool enableShadows = true;
        public bool visibleToCamera = true;
        public bool proneDisableBody;
        public bool showBodyGizmos;

        [Header("Body Death Settings")]
        public bool ragdollDeath;
        public Transform CameraRoot;
        public Transform NewDeathCamParent;
        [Layer] public int LimbVisibleLayer;
        public GameObject[] ActivateLimbs;
        public GameObject[] DeactivateLimbs;

        [Header("Body Adjustment")]
        [Space(10)]
        public Vector3 originalOffset;
        [Space(5)]
        public Vector3 runningOffset;
        [Space(5)]
        public Vector3 crouchOffset;
        [Space(5)]
        public Vector3 jumpOffset;
        [Space(5)]
        public Vector3 proneOffset;
        [Space(5)]
        public Vector3 turnOffset;
        [Space(10)]
        public Vector3 bodyAngle;
        [Space(5)]
        public Vector2 spineMaxRotation;

        private RagdollPart[] ragdollParts;

        private Vector2 movement;
        private Vector3 localBodyAngle;
        private Vector3 adjustedSpineEuler;

        private float spineAngle;
        private float mouseSpeed;
        private float yBodyRotation;
        private float yBodyAngle;
        private float inputAngle;
        private float movementInput;
        private float animationSpeed;
        private float inputMagnitude;

        private bool blockWalk = false;
        private bool bodyControl = false;
        private bool ladderReady = false;
        private bool death = false;

        private float smoothAimWeight;
        private float smoothAimVelocity;
        private float smoothBodyVelocity;
        private Vector3 smoothLookAngle;

        void Awake()
        {
            if(PlayerController.HasReference)
                controller = PlayerController.Instance;
            else
                throw new System.NullReferenceException("Could not find Player Controller reference!");

            if (ScriptManager.HasReference)
                scriptManager = ScriptManager.Instance;
            else
                throw new System.NullReferenceException("Could not find Script Manager reference!");

            if (!controller.gameObject.HasComponent(out health))
                throw new System.NullReferenceException("Could not find Health Manager component in Player object!");

            if (!scriptManager.gameObject.HasComponent(out mouseLook))
                throw new System.NullReferenceException("Could not find Mouse Look component!");

            if (scriptManager.MainCamera != null)
                cam = scriptManager.MainCamera.transform;
            else
                throw new System.NullReferenceException("Script Manager does not have a Main Camera assigned!");

            if (!gameObject.HasComponent(out anim, true))
                throw new System.NullReferenceException("Could not find Animator component!");


            localBodyAngle = transform.localEulerAngles;
            localBodyAngle.y = 0;
            originalOffset = transform.localPosition;

            yBodyAngle = transform.root.eulerAngles.y;
            Vector3 current = transform.localEulerAngles; current.x = 0; current.z = 0;
            Vector3 angle = bodyAngle + current;
            yBodyRotation = yBodyAngle + angle.y;
            transform.localEulerAngles = angle;
        }

        void Start()
        {
            anim.SetBool("Idle", true);

            if (!enableShadows)
            {
                foreach (SkinnedMeshRenderer renderer in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }
            }

            if (!visibleToCamera)
            {
                foreach (SkinnedMeshRenderer renderer in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    renderer.gameObject.layer = InvisibleLayer;
                }
            }

            mouseLook.SetClampRange(minBodyMaxDeadzone, maxBodyMaxDeadzone);

            ragdollParts = (from obj in anim.GetComponentsInChildren<Rigidbody>()
                            let col = obj.GetComponent<Collider>()
                            select new RagdollPart(col, obj)).ToArray();

            Ragdoll(false);
        }

        float InputToAngle(Vector2 input)
        {
            float raw = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;

            if (raw < (-90 - dirAngleIncrease))
            {
                raw += 180 + dirAngleIncrease;
            }
            else if (raw > (90 + dirAngleIncrease))
            {
                raw -= 180 + dirAngleIncrease;
            }

            return raw;
        }

        float InputMagnitude(Vector2 input)
        {
            float mag = Mathf.Clamp01(new Vector2(input.x, input.y).magnitude);
            return input.y > 0.01f ? mag : input.y < -0.01f ? mag * -1 : mag;
        }

        float InputMagnitudeClamped(Vector2 input)
        {
            return Mathf.Clamp01(new Vector2(input.x, input.y).magnitude);
        }

        void Update()
        {
            if (mouseLook)
            {
                mouseSpeed = mouseLook.GetInputDelta().x;
            }

            ladderReady = controller.ladderReady;
            movement = controller.GetMovementValue();
            inputAngle = InputToAngle(movement);
            movementInput = InputMagnitude(movement);
            inputMagnitude = InputMagnitudeClamped(movement);

            anim.SetBool("Crouch", controller.characterState != PlayerController.CharacterState.Stand);
            anim.SetBool("ClimbLadder", ladderReady);

            int state = (int)controller.characterState;

            if (controller.isControllable && !ladderReady)
            {
                /* POSITIONING */
                if (controller.isRunning && state == 0 && movement.y > 0.1f)
                {
                    if (controller.velMagnitude >= animStartVelocity)
                    {
                        transform.localPosition = Vector3.Lerp(transform.localPosition, runningOffset, Time.deltaTime * AdjustSmooth);
                    }
                }
                else if (!controller.IsGrounded())
                {
                    transform.localPosition = Vector3.Lerp(transform.localPosition, jumpOffset, Time.deltaTime * AdjustSmooth);
                }
                else if (state == 1)
                {
                    transform.localPosition = Vector3.Lerp(transform.localPosition, crouchOffset, Time.deltaTime * AdjustSmooth);
                }
                else if (state == 2)
                {
                    transform.localPosition = Vector3.Lerp(transform.localPosition, proneOffset, Time.deltaTime * AdjustSmooth);
                }
                else if (movement.x > 0.1f || movement.x < -0.1f)
                {
                    if (controller.velMagnitude >= animStartVelocity)
                    {
                        transform.localPosition = Vector3.Lerp(transform.localPosition, turnOffset, Time.deltaTime * AdjustSmooth);
                    }
                }
                else
                {
                    transform.localPosition = Vector3.Lerp(transform.localPosition, originalOffset, Time.deltaTime * AdjustSmooth);
                }

                if (controller.velMagnitude >= animStartVelocity)
                {
                    blockWalk = false;

                    /* MOVEMENT ANIMATIONS */
                    movementInput = InputMagnitude(movement);
                    anim.SetFloat("Movement", movementInput);

                    /* ROTATIONS */
                    localBodyAngle.y = inputAngle;
                }
                else if (controller.velMagnitude <= blockStopVelocity && Mathf.Abs(movement.x) < 0.1f)
                {
                    localBodyAngle.y = 0;
                    anim.SetBool("Run", false);
                    anim.SetFloat("Movement", 0f);
                    blockWalk = true;
                }

                if (!controller.IsGrounded())
                {
                    localBodyAngle.y = 0;
                    anim.SetBool("Jump", true);
                    anim.SetBool("Idle", false);
                    bodyControl = false;
                }
                else
                {
                    if (inputMagnitude < 0.1f || blockWalk)
                    {
                        anim.SetBool("Idle", true);
                    }
                    else
                    {
                        anim.SetBool("Idle", false);
                    }

                    bodyControl = inputMagnitude < 0.1f;

                    anim.SetBool("Jump", false);
                    anim.SetBool("Run", controller.isRunning && !blockWalk);
                }

                if (!angledBody)
                {
                    if (scriptManager.C<MouseLook>().enabled)
                    {
                        if (mouseSpeed > turnMouseTrigger)
                        {
                            anim.SetBool("TurningRight", true);
                            anim.SetBool("TurningLeft", false);
                        }
                        else if (mouseSpeed < -turnMouseTrigger)
                        {
                            anim.SetBool("TurningRight", false);
                            anim.SetBool("TurningLeft", true);
                        }
                        else
                        {
                            anim.SetBool("TurningRight", false);
                            anim.SetBool("TurningLeft", false);
                        }
                    }
                    else
                    {
                        anim.SetBool("TurningRight", false);
                        anim.SetBool("TurningLeft", false);
                    }
                }
                else
                {
                    if (Mathf.Abs(mouseSpeed) < 0.1f)
                    {
                        anim.SetBool("TurningRight", false);
                        anim.SetBool("TurningLeft", false);
                    }
                }
            }
            else
            {
                if (!ladderReady)
                {
                    if (controller.IsGrounded())
                    {
                        anim.SetBool("TurningRight", false);
                        anim.SetBool("TurningLeft", false);
                        anim.SetBool("Jump", false);
                        anim.SetBool("Run", false);
                        anim.SetBool("Idle", true);
                    }
                    else
                    {
                        anim.SetBool("Jump", true);
                    }
                }
                else
                {
                    anim.SetBool("TurningRight", false);
                    anim.SetBool("TurningLeft", false);
                    anim.SetBool("Idle", false);
                    anim.SetBool("Jump", false);
                    anim.SetFloat("ClimbSpeed", movementInput);
                }

                if (state == 0)
                {
                    transform.localPosition = Vector3.Lerp(transform.localPosition, originalOffset, Time.deltaTime * AdjustSmooth);
                }
                else if (state == 1)
                {
                    transform.localPosition = Vector3.Lerp(transform.localPosition, crouchOffset, Time.deltaTime * AdjustSmooth);
                }
                else
                {
                    transform.localPosition = Vector3.Lerp(transform.localPosition, proneOffset, Time.deltaTime * AdjustSmooth);
                }

                localBodyAngle.y = 0;
            }

            if (proneDisableBody)
            {
                if (transform.localPosition.y <= (proneOffset.y + 0.1) && transform.localPosition.z <= (proneOffset.z + 0.1))
                {
                    foreach (SkinnedMeshRenderer smr in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        smr.enabled = false;
                    }
                }
                else
                {
                    foreach (SkinnedMeshRenderer smr in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        smr.enabled = true;
                    }
                }
            }
            else
            {
                foreach (SkinnedMeshRenderer smr in gameObject.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    smr.enabled = true;
                }
            }

            if (!controller.isInWater && !controller.isRunning)
            {
                if (controller.characterState == PlayerController.CharacterState.Stand)
                {
                    animationSpeed = animWalkSpeed;
                }
                else if (controller.characterState == PlayerController.CharacterState.Crouch)
                {
                    animationSpeed = animCrouchSpeed;
                }
            }
            else if (!controller.isInWater && controller.isRunning)
            {
                animationSpeed = animRunSpeed;
            }
            else if (controller.isInWater)
            {
                animationSpeed = animWaterSpeed;
            }

            float movementVelocity = Mathf.Clamp(controller.velMagnitude, 0, animationSpeed);
            anim.SetFloat("AnimationSpeed", velocityBasedAnim ? movementVelocity : animationSpeed);

            if (health.isDead && !death)
            {
                if (ragdollDeath && CameraRoot && NewDeathCamParent)
                {
                    CameraRoot.SetParent(NewDeathCamParent);
                    CameraRoot.localPosition = Vector3.zero;
                }

                if (ActivateLimbs.Length > 0)
                {
                    foreach (var limb in ActivateLimbs)
                    {
                        limb.layer = LimbVisibleLayer;
                    }
                }

                if (DeactivateLimbs.Length > 0)
                {
                    foreach (var limb in DeactivateLimbs)
                    {
                        limb.gameObject.SetActive(false);
                    }
                }

                anim.enabled = false;
                Ragdoll(ragdollDeath);
                death = true;
            }

            if (!death)
            {
                if (angledBody)
                {
                    if (bodyControl && !ladderReady)
                    {
                        if ((spineAngle <= minBodyMaxDeadzone) || (spineAngle >= maxBodyMaxDeadzone))
                        {
                            yBodyAngle = transform.root.eulerAngles.y - spineAngle;
                            float sub = Mathf.Abs(maxBodyMaxDeadzone - maxAngleBody);

                            if (spineAngle > 0)
                                yBodyAngle -= sub;
                            else
                                yBodyAngle += sub;

                            yBodyRotation = yBodyAngle;
                            TurnAnimation(spineAngle);
                        }
                        else if ((spineAngle <= minAngleBody) || (spineAngle >= maxAngleBody))
                        {
                            yBodyAngle = transform.root.eulerAngles.y;
                            TurnAnimation(spineAngle);
                        }

                        yBodyRotation = Mathf.SmoothDampAngle(yBodyRotation, yBodyAngle, ref smoothBodyVelocity, Time.deltaTime * rotateBodySpeed);
                        //yBodyRotation = Mathf.MoveTowardsAngle(yBodyRotation, yBodyAngle, Time.smoothDeltaTime * angleSpeed);
                    }
                    else
                    {
                        Vector3 current = transform.localEulerAngles; 
                        current.x = 0; 
                        current.z = 0;

                        Vector3 angle = bodyAngle + current;
                        angle.y = Mathf.LerpAngle(angle.y, localBodyAngle.y, Time.deltaTime * TurnSmooth);
                        transform.localEulerAngles = angle;

                        yBodyAngle = transform.root.eulerAngles.y;
                        yBodyRotation = yBodyAngle + angle.y;

                        mouseLook.SetClampRange(minBodyMaxDeadzone, maxBodyMaxDeadzone);
                    }
                }
                else
                {
                    Vector3 current = transform.localEulerAngles; current.x = 0; current.z = 0;
                    Vector3 angle = bodyAngle + current;
                    angle.y = Mathf.LerpAngle(angle.y, localBodyAngle.y, Time.deltaTime * TurnSmooth);
                    transform.localEulerAngles = angle;
                }

                Vector3 relative = transform.InverseTransformPoint(cam.position);
                spineAngle = Mathf.Atan2(relative.x, relative.z) * Mathf.Rad2Deg;
                spineAngle = Mathf.Clamp(spineAngle, spineMaxRotation.x, spineMaxRotation.y);
                adjustedSpineEuler = new Vector3(0, spineAngle, 0);
            }
        }

        void Ragdoll(bool state)
        {
            if (ragdollParts.Length > 0)
            {
                controller.CharacterControl.enabled = !state;

                foreach (var part in ragdollParts)
                {
                    if (state)
                    {
                        part.rigidbody.isKinematic = false;
                        part.rigidbody.useGravity = true;
                        part.collider.enabled = true;
                    }
                    else
                    {
                        part.rigidbody.isKinematic = true;
                        part.rigidbody.useGravity = false;
                        part.collider.enabled = false;
                    }
                }
            }
            else
            {
                Debug.LogError("[Player Body] Cannot activate body ragdoll. Ragdoll Parts was not located!");
            }
        }

        void TurnAnimation(float angle)
        {
            if (angle > 0)
            {
                anim.SetBool("TurningRight", true);
                anim.SetBool("TurningLeft", false);
            }
            else
            {
                anim.SetBool("TurningRight", false);
                anim.SetBool("TurningLeft", true);
            }
        }

        void LateUpdate()
        {
            if (death) return;

            anim.transform.localPosition = Vector3.zero;
            //MiddleSpine.localRotation = Quaternion.Euler(adjustedSpineEuler);

            Vector3 lookEuler = adjustedSpineEuler + transform.localEulerAngles;

            smoothLookAngle.x = Mathf.LerpAngle(smoothLookAngle.x, lookEuler.x, Time.smoothDeltaTime * lookSmoothSpeed);
            smoothLookAngle.y = Mathf.LerpAngle(smoothLookAngle.y, lookEuler.y, Time.smoothDeltaTime * lookSmoothSpeed);
            smoothLookAngle.z = Mathf.LerpAngle(smoothLookAngle.z, lookEuler.z, Time.smoothDeltaTime * lookSmoothSpeed);

            float weight = 1f;
            weight -= aimWeight / 10f;

            if (weight < minWeight)
                weight = minWeight;

            smoothAimWeight = Mathf.SmoothDamp(smoothAimWeight, weight, ref smoothAimVelocity, Time.smoothDeltaTime * 4f);
            weight = smoothAimWeight;

            Quaternion quaternion = Quaternion.Inverse(Quaternion.Euler(smoothLookAngle));
            foreach (var spine in SpineBones)
            {
                spine.Bone.localRotation = Quaternion.Lerp(spine.Bone.localRotation, quaternion, Mathf.Lerp(spine.WeightMin, spine.WeightMax, quaternion.x) * weight);
            }

            if (bodyControl && angledBody)
            {
                transform.rotation = Quaternion.Euler(new Vector3(transform.eulerAngles.x, yBodyRotation, transform.eulerAngles.z));
            }
        }

        private void OnDrawGizmos()
        {
            if (showBodyGizmos)
            {
                Vector3 angleLeft = Quaternion.AngleAxis(minAngleBody, Vector3.up) * transform.forward;
                Vector3 angleRight = Quaternion.AngleAxis(maxAngleBody, Vector3.up) * transform.forward;

                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, angleLeft * 2);
                Gizmos.DrawRay(transform.position, angleRight * 2);

                Vector3 angleLeftMax = Quaternion.AngleAxis(minBodyMaxDeadzone, Vector3.up) * transform.forward;
                Vector3 angleRightMax = Quaternion.AngleAxis(maxBodyMaxDeadzone, Vector3.up) * transform.forward;

                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, angleLeftMax * 2);
                Gizmos.DrawRay(transform.position, angleRightMax * 2);

                Vector3 spineAngleGizmo = Quaternion.AngleAxis(spineAngle, Vector3.up) * transform.forward;

                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, spineAngleGizmo * 1.5f);
            }
        }

        public struct RagdollPart
        {
            public Collider collider;
            public Rigidbody rigidbody;

            public RagdollPart(Collider col, Rigidbody rb)
            {
                collider = col;
                rigidbody = rb;
            }
        }

        [System.Serializable]
        public struct SpineBone
        {
            public Transform Bone;
            public float WeightMin;
            public float WeightMax;
        }
    }
}