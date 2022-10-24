using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine;

public class elevator_trigger : MonoBehaviour 
{
	public GameObject player;
	public GameObject newParent;
	//string triggerName = "fred"; 

	void Awake ()
	{
		//animator = GetComponent <Animator>();
		//Debug.Log (elevatorLevel);
		//Debug.Log ("animator controller is " + animator);
		//Debug.Log ("otherObject is " +otherObject);
	}

	void OnTriggerEnter (Collider other)
	{
		Debug.Log (gameObject.name + " entered trigger " + other.gameObject.name);
		if (other.gameObject.name == "t_ground") 
		{
			globals.triggerName = "groundLevel";
			Debug.Log ("trigger set to " + globals.triggerName);
			//player.transform.parent = null;
			//Display the parent's name in the console.
			//Debug.Log("Player's Parent: " + player.transform.parent.name);
		}
		else if (other.gameObject.name == "t_car") 
		{
			globals.triggerName = "inCar";
			Debug.Log (globals.triggerName);
			{
				//Makes the GameObject "newParent" the parent of the GameObject "player".
				player.transform.parent = newParent.transform;

				//Display the parent's name in the console.
				Debug.Log("Player's Parent: " + player.transform.parent.name);

				// Check if the new parent has a parent GameObject.
				if (newParent.transform.parent != null)
				{
					//Display the name of the grand parent of the player.
					Debug.Log("Player's Grand parent: " + player.transform.parent.parent.name);
				}
			}
		}
		else if (other.gameObject.name == "t_subway") 
		{
			globals.triggerName = "subwayLevel";
			Debug.Log (globals.triggerName);
			//player.transform.parent = null;
			//Display the parent's name in the console.
			//Debug.Log("Player's Parent: " + player.transform.parent.name);
		}
		//Debug.Log ("animator controller is " + animator);
	}

	void OnTriggerExit (Collider other)
	{
		globals.triggerName = "noTrigger";
		Debug.Log (gameObject.name + " exited trigger " + other.gameObject.name);
		Debug.Log (globals.triggerName);
		player.transform.parent = null;
	}
}