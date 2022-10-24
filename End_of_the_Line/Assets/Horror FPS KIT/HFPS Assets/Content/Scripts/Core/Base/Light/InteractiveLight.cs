/*
 * InteractiveLight.cs - wirted by ThunderWire Games
 * ver. 2.0
*/

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace HFPS.Systems
{
    public class InteractiveLight : MonoBehaviour, ISaveable
    {
        public enum InteractType { Lamp, Switch }
        public enum LightType { Normal, Animation }

        public InteractType interactType = InteractType.Lamp;
        public LightType lightType = LightType.Normal;

        public Light lightObj;
        public MeshRenderer emissionObj;
        public Electricity electricity;
        public string emissionKeyword = "_EMISSION";
        public string emissionString = "_EmissionColor";

        public Animation animationObj;
        public string animationName;

        public string switchOnAnim;
        public string switchOffAnim;

        public AudioClip SwitchOn;
        public AudioClip SwitchOff;
        public float volume = 1f;

        public bool isPoweredOn;
        private bool defaultPower;

        void Awake()
        {
            if (!emissionObj)
            {
                emissionObj = GetComponent<MeshRenderer>();
            }

            if (!lightObj)
            {
                lightObj = GetComponentInChildren<Light>();
            }

            if (lightObj)
            {
                isPoweredOn = lightObj.enabled;
            }
        }

        void Start()
        {
            defaultPower = isPoweredOn;

            if (electricity)
            {
                isPoweredOn = electricity.isPoweredOn && isPoweredOn;
            }

            if (animationObj)
            {
                if (isPoweredOn && lightType == LightType.Animation)
                {
                    if (interactType == InteractType.Lamp)
                    {
                        animationObj.Play(animationName);
                    }
                    else
                    {
                        animationObj.Play(switchOnAnim);
                    }
                }
                else
                {
                    animationObj.Stop();
                }
            }
        }

        void Update()
        {
            if (interactType == InteractType.Switch)
            {
                if (electricity)
                {
                    if (electricity.isPoweredOn)
                    {
                        isPoweredOn = electricity.isPoweredOn && defaultPower;
                    }
                    else
                    {
                        isPoweredOn = false;
                    }
                }
            }
            else
            {
                if (lightType == LightType.Animation)
                {
                    if (electricity)
                    {
                        if (!electricity.isPoweredOn)
                        {
                            animationObj.Stop();
                            lightObj.enabled = false;
                        }
                        else if (isPoweredOn && !animationObj.isPlaying)
                        {
                            animationObj.Play();
                        }
                    }
                }
                else
                {
                    if (electricity)
                    {
                        lightObj.enabled = electricity.isPoweredOn && isPoweredOn;
                    }
                    else
                    {
                        lightObj.enabled = isPoweredOn;
                    }
                }

                if (emissionObj && interactType == InteractType.Lamp)
                {
                    if (lightObj.enabled)
                    {
                        emissionObj.material.EnableKeyword(emissionKeyword);
                        emissionObj.material.SetColor(emissionString, new Color(1f, 1f, 1f));
                    }
                    else
                    {
                        emissionObj.material.SetColor(emissionString, new Color(0f, 0f, 0f));
                    }
                }
            }
        }

        public void UseObject()
        {
            if (electricity)
            {
                if (!electricity.isPoweredOn)
                {
                    electricity.ShowOffHint();
                }
                else
                {
                    SwitchLight();
                }
            }
            else
            {
                SwitchLight();
            }
        }

        public void ToNormalType(bool lightEnb)
        {
            lightType = LightType.Normal;
            animationObj.Stop();
            lightObj.enabled = lightEnb;
            defaultPower = lightEnb;
            isPoweredOn = lightEnb;
        }

        void SwitchLight()
        {
            if (!isPoweredOn)
            {
                if (lightType == LightType.Animation)
                {
                    if (interactType == InteractType.Lamp)
                    {
                        animationObj.Play(animationName);
                    }
                    else
                    {
                        animationObj.Play(switchOnAnim);
                    }
                }

                if (SwitchOn) { AudioSource.PlayClipAtPoint(SwitchOn, transform.position, volume); }
                isPoweredOn = true;
                defaultPower = true;
            }
            else
            {
                if (lightType == LightType.Animation)
                {
                    if (interactType == InteractType.Lamp)
                    {
                        animationObj.Stop();
                        isPoweredOn = false;
                        lightObj.enabled = false;
                    }
                    else
                    {
                        animationObj.Play(switchOffAnim);
                    }
                }

                if (SwitchOff) { AudioSource.PlayClipAtPoint(SwitchOff, transform.position, volume); }
                isPoweredOn = false;
                defaultPower = false;
            }
        }

        public Dictionary<string, object> OnSave()
        {
            return new Dictionary<string, object>
        {
            { "isPoweredOn", isPoweredOn },
            { "defaultPower", defaultPower },
            { "lightType", lightType }
        };
        }

        public void OnLoad(JToken token)
        {
            isPoweredOn = token["isPoweredOn"].ToObject<bool>();
            defaultPower = token["defaultPower"].ToObject<bool>();
            lightType = token["lightType"].ToObject<LightType>();

            if (animationObj)
            {
                if (isPoweredOn && lightType == LightType.Animation)
                {
                    if (interactType == InteractType.Lamp)
                    {
                        animationObj.Play(animationName);
                    }
                    else
                    {
                        animationObj.Play(switchOnAnim);
                    }
                }
                else
                {
                    animationObj.Stop();
                }
            }
        }
    }
}