/*
 * PlayerFunctions.cs - by ThunderWire Games
 * ver. 1.2
*/

using UnityEngine;
using ThunderWire.Input;
using HFPS.Systems;

namespace HFPS.Player
{
    public class PlayerFunctions : MonoBehaviour
    {
        public enum LeanDirections { Normal, Left, Right }

        [Header("Player Lean")]
        public LayerMask LeanMask;
        public float LeanRay;
        public float LeanAngle;
        public float LeanPos;
        public float LeanSpeed;
        public float LeanBackSpeed;
        public float LeanBackDistance;

        [Header("Zoom Effects")]
        public float ZoomSmooth = 10f;
        public float UnzoomSmooth = 5f;
        public float NormalFOV;
        public float ZoomFOV;

        [Header("Other")]
        public Transform inventoryDropPos;
        public bool zoomEnabled = true;

        private bool ZoomKey;
        private bool LeanRight;
        private bool LeanLeft;

        private Camera MainCamera;
        private Camera WeaponCamera;
        private float zoomVelCam;
        private float zoomVelWeap;

        private float defaultFOV;
        private float defaultZoomFOV;
        private float defaultZoomSmooth;
        private float defaultUnzoomSmooth;

        [HideInInspector]
        public bool wallHit = false;

        void Awake()
        {
            defaultFOV = NormalFOV;
            defaultZoomFOV = ZoomFOV;
            defaultZoomSmooth = ZoomSmooth;
            defaultUnzoomSmooth = UnzoomSmooth;
        }

        void Start()
        {
            MainCamera = GetComponent<ScriptManager>().MainCamera;
            WeaponCamera = GetComponent<ScriptManager>().ArmsCamera;
        }

        void Update()
        {
            if (InputHandler.InputIsInitialized)
            {
                if (InputHandler.IsActionExist("LeanLeft"))
                {
                    LeanLeft = InputHandler.ReadButton("LeanLeft");
                }

                if (InputHandler.IsActionExist("LeanRight"))
                {
                    LeanRight = InputHandler.ReadButton("LeanRight");
                }

                ZoomKey = InputHandler.ReadButton("Zoom");
            }

            LeanUpdate();

            if (zoomEnabled && !wallHit)
            {
                if (ZoomKey)
                {
                    MainCamera.fieldOfView = Mathf.SmoothDamp(MainCamera.fieldOfView, ZoomFOV, ref zoomVelCam, ZoomSmooth * Time.deltaTime);

                    if (WeaponCamera)
                    {
                        WeaponCamera.fieldOfView = Mathf.SmoothDamp(MainCamera.fieldOfView, ZoomFOV, ref zoomVelWeap, ZoomSmooth * Time.deltaTime);
                    }
                }
                else
                {
                    MainCamera.fieldOfView = Mathf.SmoothDamp(MainCamera.fieldOfView, NormalFOV, ref zoomVelCam, UnzoomSmooth * Time.deltaTime);

                    if (WeaponCamera)
                    {
                        WeaponCamera.fieldOfView = Mathf.SmoothDamp(MainCamera.fieldOfView, NormalFOV, ref zoomVelWeap, UnzoomSmooth * Time.deltaTime);
                    }
                }
            }
            else
            {
                MainCamera.fieldOfView = Mathf.SmoothDamp(MainCamera.fieldOfView, NormalFOV, ref zoomVelCam, UnzoomSmooth * Time.deltaTime);

                if (WeaponCamera)
                {
                    WeaponCamera.fieldOfView = Mathf.SmoothDamp(MainCamera.fieldOfView, NormalFOV, ref zoomVelWeap, UnzoomSmooth * Time.deltaTime);
                }
            }
        }

        void Lean(LeanDirections Direction)
        {
            switch (Direction)
            {
                case LeanDirections.Normal:
                    MainCamera.transform.localRotation = Quaternion.Slerp(MainCamera.transform.localRotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * LeanBackSpeed);
                    MainCamera.transform.localPosition = Vector3.Lerp(MainCamera.transform.localPosition, new Vector3(0, 0, 0), Time.deltaTime * LeanSpeed);
                    break;
                case LeanDirections.Right:
                    float leanAngle = -LeanAngle;
                    MainCamera.transform.localRotation = Quaternion.Slerp(MainCamera.transform.localRotation, Quaternion.Euler(0, 0, leanAngle), Time.deltaTime * LeanSpeed);
                    MainCamera.transform.localPosition = Vector3.Lerp(MainCamera.transform.localPosition, new Vector3(LeanPos, 0, 0), Time.deltaTime * LeanSpeed);
                    break;
                case LeanDirections.Left:
                    float leanPos = -LeanPos;
                    MainCamera.transform.localRotation = Quaternion.Slerp(MainCamera.transform.localRotation, Quaternion.Euler(0, 0, LeanAngle), Time.deltaTime * LeanSpeed);
                    MainCamera.transform.localPosition = Vector3.Lerp(MainCamera.transform.localPosition, new Vector3(leanPos, 0, 0), Time.deltaTime * LeanSpeed);
                    break;
            }
        }

        void LeanUpdate()
        {
            RaycastHit raycastHit;

            if (LeanRight)
            {
                if (Physics.Raycast(MainCamera.transform.parent.position, MainCamera.transform.parent.TransformDirection(Vector3.right * 1f), out raycastHit, LeanRay, LeanMask))
                {
                    float distance = Vector3.Distance(raycastHit.point, MainCamera.transform.parent.position);

                    if (distance > LeanBackDistance)
                    {
                        Lean(LeanDirections.Right);
                    }
                    else
                    {
                        Lean(LeanDirections.Normal);
                    }
                }
                else
                {
                    Lean(LeanDirections.Right);
                }
            }
            else if (LeanLeft)
            {
                if (Physics.Raycast(MainCamera.transform.parent.position, MainCamera.transform.parent.TransformDirection(Vector3.left * 1f), out raycastHit, LeanRay, LeanMask))
                {
                    float distance = Vector3.Distance(raycastHit.point, MainCamera.transform.parent.position);

                    if (distance > LeanBackDistance)
                    {
                        Lean(LeanDirections.Left);
                    }
                    else
                    {
                        Lean(LeanDirections.Normal);
                    }
                }
                else
                {
                    Lean(LeanDirections.Left);
                }
            }
            else
            {
                Lean(LeanDirections.Normal);
            }
        }

        public void ResetDefaults()
        {
            NormalFOV = defaultFOV;
            ZoomFOV = defaultZoomFOV;
            ZoomSmooth = defaultZoomSmooth;
            UnzoomSmooth = defaultUnzoomSmooth;
        }
    }
}