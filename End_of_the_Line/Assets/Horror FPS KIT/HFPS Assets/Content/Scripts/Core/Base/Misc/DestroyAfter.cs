using UnityEngine;

namespace HFPS.Systems
{
    public class DestroyAfter : MonoBehaviour
    {

        public float destroyAfter = 10f;

        void Start()
        {
            Destroy(gameObject, destroyAfter);
        }

        void OnDisable()
        {
            Destroy(gameObject);
        }
    }
}