using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class globals {

	// these set which trigger the player is in to pass through to the elevator controller.
	public static bool groundLevel = false;
	public static bool carTrigger = false;

	// used to determine which trigger zone the player is in from the trigger zone name.
	public static string triggerName = "boab";

}
