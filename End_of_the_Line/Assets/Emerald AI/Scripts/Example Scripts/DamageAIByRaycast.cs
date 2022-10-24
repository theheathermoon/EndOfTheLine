using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmeraldAI.Example
{
    /// <summary>
    /// An example script that shoots a raycast from the Camera to the mouse's position.
    /// When a collision with an AI happens, the EmeraldAISystem script is accessed causing damage to the AI.
    /// If there's a PlayerObject assigned, the AI will attack it and treat it as a player.
    /// </summary>
    public class DamageAIByRaycast : MonoBehaviour
    {
        Camera CameraComponent;
        public GameObject PlayerObject;
        public int DamageAmount = 5;
        public float RaycastDistance = 100;

        void Start()
        {
            CameraComponent = GetComponent<Camera>();
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray;
                RaycastHit hit;
                ray = CameraComponent.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, RaycastDistance))
                {
                    if (hit.collider.GetComponent<EmeraldAISystem>() != null)
                    {
                        //Get a reference to the EmeraldAISystem script that is hit by the raycast.
                        EmeraldAISystem EmeraldComponent = hit.collider.GetComponent<EmeraldAISystem>();

                        if (EmeraldComponent.LocationBasedDamageComp == null)
                        {
                            //Cause damage to the AI that is hit using the Emerald AI damage function. 
                            //This is can be a helpfull example for users who have VR games or games where the player is the camera.
                            //In this case, the PlayerObject is a child of the camera.
                            EmeraldComponent.Damage(DamageAmount, EmeraldAISystem.TargetType.Player, PlayerObject.transform, 400);
                        }
                    }

                    if (hit.collider.GetComponent<LocationBasedDamageArea>())
                    {
                        hit.collider.GetComponent<LocationBasedDamageArea>().DamageArea(DamageAmount, EmeraldAISystem.TargetType.Player, PlayerObject.transform, 400);
                    }
                }
            }
        }
    }
}