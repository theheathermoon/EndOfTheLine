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
    public class LocationsSpawn : MonoBehaviour
    {

        void OnTriggerEnter(Collider other)
        {
            Destroy(gameObject);
        }
    }
}
