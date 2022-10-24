using UnityEngine;
using ThunderWire.Utility;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace HFPS.Systems
{
    [RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
    public class DynamicObjectPlank : MonoBehaviour, ISaveable
    {

        public float strenght;
        public AudioClip[] woodCrack;

        private Rigidbody objRigidbody;
        private AudioSource audioSource;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            objRigidbody = GetComponent<Rigidbody>();
            objRigidbody.isKinematic = true;
            objRigidbody.useGravity = false;
        }

        void Start()
        {
            if (HFPS_GameManager.HasReference)
            {
                Physics.IgnoreCollision(GetComponent<Collider>(), HFPS_GameManager.Instance.m_PlayerObj.GetComponent<Collider>());
            }
        }

        public void UseObject()
        {
            if (!objRigidbody) return;

            objRigidbody.isKinematic = false;
            objRigidbody.useGravity = true;

            if (woodCrack.Length > 0)
            {
                audioSource.PlayOneShot(woodCrack[Random.Range(0, woodCrack.Length)]);
            }

            objRigidbody.AddForce(-Utilities.MainPlayerCamera().transform.forward * strenght * 10, ForceMode.Force);
            gameObject.tag = "Untagged";
            gameObject.layer = 0;

            enabled = false;
        }

        public Dictionary<string, object> OnSave()
        {
            return new Dictionary<string, object>
        {
            {"isEnabled", enabled},
            {"position", transform.position},
            {"rotation", transform.eulerAngles},
            {"rigidbody_kinematic", GetComponent<Rigidbody>().isKinematic},
            {"rigidbody_gravity", GetComponent<Rigidbody>().useGravity},
            {"rigidbody_freeze", GetComponent<Rigidbody>().freezeRotation}
        };
        }

        public void OnLoad(JToken token)
        {
            enabled = (bool)token["isEnabled"];
            transform.position = token["position"].ToObject<Vector3>();
            transform.eulerAngles = token["rotation"].ToObject<Vector3>();
            GetComponent<Rigidbody>().isKinematic = (bool)token["rigidbody_kinematic"];
            GetComponent<Rigidbody>().useGravity = (bool)token["rigidbody_gravity"];
            GetComponent<Rigidbody>().freezeRotation = (bool)token["rigidbody_freeze"];
        }
    }
}