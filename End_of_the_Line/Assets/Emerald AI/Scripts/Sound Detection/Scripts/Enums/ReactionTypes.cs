namespace EmeraldAI.SoundDetection
{
    public enum ReactionTypes
    {
        None = 0,
        AttractModifier = 25,
        DebugLogMessage = 50,
        Delay = 75,
        EnterCombatState = 100,
        ExitCombatState = 125,
        ExpandDetectionDistance = 150,
        FleeFromLoudestTarget = 162,
        LookAtLoudestTarget = 175,
        MoveAroundCurrentPosition = 200,
        MoveAroundLoudestTarget = 225,
        MoveToLoudestTarget = 250,
        PlayEmoteAnimation = 275,
        PlaySound = 300,
        ResetAllToDefault = 325,
        ResetDetectionDistance = 350,
        ResetLookAtPosition = 375,
        ReturnToStartingPosition = 400,
        SetMovementState = 425, 
    }
}