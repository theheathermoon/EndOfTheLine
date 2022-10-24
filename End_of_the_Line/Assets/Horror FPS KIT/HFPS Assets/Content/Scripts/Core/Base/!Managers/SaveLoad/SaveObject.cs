using System.Collections.Generic;
using UnityEngine;
using ThunderWire.Utility;
using Newtonsoft.Json.Linq;

namespace HFPS.Systems
{
    public class SaveObject : MonoBehaviour, ISaveable
    {
        public enum SaveType { Transform, TransformRigidbody, Position, Rotation, ObjectActive }
        public SaveType saveType = SaveType.Transform;

        public Dictionary<string, object> OnSave()
        {
            if (saveType == SaveType.Transform)
            {
                return new Dictionary<string, object>
                {
                    {"active_state", GetObjectActive()},
                    {"position", transform.position},
                    {"angles", transform.eulerAngles}
                };
            }
            else if (saveType == SaveType.TransformRigidbody)
            {
                return new Dictionary<string, object>
                {
                    {"active_state",  GetObjectActive()},
                    {"position", transform.position},
                    {"angles", transform.eulerAngles},
                    {"rigidbody_kinematic", GetComponent<Rigidbody>().isKinematic},
                    {"rigidbody_gravity", GetComponent<Rigidbody>().useGravity},
                    {"rigidbody_mass", GetComponent<Rigidbody>().mass},
                    {"rigidbody_drag", GetComponent<Rigidbody>().drag},
                    {"rigidbody_angdrag", GetComponent<Rigidbody>().angularDrag},
                    {"rigidbody_freeze", GetComponent<Rigidbody>().freezeRotation},
                    {"rigidbody_velocity", GetComponent<Rigidbody>().velocity},
                };
            }
            else if (saveType == SaveType.Position)
            {
                return new Dictionary<string, object>
                {
                    {"position", transform.position }
                };
            }
            else if (saveType == SaveType.Rotation)
            {
                return new Dictionary<string, object>
                {
                    {"angles", transform.eulerAngles }
                };
            }
            else if (saveType == SaveType.ObjectActive)
            {
                return new Dictionary<string, object>
                {
                    {"active_state", GetObjectActive()}
                };
            }

            return null;
        }

        public void OnLoad(JToken token)
        {
            if (token.HasValues)
            {
                if (saveType == SaveType.Transform)
                {
                    SetObjectActive(token["active_state"].ToObject<IDictionary<string, bool>>());
                    transform.position = token["position"].ToObject<Vector3>();
                    transform.eulerAngles = token["angles"].ToObject<Vector3>();
                }
                else if (saveType == SaveType.TransformRigidbody)
                {
                    SetObjectActive(token["active_state"].ToObject<IDictionary<string, bool>>());
                    transform.position = token["position"].ToObject<Vector3>();
                    transform.eulerAngles = token["angles"].ToObject<Vector3>();
                    GetComponent<Rigidbody>().isKinematic = token["rigidbody_kinematic"].ToObject<bool>();
                    GetComponent<Rigidbody>().useGravity = token["rigidbody_gravity"].ToObject<bool>();
                    GetComponent<Rigidbody>().mass = token["rigidbody_mass"].ToObject<float>();
                    GetComponent<Rigidbody>().drag = token["rigidbody_drag"].ToObject<float>();
                    GetComponent<Rigidbody>().angularDrag = token["rigidbody_angdrag"].ToObject<float>();
                    GetComponent<Rigidbody>().freezeRotation = token["rigidbody_freeze"].ToObject<bool>();
                    GetComponent<Rigidbody>().velocity = token["rigidbody_velocity"].ToObject<Vector3>();
                }
                else if (saveType == SaveType.Position)
                {
                    transform.position = token["position"].ToObject<Vector3>();
                }
                else if (saveType == SaveType.Rotation)
                {
                    transform.eulerAngles = token["angles"].ToObject<Vector3>();
                }
                else if (saveType == SaveType.ObjectActive)
                {
                    SetObjectActive(token["active_state"].ToObject<IDictionary<string, bool>>());
                }
            }
        }

        IDictionary<string, bool> GetObjectActive()
        {
            IDictionary<string, bool> states = new Dictionary<string, bool>();

            if (transform.HasComponent(out MeshRenderer renderer))
            {
                states.Add("renderer", renderer.enabled);
            }

            states.Add("activeSelf", gameObject.activeSelf);

            return states;
        }

        #region Custom Object Events
        public void DisableObject()
        {
            gameObject.SetActive(false);
        }

        public void EnableObject()
        {
            gameObject.SetActive(true);
        }

        public void RendererDisable()
        {
            SetRendererEnabled(false);
        }

        public void RendererEnable()
        {
            SetRendererEnabled(true);
        }

        void SetRendererEnabled(bool state)
        {
            if (gameObject.GetComponent<InteractiveItem>())
            {
                if (state)
                {
                    gameObject.GetComponent<InteractiveItem>().EnableObject();
                }
                else
                {
                    gameObject.GetComponent<InteractiveItem>().DisableObject();
                }
            }
            else
            {
                gameObject.GetComponent<MeshRenderer>().enabled = state;
                gameObject.GetComponent<Collider>().enabled = state;
            }
        }

        void SetObjectActive(IDictionary<string, bool> states)
        {
            if (states.ContainsKey("renderer") && transform.HasComponent(out MeshRenderer renderer))
            {
                renderer.enabled = states["renderer"];
            }

            gameObject.SetActive(states["activeSelf"]);
        }
        #endregion
    }
}