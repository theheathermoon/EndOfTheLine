/*
 * NumberPadlockDigits.cs - by ThunderWire Studio
 * ver. 1.0
*/

using UnityEngine;

namespace HFPS.Systems
{
    /// <summary>
    /// Extension for NumberPadlock Script
    /// </summary>
    public class NumberPadlockDigits : MonoBehaviour
    {
        [HideInInspector] public NumberPadlock numberPadlock;

        public enum RotateAround { X, Y, Z }

        [Tooltip("Start with 0")]
        public int digitIndex = 0;
        public RotateAround rotateAround = RotateAround.X;

        private float velocity;
        private Vector3 nextDigitRot;
        private Vector3 euler;

        [HideInInspector] public bool isUsable = true;

        void Start()
        {
            euler = transform.localEulerAngles;
            nextDigitRot = transform.localEulerAngles;
        }

        public void Interact()
        {
            if (!numberPadlock || !isUsable) return;

            numberPadlock.InteractDigit(digitIndex);

            if (rotateAround == RotateAround.X)
            {
                nextDigitRot.x += numberPadlock.DigitRotateAngle;
                nextDigitRot.y = 0;
                nextDigitRot.z = 0;
            }
            else if (rotateAround == RotateAround.Y)
            {
                nextDigitRot.x = 0;
                nextDigitRot.y += numberPadlock.DigitRotateAngle;
                nextDigitRot.z = 0;
            }
            else if (rotateAround == RotateAround.Z)
            {
                nextDigitRot.x = 0;
                nextDigitRot.y = 0;
                nextDigitRot.z += numberPadlock.DigitRotateAngle;
            }
        }

        void Update()
        {
            if (rotateAround == RotateAround.X)
            {
                euler.x = Mathf.SmoothDampAngle(euler.x, nextDigitRot.x, ref velocity, Time.deltaTime * numberPadlock.RotateSpeed);
            }
            else if (rotateAround == RotateAround.Y)
            {
                euler.y = Mathf.SmoothDampAngle(euler.y, nextDigitRot.y, ref velocity, Time.deltaTime * numberPadlock.RotateSpeed);
            }
            else if (rotateAround == RotateAround.Z)
            {
                euler.z = Mathf.SmoothDampAngle(euler.z, nextDigitRot.z, ref velocity, Time.deltaTime * numberPadlock.RotateSpeed);
            }

            transform.localEulerAngles = euler;
        }
    }
}