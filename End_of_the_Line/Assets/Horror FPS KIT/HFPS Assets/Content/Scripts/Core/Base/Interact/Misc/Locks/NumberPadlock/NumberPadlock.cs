/*
 * NumberPadlock.cs - by ThunderWire Studio
 * ver. 1.1
*/

using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;
using HFPS.Player;

namespace HFPS.Systems
{
    public class NumberPadlock : MonoBehaviour, ISaveable
    {
        private ExamineManager examineManager;

        [Range(0, 6)] public int DigitsCount = 3;
        public int UnlockCode = 123;
        [Space]
        public float DigitRotateAngle;
        public float RotateSpeed;
        public float UnlockWaitTime;
        [Space]
        public Animation m_animation;
        public string unlockAnimation;
        public AudioClip UnlockSound;
        public AudioClip DigitTurnSound;
        [Space]
        public UnityEvent UnlockEvent;

        private int CurrentCode;
        private char[] CodeCache;

        private bool isPlayed;
        private bool isUnlocked;

        void Start()
        {
            CodeCache = new char[DigitsCount];

            for (int i = 0; i < DigitsCount; i++)
            {
                CodeCache[i] = '0';
            }

            examineManager = ScriptManager.Instance.GetComponent<ExamineManager>();

            foreach (var digit in GetComponentsInChildren<NumberPadlockDigits>())
            {
                digit.numberPadlock = this;
            }

            examineManager.OnDropObject += delegate
            {
                if (isUnlocked)
                {
                    gameObject.SetActive(false);
                }
            };
        }

        /// <summary>
        /// Increase digit number. Start with 0.
        /// </summary>
        public void InteractDigit(int digitIndex)
        {
            int num = CodeCache[digitIndex] - '0';
            num += 1;

            if (num > 9)
            {
                num = 0;
            }

            CodeCache[digitIndex] = (char)(num + 48);

            string code = new string(CodeCache);
            CurrentCode = int.Parse(code);

            if (DigitTurnSound) { AudioSource.PlayClipAtPoint(DigitTurnSound, transform.position, 0.1f); }
        }

        void Update()
        {
            if (CurrentCode == UnlockCode)
            {
                StartCoroutine(WaitUnlock());
            }
            else
            {
                StopAllCoroutines();
            }
        }

        IEnumerator WaitUnlock()
        {
            yield return new WaitForSeconds(UnlockWaitTime);

            if (m_animation && !string.IsNullOrEmpty(unlockAnimation))
            {
                if (!isPlayed)
                {
                    if (UnlockSound) { AudioSource.PlayClipAtPoint(UnlockSound, transform.position, 0.75f); }
                    m_animation.wrapMode = WrapMode.Once;
                    m_animation.Play(unlockAnimation);
                    isPlayed = true;
                }

                yield return new WaitUntil(() => !m_animation.isPlaying);
                yield return new WaitForSeconds(0.5f);
            }

            foreach (var digit in GetComponentsInChildren<NumberPadlockDigits>())
            {
                digit.isUsable = false;
            }

            UnlockEvent.Invoke();
            examineManager.CancelExamine();

            isUnlocked = true;
        }

        public Dictionary<string, object> OnSave()
        {
            return new Dictionary<string, object>
            {
                { "isUnlocked", isUnlocked }
            };
        }

        public void OnLoad(JToken token)
        {
            isUnlocked = token["isUnlocked"].ToObject<bool>();

            if (isUnlocked)
            {
                UnlockEvent.Invoke();
                gameObject.SetActive(false);
            }
        }
    }
}