/*
 * DragRigidbody.cs - by ThunderWire Studio
 * ver. 1.22
*/

using UnityEngine;
using System.Collections;
using ThunderWire.Input;
using HFPS.Systems;

namespace HFPS.Player
{
    public class DragRigidbody : MonoBehaviour
    {
        private Camera playerCam;
        private HFPS_GameManager gameManager;
        private InteractManager interact;
        private PlayerFunctions pfunc;
        private DelayEffect delay;
        private ItemSwitcher itemSwitcher;
        private ScriptManager scriptManager;

        [Header("Main")]
        public LayerMask CullLayers;
        [Layer] public int InteractLayer;

        [Header("Drag")]
        public float ThrowStrength = 2f;
        public float DragSmoothing = 10f;
        public float minDistanceZoom = 1.5f;
        public float maxDistanceZoom = 3f;
        public float maxDistanceGrab = 4f;
        public float spamWaitTime = 0.5f;

        [Header("Other")]
        public float rotateSpeed = 10f;
        public float rotateSmoothing = 1f;
        public float rotationDeadzone = 0.1f;
        public float objectZoomSpeed = 3f;

        [Space(7)]
        public bool FreezeRotation = true;
        public bool enableObjectPull = true;
        public bool enableObjectRotation = true;
        public bool enableObjectZooming = true;
        public bool dragHideWeapon;
        public bool fixedHold = false;

        #region Private Variables
        private Transform oldParent;

        private GameObject objectHeld;
        private GameObject objectRaycast;
        private Rigidbody heldRigidbody;
        private DraggableObject heldDraggable;

        private GameObject fixedVelocityObj;
        private Rigidbody fixedVelocityRigid;

        private float PickupRange = 3f;
        private float distance;

        private bool RotateButton;
        private bool GrabObject;
        private bool isObjectHeld;
        private bool isRotatePressed;
        private bool antiSpam;

        private float timeDropCheck;
        private float zoomInputY;
        private Vector2 rotateValue;
        private Vector2 rotationVelocity;
        private Vector2 smoothRotation;
        #endregion

        void OnEnable()
        {
            InputHandler.GetInputAction("Zoom").canceled += OnZoom;
        }

        private void OnZoom(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            pfunc.zoomEnabled = true;
        }

        void Awake()
        {
            scriptManager = ScriptManager.Instance;
            delay = transform.root.GetComponentInChildren<DelayEffect>(true);
            interact = GetComponent<InteractManager>();
            gameManager = HFPS_GameManager.Instance;
            pfunc = GetComponent<PlayerFunctions>();
            playerCam = scriptManager.MainCamera;
            PickupRange = interact.RaycastRange;
        }

        void Start()
        {
            itemSwitcher = GetComponent<ScriptManager>().C<ItemSwitcher>();
            isObjectHeld = false;
            objectHeld = null;
        }

