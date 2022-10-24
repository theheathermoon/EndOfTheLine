using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EmeraldAI.Utility;

namespace EmeraldAI
{
    /// <summary>
    /// This script handles all of Emerald AI's behaviors and states.
    /// </summary>
    public class EmeraldAIBehaviors : MonoBehaviour
    {
        [HideInInspector] public EmeraldAISystem EmeraldComponent;
        [HideInInspector] public bool SearchDelayActive = false;
        [HideInInspector] public bool GeneratingAttack;
        float BlockTimer;
        bool BackupDelayActive;
        float BackupDistance;
        int StartingDetectionRadius;
        float BlockSeconds = 3;

        void Start()
        {
            EmeraldComponent = GetComponent<EmeraldAISystem>();
            StartingDetectionRadius = EmeraldComponent.DetectionRadius;
        }

        /// <summary>
        /// Handles the Aggressive Behavior Type
        /// </summary>
        public void AggressiveBehavior()
        {
            if (EmeraldComponent.CurrentTarget)
            {               
                Vector3 CurrentTargetPos = EmeraldComponent.CurrentTarget.position;
                CurrentTargetPos.y = 0;
                Vector3 CurrentPos = transform.position;
                CurrentPos.y = 0;
                EmeraldComponent.DistanceFromTarget = Vector3.Distance(CurrentTargetPos, CurrentPos);
            }

            EmeraldComponent.ObstructionDetectionUpdateTimer += Time.deltaTime;
            if (!EmeraldComponent.IsTurning && EmeraldComponent.ObstructionDetectionUpdateTimer >= EmeraldComponent.ObstructionDetectionUpdateSeconds)
            {                  
                EmeraldComponent.EmeraldDetectionComponent.CheckForObstructions();
                EmeraldComponent.ObstructionDetectionUpdateTimer = 0;
            }

            if (!EmeraldComponent.IsBackingUp && EmeraldComponent.AIAgentActive && !EmeraldComponent.IsAttacking && EmeraldComponent.CurrentTarget)
            {
                AttackState();

                //If our target exceeds the max chase distance, clear the target and resume wander type by returning to the default state.
                if (EmeraldComponent.MaxChaseDistanceTypeRef == EmeraldAISystem.MaxChaseDistanceType.TargetDistance && EmeraldComponent.DistanceFromTarget > EmeraldComponent.MaxChaseDistance)
                {
                    DefaultState();
                }
                else if (EmeraldComponent.MaxChaseDistanceTypeRef == EmeraldAISystem.MaxChaseDistanceType.StartingDistance && Vector3.Distance(EmeraldComponent.StartingDestination, transform.position) > EmeraldComponent.MaxChaseDistance)
                {
                    EmeraldComponent.ReturningToStartInProgress = true;
                    DefaultState();
                }

                //If using blocking, attempt to trigger the blocking state
                if (EmeraldComponent.UseBlockingRef == EmeraldAISystem.YesOrNo.Yes)
                {
                    BlockState();
                }

                //Monitor the distance away from the current target, 
                //if the backup range is met, trigger the backup state.
                if (EmeraldComponent.BackupTypeRef != EmeraldAISystem.BackupType.Off)
                {
                    if (!EmeraldComponent.IsAttacking && !BackupDelayActive && !EmeraldComponent.IsEquipping && !EmeraldComponent.IsSwitchingWeapons && !EmeraldComponent.DeathDelayActive)
                    {
                        CalculateBackupState();
                    }                
                }
            }

            //Backs AI up when true
            if (EmeraldComponent.IsBackingUp)
            {
                BackupState();                
            }

            //Watch the current target's health for death
            CheckForTargetDeath();

            //If the AI's target becomes null, return to the default state.
            if (EmeraldComponent.CurrentTarget == null && EmeraldComponent.CombatStateRef == EmeraldAISystem.CombatState.Active && !EmeraldComponent.ReturningToStartInProgress && !EmeraldComponent.DeathDelayActive)
            {
                EmeraldComponent.DeathDelayActive = true;
            }
        }

        /// <summary>
        /// Check each TargetType for when a target dies. 
        /// </summary>
        void CheckForTargetDeath ()
        {
            if (EmeraldComponent.TargetTypeRef == EmeraldAISystem.TargetType.AI && !SearchDelayActive)
            {
                if (EmeraldComponent.TargetEmerald != null)
                {
                    if (EmeraldComponent.TargetEmerald.CurrentHealth <= 0 && !EmeraldComponent.DeathDelayActive)
                    {
                        DeathReset();
                    }
                }
            }
            else if (EmeraldComponent.TargetTypeRef == EmeraldAISystem.TargetType.Player && !SearchDelayActive)
            {
                if (EmeraldComponent.PlayerDamageComponent != null && EmeraldComponent.PlayerDamageComponent.IsDead && !EmeraldComponent.DeathDelayActive)
                {
                    DeathReset();                    
                    EmeraldAISystem.IgnoredTargetsList.Add(EmeraldComponent.CurrentTarget); //Add the killed player to the static IgnoredTargetsList so it isn't attacked again. Can be cleared with EmeraldComponent.EmeraldEventsManagerComponent.ClearIgnoredTarget
                    EmeraldComponent.PlayerDamageComponent = null;
                }
            }
            else if (EmeraldComponent.TargetTypeRef == EmeraldAISystem.TargetType.NonAITarget && !SearchDelayActive)
            {
                if (EmeraldComponent.NonAIDamageComponent != null && EmeraldComponent.NonAIDamageComponent.Health <= 0 && !EmeraldComponent.DeathDelayActive)
                {
                    DeathReset();
                    EmeraldComponent.NonAIDamageComponent = null;
                }
            }
        }

        /// <summary>
        /// Resets certain variables after a target dies.
        /// </summary>
        void DeathReset ()
        {
            EmeraldComponent.LockTurning = false;
            EmeraldComponent.OnKillTargetEvent.Invoke();
            EmeraldComponent.DestinationAdjustedAngle = 100;
            EmeraldComponent.AIAnimator.ResetTrigger("Attack");
            EmeraldComponent.AIAnimator.SetBool("Turn Left", false);
            EmeraldComponent.AIAnimator.SetBool("Turn Right", false);
            EmeraldComponent.AttackTimer = -1f;
            if (EmeraldComponent.CurrentTarget != null)
                EmeraldComponent.EmeraldLookAtComponent.SetPreviousLookAtInfo();

            if (EmeraldComponent.DetectionTypeRef == EmeraldAISystem.DetectionType.Trigger)
            {
                SearchDelayActive = true;
                EmeraldComponent.EmeraldDetectionComponent.Invoke("SearchForTarget", 0.5f);
            }
            else
            {
                //Remove the CurrentTarget from the AI's LineOfSightTargets list.
                if (EmeraldComponent.LineOfSightTargets.Contains(EmeraldComponent.CurrentTarget.GetComponent<Collider>()))
                    EmeraldComponent.LineOfSightTargets.Remove(EmeraldComponent.CurrentTarget.GetComponent<Collider>());

                EmeraldComponent.CurrentTarget = null; //Clears the current target
                EmeraldComponent.TargetEmerald = null;
                EmeraldComponent.LineOfSightRef = null;
                EmeraldComponent.EmeraldDetectionComponent.SearchForLineOfSightTarget(); //Search for new potential nearby targets
                EmeraldComponent.EmeraldDetectionComponent.LineOfSightDetection(); //Assign a target that is visible to the AI searching
            }
        }

