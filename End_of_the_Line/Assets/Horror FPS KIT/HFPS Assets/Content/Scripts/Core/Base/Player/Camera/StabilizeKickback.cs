using UnityEngine;
using System.Collections;

namespace HFPS.Player
{
    public class StabilizeKickback : MonoBehaviour
    {
        private float returnSpeed = 2.0f;

        public void ApplyKickback(Vector3 offset, float time, float returnSpeed = 2f)
        {
            this.returnSpeed = returnSpeed;
            StartCoroutine(StartKickback(offset, time));
        }

        IEnumerator StartKickback(Vector3 offset, float time)
        {
            Quaternion s = transform.localRotation;
            Quaternion e = transform.localRotation * Quaternion.Euler(offset * -1);

            float r = 1.0f / time;
            float t = 0.0f;

            while (t < 1.0f)
            {
                t += Time.deltaTime * r;
                transform.localRotation = Quaternion.Slerp(s, e, t);

                yield return null;
            }
        }

        void LateUpdate()
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, Time.deltaTime * returnSpeed);
        }
    }
}