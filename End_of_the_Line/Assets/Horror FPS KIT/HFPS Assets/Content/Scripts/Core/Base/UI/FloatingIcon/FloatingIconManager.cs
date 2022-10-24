/*
 * FloatingIconManager.cs - by ThunderWire Games
 * ver. 2.0
*/

using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ThunderWire.Utility;
using HFPS.UI;

namespace HFPS.Systems
{
    public class FloatingIconManager : Singleton<FloatingIconManager>
    {
        public class IconObjectPair
        {
            public GameObject FollowObject;
            public FloatingIcon Icon;

            public IconObjectPair(GameObject obj, FloatingIcon icon)
            {
                FollowObject = obj;
                Icon = icon;
            }
        }

        private HFPS_GameManager gameManager;

        [Header("Floating Icon Objects")]
        public List<GameObject> FloatingIcons = new List<GameObject>();
        public List<IconObjectPair> FloatingIconCache = new List<IconObjectPair>();

        [Header("Raycasting")]
        public LayerMask Layer;

        [Header("UI")]
        public GameObject FloatingIconPrefab;
        public Transform FloatingIconUI;

        [Header("Properties")]
        public float followSmooth = 8;
        public float distanceShow = 3;
        public float distanceKeep = 4.5f;
        public float distanceRemove = 6;

        private GameObject Player;
        private GameObject Cam;

        private bool IsVisibleGlobal;

        void Awake()
        {
            gameManager = GetComponent<HFPS_GameManager>();
            Player = gameManager.m_PlayerObj;
            Cam = Utilities.MainPlayerCamera().gameObject;
        }

        void Update()
        {
            if (FloatingIcons.Count > 0)
            {
                foreach (var obj in FloatingIcons)
                {
                    if (obj != null && Vector3.Distance(obj.transform.position, Player.transform.position) <= distanceShow)
                    {
                        if (!ContainsFloatingIcon(obj) && IsObjectVisibleByCamera(obj) && IsVisibleFrustum(obj))
                        {
                            AddFloatingIcon(obj);
                        }
                    }
                }
            }

            if (FloatingIconCache.Count > 0 && IsVisibleGlobal)
            {
                for (int i = 0; i < FloatingIconCache.Count; i++)
                {
                    IconObjectPair Pair = FloatingIconCache[i];
                    InteractiveItem interactiveItem;

                    if (Pair.FollowObject == null)
                    {
                        Destroy(Pair.Icon.gameObject);
                        FloatingIconCache.RemoveAt(i);
                    }
                    else
                    {
                        if ((interactiveItem = Pair.FollowObject.GetComponent<InteractiveItem>()) != null)
                        {
                            Pair.Icon.SetIconVisible(interactiveItem.floatingIcon);
                        }

                        if (Pair.Icon.isVisible)
                        {
                            if (Pair.FollowObject.GetComponent<Renderer>() && !Pair.FollowObject.GetComponent<Renderer>().enabled)
                            {
                                Destroy(Pair.Icon.gameObject);
                                FloatingIconCache.RemoveAt(i);
                                return;
                            }

                            if (Vector3.Distance(Pair.FollowObject.transform.position, Player.transform.position) <= distanceKeep && IsObjectVisibleByCamera(Pair.FollowObject))
                            {
                                Pair.Icon.OutOfDistance(false);
                            }
                            else if (Pair.Icon.gameObject.activeInHierarchy)
                            {
                                Pair.Icon.OutOfDistance(true);
                            }

                            if (Vector3.Distance(Pair.FollowObject.transform.position, Player.transform.position) >= distanceRemove)
                            {
                                Destroy(Pair.Icon.gameObject);
                                FloatingIconCache.RemoveAt(i);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if object is in distance
        /// </summary>
        public bool ContainsFloatingIcon(GameObject FollowObject)
        {
            return FloatingIconCache.Any(icon => icon.FollowObject == FollowObject);
        }

        /// <summary>
        /// Get IconObject Pair by object
        /// </summary>
        public IconObjectPair GetIcon(GameObject FollowObject)
        {
            return FloatingIconCache.SingleOrDefault(icon => icon.FollowObject == FollowObject);
        }

        /// <summary>
        /// Set visibility state of FollowObject
        /// </summary>
        public void SetIconVisible(GameObject FollowObject, bool state)
        {
            FloatingIconCache.SingleOrDefault(icon => icon.FollowObject == FollowObject).Icon.SetIconVisible(state);
        }

        /// <summary>
        /// Set visibility state of all FloatingIcons
        /// </summary>
        public void SetAllIconsVisible(bool state)
        {
            IsVisibleGlobal = state;

            if (FloatingIconCache.Count > 0)
            {
                for (int i = 0; i < FloatingIconCache.Count; i++)
                {
                    IconObjectPair pair;

                    if ((pair = FloatingIconCache[i]) != null)
                    {
                        InteractiveItem interactiveItem;

                        if (pair.FollowObject && pair.Icon)
                        {
                            if ((interactiveItem = pair.FollowObject.GetComponent<InteractiveItem>()) != null && interactiveItem.floatingIcon)
                            {
                                pair.Icon.SetIconVisible(state);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Destroy GameObject with Floating Icon Safely
        /// </summary>
        public void DestroySafely(GameObject obj, bool destroy = true)
        {
            if (FloatingIconCache.Any(x => x.FollowObject == obj))
            {
                foreach (var cache in FloatingIconCache)
                {
                    Destroy(cache.Icon.gameObject);
                }

                FloatingIconCache.RemoveAll(x => x.FollowObject == obj);
            }

            if (destroy)
            {
                Destroy(obj);
            }
        }

        private void AddFloatingIcon(GameObject FollowObject)
        {
            Vector3 screenPos = Cam.GetComponent<Camera>().WorldToScreenPoint(FollowObject.transform.position);
            GameObject icon = Instantiate(FloatingIconPrefab, screenPos, Quaternion.identity, FloatingIconUI);
            icon.GetComponent<FloatingIcon>().FollowObject = FollowObject;
            icon.GetComponent<FloatingIcon>().iconManager = this;
            icon.transform.position = new Vector2(-20, -20);
            FloatingIconCache.Add(new IconObjectPair(FollowObject, icon.GetComponent<FloatingIcon>()));
        }

        private bool IsObjectVisibleByCamera(GameObject FollowObject)
        {
            if (Physics.Linecast(Cam.transform.position, FollowObject.transform.position, out RaycastHit hit, Layer))
            {
                if (hit.collider.gameObject == FollowObject && FollowObject.GetComponent<Renderer>().isVisible)
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsVisibleFrustum(GameObject target)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Cam.GetComponent<Camera>());
            var point = target.transform.position;

            foreach (var plane in planes)
            {
                if (plane.GetDistanceToPoint(point) < 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}