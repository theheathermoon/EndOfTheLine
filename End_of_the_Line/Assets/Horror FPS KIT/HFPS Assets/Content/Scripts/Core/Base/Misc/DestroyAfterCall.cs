using UnityEngine;

namespace HFPS.Systems
{
    public class DestroyAfterCall : MonoBehaviour
    {
        public void DestroyObject(float after)
        {
            Destroy(gameObject, after);
        }
    }
}