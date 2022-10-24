using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI.Example
{
    /// <summary>
    /// Disables the projectile part of an Emerald AI Projectile so its trail has time to reach the target and the projectile object isn't visible while waiting.
    /// </summary>
    public class DisableProjectileObject : MonoBehaviour
    {
        public GameObject ObjectToDisable;

        private void OnEnable()
        {
            ObjectToDisable.SetActive(true);
        }

        private void OnTriggerEnter(Collider other)
        {
            ObjectToDisable.SetActive(false);
        }
    }
}