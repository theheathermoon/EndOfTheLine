using System.Collections;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine;


public class elevator_controller : MonoBehaviour 
{

	public Animator animator;
	public bool CarUp = false;

	void Awake ()
	{
		animator = GetComponent <Animator>();
		Debug.Log ("animator controller is " + animator);
	}

	void Update()
	{
		// if you have exited triggers
		if (globals.triggerName == "noTrigger")
		{	
			animator.SetBool ("Reset", true);
			animator.SetBool ("GroundControl", false);
			animator.SetBool ("SubwayControl", false);
			animator.SetBool ("CarControl", false);
			animator.SetBool ("CallGround", false);
			animator.SetBool ("CallSubway", false);
			animator.SetBool ("DoorsOpen", false);
			animator.SetBool ("DoorsOpenGround", false);
			animator.SetBool ("DoorsOpenSubway", false);
		}
		// if you are are ground level
		if (globals.triggerName == "groundLevel")
		{	
			animator.SetBool ("GroundControl", true);
			animator.SetBool ("SubwayControl", false);
			animator.SetBool ("CarControl", false);
			animator.SetBool ("Reset", false);
		}
		// if you are at the subway level
		if (globals.triggerName == "subwayLevel")
		{	
			animator.SetBool ("SubwayControl", true);
			animator.SetBool ("CarControl", false);
			animator.SetBool ("GroundControl", false);
			animator.SetBool ("Reset", false);
		}
		// if you are in the elevator
		if (globals.triggerName == "inCar")
		{	
			animator.SetBool ("CarControl", true);
			animator.SetBool ("GroundControl", false);
			animator.SetBool ("SubwayControl", false);
			animator.SetBool ("Reset", false);
		}

		// if you are are ground level
		if ((globals.triggerName == "groundLevel") && (Input.GetKeyDown ("e")))
		{
			Debug.Log ("key pressed");
			Debug.Log ("animator controller is " + animator);
			Debug.Log ("globals groundLevel is " + globals.groundLevel);
			if ((globals.groundLevel == false) && (CarUp == false))
			{
				animator.SetBool ("CallGround", true);
				animator.SetBool ("DoorsOpenGround", true);
				animator.SetBool ("CarUp", true);
				animator.SetBool ("CarDown", false);
				//elevatorLevel = 1;
				globals.groundLevel = true;
				CarUp = true;
				Debug.Log (CarUp);
			} 
			else 
			{
				animator.SetBool ("DoorsOpenGround", true);
			}
		}

		// if you are at the subway level
		if ((globals.triggerName == "subwayLevel") && (Input.GetKeyDown ("e")))
		{
			Debug.Log ("key pressed");
			Debug.Log ("animator controller is " + animator);
			if (CarUp == true) 
			{
				animator.SetBool ("CallSubway", true);
				animator.SetBool ("DoorsOpenSubway", true);
				animator.SetBool ("CarUp", false);
				animator.SetBool ("CarDown", true);
				globals.groundLevel = false;
				CarUp = false;
			} 
			else 
			{
				animator.SetBool ("DoorsOpenSubway", true);
			}
		}

		// if you are in the elevator
		if ((globals.triggerName == "inCar") && (Input.GetKeyDown ("e")))
		{
			Debug.Log ("key pressed");
			Debug.Log ("animator controller is " + animator);
			if (CarUp == true) 
			{
				animator.SetBool ("CarDown", true);
				animator.SetBool ("CarUp", false);
				animator.SetBool ("CallGround", false);
				globals.groundLevel = false;
				CarUp = false;
				Debug.Log ("CarUp state is " + CarUp);
			}
			else 
			{
				animator.SetBool ("CarDown", false);
				animator.SetBool ("CarUp", true);
				animator.SetBool ("CallSubway", false);
				globals.groundLevel = true;
				CarUp = true;
				Debug.Log ("CarUp state is " + CarUp);
			}

		}
	}
	



}