        /// <summary>
        /// Handles the Companion Behavior Type
        /// </summary>
        public void CompanionBehavior()
        {
            if (EmeraldComponent.CompanionTargetRef != null && EmeraldComponent.CurrentTarget == null)
            {
                if (EmeraldComponent.TargetTypeRef == EmeraldAISystem.TargetType.AI)
                {
                    if (EmeraldComponent.TargetEmerald != null && EmeraldComponent.TargetEmerald.BehaviorRef == EmeraldAISystem.CurrentBehavior.Aggressive ||
                        EmeraldComponent.TargetEmerald != null && EmeraldComponent.TargetEmerald.BehaviorRef == EmeraldAISystem.CurrentBehavior.Cautious)
                    {
                        if (EmeraldComponent.CombatTypeRef == EmeraldAISystem.CombatType.Defensive)
                        {
                            if (EmeraldComponent.TargetEmerald.CombatStateRef == EmeraldAISystem.CombatState.Active)
                            {
                                EmeraldComponent.EmeraldDetectionComponent.SetDetectedTarget(EmeraldComponent.CompanionTargetRef);
                            }
                        }
                        else if (EmeraldComponent.CombatTypeRef == EmeraldAISystem.CombatType.Offensive)
                        {
                            EmeraldComponent.EmeraldDetectionComponent.SetDetectedTarget(EmeraldComponent.CompanionTargetRef);
                        }
                    }
                }
                else
                {
                    EmeraldComponent.EmeraldDetectionComponent.SetDetectedTarget(EmeraldComponent.CompanionTargetRef);
                }
            }
            else if (EmeraldComponent.CurrentTarget != null)
            {
                EmeraldComponent.ObstructionDetectionUpdateTimer += Time.deltaTime;
                if (!EmeraldComponent.IsTurning && EmeraldComponent.ObstructionDetectionUpdateTimer >= EmeraldComponent.ObstructionDetectionUpdateSeconds)
                {
                    EmeraldComponent.EmeraldDetectionComponent.CheckForObstructions();
                    EmeraldComponent.ObstructionDetectionUpdateTimer = 0;
                }

                if (!EmeraldComponent.IsBackingUp && EmeraldComponent.AIAgentActive && !EmeraldComponent.IsAttacking && EmeraldComponent.CurrentTarget)
                {
                    Vector3 CurrentTargetPos = EmeraldComponent.CurrentTarget.position;
                    CurrentTargetPos.y = 0;
                    Vector3 CurrentPos = transform.position;
                    CurrentPos.y = 0;
                    EmeraldComponent.DistanceFromTarget = Vector3.Distance(CurrentTargetPos, CurrentPos);

                    AttackState();

                    //If our target exceeds the max chase distance, clear the target and resume wander type by returning to the default state.
                    if (EmeraldComponent.DistanceFromTarget > EmeraldComponent.MaxChaseDistance)
                    {
                        DefaultState();
                    }

                    //If using blocking, attempt to trigger the blocking state
                    if (EmeraldComponent.UseBlockingRef == EmeraldAISystem.YesOrNo.Yes)
                    {
                        BlockState();
                    }

                    //Monitor the distance away from the current target, 
                    //if the backup range is met, trigger the backup state.
                    if (EmeraldComponent.BackupTypeRef != EmeraldAISystem.BackupType.Off)
                    {
                        if (!EmeraldComponent.IsAttacking && !BackupDelayActive && !EmeraldComponent.IsEquipping && !EmeraldComponent.IsSwitchingWeapons && !EmeraldComponent.DeathDelayActive)
                        {
                            CalculateBackupState();
                        }
                    }
                }

                //Backs AI up when true
                if (EmeraldComponent.IsBackingUp)
                {
                    BackupState();
                }

                //Watch the current target's health for death
                CheckForTargetDeath();
            }
            else if (EmeraldComponent.CombatStateRef == EmeraldAISystem.CombatState.Active && EmeraldComponent.CompanionTargetRef == null && EmeraldComponent.CurrentTarget == null)
            {
                DefaultState();
            }
        }

