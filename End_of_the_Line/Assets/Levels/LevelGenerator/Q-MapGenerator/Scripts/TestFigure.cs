///////////////////////////////////
/// Create and edit by QerO
/// 09.2018
/// lidan-357@mail.ru
///////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapGenerator
{
    public class TestFigure : MonoBehaviour
    {

        public float extra_up;
        private float timer;
        private Vector3 rotate;

        void Start()
        {
            timer = 0;
            rotate = new Vector3((Random.Range(-150, 150)), (Random.Range(-150, 150)), (Random.Range(-150, 150)));
            gameObject.transform.position = new Vector3(transform.position.x, transform.position.y + extra_up, transform.position.z);
        }


        void FixedUpdate()
        {
            timer += Time.deltaTime;
            if (timer >= 1)
            {
                timer = 0;
                rotate = new Vector3((Random.Range(-150, 150)), (Random.Range(-150, 150)), (Random.Range(-150, 150)));
            }
            transform.Rotate(rotate * Time.deltaTime);
        }

    }
}
