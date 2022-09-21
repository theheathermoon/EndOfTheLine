using UnityEngine;

namespace FlashlightSystem
{
    public class FlashlightMovement : MonoBehaviour
    {
        private Vector3 v3Offset;
        private GameObject goFollow;
        [HideInInspector] public float speed = 3.0f;

        void Start()
        {
            goFollow = Camera.main.gameObject;
            v3Offset = transform.position - goFollow.transform.position;
        }

        void Update()
        {
            transform.position = goFollow.transform.position + v3Offset;
            transform.rotation = Quaternion.Slerp(transform.rotation, goFollow.transform.rotation, speed * Time.deltaTime);
        }
    }
}