        /// <summary>
        /// Handles the Coward Behavior Type
        /// </summary>
        public void CowardBehavior()
        {
            if (EmeraldComponent.CurrentTarget)
            {
                Vector3 CurrentTargetPos = EmeraldComponent.CurrentTarget.position;
                CurrentTargetPos.y = 0;
                Vector3 CurrentPos = transform.position;
                CurrentPos.y = 0;
                EmeraldComponent.DistanceFromTarget = Vector3.Distance(CurrentTargetPos, CurrentPos);

                FleeState();

                //If our target exceeds the max chase distance, clear the target and resume wander type by returning to the default state.
                if (EmeraldComponent.DistanceFromTarget > EmeraldComponent.MaxChaseDistance)
                {
                    if (EmeraldComponent.WanderTypeRef == EmeraldAISystem.WanderType.Dynamic)
                    {
                        EmeraldComponent.StartingDestination = this.transform.position + transform.forward * 8;
                    }

                    DefaultState();
                }

                //If our AI target dies, search for another target
                if (EmeraldComponent.TargetTypeRef == EmeraldAISystem.TargetType.AI)
                {
                    if (EmeraldComponent.TargetEmerald != null)
                    {
                        if (EmeraldComponent.TargetEmerald.CurrentHealth <= 0)
                        {
                            EmeraldComponent.EmeraldDetectionComponent.SearchForTarget();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Coward Behavior Type
        /// </summary>
        public void CautiousBehavior()
        {
            if (EmeraldComponent.CurrentTarget)
            {
                Vector3 CurrentTargetPos = EmeraldComponent.CurrentTarget.position;
                CurrentTargetPos.y = 0;
                Vector3 CurrentPos = transform.position;
                CurrentPos.y = 0;
                EmeraldComponent.DistanceFromTarget = Vector3.Distance(CurrentTargetPos, CurrentPos);

                //If our target exceeds the Detection Radius distance, clear the target and resume wander type by returning to the default state.
                //Also, reset the CautiousTimer to 0.
                if (EmeraldComponent.DistanceFromTarget > EmeraldComponent.DetectionRadius)
                {
                    DefaultState();
                    EmeraldComponent.CautiousTimer = 0;
                }
            }

            EmeraldComponent.CautiousTimer += Time.deltaTime;

            if (EmeraldComponent.CautiousTimer >= EmeraldComponent.CautiousSeconds)
            {
                EmeraldComponent.BehaviorRef = EmeraldAISystem.CurrentBehavior.Aggressive;
                EmeraldComponent.CautiousTimer = 0;
            }

            if (EmeraldComponent.UseWarningAnimationRef == EmeraldAISystem.YesOrNo.Yes && !EmeraldComponent.WarningAnimationTriggered && EmeraldComponent.CautiousTimer > 2)
            {
                EmeraldComponent.AIAnimator.SetTrigger("Warning");
                EmeraldComponent.WarningAnimationTriggered = true;
            }
        }

        /// <summary>
        /// Controls what happens when the AI dies
        /// </summary>
        public void DeadState()
        {
            EmeraldComponent.IsDead = true;
            EmeraldComponent.DeathEvent.Invoke();

            //If our AI has a summoner, remove self from their TotalSummonedAI.
            if (EmeraldComponent.CurrentSummoner != null)
            {
                if (EmeraldComponent.CurrentSummoner.SummonsMultipleAIRef == EmeraldAISystem.YesOrNo.Yes)
                {
                    EmeraldComponent.CurrentSummoner.TotalSummonedAI--;
                }
                EmeraldComponent.CurrentSummoner = null;
            }

            if (EmeraldComponent.m_NavMeshAgent.enabled)
            {
                EmeraldComponent.m_NavMeshAgent.ResetPath();
                EmeraldComponent.m_NavMeshAgent.isStopped = true;
                EmeraldComponent.m_NavMeshAgent.enabled = false;
            }

            EmeraldComponent.EmeraldInitializerComponent.InitializeAIDeath();

            if (EmeraldComponent.CreateHealthBarsRef == EmeraldAISystem.YesOrNo.Yes || EmeraldComponent.DisplayAINameRef == EmeraldAISystem.YesOrNo.Yes)
            {
                if (EmeraldComponent.HealthBarCanvas != null)
                {
                    EmeraldComponent.HealthBar.GetComponent<EmeraldAIHealthBar>().FadeOut();
                }
            }
        }

        /// <summary>
        /// Controls our AI's fleeing logic and functionality
        /// </summary>
        public void FleeState()
        {
            if (EmeraldComponent.m_NavMeshAgent.remainingDistance <= EmeraldComponent.StoppingDistance)
            {
                Vector3 direction = (EmeraldComponent.CurrentTarget.position - transform.position).normalized;
                Vector3 GeneratedDestination = transform.position + -direction * 30 + Random.insideUnitSphere * 10;
                GeneratedDestination.y = transform.position.y;
                EmeraldComponent.m_NavMeshAgent.destination = GeneratedDestination;
            }
        }

        /// <summary>
        /// Keeps track of whether or not certain animations are currently playing
        /// </summary>
        public void CheckAnimationStates()
        {
            //Check if an equip or unequip animation is playing
            if (EmeraldComponent.CurrentStateInfo.IsTag("Equip"))
            {
                EmeraldComponent.IsEquipping = true;
            }
            else
            {
                EmeraldComponent.IsEquipping = false;
            }

            //Check if a block idle or hit block animation is playing
            if (EmeraldComponent.CurrentStateInfo.IsTag("Block"))
            {
                EmeraldComponent.IsBlocking = true;
            }
            else
            {
                EmeraldComponent.IsBlocking = false;
            }

            //Check if an idle animation is playing
            if (EmeraldComponent.CurrentStateInfo.IsName("Combat Movement") && !EmeraldComponent.IsMoving || EmeraldComponent.CurrentStateInfo.IsName("Combat Movement (Ranged)") && !EmeraldComponent.IsMoving)
            {
                EmeraldComponent.IsIdling = true;
            }
            else
            {
                EmeraldComponent.IsIdling = false;
            }

            //Check if an attack animation is playing
            if (EmeraldComponent.CombatStateRef == EmeraldAISystem.CombatState.Active)
            {
                if (EmeraldComponent.CurrentStateInfo.IsTag("Attack"))
                {
                    EmeraldComponent.IsAttacking = true;

                    if (EmeraldComponent.CurrentBlockingState == EmeraldAISystem.BlockingState.Blocking)
                    {
                        EmeraldComponent.CurrentBlockingState = EmeraldAISystem.BlockingState.NotBlocking;
                        EmeraldComponent.AIAnimator.SetBool("Blocking", false);
                        EmeraldComponent.AIAnimator.ResetTrigger("Hit");
                    }
                }
                else
                {
                    EmeraldComponent.IsAttacking = false;
                }                  
            }

            //Check if a hit animation is playing
            if (EmeraldComponent.CurrentStateInfo.IsTag("Hit"))
            {
                EmeraldComponent.IsGettingHit = true;
            }
            else
            {
                EmeraldComponent.IsGettingHit = false;
            }

            //Check if a emote animation is playing
            if (EmeraldComponent.CurrentStateInfo.IsTag("Emote"))
            {
                EmeraldComponent.IsEmoting = true;
            }
            else
            {
                EmeraldComponent.IsEmoting = false;
            }
        }

        /// <summary>
        /// Sets the AI back to its default state
        /// </summary>
        public void DefaultState()
        {
            EmeraldComponent.BehaviorRef = (EmeraldAISystem.CurrentBehavior)EmeraldComponent.StartingBehaviorRef;
            EmeraldComponent.ConfidenceRef = (EmeraldAISystem.ConfidenceType)EmeraldComponent.StartingConfidenceRef;
            EmeraldComponent.CombatStateRef = EmeraldAISystem.CombatState.NotActive;
            EmeraldComponent.CurrentDetectionRef = EmeraldAISystem.CurrentDetection.Unaware;
            EmeraldComponent.CurrentBlockingState = EmeraldAISystem.BlockingState.NotBlocking;
            EmeraldComponent.AttackTimer = 0;
            EmeraldComponent.SwitchWeaponTimer = 0;
            EmeraldComponent.RunAttackTimer = 0;
            EmeraldComponent.IsAttacking = false;
            EmeraldComponent.WarningAnimationTriggered = false;
            EmeraldComponent.AIAnimator.SetBool("Turn Right", false);
            EmeraldComponent.AIAnimator.SetBool("Turn Left", false);
            EmeraldComponent.AIAnimator.SetBool("Blocking", false);
            EmeraldComponent.AIAnimator.ResetTrigger("Hit");
            EmeraldComponent.AIAnimator.ResetTrigger("Attack");
            EmeraldComponent.EmeraldEventsManagerComponent.ClearTarget();
            EmeraldComponent.CurrentMovementState = EmeraldComponent.StartingMovementState;
            EmeraldComponent.FirstTimeInCombat = true;
            EmeraldComponent.EmeraldLookAtComponent.SetPreviousLookAtInfo();
            EmeraldComponent.BackingUpTimer = 0;
            EmeraldComponent.CautiousTimer = 0;
            EmeraldComponent.DeathDelayTimer = 0;
            EmeraldComponent.DeathDelayActive = false;
            StopBackingUp(); //Reset the backing up settings and state.
            TransitionIK(); //Transition the AI's IK for non-combat.

            //Resets our AI's stopping distances.
            if (EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Companion)
                EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.StoppingDistance;
            else
                EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.FollowingStoppingDistance;

            if (EmeraldComponent.LookAtTarget != null)
                EmeraldComponent.EmeraldDetectionComponent.DetectTargetType(EmeraldComponent.LookAtTarget);

            if (EmeraldComponent.LookAtTarget != null)
            {
                if (EmeraldComponent.LookAtTarget.GetComponent<TargetPositionModifier>() != null)
                {
                    EmeraldComponent.CurrentTargetPositionModifierComp = EmeraldComponent.LookAtTarget.GetComponent<TargetPositionModifier>();
                    EmeraldComponent.CurrentPositionModifier = EmeraldComponent.CurrentTargetPositionModifierComp.PositionModifier;
                }
                else
                {
                    EmeraldComponent.CurrentPositionModifier = 0;
                }
            }

            //Return the AI to its starting destination to continue wandering based on it WanderType.
            StartCoroutine(DelayReturnToDestination(0));

            //Refill the AI's health according to its refill type.
            if (EmeraldComponent.RefillHealthType == EmeraldAISystem.RefillHealth.Instantly)
                EmeraldComponent.CurrentHealth = EmeraldComponent.StartingHealth;
            else if (EmeraldComponent.RefillHealthType == EmeraldAISystem.RefillHealth.OverTime)
                StartCoroutine(RefillHeathOverTime());
            
            if (EmeraldComponent.ReturningToStartInProgress)
                StartCoroutine(ReturnToStartComplete());
        }

        /// <summary>
        /// Transition the AI's IK for exiting combat.
        /// </summary>
        void TransitionIK ()
        {
            //Only delay the ending of the Combat State Active if an AI is using the Look At feature and they're ranged.
            if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee || EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged && EmeraldComponent.UseHeadLookRef == EmeraldAISystem.YesOrNo.No)
                EmeraldComponent.AIAnimator.SetBool("Combat State Active", false);
            else if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged && EmeraldComponent.UseHeadLookRef == EmeraldAISystem.YesOrNo.Yes)
                StartCoroutine(DelayEquipAnimations());

            //Fade out Range AI's Emerald IK. 
            if (EmeraldComponent.UseHeadLookRef == EmeraldAISystem.YesOrNo.Yes && EmeraldComponent.EmeraldLookAtComponent.BodyWeight > 0)
                EmeraldComponent.EmeraldLookAtComponent.FadeOutBodyIK();
        }

        /// <summary>
        /// Delay equipping until certain conditions are met.
        /// </summary>
        IEnumerator DelayEquipAnimations ()
        {
            yield return new WaitUntil(() => EmeraldComponent.EmeraldLookAtComponent.BodyWeight <= 0.025f);
            EmeraldComponent.AIAnimator.SetBool("Combat State Active", false);

            //Fade out the AI's hand IK if it was active and the AI is using equip animations. If not, ignore this as the hand IK should always be active.
            if (EmeraldComponent.EmeraldHandIKComp != null && EmeraldComponent.EmeraldHandIKComp.RightHandPosWeight > 0 && EmeraldComponent.UseEquipAnimation == EmeraldAISystem.YesOrNo.Yes)
                EmeraldComponent.EmeraldHandIKComp.FadeOutHandWeights();
        }

        /// <summary>
        /// Invoked to delay the SearchForTarget function which allows an AI not immediately attack another target after having just killed one.
        /// </summary>
        public void DelaySearch()
        {
            if (EmeraldComponent.IsDead)
                return;

            SearchDelayActive = false;
            EmeraldComponent.EmeraldDetectionComponent.SearchForTarget();
        }

        //Wait until the AI has returned to its starting destination to reset its detection radius. 
        //This is only used by AI who use the Starting Position Distance Type.
        IEnumerator ReturnToStartComplete()
        {
            EmeraldComponent.CurrentMovementState = EmeraldAISystem.MovementState.Run;
            EmeraldComponent.m_NavMeshAgent.SetDestination(EmeraldComponent.StartingDestination);
            EmeraldComponent.DetectionRadius = 1;
            yield return new WaitUntil(() => EmeraldComponent.m_NavMeshAgent.remainingDistance <= EmeraldComponent.m_NavMeshAgent.stoppingDistance && !EmeraldComponent.m_NavMeshAgent.pathPending);
            EmeraldComponent.DetectionRadius = StartingDetectionRadius;
            EmeraldComponent.CurrentMovementState = EmeraldComponent.StartingMovementState;
            EmeraldComponent.ReturningToStartInProgress = false;
        }

        IEnumerator RefillHeathOverTime ()
        {
            while (EmeraldComponent.CurrentHealth < EmeraldComponent.StartingHealth)
            {
                EmeraldComponent.RegenerateTimer += Time.deltaTime;
                if (EmeraldComponent.RegenerateTimer >= EmeraldComponent.HealthRegRate)
                {
                    EmeraldComponent.CurrentHealth += EmeraldComponent.RegenerateAmount;
                    EmeraldComponent.RegenerateTimer = 0;
                }

                if (EmeraldComponent.CombatStateRef == EmeraldAISystem.CombatState.Active)
                    break;

                yield return null;
            }

            if (EmeraldComponent.CombatStateRef == EmeraldAISystem.CombatState.NotActive)
                EmeraldComponent.CurrentHealth = EmeraldComponent.StartingHealth;
        }

        IEnumerator DelayReturnToDestination (float DelaySeconds)
        {
            yield return new WaitUntil(() => EmeraldComponent.EmeraldLookAtComponent.BodyWeight == 0); //Wait to set the AI's wander position until after it has finished play its equipping animations.
            yield return new WaitForSeconds(DelaySeconds);
            if (EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Companion)
            {
                if (EmeraldComponent.WanderTypeRef == EmeraldAISystem.WanderType.Dynamic)
                {
                    EmeraldComponent.GenerateWaypoint();
                }
                else if (EmeraldComponent.WanderTypeRef == EmeraldAISystem.WanderType.Stationary && EmeraldComponent.m_NavMeshAgent.enabled)
                {
                    EmeraldComponent.m_NavMeshAgent.destination = EmeraldComponent.StartingDestination;
                    EmeraldComponent.ReturnToStationaryPosition = true;
                }
                else if (EmeraldComponent.WanderTypeRef == EmeraldAISystem.WanderType.Waypoints && EmeraldComponent.m_NavMeshAgent.enabled)
                {
                    EmeraldComponent.m_NavMeshAgent.ResetPath();
                    if (EmeraldComponent.WaypointTypeRef != EmeraldAISystem.WaypointType.Random)
                        EmeraldComponent.m_NavMeshAgent.destination = EmeraldComponent.WaypointsList[EmeraldComponent.WaypointIndex];
                    else
                        EmeraldComponent.WaypointTimer = 1;
                }
                else if (EmeraldComponent.WanderTypeRef == EmeraldAISystem.WanderType.Destination && EmeraldComponent.m_NavMeshAgent.enabled)
                {
                    EmeraldComponent.m_NavMeshAgent.destination = EmeraldComponent.SingleDestination;
                    EmeraldComponent.ReturnToStationaryPosition = true;
                }
            }
        }

        /// <summary>
        /// Activates our AI's Combat State
        /// </summary>
        public void ActivateCombatState()
        {
            EmeraldComponent.AIAnimator.ResetTrigger("Hit");
            EmeraldComponent.CombatStateRef = EmeraldAISystem.CombatState.Active;
            EmeraldComponent.AIAnimator.SetBool("Idle Active", false);
            EmeraldComponent.AIAnimator.SetBool("Combat State Active", true);
            EmeraldComponent.CurrentMovementState = EmeraldAISystem.MovementState.Run;

            if (EmeraldComponent.ConfidenceRef == EmeraldAISystem.ConfidenceType.Coward)
                EmeraldComponent.OnFleeEvent.Invoke();
        }

        /// <summary>
        /// Handles all combat related movement.
        /// </summary>
        void CombatMovement ()
        {
            if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee && !EmeraldComponent.DeathDelayActive)
            {
                EmeraldComponent.m_NavMeshAgent.destination = EmeraldComponent.CurrentTarget.position;
            }

            if (EmeraldComponent.DistanceFromTarget > EmeraldComponent.m_NavMeshAgent.stoppingDistance && !EmeraldComponent.DeathDelayActive)
            {
                if (EmeraldComponent.TargetObstructed && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged &&
                    EmeraldComponent.TargetObstructedActionRef != EmeraldAISystem.TargetObstructedAction.StayStationary)
                {
                    EmeraldComponent.m_NavMeshAgent.destination = EmeraldComponent.CurrentTarget.position;
                }
                else if (!EmeraldComponent.TargetObstructed && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged ||
                    EmeraldComponent.TargetObstructed && EmeraldComponent.DistanceFromTarget > EmeraldComponent.AttackDistance && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged)
                {
                    EmeraldComponent.m_NavMeshAgent.destination = EmeraldComponent.CurrentTarget.position;
                }
            }
            else if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged && EmeraldComponent.DistanceFromTarget <= EmeraldComponent.m_NavMeshAgent.stoppingDistance && !EmeraldComponent.TargetObstructed && !EmeraldComponent.DeathDelayActive)
            {
                EmeraldComponent.m_NavMeshAgent.destination = transform.position;
            }

            if (EmeraldComponent.TargetObstructed && EmeraldComponent.TargetObstructedActionRef == EmeraldAISystem.TargetObstructedAction.MoveCloserAfterSetSeconds)
            {
                EmeraldComponent.ObstructionTimer += Time.deltaTime;

                if (EmeraldComponent.ObstructionTimer >= EmeraldComponent.ObstructionSeconds && EmeraldComponent.m_NavMeshAgent.stoppingDistance > 10)
                {
                    EmeraldComponent.m_NavMeshAgent.destination = EmeraldComponent.CurrentTarget.position;
                    EmeraldComponent.m_NavMeshAgent.stoppingDistance -= 5;
                    EmeraldComponent.ObstructionTimer = 0;
                }
            }
        }

        /// <summary>
        /// Calculates our AI's attacks
        /// </summary>
        public void AttackState()
        {
            CombatMovement();

            if (EmeraldComponent.CombatStateRef == EmeraldAISystem.CombatState.Active && !EmeraldComponent.DeathDelayActive && !EmeraldComponent.IsEquipping && !EmeraldComponent.IsSwitchingWeapons)
            {
                if (EmeraldComponent.m_NavMeshAgent.remainingDistance < EmeraldComponent.m_NavMeshAgent.stoppingDistance && !EmeraldComponent.m_NavMeshAgent.pathPending && !EmeraldComponent.IsMoving && !EmeraldComponent.IsAttacking && !EmeraldComponent.IsGettingHit)
                {        
                    if (!GeneratingAttack)
                        EmeraldComponent.AttackTimer += Time.deltaTime;

                    if (EmeraldComponent.AttackTimer >= (EmeraldComponent.AttackSpeed) && 
                        !EmeraldComponent.IsGettingHit && EmeraldComponent.CurrentBlockingState == EmeraldAISystem.BlockingState.NotBlocking)
                    {
                        //Get the distance between the target and the AI. Negate the x and z axes to get the y axis height between the two objects.
                        //This is used to stop AI from being able to trigger attacks that exceed the AI's Attack Height.
                        Vector3 m_TargetPos = EmeraldComponent.CurrentTarget.position;
                        m_TargetPos.x = 0;
                        m_TargetPos.z = 0;
                        Vector3 m_CurrentPos = EmeraldComponent.HitPointTransform.position;
                        m_CurrentPos.x = 0;
                        m_CurrentPos.z = 0;
                        float m_TargetHeight = Vector3.Distance(m_TargetPos, m_CurrentPos);

                        //Cancels the AI's attack if certain conditions aren'nt met.
                        if (EmeraldComponent.EmeraldLookAtComponent.LookAtInProgress && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged || EmeraldComponent.IsSwitchingWeapons || 
                            EmeraldComponent.IsEquipping || EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged && EmeraldComponent.IKType == EmeraldAISystem.IKTypes.EmeraldIK && EmeraldComponent.EmeraldLookAtComponent.BodyWeight <= 0.25f)
                        {
                            EmeraldComponent.EmeraldEventsManagerComponent.CancelAttackAnimation();
                            return;
                        }

                        float angle = EmeraldComponent.TargetAngle();
                        if (EmeraldComponent.UseHeadLookRef == EmeraldAISystem.YesOrNo.No)
                        {
                            if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged && angle > EmeraldComponent.MaxFiringAngle || EmeraldComponent.IsRunAttack || EmeraldComponent.IsTurning)
                            {
                                EmeraldComponent.EmeraldEventsManagerComponent.CancelAttackAnimation();
                                return;
                            }
                        }

                        if (!EmeraldComponent.TargetObstructed && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged || 
                            EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee && !EmeraldComponent.m_NavMeshAgent.pathPending && 
                            EmeraldComponent.DestinationAdjustedAngle <= EmeraldComponent.MaxDamageAngle && m_TargetHeight <= EmeraldComponent.AttackHeight && !EmeraldComponent.TargetObstructed)
                        {
                            EmeraldComponent.GeneratedBlockOdds = Random.Range(1, 101);
                            EmeraldComponent.GeneratedBackupOdds = Random.Range(1, 101);
                            EmeraldComponent.AIAnimator.ResetTrigger("Hit");
                            EmeraldComponent.CurrentBlockingState = EmeraldAISystem.BlockingState.NotBlocking;
                            EmeraldComponent.AIAnimator.SetBool("Blocking", false);

                            if (EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.No && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged ||
                                EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.Yes && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged)
                            {
                                float AbilityOddsRoll = Random.Range(0f, 1f);
                                AbilityOddsRoll = AbilityOddsRoll * 100;

                                if (!EmeraldComponent.m_AbilityPicked && EmeraldComponent.SupportAbilities.Count > 0 && !EmeraldComponent.HealingCooldownActive && 
                                    ((float)EmeraldComponent.CurrentHealth / EmeraldComponent.StartingHealth*100) < EmeraldComponent.HealthPercentageToHeal)
                                {
                                    EmeraldAICombatManager.GenerateSupportAbility(EmeraldComponent);
                                    EmeraldComponent.AIAnimator.SetInteger("Attack Index", EmeraldComponent.CurrentAnimationIndex + 1);
                                    EmeraldComponent.AIAnimator.SetTrigger("Attack");
                                    EmeraldComponent.m_AbilityPicked = true;
                                    GeneratingAttack = true;
                                }
                                else if (!EmeraldComponent.m_AbilityPicked && EmeraldComponent.TotalSummonedAI < EmeraldComponent.MaxAllowedSummonedAI && EmeraldComponent.SummoningAbilities.Count > 0)
                                {
                                    EmeraldAICombatManager.GenerateSummoningAbility(EmeraldComponent);
                                    EmeraldComponent.AIAnimator.SetInteger("Attack Index", EmeraldComponent.CurrentAnimationIndex + 1);
                                    EmeraldComponent.AIAnimator.SetTrigger("Attack");
                                    EmeraldComponent.m_AbilityPicked = true;
                                    GeneratingAttack = true;
                                }
                                else if (!EmeraldComponent.m_AbilityPicked && EmeraldComponent.OffensiveAbilities.Count > 0)                             
                                {
                                    if (!EmeraldComponent.EmeraldLookAtComponent.LookAtInProgress && EmeraldComponent.UseHeadLookRef == EmeraldAISystem.YesOrNo.Yes ||
                                        EmeraldComponent.UseHeadLookRef == EmeraldAISystem.YesOrNo.No)
                                    {
                                        EmeraldAICombatManager.GenerateOffensiveAbility(EmeraldComponent);
                                        EmeraldComponent.m_InitialTargetPosition = EmeraldComponent.CurrentTarget.position + (EmeraldComponent.CurrentTarget.up * EmeraldComponent.CurrentPositionModifier);
                                        EmeraldComponent.AIAnimator.SetInteger("Attack Index", EmeraldComponent.CurrentAnimationIndex + 1);
                                        EmeraldComponent.AIAnimator.SetTrigger("Attack");
                                        EmeraldComponent.m_AbilityPicked = true;
                                        GeneratingAttack = true;
                                    }
                                }
                            }

                            if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee)
                            {
                                if (!EmeraldComponent.IsTurning)
                                {
                                    EmeraldAICombatManager.GenerateMeleeAttack(EmeraldComponent);
                                    EmeraldComponent.AIAnimator.SetInteger("Attack Index", EmeraldComponent.CurrentAnimationIndex + 1);
                                    EmeraldComponent.AIAnimator.SetTrigger("Attack");
                                    GeneratingAttack = true;
                                }
                            }

                            EmeraldComponent.m_AbilityPicked = false;
                            EmeraldComponent.AttackTimer = 0;
                            if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee)
                                EmeraldComponent.AttackSpeed = Random.Range((float)EmeraldComponent.MinMeleeAttackSpeed, EmeraldComponent.MaxMeleeAttackSpeed + 1);
                            else if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged)
                                EmeraldComponent.AttackSpeed = Random.Range((float)EmeraldComponent.MinRangedAttackSpeed, EmeraldComponent.MaxRangedAttackSpeed + 1);
                            EmeraldComponent.OnAttackEvent.Invoke(); //Invoke the AttackStart event
                            Invoke("ResetAttackGeneration", 0.5f);
                        }
                        else if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee && m_TargetHeight > EmeraldComponent.AttackHeight)
                        {
                            EmeraldComponent.AttackTimer = 0;
                        }
                    }
                }
                else if (EmeraldComponent.m_NavMeshAgent.remainingDistance > EmeraldComponent.m_NavMeshAgent.stoppingDistance && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee 
                    && EmeraldComponent.CurrentBlockingState == EmeraldAISystem.BlockingState.NotBlocking)
                {
                    if (EmeraldComponent.AttackOnArrivalRef == EmeraldAISystem.YesOrNo.Yes)
                    {
                        EmeraldComponent.AttackTimer = EmeraldComponent.AttackSpeed - Mathf.Round(Random.Range(0.0f, 0.5f) * 10) / 10;
                    }
                }
                else if (EmeraldComponent.m_NavMeshAgent.remainingDistance > EmeraldComponent.m_NavMeshAgent.stoppingDistance && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged)
                {
                    EmeraldComponent.AttackTimer = 0;
                }

                if (EmeraldComponent.UseRunAttacks == EmeraldAISystem.YesOrNo.Yes)
                {
                    if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged && EmeraldComponent.RangedRunAttackAnimationList.Count > 0 ||
                        EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee && EmeraldComponent.RunAttackAnimationList.Count > 0)
                    {
                        if (Vector3.Distance(EmeraldComponent.CurrentTarget.position, transform.position) <= (EmeraldComponent.m_NavMeshAgent.stoppingDistance +
                            EmeraldComponent.RunAttackDistance) && EmeraldComponent.IsMoving)
                        {
                            EmeraldComponent.RunAttackTimer += Time.deltaTime;

                            if (EmeraldComponent.RunAttackTimer >= EmeraldComponent.RunAttackSpeed)
                            {
                                EmeraldComponent.CurrentBlockingState = EmeraldAISystem.BlockingState.NotBlocking;
                                EmeraldComponent.AIAnimator.SetBool("Blocking", false);
                                EmeraldComponent.AIAnimator.ResetTrigger("Hit");
                                if (EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.No ||
                                    EmeraldComponent.EnableBothWeaponTypes == EmeraldAISystem.YesOrNo.Yes && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee)
                                {
                                    EmeraldAICombatManager.GenerateMeleeRunAttack(EmeraldComponent);
                                    EmeraldComponent.AIAnimator.SetInteger("Run Attack Index", EmeraldComponent.CurrentRunAttackAnimationIndex + 1);
                                    EmeraldComponent.AIAnimator.SetTrigger("Run Attack");
                                }
                                else
                                {
                                    EmeraldAICombatManager.GenerateMeleeRunAttack(EmeraldComponent);
                                    EmeraldComponent.AIAnimator.SetInteger("Run Attack Index", EmeraldComponent.CurrentRunAttackAnimationIndex + 1);
                                    EmeraldComponent.AIAnimator.SetTrigger("Run Attack");
                                }
                                EmeraldComponent.RunAttackSpeed = Random.Range(EmeraldComponent.MinimumRunAttackSpeed, EmeraldComponent.MaximumRunAttackSpeed);
                                EmeraldComponent.RunAttackTimer = 0;
                            }
                        }
                    }
                }
            }
        }

        void ResetAttackGeneration ()
        {
            GeneratingAttack = false;
        }

        /// <summary>
        /// Calculates and applies the Block State
        /// </summary>
        public void BlockState()
        {
            //Activates blocking, when the appropriate conditions are met.
            if (!EmeraldComponent.IsBackingUp && !EmeraldComponent.IsMoving && EmeraldComponent.CurrentTarget && !EmeraldComponent.DeathDelayActive && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee)
            {
                if (EmeraldComponent.CombatStateRef == EmeraldAISystem.CombatState.Active && !EmeraldComponent.IsAttacking && !EmeraldComponent.IsGettingHit && EmeraldComponent.AIAgentActive && !EmeraldComponent.IsBlocking &&
                    EmeraldComponent.m_NavMeshAgent.remainingDistance <= EmeraldComponent.m_NavMeshAgent.stoppingDistance && !EmeraldComponent.m_NavMeshAgent.pathPending && !EmeraldComponent.AIAnimator.GetBool("Attack"))
                {
                    if (EmeraldComponent.AttackTimer < EmeraldComponent.AttackSpeed - 0.25f && EmeraldComponent.GeneratedBlockOdds <= EmeraldComponent.BlockOdds && 
                       EmeraldComponent.IsIdling  && EmeraldComponent.CurrentBlockingState == EmeraldAISystem.BlockingState.NotBlocking) 
                    {
                        EmeraldComponent.CurrentBlockingState = EmeraldAISystem.BlockingState.Blocking;
                        EmeraldComponent.AIAnimator.SetBool("Blocking", true);
                        EmeraldComponent.EmeraldEventsManagerComponent.CancelAttackAnimation();
                        BlockSeconds = Mathf.Round(Random.Range(1.5f, 3.5f) * 10) /10;
                    }
                }
            }

            if (EmeraldComponent.CurrentBlockingState == EmeraldAISystem.BlockingState.Blocking)
            {
                BlockTimer += Time.deltaTime;

                //Disables blocking, when the appropriate conditions are met.
                if (BlockTimer >= BlockSeconds || EmeraldComponent.IsBackingUp || EmeraldComponent.DistanceFromTarget > (EmeraldComponent.m_NavMeshAgent.stoppingDistance + 0.5f))
                {
                    EmeraldComponent.CurrentBlockingState = EmeraldAISystem.BlockingState.NotBlocking;
                    EmeraldComponent.AIAnimator.SetBool("Blocking", false);

                    if (BlockTimer >= BlockSeconds)
                    {
                        EmeraldComponent.AIAnimator.SetBool("Block Cooldown Active", true);
                        Invoke("ActivateBlockCooldown", 2f);
                        BlockTimer = 0;
                    }
                }
            }
        }

        void ActivateBlockCooldown ()
        {
            EmeraldComponent.AIAnimator.SetBool("Block Cooldown Active", false);
        }

        /// <summary>
        /// Calculates and applies our AI's Backup State
        /// </summary>
        public void BackupState ()
        {
            EmeraldComponent.BackingUpTimer += Time.deltaTime;
            EmeraldComponent.IsBackingUp = true;

            if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged)
                EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.StoppingDistance;


            if (EmeraldComponent.AnimatorType == EmeraldAISystem.AnimatorTypeState.NavMeshDriven)
            {
                if (EmeraldComponent.IsAttacking)
                {
                    EmeraldComponent.BackingUpTimer = 0;
                    EmeraldComponent.m_NavMeshAgent.speed = 0;
                }
                else if (!EmeraldComponent.IsAttacking)
                {
                    EmeraldComponent.m_NavMeshAgent.speed = EmeraldComponent.WalkBackwardsSpeed;
                }
            }

            Vector3 direction = (EmeraldComponent.CurrentTarget.position - transform.position).normalized;
            Vector3 GeneratedDestination = transform.position + -direction * 10;
            GeneratedDestination.y = transform.position.y;
            EmeraldComponent.m_NavMeshAgent.destination = GeneratedDestination;

            //Get the angle between the current target and the AI. If using the alignment feature,
            //adjust the angle to include the rotation difference of the AI's current surface angle.
            Vector3 Direction = new Vector3(EmeraldComponent.CurrentTarget.position.x, 0, EmeraldComponent.CurrentTarget.position.z) - new Vector3(transform.position.x, 0, transform.position.z);
            float TargetAngle = Vector3.Angle(transform.forward, Direction);
            Vector3 DestinationDirection = Direction;

            if (EmeraldComponent.AlignAIWithGroundRef == EmeraldAISystem.YesOrNo.Yes)
            {
                float RoationDifference = transform.localEulerAngles.x;
                RoationDifference = (RoationDifference > 180) ? RoationDifference - 360 : RoationDifference;
                EmeraldComponent.DestinationAdjustedAngle = Mathf.Abs(TargetAngle) - Mathf.Abs(RoationDifference);
            }
            else if (EmeraldComponent.AlignAIWithGroundRef == EmeraldAISystem.YesOrNo.No)
            {
                EmeraldComponent.DestinationAdjustedAngle = Mathf.Abs(TargetAngle);
            }

            if (EmeraldComponent.AlignAIWithGroundRef == EmeraldAISystem.YesOrNo.Yes)
            {
                Quaternion qTarget = Quaternion.LookRotation(DestinationDirection, Vector3.up);
                Quaternion qGround = Quaternion.FromToRotation(Vector3.up, EmeraldComponent.SurfaceNormal) * qTarget;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, qGround, Time.deltaTime * EmeraldComponent.BackupTurningSpeed);
            }
            else if (EmeraldComponent.AlignAIWithGroundRef == EmeraldAISystem.YesOrNo.No)
            {
                Quaternion qTarget = Quaternion.LookRotation(DestinationDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, qTarget, Time.deltaTime * EmeraldComponent.BackupTurningSpeed);
            }

            //Get the distance while backing up. If the hit point disatnce becomes less than or equal to 1, it will stop the back up process.
            RaycastHit HitBehind;
            Debug.DrawRay(EmeraldComponent.HeadTransform.position, -transform.forward * 8 - transform.up * 0.5f);
            if (Physics.Raycast(EmeraldComponent.HeadTransform.position, -transform.forward * 8 - transform.up * 0.5f, out HitBehind, EmeraldComponent.BackupDistance, EmeraldComponent.BackupLayerMask))
            {
                if (HitBehind.collider != null && HitBehind.collider.gameObject != this.gameObject && !HitBehind.transform.IsChildOf(this.transform))
                {
                    BackupDistance = HitBehind.distance;
                }
            }

            //Return if these conditions are met to stop an AI from backing up
            if (EmeraldComponent.CurrentTarget == null || EmeraldComponent.DeathDelayActive || BackupDistance <= EmeraldComponent.StoppingDistance-0.5f || EmeraldComponent.IsEquipping || EmeraldComponent.IsSwitchingWeapons)
            {
                StopBackingUp();
                return;
            }

            //If the AI has reached its back up seconds, or the AI has met the backup distance, stop the AI's backup process.
            if (EmeraldComponent.BackingUpTimer >= EmeraldComponent.BackingUpSeconds ||
            EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee && EmeraldComponent.DistanceFromTarget >= EmeraldComponent.m_NavMeshAgent.stoppingDistance*0.75f && !EmeraldComponent.m_NavMeshAgent.pathPending)
            {
                StopBackingUp();
            }
            
            if (EmeraldComponent.BackingUpTimer >= EmeraldComponent.BackingUpSeconds + 0.5f)
            {
                float CurrentDistance = Vector3.Distance(EmeraldComponent.CurrentTarget.position, transform.position);
                
                if (CurrentDistance > 3 && CurrentDistance < 8 && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee)
                {
                    StopBackingUp();

                    if (EmeraldComponent.UseRunAttacks == EmeraldAISystem.YesOrNo.Yes)
                    {
                        if (EmeraldComponent.RunAttackAnimationList.Count > 0)
                        {
                            EmeraldComponent.m_NavMeshAgent.destination = EmeraldComponent.CurrentTarget.position;
                            EmeraldAICombatManager.GenerateMeleeRunAttack(EmeraldComponent);
                            EmeraldComponent.AIAnimator.SetInteger("Run Attack Index", EmeraldComponent.CurrentRunAttackAnimationIndex + 1);
                            EmeraldComponent.AIAnimator.SetTrigger("Run Attack");
                            EmeraldComponent.IsRunAttack = true;
                        }
                    }
                }
                else
                {
                    EmeraldComponent.m_NavMeshAgent.destination = EmeraldComponent.CurrentTarget.position;
                }

                if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged)
                {
                    EmeraldComponent.AttackTimer = EmeraldComponent.AttackSpeed;
                }

                BackupDelayActive = true;
                Invoke("BackupDelay", 1);
            }
        }

        void BackupDelay ()
        {
            BackupDelayActive = false;
        }

        /// <summary>
        /// Stops the AI's backing up process and resets all of its settings.
        /// </summary>
        public void StopBackingUp()
        {
            EmeraldComponent.EmeraldEventsManagerComponent.CancelAttackAnimation();
            EmeraldComponent.AIAnimator.ResetTrigger("Hit");
            EmeraldComponent.AIAnimator.SetBool("Walk Backwards", false);
            EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.AttackDistance;
            EmeraldComponent.m_NavMeshAgent.updateRotation = true;
            EmeraldComponent.m_NavMeshAgent.destination = transform.position;
            EmeraldComponent.IsBackingUp = false;
            EmeraldComponent.BackingUpTimer = 0;
            EmeraldComponent.AttackTimer = 0;
            EmeraldComponent.BackingUpSeconds = Random.Range(EmeraldComponent.BackingUpSecondsMin, EmeraldComponent.BackingUpSecondsMax + 1);
            EmeraldComponent.GeneratedBackupOdds = Random.Range(1, 101);
        }

        /// <summary>
        /// Calculates backing our AI up, when the appropriate conditions are met
        /// </summary>
        public void CalculateBackupState()
        {
            if (EmeraldComponent.CurrentTarget != null && !BackupDelayActive)
            {
                if (EmeraldComponent.BackupTypeRef == EmeraldAISystem.BackupType.Instant || 
                    EmeraldComponent.BackupTypeRef == EmeraldAISystem.BackupType.Odds && EmeraldComponent.GeneratedBackupOdds <= EmeraldComponent.BackupOdds && EmeraldComponent.AttackTimer > 0.5f)
                {
                    if (EmeraldComponent.DistanceFromTarget <= EmeraldComponent.TooCloseDistance && !EmeraldComponent.m_NavMeshAgent.pathPending && !EmeraldComponent.IsAttacking)
                    {
                        float AdjustedAngle = EmeraldComponent.TargetAngle();
                        
                        if (AdjustedAngle <= 60 && !EmeraldComponent.IsTurning)
                        {
                            //Do a quick raycast to see if behind the AI is clear before calling the backup state.
                            RaycastHit HitBehind;
                            if (Physics.Raycast(EmeraldComponent.HeadTransform.position, -transform.forward, out HitBehind, EmeraldComponent.BackupDistance, EmeraldComponent.BackupLayerMask))
                            {
                                if (HitBehind.collider != null && HitBehind.distance > 5)
                                {
                                    if (EmeraldComponent.AnimatorType == EmeraldAISystem.AnimatorTypeState.NavMeshDriven)
                                    {
                                        EmeraldComponent.m_NavMeshAgent.speed = EmeraldComponent.WalkBackwardsSpeed;
                                    }
                                    EmeraldComponent.CurrentBlockingState = EmeraldAISystem.BlockingState.NotBlocking;
                                    EmeraldComponent.AIAnimator.SetBool("Blocking", false);
                                    EmeraldComponent.m_NavMeshAgent.updateRotation = false;
                                    Vector3 diff = transform.position - EmeraldComponent.CurrentTarget.position;
                                    diff.y = 0.0f;
                                    EmeraldComponent.BackupDestination = EmeraldComponent.CurrentTarget.position + diff.normalized * HitBehind.distance;
                                    EmeraldComponent.m_NavMeshAgent.destination = EmeraldComponent.BackupDestination;
                                    EmeraldComponent.AIAnimator.ResetTrigger("Hit");
                                    EmeraldComponent.AIAnimator.ResetTrigger("Attack");
                                    EmeraldComponent.AIAnimator.SetBool("Turn Left", false);
                                    EmeraldComponent.AIAnimator.SetBool("Turn Right", false);
                                    EmeraldComponent.AIAnimator.SetBool("Walk Backwards", true);
                                    BackupDistance = EmeraldComponent.BackupDistance;
                                    EmeraldComponent.IsBackingUp = true;
                                }
                            }
                            else
                            {
                                if (EmeraldComponent.AnimatorType == EmeraldAISystem.AnimatorTypeState.NavMeshDriven)
                                {
                                    EmeraldComponent.m_NavMeshAgent.speed = EmeraldComponent.WalkBackwardsSpeed;
                                }
                                EmeraldComponent.CurrentBlockingState = EmeraldAISystem.BlockingState.NotBlocking;
                                EmeraldComponent.AIAnimator.SetBool("Blocking", false);
                                EmeraldComponent.m_NavMeshAgent.updateRotation = false;
                                Vector3 diff = transform.position - EmeraldComponent.CurrentTarget.position;
                                diff.y = 0.0f;
                                EmeraldComponent.BackupDestination = EmeraldComponent.CurrentTarget.position + diff.normalized * EmeraldComponent.BackupDistance;
                                EmeraldComponent.m_NavMeshAgent.destination = EmeraldComponent.BackupDestination;
                                EmeraldComponent.AIAnimator.ResetTrigger("Hit");
                                EmeraldComponent.AIAnimator.ResetTrigger("Attack");
                                EmeraldComponent.AIAnimator.SetBool("Turn Left", false);
                                EmeraldComponent.AIAnimator.SetBool("Turn Right", false);
                                EmeraldComponent.AIAnimator.SetBool("Walk Backwards", true);
                                BackupDistance = EmeraldComponent.BackupDistance;
                                EmeraldComponent.IsBackingUp = true;
                            }
                        }
                    }
                }
            }
            else
            {
                //If our target dies or is lost, search for another target.
                EmeraldComponent.EmeraldDetectionComponent.SearchForTarget();
            }
        }
    }
}
