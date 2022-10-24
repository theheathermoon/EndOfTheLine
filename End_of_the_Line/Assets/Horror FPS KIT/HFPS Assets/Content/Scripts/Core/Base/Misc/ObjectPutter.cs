using System.Collections;
using UnityEngine;
using HFPS.Player;

namespace HFPS.Systems
{
    public class ObjectPutter : MonoBehaviour
    {
        bool isOnPosition;
        bool isOnRotation;

        Collider col;
        ExamineManager.RigidbodyExamine[] rbe;

        public void Put(Vector3 position, Quaternion rotation, float putPositionTime, float putRotationTime, ExamineManager.RigidbodyExamine[] rbExamine = null)
        {
            col = GetComponent<Collider>();
            rbe = rbExamine;

            col.enabled = false;

            StartCoroutine(MoveToPosition(position, putPositionTime));
            StartCoroutine(RotateToRotation(rotation, putRotationTime));
        }

        void Update()
        {
            if (isOnPosition && isOnRotation)
            {
                col.enabled = true;

                if (rbe.Length > 0)
                {
                    foreach (var rb in rbe)
                    {
                        rb.rbObject.GetComponent<Rigidbody>().isKinematic = rb.rbParameters.isKinematic;
                        rb.rbObject.GetComponent<Rigidbody>().useGravity = rb.rbParameters.useGravity;
                    }
                }

                Destroy(this);
            }
        }

        IEnumerator MoveToPosition(Vector3 position, float timeToMove)
        {
            var currentPos = transform.position;
            var t = 0f;
            while (t < 1)
            {
                t += Time.deltaTime / timeToMove;
                transform.position = Vector3.Lerp(currentPos, position, t);
                yield return null;
            }

            isOnPosition = true;
        }

        IEnumerator RotateToRotation(Quaternion rotation, float timeToRotate)
        {
            var currentRot = transform.rotation;
            var t = 0f;
            while (t < 1)
            {
                t += Time.deltaTime / timeToRotate;
                transform.rotation = Quaternion.Lerp(currentRot, rotation, t);
                yield return null;
            }

            isOnRotation = true;
        }
    }
}