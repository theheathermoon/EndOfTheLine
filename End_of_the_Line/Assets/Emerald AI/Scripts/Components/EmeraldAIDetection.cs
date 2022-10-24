using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace EmeraldAI.Utility
{
    public class EmeraldAIDetection : MonoBehaviour
    {
        [HideInInspector] public bool SearchingForTarget;
        [HideInInspector] public bool SearchForRandomTarget = false;
        [HideInInspector] public Vector3 TargetDirection;
        [HideInInspector] public GameObject CurrentObstruction;
        [HideInInspector] public Color DebugLineColor = Color.white;
        public enum PlayerDetectionRef { Detected, NotDetected };
        [HideInInspector] public PlayerDetectionRef PlayerDetection = PlayerDetectionRef.NotDetected;
        [HideInInspector] public float PlayerDetectionCooldown;
        [HideInInspector] public float DetectionTimer;
        [HideInInspector] public Collider CurrentTargetCollider;

        EmeraldAISystem EmeraldComponent;
        bool AvoidanceTrigger;
        float AvoidanceTimer;       
        float AvoidanceSeconds = 1.25f;
        bool m_PlayerDetected = false;
        bool m_FirstTimeDetectingPlayer;
        bool LineOfSightTargetSet;        

        void Start()
        {
            EmeraldComponent = GetComponent<EmeraldAISystem>();
        }

        void Update()
        {
            //Update our OverlapShere function based on the DetectionFrequency
            if (EmeraldComponent.TargetDetectionActive)
            {
                DetectionTimer += Time.deltaTime;

                if (DetectionTimer >= EmeraldComponent.DetectionFrequency)
                {
                    UpdateAIDetection();
                    DetectionTimer = 0;
                }
            }

            if (EmeraldComponent.EnableDebugging == EmeraldAISystem.YesOrNo.Yes && EmeraldComponent.DrawRaycastsEnabled == EmeraldAISystem.YesOrNo.Yes)
            {
                if (EmeraldComponent.CurrentTarget != null)
                {
                    if (EmeraldComponent.UseHeadLookRef == EmeraldAISystem.YesOrNo.No || EmeraldComponent.EmeraldLookAtComponent.ForceUnityIK || EmeraldComponent.IKType == EmeraldAISystem.IKTypes.UnityIK)
                        Debug.DrawRay(new Vector3(EmeraldComponent.HeadTransform.position.x, EmeraldComponent.HeadTransform.position.y, EmeraldComponent.HeadTransform.position.z), TargetDirection, DebugLineColor);
                }
                else if (EmeraldComponent.LookAtTarget != null)
                {
                    Vector3 LookAtTargetDir = (EmeraldComponent.LookAtTarget.position + (EmeraldComponent.LookAtTarget.up * EmeraldComponent.CurrentPositionModifier)) - EmeraldComponent.HeadTransform.position;
                    Debug.DrawRay(new Vector3(EmeraldComponent.HeadTransform.position.x, EmeraldComponent.HeadTransform.position.y, EmeraldComponent.HeadTransform.position.z), LookAtTargetDir, DebugLineColor);
                }
            }
        }

        void FixedUpdate()
        {
            if (EmeraldComponent.UseAIAvoidance == EmeraldAISystem.YesOrNo.Yes && EmeraldComponent.m_NavMeshAgent.enabled && EmeraldComponent.CombatStateRef == EmeraldAISystem.CombatState.NotActive)
            {
                AIAvoidance();
            }

            if (EmeraldComponent.DetectionTypeRef == EmeraldAISystem.DetectionType.LineOfSight && EmeraldComponent.OptimizedStateRef == EmeraldAISystem.OptimizedState.Inactive && EmeraldComponent.CombatStateRef == EmeraldAISystem.CombatState.NotActive)
            {
                if (EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Passive || EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Pet)
                    return;

                LineOfSightDetection();
            }
        }

        /// <summary>
        /// Check that each LineOfSightTarget is within the AI's DetectionRadius. If not, remove it from the list.
        /// </summary>
        void LineOfSightTargetsDistanceCheck ()
        {
            for (int i = 0; i < EmeraldComponent.LineOfSightTargets.Count; i++)
            {
                float distance = Vector3.Distance(EmeraldComponent.LineOfSightTargets[i].transform.position, transform.position);

                //If the distance of the detected target is greater than the DetectionRadius, remove it from the LineOfSightTargets list.
                if (distance > EmeraldComponent.DetectionRadius)
                    EmeraldComponent.LineOfSightTargets.Remove(EmeraldComponent.LineOfSightTargets[i]);
            }
        }

        //Assigns a target or follower based on the passed parameter and an AI's settings
        public void DetectTarget(GameObject C)
        {
            if (EmeraldComponent.UseHeadLookRef == EmeraldAISystem.YesOrNo.Yes && C.CompareTag(EmeraldComponent.PlayerTag) && 
                EmeraldComponent.CombatStateRef == EmeraldAISystem.CombatState.NotActive && EmeraldComponent.PlayerFaction[0].RelationTypeRef != EmeraldAISystem.PlayerFactionClass.RelationType.Enemy)
            {
                EmeraldComponent.LookAtTarget = C.transform;
            }

            //Tirgger Detection
            if (EmeraldComponent.DetectionTypeRef == EmeraldAISystem.DetectionType.Trigger && !SearchingForTarget && !EmeraldAISystem.IgnoredTargetsList.Contains(C.transform))
            {
                if (C.CompareTag(EmeraldComponent.PlayerTag))
                {
                    EmeraldComponent.TargetTypeRef = EmeraldAISystem.TargetType.Player;
                    EmeraldComponent.CurrentDetectionRef = EmeraldAISystem.CurrentDetection.Alert;

                    if (EmeraldComponent.PlayerFaction[0].RelationTypeRef == EmeraldAISystem.PlayerFactionClass.RelationType.Enemy && EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Companion
                        && EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Passive && EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Pet)
                    {
                        //Pick our target depending on the AI's options
                        if (EmeraldComponent.PickTargetMethodRef == EmeraldAISystem.PickTargetMethod.FirstDetected)
                        {
                            SetDetectedTarget(C.transform);
                        }
                        else if (EmeraldComponent.PickTargetMethodRef == EmeraldAISystem.PickTargetMethod.Closest)
                        {
                            SearchForTarget();
                        }
                        else if (EmeraldComponent.PickTargetMethodRef == EmeraldAISystem.PickTargetMethod.Random)
                        {
                            if (EmeraldComponent.FirstTimeInCombat)
                            {
                                SearchingForTarget = true;
                                EmeraldComponent.EmeraldEventsManagerComponent.Invoke("SearchForRandomTarget", 0.5f);
                            }
                            else
                            {
                                EmeraldComponent.EmeraldEventsManagerComponent.SearchForRandomTarget();
                            }
                        }
                    }
                    else if (EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Companion && !C.CompareTag(EmeraldComponent.PlayerTag))
                    {                        
                        EmeraldComponent.CompanionTargetRef = C.transform;
                    }
                    else if (EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Passive)
                    {
                        EmeraldComponent.PassiveTargetRef = C.transform;
                    }
                }
                else if (C.CompareTag(EmeraldComponent.EmeraldTag))
                {
                    GetTargetFaction(C.transform);

                    if (EmeraldComponent.AIFactionsList.Contains(EmeraldComponent.ReceivedFaction) && EmeraldComponent.FactionRelations[EmeraldComponent.AIFactionsList.IndexOf(EmeraldComponent.ReceivedFaction)] == 0)
                    {
                        EmeraldComponent.CurrentDetectionRef = EmeraldAISystem.CurrentDetection.Alert;

                        if (EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Companion && EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Passive && EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Pet)
                        {
                            //Pick our target depending on the AI's options
                            if (EmeraldComponent.PickTargetMethodRef == EmeraldAISystem.PickTargetMethod.FirstDetected)
                            {
                                SetDetectedTarget(C.transform);
                            }
                            else if (EmeraldComponent.PickTargetMethodRef == EmeraldAISystem.PickTargetMethod.Closest)
                            {
                                SearchForTarget();
                            }
                            else if (EmeraldComponent.PickTargetMethodRef == EmeraldAISystem.PickTargetMethod.Random)
                            {
                                if (EmeraldComponent.FirstTimeInCombat)
                                {
                                    SearchingForTarget = true;
                                    EmeraldComponent.EmeraldEventsManagerComponent.Invoke("SearchForRandomTarget", 0.5f);
                                }
                                else
                                {
                                    EmeraldComponent.EmeraldEventsManagerComponent.SearchForRandomTarget();
                                }
                            }
                        }
                        else if (EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Companion)
                        {
                            DetectTargetType(C.transform);
                            EmeraldComponent.CompanionTargetRef = C.transform;
                        }
                        else if (EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Passive)
                        {
                            EmeraldComponent.PassiveTargetRef = C.transform;
                        }
                    }
                }
                else if (C.tag == EmeraldComponent.NonAITag && EmeraldComponent.UseNonAITagRef == EmeraldAISystem.YesOrNo.Yes)
                {
                    if (EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Companion && EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Passive && EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Pet)
                    {
                        //Pick our target depending on the AI's options
                        if (EmeraldComponent.PickTargetMethodRef == EmeraldAISystem.PickTargetMethod.FirstDetected)
                        {
                            SetDetectedTarget(C.transform);
                        }
                        else if (EmeraldComponent.PickTargetMethodRef == EmeraldAISystem.PickTargetMethod.Closest)
                        {
                            SearchForTarget();
                        }
                        else if (EmeraldComponent.PickTargetMethodRef == EmeraldAISystem.PickTargetMethod.Random)
                        {
                            if (EmeraldComponent.FirstTimeInCombat)
                            {
                                SearchingForTarget = true;
                                EmeraldComponent.EmeraldEventsManagerComponent.Invoke("SearchForRandomTarget", 0.5f);
                            }
                            else
                            {
                                EmeraldComponent.EmeraldEventsManagerComponent.SearchForRandomTarget();
                            }
                        }
                    }
                    else if (EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Companion)
                    {
                        EmeraldComponent.CompanionTargetRef = C.transform;
                    }
                    else if (EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Passive)
                    {
                        EmeraldComponent.PassiveTargetRef = C.transform;
                    }
                }
            }
            //Line of Sight Detection
            else if (EmeraldComponent.DetectionTypeRef == EmeraldAISystem.DetectionType.LineOfSight && !SearchingForTarget)
            {
                if (C.CompareTag(EmeraldComponent.PlayerTag) && !EmeraldComponent.LineOfSightTargets.Contains(C.GetComponent<Collider>()) && EmeraldComponent.PlayerFaction[0].RelationTypeRef == EmeraldAISystem.PlayerFactionClass.RelationType.Enemy)
                {
                    EmeraldComponent.TargetTypeRef = EmeraldAISystem.TargetType.Player;
                    EmeraldComponent.CurrentDetectionRef = EmeraldAISystem.CurrentDetection.Alert;
                    EmeraldComponent.LineOfSightTargets.Add(C.GetComponent<Collider>());
                }
                else if (C.CompareTag(EmeraldComponent.EmeraldTag))
                {
                    DetectTargetType(C.transform);

                    if (EmeraldComponent.AIFactionsList.Contains(EmeraldComponent.ReceivedFaction) && EmeraldComponent.FactionRelations[EmeraldComponent.AIFactionsList.IndexOf(EmeraldComponent.ReceivedFaction)] == 0 
                        && !EmeraldComponent.LineOfSightTargets.Contains(C.GetComponent<Collider>()))
                    {
                        EmeraldComponent.CurrentDetectionRef = EmeraldAISystem.CurrentDetection.Alert;
                        EmeraldComponent.LineOfSightTargets.Add(C.GetComponent<Collider>());
                    }
                }
                else if (C.tag == EmeraldComponent.NonAITag && EmeraldComponent.UseNonAITagRef == EmeraldAISystem.YesOrNo.Yes && !EmeraldComponent.LineOfSightTargets.Contains(C.GetComponent<Collider>()))
                {
                    if (C.GetComponent<EmeraldAISystem>() == null)
                    {
                        EmeraldComponent.TargetTypeRef = EmeraldAISystem.TargetType.NonAITarget;
                    }
                    EmeraldComponent.CurrentDetectionRef = EmeraldAISystem.CurrentDetection.Alert;
                    EmeraldComponent.LineOfSightTargets.Add(C.GetComponent<Collider>());
                }
            }
        }

        //Handles the avoiding of other AI by casting a raycast from the AI's head transform is. If a target of the appropriate layer is hit, 
        //alter the AI's destination for briefly to allow both AI to move past each other without colliding.
        void AIAvoidance()
        {
            RaycastHit HitForward;            
            if (EmeraldComponent.IsMoving && Physics.Raycast(EmeraldComponent.HeadTransform.position, 
                transform.forward, out HitForward, (EmeraldComponent.StoppingDistance*2+1), EmeraldComponent.AIAvoidanceLayerMask) && !AvoidanceTrigger)
            {
                if (HitForward.transform != transform)
                {
                    EmeraldComponent.TargetDestination = transform.position + HitForward.transform.right / Random.Range(-5,6) * (EmeraldComponent.StoppingDistance*2+1);
                    EmeraldComponent.m_NavMeshAgent.destination = EmeraldComponent.TargetDestination;
                    AvoidanceTrigger = true;
                    AvoidanceTimer = 0;
                }
            }
            else if (AvoidanceTrigger)
            {
                if (EmeraldComponent.CurrentMovementState == EmeraldAISystem.MovementState.Walk)
                {
                    AvoidanceSeconds = 1;
                }
                else
                {
                    AvoidanceSeconds = 0.75f;
                }

                AvoidanceTimer += Time.deltaTime;
                if (AvoidanceTimer > AvoidanceSeconds && EmeraldComponent.CurrentTarget == null)
                {
                    if (EmeraldComponent.WanderTypeRef == EmeraldAISystem.WanderType.Dynamic)
                    {
                        EmeraldComponent.m_NavMeshAgent.destination = EmeraldComponent.NewDestination;
                    }
                    else if (EmeraldComponent.WanderTypeRef == EmeraldAISystem.WanderType.Destination)
                    {
                        EmeraldComponent.m_NavMeshAgent.destination = EmeraldComponent.SingleDestination;
                    }
                    else if (EmeraldComponent.WanderTypeRef == EmeraldAISystem.WanderType.Stationary)
                    {
                        EmeraldComponent.m_NavMeshAgent.destination = EmeraldComponent.StartingDestination;
                    }
                    if (EmeraldComponent.WanderTypeRef == EmeraldAISystem.WanderType.Waypoints)
                    {
                        EmeraldComponent.m_NavMeshAgent.destination = EmeraldComponent.WaypointsList[EmeraldComponent.WaypointIndex];
                    }

                    AvoidanceTimer = 0;
                    AvoidanceTrigger = false;
                }
            }
        }

        //Updates the AI's UI state, if it's enabled.
        void UpdateAIUI ()
        {
            if (EmeraldComponent.CreateHealthBarsRef == EmeraldAISystem.YesOrNo.Yes || EmeraldComponent.DisplayAINameRef == EmeraldAISystem.YesOrNo.Yes)
            {
                Collider[] CurrentlyDetectedTargets = Physics.OverlapSphere(transform.position, EmeraldComponent.DetectionRadius, EmeraldComponent.UILayerMask);

                foreach (Collider C in CurrentlyDetectedTargets)
                {                   
                    if (C.CompareTag(EmeraldComponent.UITag))
                    {
                        EmeraldComponent.SetUI(true);
                        return;
                    }
                    else
                    {
                        EmeraldComponent.SetUI(false);
                    }
                }

                if (CurrentlyDetectedTargets.Length == 0)
                {
                    EmeraldComponent.SetUI(false);
                }
            }
        }

        /// <summary>
        /// Handles all of an AI's target detection by using a Physics.OverlapSphere (instead of a Trigger Collider). This allows users to specifiy which layers will be considered
        /// </summary>
        public void UpdateAIDetection ()
        {
            if (EmeraldComponent.LineOfSightTargets.Count > 0) LineOfSightTargetsDistanceCheck();

            //Do a separate OverlapSphere with only the UI Layer. If the detected object has the UITag, enable the UI system.
            UpdateAIUI();

            if (EmeraldComponent != null && EmeraldComponent.CurrentTarget == null && !EmeraldComponent.ReturningToStartInProgress || 
                EmeraldComponent != null && EmeraldComponent.DeathDelayActive && !EmeraldComponent.ReturningToStartInProgress)
            {
                Collider[] CurrentlyDetectedTargets = Physics.OverlapSphere(transform.position, EmeraldComponent.DetectionRadius, EmeraldComponent.DetectionLayerMask);
                m_PlayerDetected = false;

                foreach (Collider C in CurrentlyDetectedTargets)
                {
                    //Check for a companion target, if a detected object is found that is the proper tag, assign it as the active follower.
                    if (C.CompareTag(EmeraldComponent.FollowTag) && C.tag != "Untagged" && EmeraldComponent.CurrentFollowTarget == null)
                    {
                        if (EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Companion || EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Pet)
                        {
                            EmeraldComponent.CurrentFollowTarget = C.transform;
                            EmeraldComponent.CurrentMovementState = EmeraldAISystem.MovementState.Run;
                            EmeraldComponent.StartingMovementState = EmeraldAISystem.MovementState.Run;
                        }
                    }

                    if (EmeraldComponent.CurrentTarget == null && C.gameObject != this.gameObject)
                    {
                        if (C.CompareTag(EmeraldComponent.EmeraldTag) || C.CompareTag(EmeraldComponent.PlayerTag) && EmeraldComponent.PlayerFaction[0].RelationTypeRef == EmeraldAISystem.PlayerFactionClass.RelationType.Enemy || 
                            C.tag == EmeraldComponent.NonAITag && EmeraldComponent.UseNonAITagRef == EmeraldAISystem.YesOrNo.Yes)
                        {
                            if (EmeraldComponent.DetectionTypeRef == EmeraldAISystem.DetectionType.Trigger)
                            {
                                if (EmeraldComponent.DeathDelayActive)
                                {
                                    EmeraldComponent.DeathDelayActive = false;
                                    EmeraldComponent.DeathDelayTimer = 0;
                                }

                                DetectTarget(C.gameObject);
                            }
                            else if (EmeraldComponent.DetectionTypeRef == EmeraldAISystem.DetectionType.LineOfSight && EmeraldComponent.CurrentTarget == null)
                            {
                                DetectTarget(C.gameObject);
                            }
                        }
                    }
                    else if (C.CompareTag(EmeraldComponent.EmeraldTag) && EmeraldComponent.DeathDelayActive && EmeraldComponent.CurrentTarget != null)
                    {
                        SearchForTarget();
                    }

                    if (EmeraldComponent.CurrentTarget == null && C.gameObject != this.gameObject && !EmeraldComponent.DeathDelayActive)
                    {
                        if (EmeraldComponent.CombatStateRef == EmeraldAISystem.CombatState.NotActive && C.CompareTag(EmeraldComponent.PlayerTag))
                        {
                            m_PlayerDetected = true;
                        }

                        if (m_PlayerDetected && PlayerDetection == PlayerDetectionRef.NotDetected && PlayerDetectionCooldown > 5 || 
                            m_PlayerDetected && PlayerDetection == PlayerDetectionRef.NotDetected && !m_FirstTimeDetectingPlayer)
                        {
                            EmeraldComponent.OnPlayerDetectionEvent.Invoke();
                            PlayerDetection = PlayerDetectionRef.Detected;
                            m_FirstTimeDetectingPlayer = true;
                            PlayerDetectionCooldown = 0;
                        }

                        if (EmeraldComponent.UseHeadLookRef == EmeraldAISystem.YesOrNo.Yes && C.CompareTag(EmeraldComponent.PlayerTag) && EmeraldComponent.CombatStateRef == EmeraldAISystem.CombatState.NotActive && EmeraldComponent.LookAtTarget == null && EmeraldComponent.CurrentTarget == null)
                        {
                            DetectTargetType(C.transform);

                            //Only assign the player as the EmeraldComponent.LookAtTarget if they are not an enemy as this feature is intended for non-combat
                            if (EmeraldComponent.PlayerFaction[0].RelationTypeRef != EmeraldAISystem.PlayerFactionClass.RelationType.Enemy)
                                EmeraldComponent.LookAtTarget = C.transform;
                        }
                    }
                }

                if (!m_PlayerDetected)
                {
                    PlayerDetectionCooldown += EmeraldComponent.DetectionFrequency;
                    PlayerDetection = PlayerDetectionRef.NotDetected;
                }
            }

            if (CurrentTargetCollider == null && EmeraldComponent.CurrentTarget != null)
            {
                CurrentTargetCollider = EmeraldComponent.CurrentTarget.GetComponent<Collider>();
            }

            if (EmeraldComponent.CurrentTargetPositionModifierComp != null)
            {
                EmeraldComponent.CurrentPositionModifier = EmeraldComponent.CurrentTargetPositionModifierComp.PositionModifier;
            }
            else
            {
                EmeraldComponent.CurrentPositionModifier = 0;
            }
        }

        //Calculates our AI's line of sight mechanics.
        //For each target that is within the AI's trigger radius, and they are within the AI's
        //line of sight, set the first seen target as the CurrentTarget.
        public void LineOfSightDetection ()
        {
            if (EmeraldComponent.CurrentDetectionRef == EmeraldAISystem.CurrentDetection.Alert && EmeraldComponent.CurrentTarget == null && EmeraldComponent.CurrentHealth > 0)
            {
                foreach (Collider C in EmeraldComponent.LineOfSightTargets.ToArray())
                {
                    Vector3 direction = C.bounds.center - EmeraldComponent.HeadTransform.position;
                    float angle = Vector3.Angle(new Vector3(direction.x, 0, direction.z), transform.forward);

                    if (EmeraldComponent.EnableDebugging == EmeraldAISystem.YesOrNo.Yes && EmeraldComponent.DrawRaycastsEnabled == EmeraldAISystem.YesOrNo.Yes)
                    {
                        Debug.DrawRay(EmeraldComponent.HeadTransform.position, direction, new Color(1, 0.549f, 0));
                    }

                    if (angle < EmeraldComponent.fieldOfViewAngle * 0.5f)
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(EmeraldComponent.HeadTransform.position, direction, out hit, EmeraldComponent.DetectionRadius, ~EmeraldComponent.ObstructionDetectionLayerMask))
                        {
                            if (hit.collider.CompareTag(EmeraldComponent.EmeraldTag) || hit.collider.transform.root.CompareTag(EmeraldComponent.EmeraldTag) || hit.collider.CompareTag(EmeraldComponent.PlayerTag) && EmeraldComponent.PlayerFaction[0].RelationTypeRef == EmeraldAISystem.PlayerFactionClass.RelationType.Enemy ||
                                hit.collider.tag == EmeraldComponent.NonAITag && EmeraldComponent.UseNonAITagRef == EmeraldAISystem.YesOrNo.Yes)
                            {
                                if (hit.collider != null && EmeraldComponent.LineOfSightTargets.Contains(hit.collider))
                                {
                                    if (EmeraldComponent.AIFactionsList.Contains(EmeraldComponent.ReceivedFaction) && EmeraldComponent.FactionRelations[EmeraldComponent.AIFactionsList.IndexOf(EmeraldComponent.ReceivedFaction)] == 0
                                    || hit.collider.CompareTag(EmeraldComponent.PlayerTag) && EmeraldComponent.PlayerFaction[0].RelationTypeRef == EmeraldAISystem.PlayerFactionClass.RelationType.Enemy
                                    || hit.collider.tag == EmeraldComponent.NonAITag && EmeraldComponent.UseNonAITagRef == EmeraldAISystem.YesOrNo.Yes)
                                    {
                                        SetLineOfSightTarget(hit.collider.transform);
                                        break;
                                    }
                                }
                                else if (hit.collider != null && EmeraldComponent.LineOfSightTargets.Contains(hit.collider.transform.root.GetComponent<Collider>()))
                                {
                                    if (EmeraldComponent.AIFactionsList.Contains(EmeraldComponent.ReceivedFaction) && EmeraldComponent.FactionRelations[EmeraldComponent.AIFactionsList.IndexOf(EmeraldComponent.ReceivedFaction)] == 0
                                    || hit.collider.transform.root.CompareTag(EmeraldComponent.PlayerTag) && EmeraldComponent.PlayerFaction[0].RelationTypeRef == EmeraldAISystem.PlayerFactionClass.RelationType.Enemy
                                    || hit.collider.transform.root.tag == EmeraldComponent.NonAITag && EmeraldComponent.UseNonAITagRef == EmeraldAISystem.YesOrNo.Yes)
                                    {
                                        SetLineOfSightTarget(hit.collider.transform.root);
                                        break;
                                    }
                                }
                            }                           
                        }
                    }
                }
            }
        }

        public void SetLineOfSightTarget (Transform LineOfSightTarget)
        {
            EmeraldComponent.LineOfSightRef = LineOfSightTarget;
            //Pick our target depending on the AI's settings
            if (EmeraldComponent.PickTargetMethodRef == EmeraldAISystem.PickTargetMethod.FirstDetected || EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Cautious)
            {
                if (EmeraldComponent.DeathDelayActive)
                {
                    EmeraldComponent.DeathDelayActive = false;
                    EmeraldComponent.DeathDelayTimer = 0;
                }

                SetDetectedTarget(EmeraldComponent.LineOfSightRef);
            }
            else if (EmeraldComponent.PickTargetMethodRef == EmeraldAISystem.PickTargetMethod.Closest)
            {
                SearchForTarget();
            }
            else if (EmeraldComponent.PickTargetMethodRef == EmeraldAISystem.PickTargetMethod.Random)
            {
                EmeraldComponent.EmeraldEventsManagerComponent.SearchForRandomTarget();
            }

            if (EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Companion)
            {
                EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.StoppingDistance;
            }
        }

        public void CheckForObstructions ()
        {          
            if (EmeraldComponent.CurrentTarget != null && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged)
            {
                if (EmeraldComponent.TargetTypeRef == EmeraldAISystem.TargetType.AI && EmeraldComponent.TargetEmerald != null)
                {
                    TargetDirection = EmeraldComponent.TargetEmerald.HitPointTransform.position - EmeraldComponent.HeadTransform.position;
                }
                else
                {
                    TargetDirection = (EmeraldComponent.CurrentTarget.position + (EmeraldComponent.CurrentTarget.up * EmeraldComponent.CurrentPositionModifier)) - EmeraldComponent.HeadTransform.position;
                }

                float angle = Vector3.Angle(new Vector3(TargetDirection.x, 0, TargetDirection.z), new Vector3(transform.forward.x, 0, transform.forward.z));
                RaycastHit hit;

                //Check for obstructions and incrementally lower our AI's stopping distance until one is found. If none are found when the distance has reached 5 or below, search for a new target to see if there is a better option
                if (Physics.Raycast(EmeraldComponent.HeadTransform.position, (TargetDirection), out hit, EmeraldComponent.DistanceFromTarget + 2, ~EmeraldComponent.ObstructionDetectionLayerMask))
                {
                    if (EmeraldComponent.CurrentTarget != null && angle > 45 && EmeraldComponent.m_NavMeshAgent.stoppingDistance > 5 && hit.collider.gameObject != this.gameObject && hit.collider.gameObject != EmeraldComponent.CurrentTarget.gameObject
                        || EmeraldComponent.CurrentTarget != null && hit.collider.gameObject != EmeraldComponent.CurrentTarget.gameObject && hit.collider.gameObject != this.gameObject && EmeraldComponent.m_NavMeshAgent.stoppingDistance > 5)
                    {
                        if (!hit.collider.transform.IsChildOf(EmeraldComponent.CurrentTarget) && !hit.collider.transform.IsChildOf(this.transform) && hit.collider.transform != EmeraldComponent.CurrentTarget)
                        {
                            DebugLineColor = Color.red;
                            EmeraldComponent.TargetObstructed = true;

                            if (hit.collider.gameObject != CurrentObstruction)
                            {
                                CurrentObstruction = hit.collider.gameObject;
                                if (EmeraldComponent.EnableDebugging == EmeraldAISystem.YesOrNo.Yes &&
                                    EmeraldComponent.DebugLogObstructionsEnabled == EmeraldAISystem.YesOrNo.Yes && !EmeraldComponent.DeathDelayActive)
                                {
                                    Debug.Log("<b>" + "<color=green>" + gameObject.name + "'s Current Obstruction: " + "</color>" + "<color=red>" + hit.collider.gameObject.name + "</color>" + "</b>");
                                }
                            }

                            if (EmeraldComponent.m_NavMeshAgent.stoppingDistance > EmeraldComponent.StoppingDistance && !EmeraldComponent.IsBackingUp && !EmeraldComponent.IsTurning && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Ranged)
                            {
                                if (!EmeraldComponent.IsAttacking && hit.collider.tag != EmeraldComponent.EmeraldTag && hit.collider.tag != EmeraldComponent.PlayerTag)
                                {
                                    if (EmeraldComponent.TargetObstructedActionRef == EmeraldAISystem.TargetObstructedAction.MoveCloserImmediately)
                                    {
                                        UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
                                        EmeraldComponent.m_NavMeshAgent.CalculatePath(EmeraldComponent.CurrentTarget.position, path);

                                        if (path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
                                        {
                                            EmeraldComponent.m_NavMeshAgent.stoppingDistance -= 5;
                                        }
                                        else if (path.status == UnityEngine.AI.NavMeshPathStatus.PathPartial && EmeraldComponent.TargetObstructed)
                                        {
                                            EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.StoppingDistance;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (EmeraldComponent.CurrentTarget != null && hit.collider.gameObject == EmeraldComponent.CurrentTarget.gameObject && !EmeraldComponent.IsTurning ||
                        EmeraldComponent.CurrentTarget != null && hit.collider.transform.IsChildOf(EmeraldComponent.CurrentTarget) && !EmeraldComponent.IsTurning)
                    {
                        UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();
                        EmeraldComponent.m_NavMeshAgent.CalculatePath(EmeraldComponent.CurrentTarget.position, path);

                        DebugLineColor = Color.green;
                        EmeraldComponent.TargetObstructed = false;
                        EmeraldComponent.CurrentMovementState = EmeraldAISystem.MovementState.Run;
                        if (!EmeraldComponent.IsBackingUp && path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
                        {
                            EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.AttackDistance;
                        }
                        EmeraldComponent.ObstructionTimer = 0;
                    }
                }
                else
                {
                    DebugLineColor = Color.red;
                    EmeraldComponent.TargetObstructed = true;
                }
            }
            else if (EmeraldComponent.CurrentTarget != null && EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee)
            {
                if (EmeraldComponent.TargetTypeRef == EmeraldAISystem.TargetType.AI && EmeraldComponent.TargetEmerald != null)
                {
                    TargetDirection = EmeraldComponent.TargetEmerald.HitPointTransform.position - EmeraldComponent.HeadTransform.position;
                }
                else
                {
                    TargetDirection = (EmeraldComponent.CurrentTarget.position + (EmeraldComponent.CurrentTarget.up * EmeraldComponent.CurrentPositionModifier)) - EmeraldComponent.HeadTransform.position;
                }

                RaycastHit hit;

                //Check for obstructions and incrementally lower our AI's stopping distance until one is found. If none are found when the distance has reached 5 or below, search for a new target to see if there is a better option
                if (Physics.Raycast(EmeraldComponent.HeadTransform.position, (TargetDirection), out hit, EmeraldComponent.DistanceFromTarget + 2, ~EmeraldComponent.ObstructionDetectionLayerMask))
                {
                    if (!hit.collider.transform.IsChildOf(EmeraldComponent.CurrentTarget) && !hit.collider.transform.IsChildOf(this.transform) && hit.collider.transform != EmeraldComponent.CurrentTarget) 
                    {
                        DebugLineColor = Color.red;
                        EmeraldComponent.TargetObstructed = true;

                        if (hit.collider.gameObject != CurrentObstruction)
                        {
                            CurrentObstruction = hit.collider.gameObject;
                            if (EmeraldComponent.EnableDebugging == EmeraldAISystem.YesOrNo.Yes &&
                                EmeraldComponent.DebugLogObstructionsEnabled == EmeraldAISystem.YesOrNo.Yes && !EmeraldComponent.DeathDelayActive)
                            {
                                Debug.Log("<b>" + "<color=green>" + gameObject.name + "'s Current Obstruction: " + "</color>" + "<color=red>" + hit.collider.gameObject.name + "</color>" + "</b>");
                            }
                        }
                    }
                    else
                    {
                        DebugLineColor = Color.green;
                        EmeraldComponent.TargetObstructed = false;
                    }
                }
                else
                {
                    DebugLineColor = Color.green;
                    EmeraldComponent.TargetObstructed = false;
                }
            }
        }

        /// <summary>
        /// Search for any new potential targets within the AI's detection radius.
        /// </summary>
        public void SearchForLineOfSightTarget ()
        {
            Collider[] Col = Physics.OverlapSphere(transform.position, EmeraldComponent.DetectionRadius, EmeraldComponent.DetectionLayerMask);
            EmeraldComponent.CollidersInArea = Col;

            foreach (Collider C in EmeraldComponent.CollidersInArea)
            {
                if (C.gameObject != this.gameObject && !EmeraldComponent.LineOfSightTargets.Contains(C.GetComponent<Collider>()) && !EmeraldAISystem.IgnoredTargetsList.Contains(C.transform))
                {
                    if (C.gameObject.GetComponent<EmeraldAISystem>() != null)
                    {
                        EmeraldAISystem EmeraldRef = C.gameObject.GetComponent<EmeraldAISystem>();
                        if (EmeraldComponent.AIFactionsList.Contains(EmeraldRef.CurrentFaction) &&
                            EmeraldComponent.FactionRelations[EmeraldComponent.AIFactionsList.IndexOf(EmeraldRef.CurrentFaction)] == 0 &&
                            EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Companion)
                        {
                            EmeraldComponent.LineOfSightTargets.Add(C.GetComponent<Collider>());
                        }
                        else if (EmeraldComponent.AIFactionsList.Contains(EmeraldRef.CurrentFaction) &&
                            EmeraldComponent.FactionRelations[EmeraldComponent.AIFactionsList.IndexOf(EmeraldRef.CurrentFaction)] == 0 &&
                            EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Companion && EmeraldRef.CombatStateRef == EmeraldAISystem.CombatState.Active)
                        {
                            EmeraldComponent.LineOfSightTargets.Add(C.GetComponent<Collider>());
                        }
                    }
                    else if (C.gameObject.CompareTag(EmeraldComponent.PlayerTag) && EmeraldComponent.PlayerFaction[0].RelationTypeRef == EmeraldAISystem.PlayerFactionClass.RelationType.Enemy)
                    {
                        if (EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Companion || EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Companion && C.gameObject.transform != EmeraldComponent.CurrentFollowTarget)
                        {
                            EmeraldComponent.LineOfSightTargets.Add(C.GetComponent<Collider>());
                        }
                    }
                    else if (C.gameObject.tag == EmeraldComponent.NonAITag && EmeraldComponent.UseNonAITagRef == EmeraldAISystem.YesOrNo.Yes)
                    {
                        EmeraldComponent.LineOfSightTargets.Add(C.GetComponent<Collider>());
                    }
                }

                if (!EmeraldComponent.LineOfSightTargets.Contains(C.GetComponent<Collider>()))
                {
                    EmeraldComponent.LineOfSightTargets.Remove(C.GetComponent<Collider>());
                }
            }
        }

        //Find colliders within range using a Physics.OverlapSphere. Mask the Physics.OverlapSphere to the user set layers. 
        //This will allow the Physics.OverlapSphere to only get relevent colliders.
        //Once found, use Emerald's custom tag system to find matches for potential targets. Once found, apply them to a list for potential targets.
        //Finally, search through each target in the list and set the nearest one as our current target.
        public void SearchForTarget ()
        {
            Collider[] Col = Physics.OverlapSphere(transform.position, EmeraldComponent.DetectionRadius, EmeraldComponent.DetectionLayerMask);
            EmeraldComponent.CollidersInArea = Col;
            EmeraldComponent.potentialTargets.Clear();

            EmeraldComponent.EmeraldBehaviorsComponent.SearchDelayActive = false;

            foreach (Collider C in EmeraldComponent.CollidersInArea)
            {
                if (C.gameObject != this.gameObject && !EmeraldComponent.potentialTargets.Contains(C.gameObject) && !EmeraldAISystem.IgnoredTargetsList.Contains(C.transform))
                {                  
                    if (C.gameObject.GetComponent<EmeraldAISystem>() != null)
                    {
                        EmeraldAISystem EmeraldRef = C.gameObject.GetComponent<EmeraldAISystem>();
                        if (EmeraldComponent.AIFactionsList.Contains(EmeraldRef.CurrentFaction) && 
                            EmeraldComponent.FactionRelations[EmeraldComponent.AIFactionsList.IndexOf(EmeraldRef.CurrentFaction)] == 0 && 
                            EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Companion)
                        {
                            EmeraldComponent.potentialTargets.Add(C.gameObject);
                        }
                        else if (EmeraldComponent.AIFactionsList.Contains(EmeraldRef.CurrentFaction) && 
                            EmeraldComponent.FactionRelations[EmeraldComponent.AIFactionsList.IndexOf(EmeraldRef.CurrentFaction)] == 0 && 
                            EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Companion && EmeraldRef.CombatStateRef == EmeraldAISystem.CombatState.Active)
                        {
                            EmeraldComponent.potentialTargets.Add(C.gameObject);
                        }
                        else if (EmeraldComponent.AIFactionsList.Contains(EmeraldRef.CurrentFaction) && 
                            EmeraldComponent.FactionRelations[EmeraldComponent.AIFactionsList.IndexOf(EmeraldRef.CurrentFaction)] == 0 && 
                            EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Companion && EmeraldRef.CombatStateRef == EmeraldAISystem.CombatState.NotActive)
                        {
                            EmeraldComponent.CompanionTargetRef = C.transform;
                            EmeraldComponent.TargetTypeRef = EmeraldAISystem.TargetType.AI;
                            EmeraldComponent.TargetEmerald = C.gameObject.GetComponent<EmeraldAISystem>();
                        }
                    }
                    else if (C.gameObject.CompareTag(EmeraldComponent.PlayerTag) && EmeraldComponent.PlayerFaction[0].RelationTypeRef == EmeraldAISystem.PlayerFactionClass.RelationType.Enemy)
                    {
                        if (EmeraldComponent.BehaviorRef != EmeraldAISystem.CurrentBehavior.Companion || EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Companion && C.gameObject.transform != EmeraldComponent.CurrentFollowTarget)
                        {
                            EmeraldComponent.potentialTargets.Add(C.gameObject);
                        }
                    }
                    else if (C.gameObject.tag == EmeraldComponent.NonAITag && EmeraldComponent.UseNonAITagRef == EmeraldAISystem.YesOrNo.Yes)
                    {
                        EmeraldComponent.potentialTargets.Add(C.gameObject);
                    }
                }
            }

            //Search for a random target
            if (SearchForRandomTarget && EmeraldComponent.potentialTargets.Count > 0 && EmeraldComponent.m_NavMeshAgent.enabled)
            {
                //Find a random target within potentialTargets
                Transform RandomTarget = EmeraldComponent.potentialTargets[Random.Range(0, EmeraldComponent.potentialTargets.Count)].transform;

                if (RandomTarget == EmeraldComponent.CurrentTarget)
                    return;

                SetDetectedTarget(RandomTarget); //Sets and detects the RandomTarget as the AI's current target
                EmeraldComponent.m_NavMeshAgent.destination = RandomTarget.position; //Sets the RandomTarget as the AI's destination

                //Set SearchForRandomTarget to false and stop the function as a target has been found.
                SearchForRandomTarget = false;
                return;
            }

            //No targets were found within the AI's trigger radius. Set AI back to its default state.
            if (EmeraldComponent.potentialTargets.Count == 0)
            {
                EmeraldComponent.IsBackingUp = false;
                EmeraldComponent.AIAnimator.SetBool("Walk Backwards", false);
                if (EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Companion)
                {
                    EmeraldComponent.CompanionTargetRef = null;                 

                    if (EmeraldComponent.CurrentFollowTarget != null)
                    {
                        EmeraldComponent.m_NavMeshAgent.ResetPath();                    
                    }
                }

                EmeraldComponent.DeathDelay = Random.Range(EmeraldComponent.DeathDelayMin, EmeraldComponent.DeathDelayMax + 1);
                if (EmeraldComponent.m_NavMeshAgent.enabled)
                    EmeraldComponent.m_NavMeshAgent.SetDestination(this.transform.position);
                if (!EmeraldComponent.ReturningToStartInProgress)
                    EmeraldComponent.DeathDelayActive = true;
            }

            Transform ClosestTarget = null;
            var OrderedTargets = EmeraldComponent.potentialTargets.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).ToList();
            if (OrderedTargets.Count > 0)
            {
                ClosestTarget = OrderedTargets[0].transform;
            }

            if (ClosestTarget != null && ClosestTarget != EmeraldComponent.CurrentTarget)
            {
                SetDetectedTarget(ClosestTarget);
            }
        }

        /// <summary>
        /// Internal use only - Detects the passed target's Target Type and assigns it as the AI's current target. For assigning targets directly, use EmeraldAIEventsManager.SetCombatTarget(Transform Target)
        /// </summary>
        public void SetDetectedTarget (Transform DetectedTarget)
        {
            //Cancel the AI's current attack state if it's active when assigning a new target.
            if (EmeraldComponent.AIAnimator.GetBool("Attack"))
                EmeraldComponent.EmeraldEventsManagerComponent.CancelAttackAnimation();

            EmeraldComponent.AIAnimator.ResetTrigger("Hit");
            EmeraldComponent.AIAnimator.SetBool("Turn Left", false);
            EmeraldComponent.AIAnimator.SetBool("Turn Right", false);

            //If our combat state is not active, activate it.
            if (EmeraldComponent.CombatStateRef == EmeraldAISystem.CombatState.NotActive)
            {
                EmeraldComponent.EmeraldBehaviorsComponent.ActivateCombatState();
            }

            if (EmeraldComponent.ConfidenceRef != EmeraldAISystem.ConfidenceType.Coward)
            {
                EmeraldComponent.m_NavMeshAgent.stoppingDistance = EmeraldComponent.AttackDistance;
                EmeraldComponent.OnStartCombatEvent.Invoke();
            }

            EmeraldComponent.potentialTargets.Clear();

            //Once a target has been found, reduce the Detection Radius back to the defaul value.
            EmeraldComponent.DetectionRadius = EmeraldComponent.StartingDetectionRadius;
            EmeraldComponent.MaxChaseDistance = EmeraldComponent.StartingChaseDistance;
            EmeraldComponent.fieldOfViewAngle = EmeraldComponent.fieldOfViewAngleRef;

            if (EmeraldComponent.BehaviorRef == EmeraldAISystem.CurrentBehavior.Aggressive)
            {
                if (EmeraldComponent.WeaponTypeRef == EmeraldAISystem.WeaponType.Melee && !EmeraldComponent.IsDead)
                {
                    EmeraldComponent.m_NavMeshAgent.destination = DetectedTarget.position;
                }
            }

            if (EmeraldComponent.EmeraldLookAtComponent.BodyWeight == 0 && EmeraldComponent.IKType == EmeraldAISystem.IKTypes.EmeraldIK && EmeraldComponent.UseEquipAnimation == EmeraldAISystem.YesOrNo.No)
                EmeraldComponent.EmeraldLookAtComponent.FadeInBodyIK();

            //Invoke our OnStartCombatEvent that happens when the AI starts fighting its first target for the current battle.
            if (EmeraldComponent.FirstTimeInCombat)
                EmeraldComponent.OnStartCombatEvent.Invoke();

            EmeraldComponent.FirstTimeInCombat = false;
            EmeraldComponent.IsTurning = false;
            SearchingForTarget = false;
            SearchForRandomTarget = false;

            EmeraldComponent.DeathDelayActive = false;
            EmeraldComponent.DeathDelayTimer = 0;

            EmeraldComponent.EmeraldLookAtComponent.SetPreviousLookAtInfo();
            DetectTargetType(DetectedTarget);
            EmeraldComponent.CurrentTarget = DetectedTarget;

            //Invokes the OnDetectTargetEvent when an AI successfully detects a target.
            EmeraldComponent.OnDetectTargetEvent.Invoke();

            if (EmeraldComponent.EnableDebugging == EmeraldAISystem.YesOrNo.Yes && EmeraldComponent.DebugLogTargetsEnabled == EmeraldAISystem.YesOrNo.Yes && !EmeraldComponent.DeathDelayActive)
            {
                if (DetectedTarget != null)
                {
                    Debug.Log("<b>" + "<color=green>" + gameObject.name + "'s Current Target: " + "</color>" + "<color=red>" + DetectedTarget.gameObject.name + "</color>" + "</b>" + "  |" +
                        "<b>" + "<color=green>" + "  Target Type: " + "</color>" + "<color=red>" + EmeraldComponent.TargetTypeRef + "</color>" + "</b>");
                }
            }
        }

        /// <summary>
        /// Used for detecting and assigning the AI's target type.
        /// </summary>
        public void DetectTargetType (Transform Target, bool? OverrideFactionRequirement = false)
        {
            if (Target != null)
            {
                if (Target.tag == EmeraldComponent.EmeraldTag)
                {
                    EmeraldComponent.ReceivedFaction = Target.GetComponent<EmeraldAISystem>().CurrentFaction;
                }

                if (Target.tag == EmeraldComponent.EmeraldTag && EmeraldComponent.AIFactionsList.Contains(EmeraldComponent.ReceivedFaction) &&
                    EmeraldComponent.FactionRelations[EmeraldComponent.AIFactionsList.IndexOf(EmeraldComponent.ReceivedFaction)] == 0 || 
                    Target.tag == EmeraldComponent.EmeraldTag && (bool)OverrideFactionRequirement)
                {
                    EmeraldComponent.TargetTypeRef = EmeraldAISystem.TargetType.AI;
                    EmeraldComponent.TargetEmerald = Target.GetComponent<EmeraldAISystem>();
                }
                else if (Target.tag == EmeraldComponent.PlayerTag && EmeraldComponent.PlayerFaction[0].RelationTypeRef == EmeraldAISystem.PlayerFactionClass.RelationType.Enemy) //Enemy Player
                {
                    if (Target.GetComponent<EmeraldAIPlayerDamage>() == null)
                    {
                        EmeraldComponent.PlayerDamageComponent = Target.gameObject.AddComponent<EmeraldAIPlayerDamage>();
                    }
                    else 
                    {
                        if (EmeraldComponent.PlayerDamageComponent == null)
                        {
                            EmeraldComponent.PlayerDamageComponent = Target.GetComponent<EmeraldAIPlayerDamage>();
                        }
                    }

                    EmeraldComponent.TargetTypeRef = EmeraldAISystem.TargetType.Player;
                    EmeraldComponent.TargetEmerald = null;
                }
                else if (Target.tag == EmeraldComponent.PlayerTag && EmeraldComponent.PlayerFaction[0].RelationTypeRef != EmeraldAISystem.PlayerFactionClass.RelationType.Enemy) //Friendly Player
                {
                    EmeraldComponent.TargetTypeRef = EmeraldAISystem.TargetType.Player;
                    EmeraldComponent.TargetEmerald = null;
                }
                else if (Target.tag == EmeraldComponent.NonAITag && EmeraldComponent.UseNonAITagRef == EmeraldAISystem.YesOrNo.Yes)
                {
                    if (Target.GetComponent<EmeraldAINonAIDamage>() == null)
                    {
                        EmeraldComponent.NonAIDamageComponent = Target.gameObject.AddComponent<EmeraldAINonAIDamage>();
                    }
                    else
                    {
                        if (EmeraldComponent.NonAIDamageComponent == null)
                        {
                            EmeraldComponent.NonAIDamageComponent = Target.GetComponent<EmeraldAINonAIDamage>();
                        }
                    }

                    EmeraldComponent.TargetTypeRef = EmeraldAISystem.TargetType.NonAITarget;
                    EmeraldComponent.TargetEmerald = null;
                }

                GetTargetPositionModifier(Target);
            }
        }

        public void StartRandomTarget ()
        {
            SearchForRandomTarget = true;
        }

        /// <summary>
        /// Gets and sets the faction of the passed AI target. 
        /// </summary>
        void GetTargetFaction (Transform Target)
        {
            if (Target.GetComponent<EmeraldAISystem>() != null)
            {
                EmeraldComponent.ReceivedFaction = Target.GetComponent<EmeraldAISystem>().CurrentFaction;
            }
            else
            {
                Debug.Log("The Target parameter is not an Emerald AI agent.");
            }
        }

        /// <summary>
        /// If a TargetPositionModifier is found, apply the height to the CurrentPositionModifier
        /// </summary>
        /// <param name="Target"></param>
        void GetTargetPositionModifier(Transform Target)
        {
            if (Target.GetComponent<TargetPositionModifier>() != null)
            {
                EmeraldComponent.CurrentTargetPositionModifierComp = Target.GetComponent<TargetPositionModifier>();
                EmeraldComponent.CurrentPositionModifier = EmeraldComponent.CurrentTargetPositionModifierComp.PositionModifier;
            }
            else
            {
                EmeraldComponent.CurrentPositionModifier = 0;
            }
        }
    }
}