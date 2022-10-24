using System.Collections.Generic;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HFPS.Systems
{
    [Serializable]
    public sealed class ObjectReference
    {
        public string GUID;
        public GameObject Object;
    }

    public class ObjectReferences : ScriptableObject
    {
        public sealed class InstantiationResult
        {
            public ObjectReference Reference;
            public GameObject Instantiation;

            public InstantiationResult(ObjectReference reference, GameObject instantiation)
            {
                Reference = reference;
                Instantiation = instantiation;
            }
        }

        public List<ObjectReference> References = new List<ObjectReference>();

        public GameObject Instantiate(string guid, Vector3 position, Quaternion rotation)
        {
            foreach (var elm in References)
            {
                if(elm.GUID == guid)
                {
                    return Object.Instantiate(elm.Object, position, rotation);
                }
            }

            return null;
        }

        public InstantiationResult Instantiate(GameObject gameObject, Vector3 position, Quaternion rotation)
        {
            foreach (var elm in References)
            {
                if (elm.Object == gameObject)
                {
                    GameObject result = Object.Instantiate(gameObject, position, rotation);
                    return new InstantiationResult(elm, result);
                }
            }

            return null;
        }

        public ObjectReference GetObjectReference(string GUID)
        {
            foreach (var elm in References)
            {
                if (elm.GUID == GUID)
                {
                    return elm;
                }
            }

            return null;
        }

        public bool HasReference(string GUID)
        {
            foreach (var elm in References)
            {
                if (elm.GUID == GUID)
                    return true;
            }

            return false;
        }

        public bool HasReference(GameObject Obj)
        {
            foreach (var elm in References)
            {
                if (elm.Object == Obj)
                    return true;
            }

            return false;
        }
    }
}