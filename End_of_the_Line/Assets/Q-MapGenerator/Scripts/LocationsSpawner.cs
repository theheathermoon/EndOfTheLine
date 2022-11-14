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
    public class LocationsSpawner : MonoBehaviour
    {

        public GameObject locations_spawns;

        void Awake()
        {
            Instantiate(locations_spawns, gameObject.transform.position, gameObject.transform.rotation);
        }

    }
}