        void Update()
        {
            gameManager.isHeld = objectHeld != false;
            interact.isHeld = objectHeld != false;

            if (InputHandler.InputIsInitialized && !gameManager.isPaused && !gameManager.isInventoryShown)
            {
                RotateButton = InputHandler.ReadButton("Fire");

                if (objectRaycast && !antiSpam && !gameManager.isWeaponZooming)
                {
                    if (InputHandler.ReadButtonOnce(this, "Examine"))
                    {
                        GrabObject = !GrabObject;
                    }
                }

                if (InputHandler.ReadButtonOnce(this, "Zoom") && objectHeld)
                {
                    ThrowObject();
                }

                Vector2 delta = InputHandler.ReadInput<Vector2>("Look", "PlayerExtra");
                rotateValue.x = Mathf.Abs(delta.x) > rotationDeadzone ? delta.x * rotateSpeed * -1 : 0;
                rotateValue.y = Mathf.Abs(delta.y) > rotationDeadzone ? delta.y * rotateSpeed : 0;
                smoothRotation = Vector2.SmoothDamp(smoothRotation, rotateValue, ref rotationVelocity, Time.deltaTime * rotateSmoothing);

                if (InputHandler.CurrentDevice == InputHandler.Device.MouseKeyboard)
                    zoomInputY = InputHandler.ReadInput<Vector2>("Scroll", "PlayerExtra").y * objectZoomSpeed;
                else
                    zoomInputY = InputHandler.ReadInput<Vector2>("Move").y * (RotateButton ? 1 : 0);
            }

            if (GrabObject)
            {
                if (!isObjectHeld)
                {
                    TryDragObject();
                }
                else if (objectHeld)
                {
                    HoldObject();
                }
                else
                {
                    ResetDrag(false);
                }
            }
            else if (isObjectHeld)
            {
                DropObject();
            }

            Ray playerAim = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (Physics.Raycast(playerAim, out RaycastHit hit, PickupRange, CullLayers))
            {
                if (hit.collider.gameObject.layer == InteractLayer)
                {
                    if (hit.collider.GetComponent<DraggableObject>())
                    {
                        objectRaycast = hit.collider.gameObject;
                        gameManager.canGrab = true;
                    }
                }
                else
                {
                    if (!isObjectHeld)
                    {
                        objectRaycast = null;
                        gameManager.canGrab = false;
                    }
                }

                scriptManager.IsGrabRaycast = objectRaycast != null;

                if (objectHeld)
                {
                    if (RotateButton && enableObjectRotation)
                    {
                        heldRigidbody.freezeRotation = true;
                        gameManager.LockPlayerControls(false, false, false);
                        objectHeld.transform.Rotate(playerCam.transform.up, smoothRotation.x, Space.World);
                        objectHeld.transform.Rotate(playerCam.transform.right, smoothRotation.y, Space.World);
                        isRotatePressed = true;
                    }
                    else if (isRotatePressed)
                    {
                        gameManager.LockPlayerControls(true, false, false);
                        isRotatePressed = false;
                    }
                }
            }
            else
            {
                if (!isObjectHeld)
                {
                    objectRaycast = null;
                    gameManager.canGrab = false;
                    scriptManager.IsGrabRaycast = false;
                }
            }
        }

        void TryDragObject()
        {
            StartCoroutine(AntiSpam());

            if (objectRaycast && !objectRaycast.GetComponent<Rigidbody>())
            {
                Debug.LogError("[DragRigidbody] " + objectRaycast.name + " does not contains a Rigidbody component!");
                GrabObject = false;
                return;
            }
            else
            {
                heldRigidbody = objectRaycast.GetComponent<Rigidbody>();
                heldDraggable = objectRaycast.GetComponent<DraggableObject>();
            }

            objectHeld = objectRaycast;

            if (enableObjectPull && heldDraggable)
            {
                if (heldDraggable.automaticDistance)
                {
                    float dist = Vector3.Distance(transform.position, objectHeld.transform.position);

                    if (dist > maxDistanceZoom)
                    {
                        distance = maxDistanceZoom - 1f;
                    }
                    else if (dist < minDistanceZoom)
                    {
                        distance = minDistanceZoom + 1f;
                    }
                    else
                    {
                        distance = dist;
                    }
                }
                else
                {
                    distance = heldDraggable.dragDistance;
                }

                heldDraggable.isGrabbed = true;
            }

            heldRigidbody.useGravity = false;
            heldRigidbody.isKinematic = false;
            heldRigidbody.freezeRotation = FreezeRotation;

            if (fixedHold)
            {
                oldParent = objectHeld.transform.parent;
                objectHeld.transform.SetParent(playerCam.transform);
                heldRigidbody.velocity = Vector3.zero;

                fixedVelocityObj = new GameObject(objectHeld.name + " velocity");
                fixedVelocityObj.transform.position = objectHeld.transform.position;

                Collider collider = fixedVelocityObj.AddComponent<SphereCollider>();
                collider.isTrigger = true;

                fixedVelocityRigid = fixedVelocityObj.AddComponent<Rigidbody>();
                fixedVelocityRigid.angularDrag = heldRigidbody.angularDrag;
                fixedVelocityRigid.drag = heldRigidbody.drag;
                fixedVelocityRigid.mass = heldRigidbody.mass;

                fixedVelocityRigid.useGravity = false;
                fixedVelocityRigid.isKinematic = false;
            }

            itemSwitcher.FreeHands(dragHideWeapon);
            gameManager.ShowGrabSprites();
            gameManager.isGrabbed = true;
            delay.isEnabled = false;
            pfunc.zoomEnabled = false;
            timeDropCheck = 0f;

            GetComponent<ScriptManager>().ScriptEnabledGlobal = false;
            Physics.IgnoreCollision(objectHeld.GetComponent<Collider>(), transform.root.GetComponent<Collider>(), true);

            if(objectHeld.GetComponent<IDragRigidbody>() is var iDrag && iDrag != null)
            {
                iDrag.OnRigidbodyDrag();
            }

            objectHeld.SendMessage("OnRigidbodyDrag", SendMessageOptions.DontRequireReceiver);
            isObjectHeld = true;
        }

