using System.Collections;
using UnityEngine;

namespace HFPS.Player
{
    public class Bullet : MonoBehaviour
    {
        public LayerMask checkMask;
        public bool raycastCheck;
        public float timeAlive = 2f;

        private Vector3 lastPosition;

        IEnumerator Start()
        {
            yield return new WaitForSeconds(timeAlive);
            Destroy(gameObject);
        }

        private void Awake()
        {
            if (raycastCheck)
                lastPosition = transform.position;
        }

        private void Update()
        {
            if (raycastCheck && Physics.Linecast(lastPosition, transform.position, checkMask))
            {
                Destroy(gameObject);
            }

            lastPosition = transform.position;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collision.gameObject.CompareTag("Player") && !raycastCheck)
            {
                Destroy(gameObject);
            }
        }

        private void OnParticleCollision(GameObject other)
        {
            if (!other.CompareTag("Player") && !raycastCheck)
            {
                Destroy(gameObject);
            }
        }
    }
}