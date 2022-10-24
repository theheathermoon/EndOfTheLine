using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ThunderWire.Utility;
using HFPS.Player;

namespace HFPS.Systems
{
    public class WaterTrigger : MonoBehaviour
    {
        private PlayerController playerController;
        private Camera main;

        [Serializable]
        public struct WaterFoamClass
        {
            public GameObject FoamObject;
            public ParticleSystem Foam;
        }

        public ParticleSystem WaterFoam;
        public GameObject WaterSplash;
        public float foamShowSpeed = 0.5f;
        public float foamHeight;
        public bool enableWaterFoam;
        public bool isParent;

        private List<WaterFoamClass> WaterFoams = new List<WaterFoamClass>();
        private bool canSplash = false;

        void Awake()
        {
            main = Utilities.MainPlayerCamera();
            playerController = PlayerController.Instance;
        }

        void Start()
        {
            StartCoroutine(SpalshWait());
        }

        IEnumerator SpalshWait()
        {
            yield return new WaitForSeconds(1);
            canSplash = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<Rigidbody>() && other.GetComponent<Rigidbody>().useGravity)
            {
                if (canSplash)
                {
                    Instantiate(WaterSplash, other.gameObject.transform.position, other.gameObject.transform.rotation);
                }

                if (other.GetComponent<DraggableObject>() && other.GetComponent<DraggableObject>().enableWaterFoam && enableWaterFoam)
                {
                    WaterFoams.Add(new WaterFoamClass
                    {
                        FoamObject = other.gameObject,
                        Foam = Instantiate(WaterFoam, other.gameObject.transform.position, transform.rotation)
                    });
                }
            }
        }

        void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (playerController.movementState == PlayerController.MovementState.Ladder)
                {
                    playerController.isInWater = false;
                }
                else
                {
                    playerController.isInWater = true;

                    if (isParent)
                    {
                        playerController.PlayerInWater(transform.parent.position.y + foamHeight);
                    }
                    else
                    {
                        playerController.PlayerInWater(transform.position.y + foamHeight);
                    }

                    playerController.characterState = PlayerController.CharacterState.Stand;
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            for (int i = 0; i < WaterFoams.Count; i++)
            {
                if (WaterFoams[i].FoamObject == other.gameObject && WaterFoams.Count > 0)
                {
                    Destroy(WaterFoams[i].Foam.gameObject);
                    WaterFoams.RemoveAt(i);
                }
            }

            if (other.gameObject == main.transform.root.gameObject)
            {
                playerController.isInWater = false;
            }
        }

        public void DestroyEvent()
        {
            GetComponent<Collider>().enabled = false;
            playerController.isInWater = false;

            if (isParent)
            {
                transform.parent.gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        void Update()
        {
            if (WaterFoams.Count > 0)
            {
                foreach (var variable in WaterFoams)
                {
                    variable.Foam.gameObject.name = "Foam_" + variable.FoamObject.name;

                    Vector3 foamPos = variable.FoamObject.transform.position;
                    if (isParent)
                    {
                        foamPos.y = transform.parent.position.y + foamHeight;
                    }
                    else
                    {
                        foamPos.y = transform.position.y + foamHeight;
                    }
                    variable.Foam.transform.position = foamPos;

                    float speed = variable.FoamObject.GetComponent<Rigidbody>().velocity.magnitude;

                    if (speed > foamShowSpeed)
                    {
                        if (variable.Foam.isStopped)
                            variable.Foam.Play(true);
                    }
                    else
                    {
                        if (variable.Foam.isPlaying)
                            variable.Foam.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    }
                }
            }
        }
    }
}