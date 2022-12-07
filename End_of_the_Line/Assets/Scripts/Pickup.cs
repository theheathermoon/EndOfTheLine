using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField]
    bool isMatches;
    [SerializeField]
    bool isLighterFluid;
    [SerializeField]
    bool isBatteries;

    bool isInTrigger;
    InputManager inputManager;
    InventoryManager inventoryManager;

    private void Start()
    {
        inputManager = InputManager.Instance;
        inventoryManager = InventoryManager.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.name == "Player")
        {
            isInTrigger = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Player")
        {
            isInTrigger = false;
        }
    }

    private void Update()
    {
        if (isInTrigger)
        {
            if (inputManager.PlayerInteracted())
            {
                if (isMatches)
                {
                    inventoryManager.PickUpMatches();
                    Destroy(gameObject);
                }
                if (isLighterFluid)
                {
                    inventoryManager.PickUpLighter();
                    Destroy(gameObject);
                }
                if (isBatteries)
                {
                    inventoryManager.PickUpBatteries();
                    Destroy(gameObject);
                }
            }
        }

    }
}
