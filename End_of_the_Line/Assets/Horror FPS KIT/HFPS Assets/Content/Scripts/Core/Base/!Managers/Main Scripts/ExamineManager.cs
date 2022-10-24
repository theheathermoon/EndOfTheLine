/*
 * ExamineManager.cs - by ThunderWire Studio
 * ver. 2.2
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Input;
using ThunderWire.Utility;
using HFPS.Systems;

namespace HFPS.Player
{
    public class ExamineManager : Singleton<ExamineManager>
    {
        #region Structures
        public struct RigidbodyExamine
        {
            public struct RigidbodyParams
            {
                public bool isKinematic;
                public bool useGravity;
            }

            public RigidbodyExamine(GameObject obj, RigidbodyParams rbp)
            {
                rbObject = obj;
                rbParameters = rbp;
            }

            public GameObject rbObject;
            public RigidbodyParams rbParameters;
        }
        #endregion

        private HFPS_GameManager gameManager;
        private Inventory inventory;
        private InteractManager interact;
        private PlayerFunctions pfunc;
        private FloatingIconManager floatingItem;
        private DelayEffect delay;
        private ScriptManager scriptManager;
        private ItemSwitcher itemSwitcher;

        private GameObject paperUI;
        private Text paperText;

        protected List<RigidbodyExamine> ExamineRBs = new List<RigidbodyExamine>();

        [Header("Raycast")]
        public LayerMask CullLayers;
        [Layer] public int InteractLayer;
        [Layer] public int ExamineLayer;
        [Layer] public int SecondExamineLayer;

        [Header("Examine Layering")]
        public LayerMask MainCameraMask;
        public LayerMask ArmsCameraMask;

        [Header("Second Examine Layering")]
        public LayerMask SecMainCameraMask;
        public LayerMask SecArmsCameraMask;

        [Header("Adjustments")]
        public float pickupSpeed = 10;
        public float pickupRotateSpeed = 10;
        [Space(5)]
        public float putBackTime = 10;
        public float putBackRotateTime = 10;
        [Space(5)]
        public float zoomSpeed = 1f;
        public float rotationDeadzone = 0.1f;
        public float rotateSpeed = 10f;
        public float gamepadRotateSpeed = 3f;
        public float rotateSmoothing = 1f;
        public float timeToExamine = 1f;
        public float spamWaitTime = 0.5f;

        [Header("Lighting")]
        public Light examineLight;

        [Header("Sounds")]
        public AudioClip examinedSound;
        public float examinedVolume = 1f;

        public event Action OnDropObject;

        [HideInInspector]
        public bool isExamining;

        #region Private Variables
        private bool isReading;
        private bool isPaper;
        private bool antiSpam;
        private bool isObjectHeld;
        private bool tryExamine;
        private bool otherHeld;
        private bool isInspect;
        private bool cursorShown;
        private bool floatingIconEn;
        private float pickupRange = 3f;

        private Quaternion faceRotationFirst;
        private Quaternion faceRotationSecond;

        private bool firstFaceBreak = false;
        private bool secondFaceBreak = false;

        private bool faceToCameraFirst = false;
        private bool faceToCameraSecond = false;
        private bool faceToCameraInspect = false;

        private Camera ArmsCam;
        private Camera PlayerCam;
        private Ray PlayerAim;

        private LayerMask DefaultMainCamMask;
        private LayerMask DefaultArmsCamMask;

        private GameObject objectRaycast;
        private GameObject objectHeld;

        private InteractiveItem firstExamine;
        private InteractiveItem secondExamine;
        private InteractiveItem priorityObject;

        private Transform oldSecondObjT;
        private Vector3 oldSecondObjPos;
        private Quaternion oldSecondRot;

        private Vector3 objectPosition;
        private Quaternion objectRotation;

        private Vector2 rotationVelocity;
        private Vector2 smoothRotation;

        private InputHandler.Device device;
        private Vector2 rotateValue;
        private Vector2 movementVector;
        private Vector2 lookVector;

        private string bp_Use;

        private bool cursorKey;
        private bool rotateKey;
        private bool selectKey;
        private bool examineKey;
        private bool readTakeKey;

        private float firstDistance;
        private float secondDistance;

        private Coroutine c_ExamineObject;
        #endregion

        #region Texts
        private string ReadText;
        #endregion

        void Awake()
        {
            TextsSource.OnInitTexts += OnInitTexts;

            scriptManager = ScriptManager.Instance;
            itemSwitcher = scriptManager.C<ItemSwitcher>();

            if (!TextsSource.HasReference)
                OnInitTexts();
        }

        private void OnInitTexts()
        {
            ReadText = TextsSource.GetText("Examine.Read", "Read");
        }

        void Start()
        {
            if (GetComponent<InteractManager>() && GetComponent<PlayerFunctions>())
            {
                gameManager = HFPS_GameManager.Instance;
                inventory = Inventory.Instance;
                floatingItem = FloatingIconManager.Instance;
                interact = GetComponent<InteractManager>();
                pfunc = GetComponent<PlayerFunctions>();
                paperUI = gameManager.gamePanels.PaperReadPanel;
                paperText = gameManager.userInterface.PaperReadText;
            }
            else
            {
                Debug.LogError("Missing one or more scripts in " + gameObject.name);
                return;
            }

            if (examineLight)
            {
                examineLight.enabled = false;
            }

            delay = transform.root.gameObject.GetComponentInChildren<DelayEffect>();
            PlayerCam = scriptManager.MainCamera;
            ArmsCam = scriptManager.ArmsCamera;
            DefaultMainCamMask = PlayerCam.cullingMask;
            DefaultArmsCamMask = ArmsCam.cullingMask;
            pickupRange = interact.RaycastRange;
        }

        void Update()
        {
            if (InputHandler.InputIsInitialized)
            {
                device = InputHandler.CurrentDevice;

                bp_Use = InputHandler.CompositeOf("Use").GetBindingPath();
                rotateKey = InputHandler.ReadButton("Fire");
                selectKey = InputHandler.ReadButtonOnce(this, "Fire");

                lookVector = InputHandler.ReadInput<Vector2>("Look", "PlayerExtra");
                movementVector = InputHandler.ReadInput<Vector2>("Move");

                if (objectRaycast || firstExamine)
                {
                    readTakeKey = InputHandler.ReadButtonOnce(this, "Use");
                    examineKey = InputHandler.ReadButtonOnce(this, "Examine");
                    cursorKey = InputHandler.ReadButtonOnce(this, "Zoom");
                }

                float currectSpeed = device.IsGamepadDevice() == 1 ? gamepadRotateSpeed : rotateSpeed;

                rotateValue.x = Mathf.Abs(lookVector.x) > rotationDeadzone ?
                    -(lookVector.x * currectSpeed) : 0;

                rotateValue.y = Mathf.Abs(lookVector.y) > rotationDeadzone ?
                    lookVector.y * currectSpeed : 0;

                smoothRotation = Vector2.SmoothDamp(smoothRotation, rotateValue, ref rotationVelocity, Time.deltaTime * rotateSmoothing);
            }

            // If the player is holding another object, avoid interacting with the object
            otherHeld = GetComponent<DragRigidbody>().CheckHold();

            if (gameManager.isPaused) return;

            if (objectRaycast && !antiSpam && firstExamine && firstExamine.examineType != InteractiveItem.ExamineType.None && !gameManager.isWeaponZooming)
            {
                if (examineKey && !otherHeld)
                {
                    isExamining = !isExamining;
                }
            }

            if (isExamining)
            {
                if (!isObjectHeld)
                {
                    FirstPhase();
                    tryExamine = true;
                }
                else
                {
                    HoldObject();
                }
            }
            else if (isObjectHeld)
            {
                if (!secondExamine)
                {
                    DropObject();
                }
                else
                {
                    SecondExaminedObject(false);
                    isExamining = true;
                }
            }

            PlayerAim = PlayerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

            if (!isInspect)
            {
                if (Physics.Raycast(PlayerAim, out RaycastHit hit, pickupRange, CullLayers))
                {
                    if (hit.collider.gameObject.layer == InteractLayer)
                    {
                        if (hit.collider.gameObject.GetComponent<InteractiveItem>())
                        {
                            objectRaycast = hit.collider.gameObject;
                            firstExamine = objectRaycast.GetComponent<InteractiveItem>();
                        }
                        else
                        {
                            if (!tryExamine)
                            {
                                objectRaycast = null;
                                firstExamine = null;
                            }
                        }
                    }
                    else
                    {
                        if (!tryExamine)
                        {
                            objectRaycast = null;
                            firstExamine = null;
                        }
                    }

                    scriptManager.IsExamineRaycast = objectRaycast != null;
                }
                else
                {
                    if (!tryExamine)
                    {
                        objectRaycast = null;
                        firstExamine = null;
                        scriptManager.IsExamineRaycast = false;
                    }
                }
            }

            if (priorityObject && isObjectHeld)
            {
                if (rotateKey && !isReading && !cursorShown && priorityObject.examineRotate != InteractiveItem.ExamineRotate.None)
                {
                    firstFaceBreak = true;

                    if (secondExamine)
                    {
                        secondFaceBreak = false;
                    }

                    if (priorityObject.examineRotate == InteractiveItem.ExamineRotate.Both)
                    {
                        priorityObject.transform.Rotate(PlayerCam.transform.up, smoothRotation.x, Space.World);
                        priorityObject.transform.Rotate(PlayerCam.transform.right, smoothRotation.y, Space.World);
                    }
                    else if (priorityObject.examineRotate == InteractiveItem.ExamineRotate.Horizontal)
                    {
                        priorityObject.transform.Rotate(PlayerCam.transform.up, smoothRotation.x, Space.World);
                    }
                    else if (priorityObject.examineRotate == InteractiveItem.ExamineRotate.Vertical)
                    {
                        priorityObject.transform.Rotate(PlayerCam.transform.right, smoothRotation.y, Space.World);
                    }
                }

                if (isPaper)
                {
                    if (readTakeKey && !string.IsNullOrEmpty(priorityObject.paperText))
                    {
                        isReading = !isReading;
                    }

                    if (isReading)
                    {
                        paperText.text = priorityObject.paperText;
                        paperText.fontSize = priorityObject.paperFontSize;
                        paperUI.SetActive(true);
                    }
                    else
                    {
                        paperUI.SetActive(false);
                    }
                }
                else if (priorityObject.allowExamineTake && priorityObject.itemType != InteractiveItem.ItemType.OnlyExamine && !isInspect && readTakeKey)
                {
                    if (priorityObject.itemType == InteractiveItem.ItemType.InventoryItem && !inventory.CheckInventorySpace())
                        return;

                    TakeObject(secondExamine, priorityObject.gameObject, priorityObject.disableType == InteractiveItem.DisableType.None);
                }

                if (priorityObject.enableCursor)
                {
                    if (cursorKey)
                    {
                        cursorShown = !cursorShown;

                        if (device == InputHandler.Device.MouseKeyboard)
                        {
                            gameManager.ShowCursor(cursorShown);
                        }
                        else if (device != InputHandler.Device.None)
                        {
                            gameManager.ShowConsoleCursor(cursorShown);
                        }
                    }
                }
                else
                {
                    cursorShown = false;

                    if (device == InputHandler.Device.MouseKeyboard)
                    {
                        gameManager.ShowCursor(false);
                    }
                    else if (device != InputHandler.Device.None)
                    {
                        gameManager.ShowConsoleCursor(false);
                    }
                }

                if (cursorShown)
                {
                    Vector3 consoleCursorPos = gameManager.userInterface.ConsoleCursor.transform.position;

                    if (device != InputHandler.Device.MouseKeyboard && device != InputHandler.Device.None)
                    {
                        gameManager.MoveConsoleCursor(movementVector);
                    }

                    if (selectKey)
                    {
                        Vector3 mousePosition = InputHandler.ReadInput<Vector2>("MousePosition", "PlayerExtra");
                        Ray ray = PlayerCam.ScreenPointToRay(device == InputHandler.Device.MouseKeyboard ? mousePosition : consoleCursorPos);

                        if (Physics.Raycast(ray, out RaycastHit rayHit, 5, CullLayers))
                        {
                            if (rayHit.collider.GetComponent<InteractiveItem>() && rayHit.collider.GetComponent<InteractiveItem>().mouseClickPickup)
                            {
                                interact.Interact(rayHit.collider.gameObject);
                            }
                            else if (rayHit.collider.GetComponent<ExamineObjectAnimation>() && rayHit.collider.GetComponent<ExamineObjectAnimation>().isEnabled)
                            {
                                rayHit.collider.GetComponent<ExamineObjectAnimation>().PlayAnimation();
                            }
                            else if (rayHit.collider.GetComponent<InteractiveItem>() is var iitem && iitem != null)
                            {
                                if (iitem != firstExamine && !secondExamine && firstExamine.examineType == InteractiveItem.ExamineType.AdvancedObject &&
                                    firstExamine.allowedInteracts.Any(x => x == iitem))
                                {
                                    secondExamine = iitem;
                                    SecondExaminedObject(true);
                                }
                            }
                            else
                            {
                                rayHit.collider.gameObject.SendMessage("Interact", SendMessageOptions.DontRequireReceiver);
                            }
                        }
                    }
                }
            }
        }

        void SecondExaminedObject(bool isExamined)
        {
            if (secondExamine)
            {
                if (isExamined)
                {
                    secondExamine.gameObject.SendMessage("OnExamine", SendMessageOptions.DontRequireReceiver);

                    priorityObject = secondExamine;
                    secondExamine.floatingIcon = false;

                    PlayerCam.cullingMask = SecMainCameraMask;
                    ArmsCam.cullingMask = SecArmsCameraMask;

                    ShowExamineUI(true);
                    ShowExamineText(secondExamine);

                    oldSecondObjT = secondExamine.transform.parent;
                    oldSecondObjPos = secondExamine.transform.position;
                    oldSecondRot = secondExamine.transform.rotation;

                    secondExamine.transform.parent = null;

                    if (secondExamine.faceCamera)
                    {
                        Vector3 rotation = secondExamine.faceRotation;
                        faceRotationSecond = Quaternion.LookRotation(PlayerCam.transform.forward, PlayerCam.transform.up) * Quaternion.Euler(rotation);
                        faceToCameraSecond = true;
                    }

                    foreach (MeshFilter mesh in objectHeld.GetComponentsInChildren<MeshFilter>())
                    {
                        mesh.gameObject.layer = SecondExamineLayer;
                    }

                    if (secondExamine.examineType == InteractiveItem.ExamineType.AdvancedObject)
                    {
                        if (secondExamine.collidersDisable.Length > 0)
                        {
                            foreach (var col in secondExamine.collidersDisable)
                            {
                                col.enabled = false;
                            }
                        }

                        if (secondExamine.collidersEnable.Length > 0)
                        {
                            foreach (var col in secondExamine.collidersEnable)
                            {
                                col.enabled = true;
                            }
                        }
                    }
                }
                else
                {
                    priorityObject = firstExamine;

                    PlayerCam.cullingMask = MainCameraMask;
                    ArmsCam.cullingMask = ArmsCameraMask;

                    ShowExamineUI(false);
                    ShowExamineText(firstExamine);

                    secondExamine.transform.SetParent(oldSecondObjT);
                    secondExamine.transform.position = oldSecondObjPos;
                    secondExamine.transform.rotation = oldSecondRot;

                    secondExamine.floatingIcon = true;

                    if (secondExamine.examineType == InteractiveItem.ExamineType.AdvancedObject)
                    {
                        if (secondExamine.collidersDisable.Length > 0)
                        {
                            foreach (var col in secondExamine.collidersDisable)
                            {
                                col.enabled = true;
                            }
                        }

                        if (secondExamine.collidersEnable.Length > 0)
                        {
                            foreach (var col in secondExamine.collidersEnable)
                            {
                                col.enabled = false;
                            }
                        }
                    }

                    secondExamine = null;
                    secondFaceBreak = false;
                    faceToCameraSecond = false;

                    foreach (MeshFilter mesh in objectHeld.GetComponentsInChildren<MeshFilter>())
                    {
                        mesh.gameObject.layer = ExamineLayer;
                    }
                }
            }
            else
            {
                priorityObject = firstExamine;
                ShowExamineUI(false);

                foreach (MeshFilter mesh in objectHeld.GetComponentsInChildren<MeshFilter>())
                {
                    mesh.gameObject.layer = InteractLayer;
                }
            }
        }

        void ShowExamineUI(bool SecondItem = false)
        {
            if (!SecondItem)
            {
                if (priorityObject.itemType != InteractiveItem.ItemType.OnlyExamine)
                {
                    if (!isInspect)
                    {
                        if (priorityObject.itemType == InteractiveItem.ItemType.GenericItem)
                        {
                            gameManager.ShowExamineSprites(btn2: priorityObject.allowExamineTake, btn3: priorityObject.examineRotate != InteractiveItem.ExamineRotate.None, btn4: priorityObject.enableCursor);
                        }
                        else
                        {
                            gameManager.ShowExamineSprites(btn2: inventory.CheckInventorySpace(), btn3: priorityObject.examineRotate != InteractiveItem.ExamineRotate.None, btn4: priorityObject.enableCursor);
                        }
                    }
                    else
                    {
                        gameManager.ShowExamineSprites(btn2: false, btn3: priorityObject.examineRotate != InteractiveItem.ExamineRotate.None, btn4: priorityObject.enableCursor);
                    }
                }
                else
                {
                    gameManager.ShowExamineSprites(btn2: false, btn3: priorityObject.examineRotate != InteractiveItem.ExamineRotate.None, btn4: priorityObject.enableCursor);
                }
            }
            else
            {
                InteractiveItem secondItem = secondExamine.GetComponent<InteractiveItem>();
                gameManager.ShowExamineSprites(btn2: secondItem.itemType != InteractiveItem.ItemType.OnlyExamine, btn3: priorityObject.examineRotate != InteractiveItem.ExamineRotate.None, btn4: priorityObject.enableCursor);
            }
        }

        void FirstPhase()
        {
            StartCoroutine(AntiSpam());

            priorityObject = firstExamine;
            objectHeld = objectRaycast.gameObject;
            objectPosition = objectHeld.transform.position;
            objectRotation = objectHeld.transform.rotation;

            isPaper = firstExamine.examineType == InteractiveItem.ExamineType.Paper;

            if (examineLight)
            {
                examineLight.enabled = true;
            }

            if (isInspect) firstExamine.isExamined = true;

            if (!isPaper)
            {
                if (!string.IsNullOrEmpty(firstExamine.examineTitle))
                    ShowExamineText(firstExamine);

                ShowExamineUI();
            }
            else
            {
                bool canRotate = firstExamine.examineRotate != InteractiveItem.ExamineRotate.None;
                bool canRead = !string.IsNullOrEmpty(firstExamine.paperText);

                gameManager.ShowPaperExamineSprites(bp_Use, canRotate, canRead, ReadText);
            }

            if (firstExamine.examineSound)
            {
                Utilities.PlayOneShot2D(transform.position, firstExamine.examineSound, firstExamine.examineVolume);
            }

            foreach (MeshFilter mesh in objectHeld.GetComponentsInChildren<MeshFilter>())
            {
                mesh.gameObject.layer = ExamineLayer;
            }

            foreach (MeshRenderer renderer in objectHeld.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            foreach (Collider col in objectHeld.GetComponentsInChildren<Collider>())
            {
                if (col.GetType() != typeof(MeshCollider))
                {
                    col.isTrigger = true;
                }
            }

            if (firstExamine.examineType == InteractiveItem.ExamineType.AdvancedObject)
            {
                if (firstExamine.collidersDisable.Length > 0)
                {
                    foreach (var col in objectHeld.GetComponent<InteractiveItem>().collidersDisable)
                    {
                        col.enabled = false;
                    }
                }

                if (firstExamine.collidersEnable.Length > 0)
                {
                    foreach (var col in objectHeld.GetComponent<InteractiveItem>().collidersEnable)
                    {
                        col.enabled = true;
                    }
                }
            }

            foreach (Rigidbody rb in objectHeld.GetComponentsInChildren<Rigidbody>())
            {
                ExamineRBs.Add(new RigidbodyExamine(rb.gameObject, new RigidbodyExamine.RigidbodyParams() { isKinematic = rb.isKinematic, useGravity = rb.useGravity }));
            }

            foreach (var col in objectHeld.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(col, PlayerCam.transform.root.gameObject.GetComponent<CharacterController>(), true);
            }

            if (firstExamine.faceCamera)
            {
                Vector3 rotation = objectHeld.GetComponent<InteractiveItem>().faceRotation;
                faceRotationFirst = Quaternion.LookRotation(PlayerCam.transform.forward, PlayerCam.transform.up) * Quaternion.Euler(rotation);
                faceToCameraFirst = true;
            }

            PlayerCam.cullingMask = MainCameraMask;
            ArmsCam.cullingMask = ArmsCameraMask;

            floatingIconEn = firstExamine.floatingIcon;
            SetFloatingIconsVisible(false);

            delay.isEnabled = false;
            firstExamine.isExamining = true;
            gameManager.isExamining = true;
            gameManager.HideSprites(0);
            gameManager.LockPlayerControls(false, false, false, 1, true, true, 1);
            GetComponent<ScriptManager>().ScriptEnabledGlobal = false;
            firstDistance = firstExamine.examineDistance.value;
            itemSwitcher.FreeHands(true);

            firstExamine.gameObject.SendMessage("OnExamine", SendMessageOptions.DontRequireReceiver);
            isObjectHeld = true;
        }

        void SetFloatingIconsVisible(bool visible)
        {
            GameObject SecondItem = null;

            if (!firstExamine) return;

            if (firstExamine.examineType == InteractiveItem.ExamineType.AdvancedObject)
            {
                if (objectHeld.GetComponentsInChildren<Transform>().Count(obj => floatingItem.ContainsFloatingIcon(obj.gameObject)) > 0)
                {
                    foreach (var item in objectHeld.GetComponentsInChildren<Transform>().Where(obj => floatingItem.ContainsFloatingIcon(obj.gameObject)).ToArray())
                    {
                        if (item.GetComponent<InteractiveItem>())
                        {
                            item.GetComponent<InteractiveItem>().floatingIcon = !visible;
                            SecondItem = item.gameObject;
                        }
                    }
                }
            }

            foreach (var item in floatingItem.FloatingIconCache)
            {
                if (item.FollowObject != SecondItem)
                {
                    if (item.FollowObject.HasComponent(out InteractiveItem interactiveItem))
                    {
                        interactiveItem.floatingIcon = visible && interactiveItem.floatingIconState;
                    }
                }
            }
        }

        void HoldObject()
        {
            interact.CrosshairVisible(false);
            pfunc.enabled = false;

            float scrollValue = InputHandler.ReadInput<Vector2>("Scroll", "PlayerExtra").y * zoomSpeed;

            if (!secondExamine)
            {
                firstDistance = Mathf.Clamp(firstDistance + scrollValue, firstExamine.examineDistance.RealMin, firstExamine.examineDistance.RealMax);
            }
            else
            {
                secondDistance = Mathf.Clamp(secondDistance + scrollValue, secondExamine.examineDistance.RealMin, secondExamine.examineDistance.RealMax);
                Vector3 second_nextPos = PlayerCam.transform.position + PlayerAim.direction * secondDistance;
                secondExamine.transform.position = Vector3.Lerp(secondExamine.transform.position, second_nextPos, Time.deltaTime * pickupSpeed);

                if (!secondFaceBreak && faceToCameraSecond)
                {
                    secondExamine.transform.rotation = Quaternion.Lerp(secondExamine.transform.rotation, faceRotationSecond, Time.deltaTime * pickupRotateSpeed);
                }
            }

            Vector3 nextPos = PlayerCam.transform.position + PlayerAim.direction * firstDistance;

            if (objectHeld)
            {
                if (objectHeld.GetComponent<Rigidbody>())
                {
                    objectHeld.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Discrete;
                    objectHeld.GetComponent<Rigidbody>().isKinematic = true;
                    objectHeld.GetComponent<Rigidbody>().useGravity = false;
                }

                objectHeld.transform.position = Vector3.Lerp(objectHeld.transform.position, nextPos, Time.deltaTime * pickupSpeed);

                if (!firstFaceBreak && faceToCameraFirst)
                {
                    objectHeld.transform.rotation = Quaternion.Lerp(objectHeld.transform.rotation, faceRotationFirst, Time.deltaTime * pickupRotateSpeed);
                }
            }
        }

        public void ExamineObject(GameObject obj, Vector3 rotation)
        {
            InteractiveItem item = obj.GetComponent<InteractiveItem>();
            firstExamine = item;
            priorityObject = item;

            Vector3 nextPos = PlayerCam.transform.position + PlayerAim.direction * item.examineDistance.value;

            if (!faceToCameraInspect)
            {
                Quaternion faceRotation = Quaternion.LookRotation(PlayerCam.transform.forward, PlayerCam.transform.up) * Quaternion.Euler(rotation);
                obj.transform.rotation = faceRotation;
                faceToCameraInspect = true;
            }

            obj.name = "Inspect: " + item.examineTitle;
            if (obj.GetComponent<Rigidbody>())
            {
                obj.GetComponent<Rigidbody>().isKinematic = false;
                obj.GetComponent<Rigidbody>().useGravity = false;
            }
            obj.transform.position = nextPos;

            if (obj.GetComponent<SaveObject>())
            {
                Destroy(obj.GetComponent<SaveObject>());
            }

            foreach (var col in obj.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(col, PlayerCam.transform.root.gameObject.GetComponent<CharacterController>(), true);
            }

            gameManager.isExamining = true;
            objectRaycast = obj;
            isExamining = true;
            isInspect = true;
        }

        public void CancelExamine()
        {
            isExamining = false;
            gameManager.isExamining = false;
        }

        void DropObject()
        {
            firstExamine.gameObject.SendMessage("OnEndExamine", SendMessageOptions.DontRequireReceiver);

            SetFloatingIconsVisible(true);
            SecondExaminedObject(false);

            firstDistance = 0;
            secondDistance = 0;

            if (examineLight)
            {
                examineLight.enabled = false;
            }

            foreach (MeshFilter mesh in objectHeld.GetComponentsInChildren<MeshFilter>())
            {
                mesh.gameObject.layer = InteractLayer;
            }

            foreach (MeshRenderer renderer in objectHeld.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }

            foreach (Collider col in objectHeld.GetComponentsInChildren<Collider>())
            {
                if (col.GetType() != typeof(MeshCollider))
                {
                    col.isTrigger = false;
                }
            }

            foreach (var col in objectHeld.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(col, PlayerCam.transform.root.gameObject.GetComponent<CharacterController>(), false);
            }

            if (firstExamine.examineType == InteractiveItem.ExamineType.AdvancedObject)
            {
                if (firstExamine.collidersDisable.Length > 0)
                {
                    foreach (var col in objectHeld.GetComponent<InteractiveItem>().collidersDisable)
                    {
                        col.enabled = true;
                    }
                }

                if (firstExamine.collidersEnable.Length > 0)
                {
                    foreach (var col in objectHeld.GetComponent<InteractiveItem>().collidersEnable)
                    {
                        col.enabled = false;
                    }
                }
            }

            if (!isInspect)
            {
                ObjectPutter putter = objectHeld.AddComponent<ObjectPutter>();
                putter.Put(objectPosition, objectRotation, putBackTime, putBackRotateTime, ExamineRBs.ToArray());
            }
            else
            {
                Destroy(objectHeld);
                isInspect = false;
            }

            if (!isPaper)
            {
                if (objectHeld.GetComponent<Collider>().GetType() != typeof(MeshCollider))
                {
                    objectHeld.GetComponent<Collider>().isTrigger = false;
                }
            }

            StopAllCoroutines();
            GetComponent<ScriptManager>().ScriptEnabledGlobal = true;
            gameManager.ShowInventory(false);
            gameManager.ShowExamineName(false, "");
            gameManager.HideSprites(1);
            gameManager.ShowConsoleCursor(false);
            gameManager.isExamining = false;
            scriptManager.IsExamineRaycast = false;
            firstExamine.isExamining = false;
            firstExamine.floatingIcon = floatingIconEn;
            floatingItem.SetAllIconsVisible(true);
            delay.isEnabled = true;
            PlayerCam.cullingMask = DefaultMainCamMask;
            ArmsCam.cullingMask = DefaultArmsCamMask;

            paperUI.SetActive(false);
            pfunc.enabled = true;
            isObjectHeld = false;
            isExamining = false;
            isReading = false;
            tryExamine = false;
            cursorShown = false;
            floatingIconEn = false;

            firstFaceBreak = false;
            secondFaceBreak = false;
            faceToCameraFirst = false;
            faceToCameraSecond = false;
            faceToCameraInspect = false;

            firstExamine = null;
            priorityObject = null;
            objectRaycast = null;
            objectHeld = null;
            ExamineRBs.Clear();

            OnDropObject?.Invoke();
            itemSwitcher.FreeHands(false);

            StartCoroutine(AntiSpam());
            StartCoroutine(UnlockPlayer());
        }

        void TakeObject(bool takeSecond, GameObject take, bool put = false)
        {
            SetFloatingIconsVisible(true);
            SecondExaminedObject(false);
            objectHeld.SendMessage("OnEndExamine", SendMessageOptions.DontRequireReceiver);

            if (put)
            {
                ObjectPutter putter = objectHeld.AddComponent<ObjectPutter>();
                putter.Put(objectPosition, objectRotation, putBackTime, putBackRotateTime, ExamineRBs.ToArray());
            }

            if (!takeSecond)
            {
                StopAllCoroutines();
                GetComponent<ScriptManager>().ScriptEnabledGlobal = true;
                gameManager.ShowInventory(false);
                gameManager.ShowExamineName(false, "");
                gameManager.HideSprites(1);
                gameManager.ShowConsoleCursor(false);
                floatingItem.SetAllIconsVisible(true);
                gameManager.isExamining = false;
                delay.isEnabled = true;
                PlayerCam.cullingMask = DefaultMainCamMask;
                ArmsCam.cullingMask = DefaultArmsCamMask;
                paperUI.SetActive(false);
                pfunc.enabled = true;
                firstExamine.isExamining = false;
                firstExamine = null;
                isObjectHeld = false;
                isExamining = false;
                isReading = false;
                tryExamine = false;
                cursorShown = false;

                firstFaceBreak = false;
                faceToCameraFirst = false;
                faceToCameraInspect = false;

                objectRaycast = null;
                objectHeld = null;
                ExamineRBs.Clear();
                itemSwitcher.FreeHands(false);

                if (examineLight)
                {
                    examineLight.enabled = false;
                }

                StartCoroutine(AntiSpam());
                StartCoroutine(UnlockPlayer());
            }

            interact.Interact(take);
            if (put) take.GetComponent<InteractiveItem>().itemType = InteractiveItem.ItemType.OnlyExamine;
        }

        void ShowExamineText(InteractiveItem IntItem, bool PlaySound = true, bool ExamineOthers = true)
        {
            if (!string.IsNullOrEmpty(IntItem.examineTitle))
            {
                gameManager.isExamining = true;

                if (c_ExamineObject != null)
                    StopCoroutine(c_ExamineObject);

                if (!IntItem.isExamined)
                    c_ExamineObject = StartCoroutine(StartExamining(IntItem, PlaySound, ExamineOthers));
                else
                    gameManager.ShowExamineName(true, IntItem.examineTitle);
            }
            else
            {
                gameManager.ShowExamineName(false, "");
            }
        }

        IEnumerator StartExamining(InteractiveItem IntItem, bool PlaySound, bool ExamineOthers)
        {
            InteractiveItem[] examineItems = null;

            if (ExamineOthers)
            {
                examineItems = FindObjectsOfType<InteractiveItem>().Where(i => i.examineTitle == IntItem.examineTitle).ToArray();
            }

            yield return new WaitForSeconds(timeToExamine);

            gameManager.ShowExamineName(true, IntItem.examineTitle);

            if (examinedSound && PlaySound && !IntItem.isExamined)
            {
                Utilities.PlayOneShot2D(transform.position, examinedSound, examinedVolume);
            }

            if (ExamineOthers && examineItems != null)
            {
                foreach (var inst in examineItems)
                {
                    inst.isExamined = true;
                }
            }
        }

        IEnumerator UnlockPlayer()
        {
            yield return new WaitForFixedUpdate();
            gameManager.LockPlayerControls(true, true, false, 1, false, false, 2);
            interact.CrosshairVisible(true);
        }

        IEnumerator AntiSpam()
        {
            antiSpam = true;
            yield return new WaitForSeconds(spamWaitTime);
            antiSpam = false;
        }
    }
}