/*
 * DraggableObject.cs - by ThunderWire Studio
 * ver. 1.0
*/

using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using WaterBuoyancy;
using WaterBuoyancy.Collections;

namespace HFPS.Systems
{
    /// <summary>
    /// Script defining Draggable Objects.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class DraggableObject : MonoBehaviour, ISaveable
    {
        public enum DragSoundType { None, MoreTimes, Once }

        private WaterVolume water;
        private Collider m_collider;
        private Rigidbody m_rigidbody;

        [Header("Drag Settings")]
        public float dragDistance = 2f;
        public bool dragAndUse = false;
        public bool automaticDistance = true;

        [Header("Drag Sounds")]
        public DragSoundType soundType = DragSoundType.None;
        public AudioClip dragSound;
        [Range(0, 2)] public float dragVolume = 1f;

        [Header("Water Buoyancy")]
        public bool enableWaterBuoyancy = true;
        public bool enableWaterFoam = true;
        public bool calculateDensity = false;
        [Space(5)]
        public float density = 0.75f;
        [Range(0f, 1f)] public float normalizedVoxelSize = 0.5f;
        public float dragInWater = 1f;
        public float angularDragInWater = 1f;

        private float initialDrag;
        private float initialAngularDrag;
        private Vector3 voxelSize;
        private Vector3[] voxels;

        private bool isPlayedOnce;
        private bool isPlayed;

        [HideInInspector]
        public bool isGrabbed;

        void Awake()
        {
            m_collider = GetComponent<Collider>();
            m_rigidbody = GetComponent<Rigidbody>();

            initialDrag = m_rigidbody.drag;
            initialAngularDrag = m_rigidbody.angularDrag;

            if (calculateDensity)
            {
                float objectVolume = MathfUtils.CalculateVolume_Mesh(GetComponent<MeshFilter>().mesh, transform);
                density = m_rigidbody.mass / objectVolume;
            }
        }

        void FixedUpdate()
        {
            if (water != null && voxels.Length > 0)
            {
                Vector3 forceAtSingleVoxel = CalculateMaxBuoyancyForce() / voxels.Length;
                Bounds bounds = m_collider.bounds;
                float voxelHeight = bounds.size.y * normalizedVoxelSize;

                float submergedVolume = 0f;
                for (int i = 0; i < voxels.Length; i++)
                {
                    Vector3 worldPoint = transform.TransformPoint(voxels[i]);

                    float waterLevel = water.GetWaterLevel(worldPoint);
                    float deepLevel = waterLevel - worldPoint.y + (voxelHeight / 2f); // How deep is the voxel                    
                    float submergedFactor = Mathf.Clamp(deepLevel / voxelHeight, 0f, 1f); // 0 - voxel is fully out of the water, 1 - voxel is fully submerged
                    submergedVolume += submergedFactor;

                    Vector3 surfaceNormal = water.GetSurfaceNormal(worldPoint);
                    Quaternion surfaceRotation = Quaternion.FromToRotation(water.transform.up, surfaceNormal);
                    surfaceRotation = Quaternion.Slerp(surfaceRotation, Quaternion.identity, submergedFactor);

                    Vector3 finalVoxelForce = surfaceRotation * (forceAtSingleVoxel * submergedFactor);
                    m_rigidbody.AddForceAtPosition(finalVoxelForce, worldPoint);

                    Debug.DrawLine(worldPoint, worldPoint + finalVoxelForce.normalized, Color.blue);
                }

                submergedVolume /= voxels.Length; // 0 - object is fully out of the water, 1 - object is fully submerged

                m_rigidbody.drag = Mathf.Lerp(initialDrag, dragInWater, submergedVolume);
                m_rigidbody.angularDrag = Mathf.Lerp(initialAngularDrag, angularDragInWater, submergedVolume);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<WaterVolume>() && enableWaterBuoyancy)
            {
                water = other.GetComponent<WaterVolume>();

                if (voxels == null)
                {
                    voxels = CutIntoVoxels();
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<WaterVolume>())
            {
                water = null;
            }
        }

        private Vector3 CalculateMaxBuoyancyForce()
        {
            float objectVolume = m_rigidbody.mass / density;
            Vector3 maxBuoyancyForce = water.Density * objectVolume * -Physics.gravity;

            return maxBuoyancyForce;
        }

        private Vector3[] CutIntoVoxels()
        {
            Quaternion initialRotation = transform.rotation;
            transform.rotation = Quaternion.identity;

            Bounds bounds = m_collider.bounds;
            voxelSize.x = bounds.size.x * normalizedVoxelSize;
            voxelSize.y = bounds.size.y * normalizedVoxelSize;
            voxelSize.z = bounds.size.z * normalizedVoxelSize;
            int voxelsCountForEachAxis = Mathf.RoundToInt(1f / normalizedVoxelSize);
            List<Vector3> voxels = new List<Vector3>(voxelsCountForEachAxis * voxelsCountForEachAxis * voxelsCountForEachAxis);

            for (int i = 0; i < voxelsCountForEachAxis; i++)
            {
                for (int j = 0; j < voxelsCountForEachAxis; j++)
                {
                    for (int k = 0; k < voxelsCountForEachAxis; k++)
                    {
                        float pX = bounds.min.x + voxelSize.x * (0.5f + i);
                        float pY = bounds.min.y + voxelSize.y * (0.5f + j);
                        float pZ = bounds.min.z + voxelSize.z * (0.5f + k);

                        Vector3 point = new Vector3(pX, pY, pZ);
                        if (ColliderUtils.IsPointInsideCollider(point, m_collider, ref bounds))
                        {
                            voxels.Add(transform.InverseTransformPoint(point));
                        }
                    }
                }
            }

            transform.rotation = initialRotation;

            return voxels.ToArray();
        }

        public void OnRigidbodyDrag()
        {
            if (!isPlayedOnce && dragSound && soundType != DragSoundType.None)
            {
                if (soundType == DragSoundType.MoreTimes)
                {
                    AudioSource.PlayClipAtPoint(dragSound, transform.position, dragVolume);
                }
                else if (!isPlayed)
                {
                    AudioSource.PlayClipAtPoint(dragSound, transform.position, dragVolume);
                    isPlayed = true;
                }

                isPlayedOnce = true;
            }
        }

        public void OnRigidbodyRelease()
        {
            isPlayedOnce = false;
        }

        void OnDrawGizmos()
        {
            if (voxels != null)
            {
                for (int i = 0; i < voxels.Length; i++)
                {
                    Gizmos.color = Color.magenta - new Color(0f, 0f, 0f, 0.75f);
                    Gizmos.DrawCube(transform.TransformPoint(voxels[i]), voxelSize * 0.8f);
                }
            }
        }

        public Dictionary<string, object> OnSave()
        {
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            bool isKinematic = rigidbody.isKinematic;
            bool useGravity = rigidbody.useGravity;
            bool freezeRotation = rigidbody.freezeRotation;

            if (isGrabbed)
            {
                isKinematic = false;
                useGravity = true;
                freezeRotation = false;
            }

            return new Dictionary<string, object>()
            {
                { "position", transform.position },
                { "rotation", transform.eulerAngles },
                { "isPlayed", isPlayed },
                { "rigidbody_kinematic", isKinematic },
                { "rigidbody_gravity", useGravity },
                { "rigidbody_freeze", freezeRotation },
            };
        }

        public void OnLoad(JToken token)
        {
            transform.position = token["position"].ToObject<Vector3>();
            transform.eulerAngles = token["rotation"].ToObject<Vector3>();
            isPlayed = (bool)token["isPlayed"];
            GetComponent<Rigidbody>().isKinematic = (bool)token["rigidbody_kinematic"];
            GetComponent<Rigidbody>().useGravity = (bool)token["rigidbody_gravity"];
            GetComponent<Rigidbody>().freezeRotation = (bool)token["rigidbody_freeze"];
        }
    }
}