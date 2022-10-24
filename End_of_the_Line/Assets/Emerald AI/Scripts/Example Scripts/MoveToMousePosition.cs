using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;

namespace EmeraldAI.Example
{
    /// <summary>
    /// An example script that gets an AI with a raycast then moves said AI to the position of the mouse on the terrain.
    /// </summary>
    public class MoveToMousePosition : MonoBehaviour
    {
        public GameObject DestinationEffect;
        public GameObject ArrowIndicatorObject;
        Camera CameraComponent;
        EmeraldAISystem EmeraldComponent;
        Vector3 MovePosition;

        void Start()
        {
            CameraComponent = GetComponent<Camera>();
            ArrowIndicatorObject.SetActive(false);
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray;
                RaycastHit hit;
                ray = CameraComponent.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, 45))
                {
                    if (hit.collider.GetComponent<EmeraldAISystem>() != null)
                    {
                        if (hit.collider.GetComponent<EmeraldAISystem>() != null)
                        {
                            //Only allow the faction of Creature to be selected. For this example, this is the Grenadier (robot) AI.
                            if (hit.collider.GetComponent<EmeraldAISystem>().EmeraldEventsManagerComponent.GetFaction() != "Creature")
                                return;

                            EmeraldComponent = hit.collider.GetComponent<EmeraldAISystem>();
                            ArrowIndicatorObject.SetActive(true);
                        }  
                    }

                    if (EmeraldComponent != null)
                    {
                        if (hit.collider.GetComponent<EmeraldAISystem>() == null)
                        {
                            EmeraldComponent.EmeraldEventsManagerComponent.SetDestinationPosition(hit.point);
                        }

                        if (hit.collider.GetComponent<EmeraldAISystem>() == null)
                        {
                            EmeraldAIObjectPool.SpawnEffect(DestinationEffect, hit.point+Vector3.up*0.25f, Quaternion.identity, 1);
                        }                        
                    }
                    else
                    {
                        ArrowIndicatorObject.SetActive(false);
                    }
                }
            }

            if (EmeraldComponent != null)
            {
                ArrowIndicatorObject.transform.position = EmeraldComponent.transform.position+new Vector3(0,3.5f,0);

                if (EmeraldComponent.CurrentHealth <= 0)
                {
                    EmeraldComponent = null;
                    ArrowIndicatorObject.SetActive(false);
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EmeraldComponent = null;
                ArrowIndicatorObject.SetActive(false);
            }
        }
    }
}