        void HoldObject()
        {
            interact.CrosshairVisible(false);
            gameManager.HideSprites(0);
            distance = Mathf.Clamp(distance, minDistanceZoom, maxDistanceZoom - 0.5f);

            Ray playerAim = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            Vector3 currPos = objectHeld.transform.position;
            Vector3 grabPos = playerAim.origin + playerAim.direction * distance;

            heldRigidbody.velocity = (grabPos - currPos) * DragSmoothing;

            if (fixedHold && fixedVelocityObj && fixedVelocityRigid)
            {
                Vector3 fixedVelocity = Vector3.zero;
                Vector3 currFixedPos = fixedVelocityObj.transform.position;
                Vector3.SmoothDamp(currFixedPos, grabPos, ref fixedVelocity, Time.deltaTime * DragSmoothing);
                fixedVelocityRigid.velocity = fixedVelocity;
            }

            if (enableObjectZooming)
            {
                float mw = zoomInputY;

                if (mw > 0 && distance < maxDistanceZoom)
                {
                    distance += mw;
                }
                else if (mw < 0 && distance > minDistanceZoom)
                {
                    distance += mw;
                }
            }

            if (timeDropCheck < 1f)
            {
                timeDropCheck += Time.deltaTime;
            }
            else if (Vector3.Distance(objectHeld.transform.position, playerCam.transform.position) > maxDistanceGrab)
            {
                DropObject();
            }
        }

        public bool CheckHold()
        {
            return isObjectHeld;
        }

        public void ResetDrag(bool throwObj)
        {
            if (dragHideWeapon) itemSwitcher.FreeHands(false);

            gameManager.LockPlayerControls(true, true, false);
            gameManager.HideSprites(1);
            gameManager.isGrabbed = false;
            heldDraggable.isGrabbed = false;

            if (objectHeld && heldRigidbody)
            {
                if (fixedHold) objectHeld.transform.parent = oldParent;

                heldRigidbody.useGravity = true;
                heldRigidbody.freezeRotation = false;

                Physics.IgnoreCollision(objectHeld.GetComponent<Collider>(), transform.root.GetComponent<Collider>(), false);

                if (objectHeld.GetComponent<IDragRigidbody>() is var iDrag && iDrag != null)
                {
                    iDrag.OnRigidbodyRelease();
                }

                objectHeld.SendMessage("OnRigidbodyRelease", SendMessageOptions.DontRequireReceiver);

                if (throwObj)
                {
                    heldRigidbody.AddForce(playerCam.transform.forward * ThrowStrength, ForceMode.Impulse);
                }
                else if (fixedHold && fixedVelocityObj)
                {
                    heldRigidbody.AddForce(fixedVelocityRigid.velocity, ForceMode.VelocityChange);
                }

                if (fixedVelocityObj)
                {
                    Destroy(fixedVelocityObj);
                }
            }

            interact.CrosshairVisible(true);
            GetComponent<ScriptManager>().ScriptEnabledGlobal = true;

            delay.isEnabled = true;
            objectRaycast = null;
            objectHeld = null;
            heldRigidbody = null;
            heldDraggable = null;
            GrabObject = false;
            isObjectHeld = false;
            timeDropCheck = 0f;
        }

        void DropObject()
        {
            ResetDrag(false);
        }

        void ThrowObject()
        {
            ResetDrag(true);
        }

        IEnumerator AntiSpam()
        {
            antiSpam = true;
            yield return new WaitForSeconds(spamWaitTime);
            antiSpam = false;
        }
    }
}