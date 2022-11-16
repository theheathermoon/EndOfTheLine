using UnityEngine;

namespace MapGenerator
{
    public class CreatePlace_forDoor : MonoBehaviour
    {

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Wall"))
            {
                Destroy(other.gameObject);
            }
        }

    }
}
